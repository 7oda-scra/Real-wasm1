using RealDesign2.Shared.Models;

namespace RealDesign2.Client.Services;

public sealed class HrMockDataService
{
    public IReadOnlyList<Sector> GetSectors()
    {
        return
        [
            new Sector { Id = "SEC-001", Name = "Corporate Operations" },
            new Sector { Id = "SEC-002", Name = "Product Delivery" },
            new Sector { Id = "SEC-003", Name = "Customer Success" }
        ];
    }

    public IReadOnlyList<Department> GetDepartments()
    {
        return
        [
            new Department { Id = "DEP-001", Name = "Human Resources" },
            new Department { Id = "DEP-002", Name = "Finance" },
            new Department { Id = "DEP-003", Name = "Technology" }
        ];
    }

    public IReadOnlyList<Job> GetJobs()
    {
        return
        [
            new Job { Id = "JOB-001", Name = "HR Specialist" },
            new Job { Id = "JOB-002", Name = "Operations Analyst" },
            new Job { Id = "JOB-003", Name = "Support Engineer" }
        ];
    }

    public IReadOnlyList<Qualification> GetQualifications()
    {
        return
        [
            new Qualification { Id = "QLF-001", Name = "Bachelor Degree" },
            new Qualification { Id = "QLF-002", Name = "Professional Diploma" },
            new Qualification { Id = "QLF-003", Name = "Industry Certification" }
        ];
    }

    public IReadOnlyList<Section> GetSections()
    {
        return
        [
            new Section { Id = "SCT-001", Name = "People Operations" },
            new Section { Id = "SCT-002", Name = "Talent Acquisition" },
            new Section { Id = "SCT-003", Name = "Employee Experience" }
        ];
    }

    public IReadOnlyList<AttendanceSystem> GetAttendanceSystems()
    {
        return
        [
            new AttendanceSystem
            {
                Id = "ATT-001",
                Name = "Biometric Core",
                TrackingType = AttendanceTrackingType.FingerprintRequired
            },
            new AttendanceSystem
            {
                Id = "ATT-002",
                Name = "Flexible Operations",
                TrackingType = AttendanceTrackingType.OpenShift
            }
        ];
    }

    public IReadOnlyList<Shift> GetShifts()
    {
        return
        [
            new Shift
            {
                Id = "SFT-001",
                Name = "Morning Shift",
                AttendanceSystemId = "ATT-001",
                StartTime = new TimeSpan(8, 0, 0),
                EndTime = new TimeSpan(16, 0, 0),
                WorkingHours = 8,
                ClosesNextDay = false
            },
            new Shift
            {
                Id = "SFT-002",
                Name = "Support Rotation",
                AttendanceSystemId = "ATT-002",
                StartTime = new TimeSpan(16, 0, 0),
                EndTime = new TimeSpan(0, 0, 0),
                WorkingHours = 8,
                ClosesNextDay = true
            }
        ];
    }
}
