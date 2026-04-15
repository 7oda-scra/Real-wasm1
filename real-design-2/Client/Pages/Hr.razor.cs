using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using RealDesign2.Client.Services;
using RealDesign2.Shared.Models;

namespace RealDesign2.Client.Pages;

public partial class Hr : ComponentBase
{
    [Inject]
    private HrMockDataService HrMockDataService { get; set; } = default!;

    [Inject]
    private ISnackbar Snackbar { get; set; } = default!;

    private readonly IReadOnlyList<HrNavigationSection> _navigationSections =
    [
        new HrNavigationSection(
            "Employee Data",
            Icons.Material.Filled.FolderShared,
            [
                new HrNavigationItem(HrViewKeys.Sectors, "Sectors"),
                new HrNavigationItem(HrViewKeys.Departments, "Departments"),
                new HrNavigationItem(HrViewKeys.Sections, "Sections"),
                new HrNavigationItem(HrViewKeys.Jobs, "Positions / Jobs"),
                new HrNavigationItem(HrViewKeys.Qualifications, "Qualifications"),
                new HrNavigationItem(HrViewKeys.Employees, "Employees")
            ]),
        new HrNavigationSection(
            "Shifts",
            Icons.Material.Filled.Schedule,
            [
                new HrNavigationItem(HrViewKeys.AttendanceSystems, "Time and Attendance"),
                new HrNavigationItem(HrViewKeys.Shifts, "Shifts")
            ]),
        new HrNavigationSection(
            "Salary Settings",
            Icons.Material.Filled.Paid,
            [
                new HrNavigationItem(HrViewKeys.SalaryItems, "Salary Items"),
                new HrNavigationItem(HrViewKeys.OvertimeSettings, "Overtime Settings"),
                new HrNavigationItem(HrViewKeys.SalaryCalculationRules, "Salary Calculation Rules")
            ])
    ];

    private string _activeForm = HrViewKeys.Sectors;

    private List<Sector> _sectors = [];
    private List<Department> _departments = [];
    private List<Section> _sections = [];
    private List<Job> _jobs = [];
    private List<Qualification> _qualifications = [];
    private List<EmployeeRecord> _employees = [];
    private List<AttendanceSystem> _attendanceSystems = [];
    private List<Shift> _shifts = [];
    private List<BaseDto> _salaryItems = [];
    private List<BaseDto> _overtimeSettings = [];
    private List<BaseDto> _salaryCalculationRules = [];

    private Sector _sectorDraft = new();
    private Department _departmentDraft = new();
    private Section _sectionDraft = new();
    private Job _jobDraft = new();
    private Qualification _qualificationDraft = new();
    private EmployeeRecord _employeeDraft = new();
    private AttendanceSystem _attendanceSystemDraft = new();
    private Shift _shiftDraft = new();
    private BaseDto _salaryItemDraft = new();
    private BaseDto _overtimeSettingDraft = new();
    private BaseDto _salaryCalculationRuleDraft = new();

    protected override void OnInitialized()
    {
        _sectors = [.. HrMockDataService.GetSectors()];
        _departments = [.. HrMockDataService.GetDepartments()];
        _sections = [.. HrMockDataService.GetSections()];
        _jobs = [.. HrMockDataService.GetJobs()];
        _qualifications = [.. HrMockDataService.GetQualifications()];
        _attendanceSystems = [.. HrMockDataService.GetAttendanceSystems()];
        _shifts = [.. HrMockDataService.GetShifts()];
        _employees =
        [
            new EmployeeRecord
            {
                Id = "EMP-001",
                Name = "Mahmoud Diaa",
                Title = "Frontend Programmer",
                Email = "mahmoud.diaa@smartcode.local",
                Department = "Technology"
            },
            new EmployeeRecord
            {
                Id = "EMP-002",
                Name = "Ahmed Samak",
                Title = "Tech Support",
                Email = "ahmed.samak@smartcode.local",
                Department = "Human Resources"
            }
        ];
        _salaryItems =
        [
            new BaseDto { Id = "SAL-001", Name = "Housing Allowance" },
            new BaseDto { Id = "SAL-002", Name = "Transport Allowance" },
            new BaseDto { Id = "SAL-003", Name = "Medical Deduction" }
        ];
        _overtimeSettings =
        [
            new BaseDto { Id = "OVT-001", Name = "Weekday Overtime" },
            new BaseDto { Id = "OVT-002", Name = "Weekend Overtime" },
            new BaseDto { Id = "OVT-003", Name = "Holiday Overtime" }
        ];
        _salaryCalculationRules =
        [
            new BaseDto { Id = "CAL-001", Name = "Gross to Net Standard" },
            new BaseDto { Id = "CAL-002", Name = "Shift Differential Rule" },
            new BaseDto { Id = "CAL-003", Name = "Pro-rated Joiner Rule" }
        ];

        ResetAllDrafts();
    }

