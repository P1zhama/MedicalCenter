using System.Collections.Generic;

namespace Gateway.Api.Models;

public record AvailableSlotWebResponse(
    string StartTime,
    IReadOnlyList<string> DoctorIds
);
