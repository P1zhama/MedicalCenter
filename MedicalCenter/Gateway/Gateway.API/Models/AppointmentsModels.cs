using System.Collections.Generic;

namespace Gateway.Api.Models;

public record AvailableSlotWebResponse(
    string StartTime,
    IReadOnlyList<string> DoctorIds
);

public record CreateAppointmentWebRequest(
    string ServiceId,
    string DoctorId,
    string OfficeId,
    string Date,
    string StartTime
);

public record CreateAppointmentByReceptionistWebRequest(
    string PatientId,
    string ServiceId,
    string DoctorId,
    string OfficeId,
    string Date,
    string StartTime
);

public record RescheduleAppointmentWebRequest(
    string DoctorId,
    string Date,
    string StartTime
);

public record CreatedAppointmentWebResponse(string AppointmentId);
