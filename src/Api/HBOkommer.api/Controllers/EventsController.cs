using HboKommer.Shared.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace HboKommer.Api.Controllers;

[ApiController]
[Route("api/v1/events")]
public sealed class EventsController : ControllerBase
{
    [HttpPost("visit-started")]
    public IActionResult ReceiveVisitStarted([FromBody] VisitStartedEventV1 evt)
    {
        // Trinn 1: Minimal validering + kvittering.
        // Ingen DB, ingen idempotens, ingen regler, ingen SMS.

        if (evt is null)
            return BadRequest(new { reasonCode = "INVALID_SCHEMA" });

        // Minimum: sjekk på tomme felt (fail-closed)
        if (string.IsNullOrWhiteSpace(evt.EventId) ||
            string.IsNullOrWhiteSpace(evt.EventType) ||
            string.IsNullOrWhiteSpace(evt.MunicipalityId) ||
            string.IsNullOrWhiteSpace(evt.UnitId) ||
            string.IsNullOrWhiteSpace(evt.SubjectRef) ||
            evt.OccurredAtUtc == default)
        {
            return BadRequest(new { reasonCode = "INVALID_SCHEMA" });
        }

        // Pilotforventning: eventType "VISIT_STARTED"
        if (!string.Equals(evt.EventType, "VISIT_STARTED", StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { reasonCode = "INVALID_SCHEMA" });

        // Kvitter raskt (asynkron behandling kommer senere i produksjonsløpet)
        return Accepted(new
        {
            received = true,
            evt.EventId,
            evt.EventType
        });
    }
}
