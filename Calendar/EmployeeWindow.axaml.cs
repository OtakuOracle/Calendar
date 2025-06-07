using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Calendar.Models;
using Microsoft.EntityFrameworkCore;

namespace Calendar;

public partial class EmployeeWindow : Window
{
    private readonly Employee _employee;
    private bool _isEditing;
    private readonly User7Context _db;
    public Employee Employee => _employee;

    private List<DepartmentSubdivision> _subdivisions;
    private List<Position> _positions;
    private List<Employee> _assistants;

    public EmployeeWindow()
    {
        InitializeComponent();
        _employee = new Employee();
        _db = new User7Context();
        LoadData();
        SetEditMode(false);
    }

    public EmployeeWindow(Employee? employee = null, bool isEditMode = false)
    {
        InitializeComponent();
        _db = new User7Context();
        _employee = employee ?? new Employee();
        LoadData();
        LoadEmployee(_employee);
        SetEditMode(isEditMode);
    }

    private void LoadData()
    {
        _subdivisions = _db.DepartmentSubdivisions.ToList();
        _positions = _db.Positions.ToList();
        _assistants = _db.Employees
            .Where(e => e.Employeeid != _employee.Employeeid) // исключаем самого себя из сотрудников
            .ToList();

        this.FindControl<ComboBox>("DepartmentsComboBox").ItemsSource = _subdivisions; // комбобокс для подразделения
        this.FindControl<ComboBox>("PositionsComboBox").ItemsSource = _positions; // комбобокс для должности
        this.FindControl<ComboBox>("AssistantsComboBox").ItemsSource = _assistants; // комбобокс для ассистента
    }

    private void SetEditMode(bool isEditing) // редактировать
    {
        _isEditing = isEditing;

        this.FindControl<TextBox>("LastNameTextBox").IsReadOnly = !isEditing;
        this.FindControl<TextBox>("FirstNameTextBox").IsReadOnly = !isEditing;
        this.FindControl<TextBox>("MiddleNameTextBox").IsReadOnly = !isEditing;
        this.FindControl<TextBox>("MobiletelTextBox").IsReadOnly = !isEditing;
        this.FindControl<TextBox>("WorktelTextBox").IsReadOnly = !isEditing;
        this.FindControl<TextBox>("EmailTextBox").IsReadOnly = !isEditing;
        this.FindControl<TextBox>("OfficeTextBox").IsReadOnly = !isEditing;
        this.FindControl<DatePicker>("BirthDatePicker").IsEnabled = isEditing;
        this.FindControl<ComboBox>("DepartmentsComboBox").IsEnabled = isEditing;
        this.FindControl<ComboBox>("PositionsComboBox").IsEnabled = isEditing;
        this.FindControl<ComboBox>("AssistantsComboBox").IsEnabled = isEditing;

        this.FindControl<Button>("EditButton").IsVisible = !isEditing;
        this.FindControl<Button>("CancelButton").IsVisible = isEditing;
    }

    private void EditButton_Click(object sender, RoutedEventArgs e)
    {
        SetEditMode(true);
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e) // сохранить
    {
        if (!Validate()) return;

        _employee.Lastname = this.FindControl<TextBox>("LastNameTextBox").Text;
        _employee.Firstname = this.FindControl<TextBox>("FirstNameTextBox").Text;
        _employee.Middlename = this.FindControl<TextBox>("MiddleNameTextBox").Text;
        _employee.Mobiletel = this.FindControl<TextBox>("MobiletelTextBox").Text;
        _employee.Worktel = this.FindControl<TextBox>("WorktelTextBox").Text;
        _employee.Email = this.FindControl<TextBox>("EmailTextBox").Text;
        _employee.Office = this.FindControl<TextBox>("OfficeTextBox").Text;
        _employee.Birthdate = this.FindControl<DatePicker>("BirthDatePicker").SelectedDate?.Date is { } dt
            ? DateOnly.FromDateTime(dt)
            : null;

        _employee.Subdivisionid = (this.FindControl<ComboBox>("DepartmentsComboBox").SelectedItem as DepartmentSubdivision)?.Subdivisionid ?? 0;
        _employee.Positionid = (this.FindControl<ComboBox>("PositionsComboBox").SelectedItem as Position)?.Positionid ?? 0;

        var assistant = this.FindControl<ComboBox>("AssistantsComboBox").SelectedItem as Employee;
        _employee.EmployeeRelationAssistants.Clear();
        if (assistant != null)
        {
            _employee.EmployeeRelationAssistants.Add(new EmployeeRelation
            {
                Assistantid = assistant.Employeeid,
                Employeeid = _employee.Employeeid
            });
        }

        if (_employee.Employeeid == 0)
        {
            _db.Employees.Add(_employee);
            _db.SaveChanges();
            _employee.Personalnumber = $"EMP{_employee.Employeeid}";
            _db.SaveChanges();
        }
        else
        {
            var existing = _db.Employees
                .Include(e => e.EmployeeRelationAssistants)
                .First(e => e.Employeeid == _employee.Employeeid);

            _db.Entry(existing).CurrentValues.SetValues(_employee);
            _db.SaveChanges();
        }

        SetEditMode(false);
        Close(true);
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e) // отмена
    {
        LoadEmployee(_employee);
        SetEditMode(false);
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e) // закрыть
    {
        Close(false);
    }

