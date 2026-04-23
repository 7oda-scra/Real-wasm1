using Microsoft.AspNetCore.Components;
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
    private bool _isDetailPanelOpen;

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

    private Sector? _selectedSector;
    private Department? _selectedDepartment;
    private Section? _selectedSection;
    private Job? _selectedJob;
    private Qualification? _selectedQualification;
    private EmployeeRecord? _selectedEmployee;
    private AttendanceSystem? _selectedAttendanceSystem;
    private Shift? _selectedShift;
    private BaseDto? _selectedSalaryItem;
    private BaseDto? _selectedOvertimeSetting;
    private BaseDto? _selectedSalaryCalculationRule;

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

    private List<BreadcrumbItem> BuildBreadcrumbs()
    {
        var items = new List<BreadcrumbItem>
        {
            new("Home", "/"),
            new("Human Resources", "/hr")
        };

        var activeSection = GetActiveSection();
        if (activeSection is not null)
        {
            items.Add(new BreadcrumbItem(activeSection.Title, href: null, disabled: true));
        }

        var activeLabel = GetActiveFormLabel();
        if (!string.IsNullOrWhiteSpace(activeLabel))
        {
            items.Add(new BreadcrumbItem(activeLabel, href: null, disabled: true));
        }

        return items;
    }

    private string GetNavLinkCss(string formName)
    {
        return _activeForm == formName
            ? "hr-nav-menu__link mud-nav-link-active"
            : "hr-nav-menu__link";
    }

    private void SetActiveForm(string formName)
    {
        _activeForm = formName;
        LoadDraftForActiveForm();
        _isDetailPanelOpen = false;
    }

    private void HandleAdd(MouseEventArgs _)
    {
        ClearActiveSelection();
        ResetDraftForActiveForm();
        _isDetailPanelOpen = true;
        Snackbar.Add($"{GetActiveFormLabel()} is ready for a new entry.", Severity.Info);
    }

    private void HandleModify(MouseEventArgs _)
    {
        if (!TryOpenCurrentSelection())
        {
            Snackbar.Add($"Select a {GetActiveFormLabel().ToLowerInvariant()} row before modifying.", Severity.Warning);
            return;
        }

        Snackbar.Add($"{GetActiveFormLabel()} loaded into the detail panel.", Severity.Normal);
    }

    private void HandleDelete(MouseEventArgs _)
    {
        switch (_activeForm)
        {
            case HrViewKeys.Sectors:
                DeleteBasicItem(_sectors, _sectorDraft, value => _selectedSector = value, value => _sectorDraft = value);
                break;
            case HrViewKeys.Departments:
                DeleteBasicItem(_departments, _departmentDraft, value => _selectedDepartment = value, value => _departmentDraft = value);
                break;
            case HrViewKeys.Sections:
                DeleteBasicItem(_sections, _sectionDraft, value => _selectedSection = value, value => _sectionDraft = value);
                break;
            case HrViewKeys.Jobs:
                DeleteBasicItem(_jobs, _jobDraft, value => _selectedJob = value, value => _jobDraft = value);
                break;
            case HrViewKeys.Qualifications:
                DeleteBasicItem(_qualifications, _qualificationDraft, value => _selectedQualification = value, value => _qualificationDraft = value);
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
                DeleteBasicItem(_salaryItems, _salaryItemDraft, value => _selectedSalaryItem = value, value => _salaryItemDraft = value);
                break;
            case HrViewKeys.OvertimeSettings:
                DeleteBasicItem(_overtimeSettings, _overtimeSettingDraft, value => _selectedOvertimeSetting = value, value => _overtimeSettingDraft = value);
                break;
            case HrViewKeys.SalaryCalculationRules:
                DeleteBasicItem(_salaryCalculationRules, _salaryCalculationRuleDraft, value => _selectedSalaryCalculationRule = value, value => _salaryCalculationRuleDraft = value);
                break;
        }
    }

    private void HandleSave(MouseEventArgs _)
    {
        switch (_activeForm)
        {
            case HrViewKeys.Sectors:
                SaveBasicItem(_sectors, _sectorDraft, value => _sectorDraft = value, value => _selectedSector = value);
                break;
            case HrViewKeys.Departments:
                SaveBasicItem(_departments, _departmentDraft, value => _departmentDraft = value, value => _selectedDepartment = value);
                break;
            case HrViewKeys.Sections:
                SaveBasicItem(_sections, _sectionDraft, value => _sectionDraft = value, value => _selectedSection = value);
                break;
            case HrViewKeys.Jobs:
                SaveBasicItem(_jobs, _jobDraft, value => _jobDraft = value, value => _selectedJob = value);
                break;
            case HrViewKeys.Qualifications:
                SaveBasicItem(_qualifications, _qualificationDraft, value => _qualificationDraft = value, value => _selectedQualification = value);
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
                SaveBasicItem(_salaryItems, _salaryItemDraft, value => _salaryItemDraft = value, value => _selectedSalaryItem = value);
                break;
            case HrViewKeys.OvertimeSettings:
                SaveBasicItem(_overtimeSettings, _overtimeSettingDraft, value => _overtimeSettingDraft = value, value => _selectedOvertimeSetting = value);
                break;
            case HrViewKeys.SalaryCalculationRules:
                SaveBasicItem(_salaryCalculationRules, _salaryCalculationRuleDraft, value => _salaryCalculationRuleDraft = value, value => _selectedSalaryCalculationRule = value);
                break;
        }
    }

    private void HandleToolbarClose(MouseEventArgs _)
    {
        _isDetailPanelOpen = false;
    }

    private void ResetAllDrafts()
    {
        _sectorDraft = new Sector();
        _departmentDraft = new Department();
        _sectionDraft = new Section();
        _jobDraft = new Job();
        _qualificationDraft = new Qualification();
        _employeeDraft = new EmployeeRecord();
        _attendanceSystemDraft = new AttendanceSystem();
        _shiftDraft = new Shift();
        _salaryItemDraft = new BaseDto();
        _overtimeSettingDraft = new BaseDto();
        _salaryCalculationRuleDraft = new BaseDto();
    }

    private void LoadDraftForActiveForm()
    {
        switch (_activeForm)
        {
            case HrViewKeys.Sectors:
                _sectorDraft = _selectedSector is not null ? CloneBaseDto(_selectedSector) : new Sector();
                break;
            case HrViewKeys.Departments:
                _departmentDraft = _selectedDepartment is not null ? CloneBaseDto(_selectedDepartment) : new Department();
                break;
            case HrViewKeys.Sections:
                _sectionDraft = _selectedSection is not null ? CloneBaseDto(_selectedSection) : new Section();
                break;
            case HrViewKeys.Jobs:
                _jobDraft = _selectedJob is not null ? CloneBaseDto(_selectedJob) : new Job();
                break;
            case HrViewKeys.Qualifications:
                _qualificationDraft = _selectedQualification is not null ? CloneBaseDto(_selectedQualification) : new Qualification();
                break;
            case HrViewKeys.Employees:
                _employeeDraft = _selectedEmployee is not null ? CloneEmployee(_selectedEmployee) : new EmployeeRecord();
                break;
            case HrViewKeys.AttendanceSystems:
                _attendanceSystemDraft = _selectedAttendanceSystem is not null ? CloneAttendanceSystem(_selectedAttendanceSystem) : new AttendanceSystem();
                break;
            case HrViewKeys.Shifts:
                _shiftDraft = _selectedShift is not null ? CloneShift(_selectedShift) : new Shift();
                break;
            case HrViewKeys.SalaryItems:
                _salaryItemDraft = _selectedSalaryItem is not null ? CloneBaseDto(_selectedSalaryItem) : new BaseDto();
                break;
            case HrViewKeys.OvertimeSettings:
                _overtimeSettingDraft = _selectedOvertimeSetting is not null ? CloneBaseDto(_selectedOvertimeSetting) : new BaseDto();
                break;
            case HrViewKeys.SalaryCalculationRules:
                _salaryCalculationRuleDraft = _selectedSalaryCalculationRule is not null ? CloneBaseDto(_selectedSalaryCalculationRule) : new BaseDto();
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

    private void ClearActiveSelection()
    {
        switch (_activeForm)
        {
            case HrViewKeys.Sectors:
                _selectedSector = null;
                break;
            case HrViewKeys.Departments:
                _selectedDepartment = null;
                break;
            case HrViewKeys.Sections:
                _selectedSection = null;
                break;
            case HrViewKeys.Jobs:
                _selectedJob = null;
                break;
            case HrViewKeys.Qualifications:
                _selectedQualification = null;
                break;
            case HrViewKeys.Employees:
                _selectedEmployee = null;
                break;
            case HrViewKeys.AttendanceSystems:
                _selectedAttendanceSystem = null;
                break;
            case HrViewKeys.Shifts:
                _selectedShift = null;
                break;
            case HrViewKeys.SalaryItems:
                _selectedSalaryItem = null;
                break;
            case HrViewKeys.OvertimeSettings:
                _selectedOvertimeSetting = null;
                break;
            case HrViewKeys.SalaryCalculationRules:
                _selectedSalaryCalculationRule = null;
                break;
        }
    }

    private bool TryOpenCurrentSelection()
    {
        switch (_activeForm)
        {
            case HrViewKeys.Sectors:
                return TryOpenSelection(_selectedSector, _sectors, SelectSector);
            case HrViewKeys.Departments:
                return TryOpenSelection(_selectedDepartment, _departments, SelectDepartment);
            case HrViewKeys.Sections:
                return TryOpenSelection(_selectedSection, _sections, SelectSection);
            case HrViewKeys.Jobs:
                return TryOpenSelection(_selectedJob, _jobs, SelectJob);
            case HrViewKeys.Qualifications:
                return TryOpenSelection(_selectedQualification, _qualifications, SelectQualification);
            case HrViewKeys.Employees:
                return TryOpenSelection(_selectedEmployee, _employees, SelectEmployee);
            case HrViewKeys.AttendanceSystems:
                return TryOpenSelection(_selectedAttendanceSystem, _attendanceSystems, SelectAttendanceSystem);
            case HrViewKeys.Shifts:
                return TryOpenSelection(_selectedShift, _shifts, SelectShift);
            case HrViewKeys.SalaryItems:
                return TryOpenSelection(_selectedSalaryItem, _salaryItems, SelectSalaryItem);
            case HrViewKeys.OvertimeSettings:
                return TryOpenSelection(_selectedOvertimeSetting, _overtimeSettings, SelectOvertimeSetting);
            case HrViewKeys.SalaryCalculationRules:
                return TryOpenSelection(_selectedSalaryCalculationRule, _salaryCalculationRules, SelectSalaryCalculationRule);
            default:
                return false;
        }
    }

    private static bool TryOpenSelection<T>(T? selected, List<T> items, Action<T?> select)
        where T : class
    {
        if (selected is not null)
        {
            select(selected);
            return true;
        }

        if (items.Count == 0)
        {
            return false;
        }

        select(items[0]);
        return true;
    }

    private void SaveBasicItem<T>(List<T> items, T draft, Action<T> setDraft, Action<T?> setSelected)
        where T : BaseDto, new()
    {
        if (string.IsNullOrWhiteSpace(draft.Id) || string.IsNullOrWhiteSpace(draft.Name))
        {
            Snackbar.Add("ID and Name are required before saving.", Severity.Warning);
            return;
        }

        var existing = items.FirstOrDefault(item => item.Id.Equals(draft.Id, StringComparison.OrdinalIgnoreCase));
        T target;

        if (existing is null)
        {
            target = CloneBaseDto(draft);
            items.Add(target);
            Snackbar.Add($"{GetActiveFormLabel()} entry added.", Severity.Success);
        }
        else
        {
            existing.Name = draft.Name;
            target = existing;
            Snackbar.Add($"{GetActiveFormLabel()} entry updated.", Severity.Success);
        }

        setSelected(target);
        setDraft(CloneBaseDto(target));
        _isDetailPanelOpen = true;
    }

    private void DeleteBasicItem<T>(List<T> items, T draft, Action<T?> setSelected, Action<T> setDraft)
        where T : BaseDto, new()
    {
        if (string.IsNullOrWhiteSpace(draft.Id))
        {
            Snackbar.Add("Select or enter an ID before deleting.", Severity.Warning);
            return;
        }

        var removedCount = items.RemoveAll(item => item.Id.Equals(draft.Id, StringComparison.OrdinalIgnoreCase));
        Snackbar.Add(
            removedCount > 0 ? $"{GetActiveFormLabel()} entry deleted." : "No matching entry was found.",
            removedCount > 0 ? Severity.Success : Severity.Info);

        if (removedCount > 0)
        {
            setSelected(null);
            setDraft(new T());
            _isDetailPanelOpen = false;
        }
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
        EmployeeRecord target;

        if (existing is null)
        {
            target = CloneEmployee(_employeeDraft);
            _employees.Add(target);
            Snackbar.Add("Employee added to the directory.", Severity.Success);
        }
        else
        {
            existing.Name = _employeeDraft.Name;
            existing.Title = _employeeDraft.Title;
            existing.Email = _employeeDraft.Email;
            existing.Department = _employeeDraft.Department;
            target = existing;
            Snackbar.Add("Employee record updated.", Severity.Success);
        }

        _selectedEmployee = target;
        _employeeDraft = CloneEmployee(target);
        _isDetailPanelOpen = true;
    }

    private void DeleteEmployee()
    {
        if (string.IsNullOrWhiteSpace(_employeeDraft.Id))
        {
            Snackbar.Add("Select an employee before deleting.", Severity.Warning);
            return;
        }

        var removedCount = _employees.RemoveAll(item => item.Id.Equals(_employeeDraft.Id, StringComparison.OrdinalIgnoreCase));
        Snackbar.Add(
            removedCount > 0 ? "Employee removed from the directory." : "No employee matched that ID.",
            removedCount > 0 ? Severity.Success : Severity.Info);

        if (removedCount > 0)
        {
            _selectedEmployee = null;
            _employeeDraft = new EmployeeRecord();
            _isDetailPanelOpen = false;
        }
    }

    private void SaveAttendanceSystem()
    {
        if (string.IsNullOrWhiteSpace(_attendanceSystemDraft.Id) || string.IsNullOrWhiteSpace(_attendanceSystemDraft.Name))
        {
            Snackbar.Add("Attendance system ID and name are required before saving.", Severity.Warning);
            return;
        }

        var existing = _attendanceSystems.FirstOrDefault(item => item.Id.Equals(_attendanceSystemDraft.Id, StringComparison.OrdinalIgnoreCase));
        AttendanceSystem target;

        if (existing is null)
        {
            target = CloneAttendanceSystem(_attendanceSystemDraft);
            _attendanceSystems.Add(target);
            Snackbar.Add("Attendance system added.", Severity.Success);
        }
        else
        {
            existing.Name = _attendanceSystemDraft.Name;
            existing.TrackingType = _attendanceSystemDraft.TrackingType;
            target = existing;
            Snackbar.Add("Attendance system updated.", Severity.Success);
        }

        _selectedAttendanceSystem = target;
        _attendanceSystemDraft = CloneAttendanceSystem(target);
        _isDetailPanelOpen = true;
    }

    private void DeleteAttendanceSystem()
    {
        if (string.IsNullOrWhiteSpace(_attendanceSystemDraft.Id))
        {
            Snackbar.Add("Select an attendance system before deleting.", Severity.Warning);
            return;
        }

        var removedCount = _attendanceSystems.RemoveAll(item => item.Id.Equals(_attendanceSystemDraft.Id, StringComparison.OrdinalIgnoreCase));
        Snackbar.Add(
            removedCount > 0 ? "Attendance system deleted." : "No attendance system matched that ID.",
            removedCount > 0 ? Severity.Success : Severity.Info);

        if (removedCount > 0)
        {
            _selectedAttendanceSystem = null;
            _attendanceSystemDraft = new AttendanceSystem();
            _isDetailPanelOpen = false;
        }
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
        Shift target;

        if (existing is null)
        {
            target = CloneShift(_shiftDraft);
            _shifts.Add(target);
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
            target = existing;
            Snackbar.Add("Shift updated.", Severity.Success);
        }

        _selectedShift = target;
        _shiftDraft = CloneShift(target);
        _isDetailPanelOpen = true;
    }

    private void DeleteShift()
    {
        if (string.IsNullOrWhiteSpace(_shiftDraft.Id))
        {
            Snackbar.Add("Select a shift before deleting.", Severity.Warning);
            return;
        }

        var removedCount = _shifts.RemoveAll(item => item.Id.Equals(_shiftDraft.Id, StringComparison.OrdinalIgnoreCase));
        Snackbar.Add(
            removedCount > 0 ? "Shift deleted." : "No shift matched that ID.",
            removedCount > 0 ? Severity.Success : Severity.Info);

        if (removedCount > 0)
        {
            _selectedShift = null;
            _shiftDraft = new Shift();
            _isDetailPanelOpen = false;
        }
    }

    private void SelectSector(Sector? item)
    {
        SetBasicSelection(item, value => _selectedSector = value, value => _sectorDraft = value);
    }

    private void SelectDepartment(Department? item)
    {
        SetBasicSelection(item, value => _selectedDepartment = value, value => _departmentDraft = value);
    }

    private void SelectSection(Section? item)
    {
        SetBasicSelection(item, value => _selectedSection = value, value => _sectionDraft = value);
    }

    private void SelectJob(Job? item)
    {
        SetBasicSelection(item, value => _selectedJob = value, value => _jobDraft = value);
    }

    private void SelectQualification(Qualification? item)
    {
        SetBasicSelection(item, value => _selectedQualification = value, value => _qualificationDraft = value);
    }

    private void SelectSalaryItem(BaseDto? item)
    {
        SetBasicSelection(item, value => _selectedSalaryItem = value, value => _salaryItemDraft = value);
    }

    private void SelectOvertimeSetting(BaseDto? item)
    {
        SetBasicSelection(item, value => _selectedOvertimeSetting = value, value => _overtimeSettingDraft = value);
    }

    private void SelectSalaryCalculationRule(BaseDto? item)
    {
        SetBasicSelection(item, value => _selectedSalaryCalculationRule = value, value => _salaryCalculationRuleDraft = value);
    }

    private void SelectEmployee(EmployeeRecord? item)
    {
        _selectedEmployee = item;
        _employeeDraft = item is not null ? CloneEmployee(item) : new EmployeeRecord();
        _isDetailPanelOpen = item is not null;
    }

    private void SelectAttendanceSystem(AttendanceSystem? item)
    {
        _selectedAttendanceSystem = item;
        _attendanceSystemDraft = item is not null ? CloneAttendanceSystem(item) : new AttendanceSystem();
        _isDetailPanelOpen = item is not null;
    }

    private void SelectShift(Shift? item)
    {
        _selectedShift = item;
        _shiftDraft = item is not null ? CloneShift(item) : new Shift();
        _isDetailPanelOpen = item is not null;
    }

    private void SetBasicSelection<T>(T? item, Action<T?> setSelected, Action<T> setDraft)
        where T : BaseDto, new()
    {
        setSelected(item);
        setDraft(item is not null ? CloneBaseDto(item) : new T());
        _isDetailPanelOpen = item is not null;
    }

    private HrNavigationSection? GetActiveSection()
    {
        return _navigationSections.FirstOrDefault(section => section.Items.Any(item => item.Key == _activeForm));
    }

    private string GetActiveFormLabel()
    {
        return _navigationSections
            .SelectMany(section => section.Items)
            .FirstOrDefault(item => item.Key == _activeForm)?
            .Label ?? _activeForm;
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
