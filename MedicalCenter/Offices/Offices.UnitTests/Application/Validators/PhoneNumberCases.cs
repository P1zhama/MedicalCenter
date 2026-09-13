namespace Offices.UnitTests.Application.Validators;

public static class PhoneNumberCases
{
    public static TheoryData<string> WellFormed =>
    [
        "12345",
        "+12345",
        "74951234567",
        "+74951234567",
        "12345678901234567890",
        "+12345678901234567890"
    ];

    public static TheoryData<string> Malformed =>
    [
        "1234",
        "+1234",
        "123456789012345678901",
        "+7 495 123-45-67",
        "+7-495-1234567",
        "8(495)1234567",
        "++74951234567",
        "74951234567+",
        "7495123456a",
        " +74951234567"
    ];
}
