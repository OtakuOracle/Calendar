using Avalonia.Controls;
using Avalonia.Interactivity;
using Calendar.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Avalonia;

namespace Calendar
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private readonly User7Context db = new();

        private IEnumerable<Department> _departments = new List<Department>();
        private IEnumerable<Employee> _employees = new List<Employee>();

        public IEnumerable<Department> Departments
        {
            get => _departments;
            set
            {
                _departments = value;
                OnPropertyChanged();
            }
        }

        public IEnumerable<Employee> Employees
        {
            get => _employees;
            set
            {
                _employees = value;
                OnPropertyChanged();
            }
        }

        private async void EmployeeListBox_DoubleTapped(object? sender, RoutedEventArgs e) // обработка двойного нажатия 
        {
            if (EmployeeListBox.SelectedItem is Employee selectedEmployee)
            {

                var employeeFromDb = db.Employees
                    .Include(e => e.Position)
                    .Include(e => e.Subdivision)
                    .First(e => e.Employeeid == selectedEmployee.Employeeid);


                var dialog = new EmployeeWindow(employeeFromDb, isEditMode: false);


                var result = await dialog.ShowDialog<bool>(this);


                if (result)
                {
                    LoadAllEmployees();
                }
            }
        }


        public event PropertyChangedEventHandler? PropertyChanged; // объявление события
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) // метод вызова события
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            Loaded += MainWindow_Loaded;
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void MainWindow_Loaded(object? sender, System.EventArgs e) // главное окно
        {
            LoadDepartments();
            LoadAllEmployees();
        }

        private void LoadDepartments() // список отделов
        {
            Departments = db.Departments
                .Include(d => d.DepartmentSubdivisions)
                .OrderBy(d => d.Departmentname)
                .ToList();
        }

        private void LoadAllEmployees() // список сотрудников
        {
            Employees = db.Employees
                .Include(e => e.Position)
                .Include(e => e.Subdivision)
                .OrderBy(e => e.Lastname)
                .ToList();
        }

        private void ShowSubdivisionEmployees(DepartmentSubdivision subdivision) // список сотрудников, относящихся к опр подразделению
        {
            Employees = db.Employees
                .Include(e => e.Position)
                .Include(e => e.Subdivision)
                .Where(e => e.Subdivisionid == subdivision.Subdivisionid)
                .OrderBy(e => e.Lastname)
                .ToList();
        }

        private void ShowDepartmentEmployees(Department department) // список  сотрудников, относящихся ко всем подразделениям, принадлежащим опр департаменту
        {
            var subIds = db.DepartmentSubdivisions
                .Where(s => s.Departmentid == department.Departmentid)
                .Select(s => s.Subdivisionid)
                .ToList();

            Employees = db.Employees
                .Include(e => e.Position)
                .Include(e => e.Subdivision)
                .Where(e => e.Subdivisionid != null && subIds.Contains(e.Subdivisionid.Value))
                .OrderBy(e => e.Lastname)
                .ToList();
        }

        private void DepartmentTree_SelectionChanged(object? sender, SelectionChangedEventArgs e) // обработка изменеений
        {
            if (e.AddedItems.Count == 0)
                return;

            var selectedItem = e.AddedItems[0];

            if (selectedItem is DepartmentSubdivision subdivision)
                ShowSubdivisionEmployees(subdivision);
            else if (selectedItem is Department department)
                ShowDepartmentEmployees(department);
        }

        private void SearchBox_TextChanged(object? sender, TextChangedEventArgs e) // обработка изменения в тексте 
        {
            var text = SearchBox.Text?.ToLower() ?? "";

            if (string.IsNullOrWhiteSpace(text))
            {
                LoadAllEmployees();
                return;
            }

            Employees = db.Employees
                .Include(e => e.Position)
                .Include(e => e.Subdivision)
                .Where(e =>
                    e.Firstname.ToLower().Contains(text) ||
                    e.Lastname.ToLower().Contains(text) ||
                    e.Position.Positionname.ToLower().Contains(text) ||
                    e.Subdivision.Subdivisionname.ToLower().Contains(text))
                .OrderBy(e => e.Lastname)
                .ToList();
        }
        private async void AddEmployee_Click(object? sender, RoutedEventArgs e) // добавить нового сотрудника
        {
            var dialog = new EmployeeWindow();
            if (await dialog.ShowDialog<bool>(this))
            {
                db.Employees.Add(dialog.Employee);
                db.SaveChanges();
                LoadAllEmployees();
            }
        }
    }
}