    private string GetActiveCss(string formName)
    {
        return _activeForm == formName ? "mud-nav-link-active" : string.Empty;
    }

    private void SetActiveForm(string formName)
    {
        _activeForm = formName;
        LoadDraftForActiveForm();
    }

    private void CloseForm()
    {
        _activeForm = string.Empty;
    }

    private void HandleAdd(MouseEventArgs _)
    {
        ResetDraftForActiveForm();
        Snackbar.Add($"{_activeForm} is ready for a new entry.", Severity.Info);
    }

    private void HandleModify(MouseEventArgs _)
    {
        LoadDraftForActiveForm();
        Snackbar.Add($"{_activeForm} sample loaded into the form.", Severity.Normal);
    }

    private void HandleDelete(MouseEventArgs _)
    {
        switch (_activeForm)
        {
            case HrViewKeys.Sectors:
                DeleteBasicItem(_sectors, _sectorDraft);
                ResetBasicDraft(_sectors, value => _sectorDraft = value);
                break;
            case HrViewKeys.Departments:
                DeleteBasicItem(_departments, _departmentDraft);
                ResetBasicDraft(_departments, value => _departmentDraft = value);
                break;
            case HrViewKeys.Sections:
                DeleteBasicItem(_sections, _sectionDraft);
                ResetBasicDraft(_sections, value => _sectionDraft = value);
                break;
            case HrViewKeys.Jobs:
                DeleteBasicItem(_jobs, _jobDraft);
                ResetBasicDraft(_jobs, value => _jobDraft = value);
                break;
            case HrViewKeys.Qualifications:
                DeleteBasicItem(_qualifications, _qualificationDraft);
                ResetBasicDraft(_qualifications, value => _qualificationDraft = value);
                break;
            case HrViewKeys.Employees:
                DeleteEmployee();
                break;
            case HrViewKeys.AttendanceSystems:
                DeleteAttendanceSystem();
                break;
            case HrViewKeys.Shifts:
                DeleteShift();
                break;
            case HrViewKeys.SalaryItems:
                DeleteBasicItem(_salaryItems, _salaryItemDraft);
                ResetBasicDraft(_salaryItems, value => _salaryItemDraft = value);
                break;
            case HrViewKeys.OvertimeSettings:
                DeleteBasicItem(_overtimeSettings, _overtimeSettingDraft);
                ResetBasicDraft(_overtimeSettings, value => _overtimeSettingDraft = value);
                break;
            case HrViewKeys.SalaryCalculationRules:
                DeleteBasicItem(_salaryCalculationRules, _salaryCalculationRuleDraft);
                ResetBasicDraft(_salaryCalculationRules, value => _salaryCalculationRuleDraft = value);
                break;
        }
    }

    private void HandleSave(MouseEventArgs _)
    {
        switch (_activeForm)
        {
            case HrViewKeys.Sectors:
                SaveBasicItem(_sectors, _sectorDraft, value => _sectorDraft = value);
                break;
            case HrViewKeys.Departments:
                SaveBasicItem(_departments, _departmentDraft, value => _departmentDraft = value);
                break;
            case HrViewKeys.Sections:
                SaveBasicItem(_sections, _sectionDraft, value => _sectionDraft = value);
                break;
            case HrViewKeys.Jobs:
                SaveBasicItem(_jobs, _jobDraft, value => _jobDraft = value);
                break;
            case HrViewKeys.Qualifications:
                SaveBasicItem(_qualifications, _qualificationDraft, value => _qualificationDraft = value);
                break;
            case HrViewKeys.Employees:
                SaveEmployee();
                break;
            case HrViewKeys.AttendanceSystems:
                SaveAttendanceSystem();
                break;
            case HrViewKeys.Shifts:
                SaveShift();
                break;
            case HrViewKeys.SalaryItems:
                SaveBasicItem(_salaryItems, _salaryItemDraft, value => _salaryItemDraft = value);
                break;
            case HrViewKeys.OvertimeSettings:
                SaveBasicItem(_overtimeSettings, _overtimeSettingDraft, value => _overtimeSettingDraft = value);
                break;
            case HrViewKeys.SalaryCalculationRules:
                SaveBasicItem(_salaryCalculationRules, _salaryCalculationRuleDraft, value => _salaryCalculationRuleDraft = value);
                break;
        }
    }

