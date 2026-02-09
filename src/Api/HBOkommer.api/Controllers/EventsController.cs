using System.Data;
using HboKommer.Shared.Contracts;
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

        // Vi returnerer 202 uansett; forskjellen er om det var duplikat.
        return Accepted(new
        {
            received = true,
            duplicate = !inserted,
            evt.EventId,
            evt.EventType
        });
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
            // 2601/2627 = unique index/constraint violation => EventId finnes allerede
            return false; // duplicate
        }
    }
}
