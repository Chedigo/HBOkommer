using HboKommer.Api.Services;
using System.Data;
using HboKommer.Shared.Contracts;
using HboKommer.Shared.Policy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace HboKommer.Api.Controllers;

[ApiController]
[Route("api/v1/events")]
public sealed class EventsController : ControllerBase
{
    private readonly IConfiguration _config;

    public EventsController(IConfiguration config)
    {
        _config = config;
    }

    [HttpPost("visit-started")]
    public async Task<IActionResult> ReceiveVisitStarted([FromBody] VisitStartedEventV1 evt)
    {
        // Trinn 1: Minimal validering (fail-closed)
        if (evt is null)
            return BadRequest(new { reasonCode = "INVALID_SCHEMA" });

        if (string.IsNullOrWhiteSpace(evt.EventId) ||
            string.IsNullOrWhiteSpace(evt.EventType) ||
            string.IsNullOrWhiteSpace(evt.MunicipalityId) ||
            string.IsNullOrWhiteSpace(evt.UnitId) ||
            string.IsNullOrWhiteSpace(evt.SubjectRef) ||
            evt.OccurredAtUtc == default)
        {
            return BadRequest(new { reasonCode = "INVALID_SCHEMA" });
        }

        if (!string.Equals(evt.EventType, "VISIT_STARTED", StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { reasonCode = "INVALID_SCHEMA" });

        var cs = _config.GetConnectionString("HboKommerDb");
        if (string.IsNullOrWhiteSpace(cs))
            return StatusCode(500, new { reasonCode = "SERVER_MISCONFIGURED" });

        // Trinn 2: Idempotens via unik index på EventId
        var inserted = await TryInsertInboundEventAsync(cs, evt);

        // Trinn 3: Duplikat behandles ikke videre
        if (!inserted)
        {
            return Accepted(new
            {
                received = true,
                duplicate = true,
                eligible = false,
                reasonCode = "DUPLICATE_EVENT_IGNORED",
                evt.EventId,
                evt.EventType
            });
        }

        // Trinn 3: Policykontroll
        var receivedAtUtc = DateTimeOffset.UtcNow;
        var decision = EvaluatePilotPolicy(evt, receivedAtUtc);

        // Hvis policy sier NEI, gjør vi ikke kontaktoppslag
        if (!decision.Eligible)
        {
            return Accepted(new
            {
                received = true,
                duplicate = false,
                eligible = false,
                reasonCode = decision.ReasonCode,
                evt.EventId,
                evt.EventType
            });
        }

        // Trinn 4: Kontaktoppslag (mapping-tabell)
        var resolver = new ContactResolver();
        var contact = await resolver.TryResolveAsync(cs, evt.SubjectRef);

        if (!contact.Found)
        {
            return Accepted(new
            {
                received = true,
                duplicate = false,
                eligible = false,
                reasonCode = "CONTACT_NOT_FOUND",
                hasContact = false,
                evt.EventId,
                evt.EventType
            });
        }

        // Fortsatt ingen SMS. Vi bekrefter bare at vi har kontaktdata.
        return Accepted(new
        {
            received = true,
            duplicate = false,
            eligible = true,
            reasonCode = "OK",
            hasContact = true,
            contactSource = contact.Source,
            phoneE164 = contact.PhoneE164, // OK i pilot/test; kan fjernes senere
            evt.EventId,
            evt.EventType
        });
    }

    private static PolicyDecision EvaluatePilotPolicy(VisitStartedEventV1 evt, DateTimeOffset receivedAtUtc)
    {
        // 1) Gyldighetsvindu (pilot): avvis hendelser som er for gamle
        var maxAge = TimeSpan.FromHours(2);
        var age = receivedAtUtc - evt.OccurredAtUtc;

        if (age > maxAge)
            return new PolicyDecision { Eligible = false, ReasonCode = "EVENT_TOO_OLD" };

        // 2) Samtykke (pilot-stub)
        // TODO (senere trinn): erstatt HasConsent(...) med reell samtykke-/policy-motor
        // basert på kommunal autoritativ kilde / konfigurasjon.
        if (!HasConsent(evt.MunicipalityId, evt.UnitId, evt.SubjectRef))
            return new PolicyDecision { Eligible = false, ReasonCode = "CONSENT_NOT_GRANTED" };

        return new PolicyDecision { Eligible = true, ReasonCode = "OK" };
    }

    private static bool HasConsent(string municipalityId, string unitId, string subjectRef)
    {
        // PILOT: samtykke antas OK for å la flyten gå videre i testmiljø.
        // Senere: slå opp samtykke eksplisitt (ikke default-true i prod).
        return true;
    }


    private static async Task<bool> TryInsertInboundEventAsync(string connectionString, VisitStartedEventV1 evt)
    {
        const string sql = @"
INSERT INTO dbo.InboundEvents (EventId, EventType, MunicipalityId, UnitId, SubjectRef, OccurredAtUtc)
VALUES (@EventId, @EventType, @MunicipalityId, @UnitId, @SubjectRef, @OccurredAtUtc);";

        try
        {
            await using var conn = new SqlConnection(connectionString);
            await conn.OpenAsync();

            await using var cmd = new SqlCommand(sql, conn);
            cmd.CommandType = CommandType.Text;

            cmd.Parameters.AddWithValue("@EventId", evt.EventId);
            cmd.Parameters.AddWithValue("@EventType", evt.EventType);
            cmd.Parameters.AddWithValue("@MunicipalityId", evt.MunicipalityId);
            cmd.Parameters.AddWithValue("@UnitId", evt.UnitId);
            cmd.Parameters.AddWithValue("@SubjectRef", evt.SubjectRef);
            cmd.Parameters.AddWithValue("@OccurredAtUtc", evt.OccurredAtUtc);

            await cmd.ExecuteNonQueryAsync();
            return true; // inserted
        }
        catch (SqlException ex) when (ex.Number is 2601 or 2627)
        {
            return false; // duplicate
        }
    }
}