    private void HandleToolbarClose(MouseEventArgs _)
    {
        CloseForm();
    }

    private void ResetAllDrafts()
    {
        ResetBasicDraft(_sectors, value => _sectorDraft = value);
        ResetBasicDraft(_departments, value => _departmentDraft = value);
        ResetBasicDraft(_sections, value => _sectionDraft = value);
        ResetBasicDraft(_jobs, value => _jobDraft = value);
        ResetBasicDraft(_qualifications, value => _qualificationDraft = value);
        _employeeDraft = _employees.Count > 0 ? CloneEmployee(_employees[0]) : new EmployeeRecord();
        _attendanceSystemDraft = _attendanceSystems.Count > 0 ? CloneAttendanceSystem(_attendanceSystems[0]) : new AttendanceSystem();
        _shiftDraft = _shifts.Count > 0 ? CloneShift(_shifts[0]) : new Shift();
        ResetBasicDraft(_salaryItems, value => _salaryItemDraft = value);
        ResetBasicDraft(_overtimeSettings, value => _overtimeSettingDraft = value);
        ResetBasicDraft(_salaryCalculationRules, value => _salaryCalculationRuleDraft = value);
    }

    private void LoadDraftForActiveForm()
    {
        switch (_activeForm)
        {
            case HrViewKeys.Sectors:
                ResetBasicDraft(_sectors, value => _sectorDraft = value);
                break;
            case HrViewKeys.Departments:
                ResetBasicDraft(_departments, value => _departmentDraft = value);
                break;
            case HrViewKeys.Sections:
                ResetBasicDraft(_sections, value => _sectionDraft = value);
                break;
            case HrViewKeys.Jobs:
                ResetBasicDraft(_jobs, value => _jobDraft = value);
                break;
            case HrViewKeys.Qualifications:
                ResetBasicDraft(_qualifications, value => _qualificationDraft = value);
                break;
            case HrViewKeys.Employees:
                _employeeDraft = _employees.Count > 0 ? CloneEmployee(_employees[0]) : new EmployeeRecord();
                break;
            case HrViewKeys.AttendanceSystems:
                _attendanceSystemDraft = _attendanceSystems.Count > 0 ? CloneAttendanceSystem(_attendanceSystems[0]) : new AttendanceSystem();
                break;
            case HrViewKeys.Shifts:
                _shiftDraft = _shifts.Count > 0 ? CloneShift(_shifts[0]) : new Shift();
                break;
            case HrViewKeys.SalaryItems:
                ResetBasicDraft(_salaryItems, value => _salaryItemDraft = value);
                break;
            case HrViewKeys.OvertimeSettings:
                ResetBasicDraft(_overtimeSettings, value => _overtimeSettingDraft = value);
                break;
            case HrViewKeys.SalaryCalculationRules:
                ResetBasicDraft(_salaryCalculationRules, value => _salaryCalculationRuleDraft = value);
                break;
        }
    }

    private void ResetDraftForActiveForm()
    {
        switch (_activeForm)
        {
            case HrViewKeys.Sectors:
                _sectorDraft = new Sector();
                break;
            case HrViewKeys.Departments:
                _departmentDraft = new Department();
                break;
            case HrViewKeys.Sections:
                _sectionDraft = new Section();
                break;
            case HrViewKeys.Jobs:
                _jobDraft = new Job();
                break;
            case HrViewKeys.Qualifications:
                _qualificationDraft = new Qualification();
                break;
            case HrViewKeys.Employees:
                _employeeDraft = new EmployeeRecord();
                break;
            case HrViewKeys.AttendanceSystems:
                _attendanceSystemDraft = new AttendanceSystem();
                break;
            case HrViewKeys.Shifts:
                _shiftDraft = new Shift();
                break;
            case HrViewKeys.SalaryItems:
                _salaryItemDraft = new BaseDto();
                break;
            case HrViewKeys.OvertimeSettings:
                _overtimeSettingDraft = new BaseDto();
                break;
            case HrViewKeys.SalaryCalculationRules:
                _salaryCalculationRuleDraft = new BaseDto();
                break;
        }
    }