    private bool Validate() // валидация
    {
        var errors = new List<string>();
        var lastName = this.FindControl<TextBox>("LastNameTextBox").Text;
        var firstName = this.FindControl<TextBox>("FirstNameTextBox").Text;
        var workPhone = this.FindControl<TextBox>("WorktelTextBox").Text;
        var email = this.FindControl<TextBox>("EmailTextBox").Text;
        var office = this.FindControl<TextBox>("OfficeTextBox").Text;

        if (string.IsNullOrWhiteSpace(lastName))
            errors.Add("Фамилия обязательна");
        if (string.IsNullOrWhiteSpace(firstName))
            errors.Add("Имя обязательно");
        if (string.IsNullOrWhiteSpace(workPhone))
            errors.Add("Рабочий телефон обязателен");
        if (string.IsNullOrWhiteSpace(email))
            errors.Add("Email обязателен");
        if (string.IsNullOrWhiteSpace(office))
            errors.Add("Офис обязателен");

        var phoneRegex = new Regex(@"^[0-9+()\-\s#]{1,20}$");
        if (!string.IsNullOrEmpty(this.FindControl<TextBox>("MobiletelTextBox").Text) &&
            !phoneRegex.IsMatch(this.FindControl<TextBox>("MobiletelTextBox").Text))
            errors.Add("Неверный формат мобильного телефона");
        if (!phoneRegex.IsMatch(workPhone))
            errors.Add("Неверный формат рабочего телефона");

        var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        if (!emailRegex.IsMatch(email))
            errors.Add("Некорректный email");

        return true;
    }

    private void LoadEmployee(Employee employee) // загруженный сотрудник
    {
        this.FindControl<TextBox>("LastNameTextBox").Text = employee.Lastname;
        this.FindControl<TextBox>("FirstNameTextBox").Text = employee.Firstname;
        this.FindControl<TextBox>("MiddleNameTextBox").Text = employee.Middlename;
        this.FindControl<TextBox>("MobiletelTextBox").Text = employee.Mobiletel;
        this.FindControl<TextBox>("WorktelTextBox").Text = employee.Worktel;
        this.FindControl<TextBox>("OfficeTextBox").Text = employee.Office;
        this.FindControl<DatePicker>("BirthDatePicker").SelectedDate = employee.Birthdate?.ToDateTime(TimeOnly.MinValue);

        this.FindControl<ComboBox>("DepartmentsComboBox").SelectedItem =
            _subdivisions.FirstOrDefault(s => s.Subdivisionid == employee.Subdivisionid);
        this.FindControl<ComboBox>("PositionsComboBox").SelectedItem =
            _positions.FirstOrDefault(p => p.Positionid == employee.Positionid);

        var assistantRelation = _db.EmployeeRelations.FirstOrDefault(r => r.Employeeid == employee.Employeeid);
        if (assistantRelation != null)
        {
            this.FindControl<ComboBox>("AssistantsComboBox").SelectedItem =
                _assistants.FirstOrDefault(a => a.Employeeid == assistantRelation.Assistantid);
        }
        else
        {
            this.FindControl<ComboBox>("AssistantsComboBox").SelectedItem = null;
        }
    }
}
