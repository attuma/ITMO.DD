using System.Collections.ObjectModel;
using itmodd.Models;
using itmodd.Services;

namespace itmodd.ViewModels;

public class AddTaskViewModel : BaseViewModel
{
    private readonly IApiClient _api;

    public AddTaskViewModel(IApiClient api)
    {
        _api = api;
        SaveCommand = new Command(async () => await SaveAsync(), () => !IsBusy);
        ToggleNewSubjectCommand = new Command(() => ShowNewSubject = !ShowNewSubject);
        CreateSubjectCommand = new Command(async () => await CreateSubjectAsync(), () => !IsBusy);
    }

    // задача сохранена — страница закроется и обновит календарь
    public event Action? Saved;

    // ===== поля задачи =====
    private string _title = string.Empty;
    public string Title { get => _title; set { _title = value; OnPropertyChanged(); } }

    private string _description = string.Empty;
    public string Description { get => _description; set { _description = value; OnPropertyChanged(); } }

    public ObservableCollection<SubjectApiResponse> Subjects { get; } = new();

    private SubjectApiResponse? _selectedSubject;
    public SubjectApiResponse? SelectedSubject { get => _selectedSubject; set { _selectedSubject = value; OnPropertyChanged(); } }

    private DateTime _deadlineDate = DateTime.Today;
    public DateTime DeadlineDate { get => _deadlineDate; set { _deadlineDate = value; OnPropertyChanged(); } }

    private TimeSpan _deadlineTime = new(23, 0, 0);
    public TimeSpan DeadlineTime { get => _deadlineTime; set { _deadlineTime = value; OnPropertyChanged(); } }

    // ===== создание предмета =====
    private bool _showNewSubject;
    public bool ShowNewSubject { get => _showNewSubject; set { _showNewSubject = value; OnPropertyChanged(); } }

    private string _newSubjectName = string.Empty;
    public string NewSubjectName { get => _newSubjectName; set { _newSubjectName = value; OnPropertyChanged(); } }

    // палитра для нового предмета
    public ObservableCollection<string> AvailableColors { get; } = new()
    {
        "#FF9F0A", "#0A84FF", "#30D158", "#FF453A", "#BF5AF2", "#64D2FF", "#FF375F", "#FFD60A"
    };

    private string _selectedColor = "#0A84FF";
    public string SelectedColor { get => _selectedColor; set { _selectedColor = value; OnPropertyChanged(); } }

    // ===== общее =====
    private string _errorMessage = string.Empty;
    public string ErrorMessage
    {
        get => _errorMessage;
        set { _errorMessage = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasError)); }
    }
    public bool HasError => !string.IsNullOrEmpty(_errorMessage);

    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            _isBusy = value;
            OnPropertyChanged();
            ((Command)SaveCommand).ChangeCanExecute();
            ((Command)CreateSubjectCommand).ChangeCanExecute();
        }
    }

    public Command SaveCommand { get; }
    public Command ToggleNewSubjectCommand { get; }
    public Command CreateSubjectCommand { get; }

    // загрузка предметов при открытии формы
    public async Task InitAsync()
    {
        var subjects = await _api.GetSubjectsAsync();
        Subjects.Clear();
        foreach (var s in subjects.Where(s => !s.IsArchived))
            Subjects.Add(s);

        SelectedSubject ??= Subjects.FirstOrDefault();
    }

    private async Task CreateSubjectAsync()
    {
        if (string.IsNullOrWhiteSpace(NewSubjectName))
        {
            ErrorMessage = "Введи название предмета";
            return;
        }

        IsBusy = true;
        try
        {
            var created = await _api.CreateSubjectAsync(NewSubjectName.Trim(), SelectedColor);
            if (created is null)
            {
                ErrorMessage = "Не удалось создать предмет";
                return;
            }

            Subjects.Add(created);
            SelectedSubject = created;
            NewSubjectName = string.Empty;
            ShowNewSubject = false;
            ErrorMessage = string.Empty;
        }
        finally { IsBusy = false; }
    }

    private async Task SaveAsync()
    {
        if (IsBusy) return;
        ErrorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(Title))
        {
            ErrorMessage = "Введи название задачи";
            return;
        }
        if (SelectedSubject is null)
        {
            ErrorMessage = "Выбери предмет";
            return;
        }

        // дата + время -> единый дедлайн
        var deadline = DeadlineDate.Date + DeadlineTime;

        IsBusy = true;
        try
        {
            var error = await _api.CreateTaskAsync(
                Title.Trim(),
                string.IsNullOrWhiteSpace(Description) ? null : Description.Trim(),
                SelectedSubject.SubjectId,
                deadline);

            if (error is null)
                Saved?.Invoke();
            else
                ErrorMessage = error;
        }
        finally { IsBusy = false; }
    }
}