    private void SaveBasicItem<T>(List<T> items, T draft, Action<T> setDraft)
        where T : BaseDto, new()
    {
        if (string.IsNullOrWhiteSpace(draft.Id) || string.IsNullOrWhiteSpace(draft.Name))
        {
            Snackbar.Add("ID and Name are required before saving.", Severity.Warning);
            return;
        }

        var existing = items.FirstOrDefault(item => item.Id.Equals(draft.Id, StringComparison.OrdinalIgnoreCase));
        if (existing is null)
        {
            items.Add(CloneBaseDto(draft));
            Snackbar.Add($"{_activeForm} entry added.", Severity.Success);
        }
        else
        {
            existing.Name = draft.Name;
            Snackbar.Add($"{_activeForm} entry updated.", Severity.Success);
        }

        setDraft(CloneBaseDto(draft));
    }

    private void DeleteBasicItem<T>(List<T> items, T draft)
        where T : BaseDto, new()
    {
        if (string.IsNullOrWhiteSpace(draft.Id))
        {
            Snackbar.Add("Select or enter an ID before deleting.", Severity.Warning);
            return;
        }

        var removedCount = items.RemoveAll(item => item.Id.Equals(draft.Id, StringComparison.OrdinalIgnoreCase));
        Snackbar.Add(
            removedCount > 0 ? $"{_activeForm} entry deleted." : "No matching entry was found.",
            removedCount > 0 ? Severity.Success : Severity.Info);
    }

    private void ResetBasicDraft<T>(List<T> items, Action<T> setDraft)
        where T : BaseDto, new()
    {
        setDraft(items.Count > 0 ? CloneBaseDto(items[0]) : new T());
    }

    private void SaveEmployee()
    {
        if (string.IsNullOrWhiteSpace(_employeeDraft.Id) ||
            string.IsNullOrWhiteSpace(_employeeDraft.Name) ||
            string.IsNullOrWhiteSpace(_employeeDraft.Title))
        {
            Snackbar.Add("Employee ID, name, and title are required before saving.", Severity.Warning);
            return;
        }

        var existing = _employees.FirstOrDefault(item => item.Id.Equals(_employeeDraft.Id, StringComparison.OrdinalIgnoreCase));
        if (existing is null)
        {
            _employees.Add(CloneEmployee(_employeeDraft));
            Snackbar.Add("Employee added to the directory.", Severity.Success);
        }
        else
        {
            existing.Name = _employeeDraft.Name;
            existing.Title = _employeeDraft.Title;
            existing.Email = _employeeDraft.Email;
            existing.Department = _employeeDraft.Department;
            Snackbar.Add("Employee record updated.", Severity.Success);
        }
    }

    private void DeleteEmployee()
    {
        if (string.IsNullOrWhiteSpace(_employeeDraft.Id))
        {
            Snackbar.Add("Enter an employee ID before deleting.", Severity.Warning);
            return;
        }

        var removedCount = _employees.RemoveAll(item => item.Id.Equals(_employeeDraft.Id, StringComparison.OrdinalIgnoreCase));
        Snackbar.Add(
            removedCount > 0 ? "Employee removed from the directory." : "No employee matched that ID.",
            removedCount > 0 ? Severity.Success : Severity.Info);

        _employeeDraft = _employees.Count > 0 ? CloneEmployee(_employees[0]) : new EmployeeRecord();
    }

    private void SaveAttendanceSystem()
    {
        if (string.IsNullOrWhiteSpace(_attendanceSystemDraft.Id) || string.IsNullOrWhiteSpace(_attendanceSystemDraft.Name))
        {
            Snackbar.Add("Attendance system ID and name are required before saving.", Severity.Warning);
            return;
        }

        var existing = _attendanceSystems.FirstOrDefault(item => item.Id.Equals(_attendanceSystemDraft.Id, StringComparison.OrdinalIgnoreCase));
        if (existing is null)
        {
            _attendanceSystems.Add(CloneAttendanceSystem(_attendanceSystemDraft));
            Snackbar.Add("Attendance system added.", Severity.Success);
        }
        else
        {
            existing.Name = _attendanceSystemDraft.Name;
            existing.TrackingType = _attendanceSystemDraft.TrackingType;
            Snackbar.Add("Attendance system updated.", Severity.Success);
        }
    }

    private void DeleteAttendanceSystem()
    {
        if (string.IsNullOrWhiteSpace(_attendanceSystemDraft.Id))
        {
            Snackbar.Add("Enter an attendance system ID before deleting.", Severity.Warning);
            return;
        }

        var removedCount = _attendanceSystems.RemoveAll(item => item.Id.Equals(_attendanceSystemDraft.Id, StringComparison.OrdinalIgnoreCase));
        Snackbar.Add(
            removedCount > 0 ? "Attendance system deleted." : "No attendance system matched that ID.",
            removedCount > 0 ? Severity.Success : Severity.Info);

        _attendanceSystemDraft = _attendanceSystems.Count > 0 ? CloneAttendanceSystem(_attendanceSystems[0]) : new AttendanceSystem();
    }

