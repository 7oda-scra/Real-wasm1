namespace RealDesign2.Shared.Models;

public class BaseDto
{
    public string Id { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;
}

public sealed class Sector : BaseDto
{
}

public sealed class Department : BaseDto
{
}

public sealed class Job : BaseDto
{
}

public sealed class Qualification : BaseDto
{
}

public sealed class Section : BaseDto
{
}

public enum AttendanceTrackingType
{
    FingerprintRequired,
    OpenShift
}

public sealed class AttendanceSystem : BaseDto
{
    public AttendanceTrackingType TrackingType { get; set; } = AttendanceTrackingType.FingerprintRequired;
}

public sealed class Shift : BaseDto
{
    public string AttendanceSystemId { get; set; } = string.Empty;

    public TimeSpan? StartTime { get; set; }

    public TimeSpan? EndTime { get; set; }

    public int WorkingHours { get; set; }

    public bool ClosesNextDay { get; set; }
}