    private void SaveShift()
    {
        if (string.IsNullOrWhiteSpace(_shiftDraft.Id) ||
            string.IsNullOrWhiteSpace(_shiftDraft.Name) ||
            string.IsNullOrWhiteSpace(_shiftDraft.AttendanceSystemId))
        {
            Snackbar.Add("Shift ID, name, and attendance system are required before saving.", Severity.Warning);
            return;
        }

        var existing = _shifts.FirstOrDefault(item => item.Id.Equals(_shiftDraft.Id, StringComparison.OrdinalIgnoreCase));
        if (existing is null)
        {
            _shifts.Add(CloneShift(_shiftDraft));
            Snackbar.Add("Shift added.", Severity.Success);
        }
        else
        {
            existing.Name = _shiftDraft.Name;
            existing.AttendanceSystemId = _shiftDraft.AttendanceSystemId;
            existing.StartTime = _shiftDraft.StartTime;
            existing.EndTime = _shiftDraft.EndTime;
            existing.WorkingHours = _shiftDraft.WorkingHours;
            existing.ClosesNextDay = _shiftDraft.ClosesNextDay;
            Snackbar.Add("Shift updated.", Severity.Success);
        }
    }

    private void DeleteShift()
    {
        if (string.IsNullOrWhiteSpace(_shiftDraft.Id))
        {
            Snackbar.Add("Enter a shift ID before deleting.", Severity.Warning);
            return;
        }

        var removedCount = _shifts.RemoveAll(item => item.Id.Equals(_shiftDraft.Id, StringComparison.OrdinalIgnoreCase));
        Snackbar.Add(
            removedCount > 0 ? "Shift deleted." : "No shift matched that ID.",
            removedCount > 0 ? Severity.Success : Severity.Info);

        _shiftDraft = _shifts.Count > 0 ? CloneShift(_shifts[0]) : new Shift();
    }

    private string GetTrackingTypeLabel(AttendanceTrackingType trackingType)
    {
        return trackingType switch
        {
            AttendanceTrackingType.FingerprintRequired => "Fingerprint Required",
            AttendanceTrackingType.OpenShift => "Open Shift",
            _ => trackingType.ToString()
        };
    }

    private string GetAttendanceSystemName(string attendanceSystemId)
    {
        return _attendanceSystems.FirstOrDefault(item => item.Id == attendanceSystemId)?.Name ?? "Unassigned";
    }

    private static T CloneBaseDto<T>(T source)
        where T : BaseDto, new()
    {
        return new T
        {
            Id = source.Id,
            Name = source.Name
        };
    }

    private static EmployeeRecord CloneEmployee(EmployeeRecord source)
    {
        return new EmployeeRecord
        {
            Id = source.Id,
            Name = source.Name,
            Title = source.Title,
            Email = source.Email,
            Department = source.Department
        };
    }

    private static AttendanceSystem CloneAttendanceSystem(AttendanceSystem source)
    {
        return new AttendanceSystem
        {
            Id = source.Id,
            Name = source.Name,
            TrackingType = source.TrackingType
        };
    }

    private static Shift CloneShift(Shift source)
    {
        return new Shift
        {
            Id = source.Id,
            Name = source.Name,
            AttendanceSystemId = source.AttendanceSystemId,
            StartTime = source.StartTime,
            EndTime = source.EndTime,
            WorkingHours = source.WorkingHours,
            ClosesNextDay = source.ClosesNextDay
        };
    }

    private sealed record HrNavigationSection(string Title, string Icon, IReadOnlyList<HrNavigationItem> Items);

    private sealed record HrNavigationItem(string Key, string Label);

    private sealed class EmployeeRecord
    {
        public string Id { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Department { get; set; } = string.Empty;
    }

    private static class HrViewKeys
    {
        public const string Sectors = "Sectors";
        public const string Departments = "Departments";
        public const string Sections = "Sections";
        public const string Jobs = "Positions/Jobs";
        public const string Qualifications = "Qualifications";
        public const string Employees = "Employees";
        public const string AttendanceSystems = "Time and Attendance system";
        public const string Shifts = "Shifts";
        public const string SalaryItems = "Salary Items";
        public const string OvertimeSettings = "Overtime settings";
        public const string SalaryCalculationRules = "Salary calculation rules";
    }
}
