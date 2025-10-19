namespace EmployeeManagementApp.Application.Services
{
    using System;
    using System.Collections.Generic;
    using EmployeeManagementApp.Application.Interfaces;
    using EmployeeManagementApp.Domain;


    public class EmployeeService
    {
        private readonly IEmployeeRepository _repository;

        public EmployeeService(IEmployeeRepository repository)
        {
            _repository = repository;
        }

        public int AddEmployee(Employee employee)
        {
            ValidateEmployee(employee);
            return _repository.Add(employee);
        }

        public IEnumerable<Employee> GetAllEmployees()
        {
            return _repository.GetAll();
        }

        public Employee GetEmployeeById(int id)
        {
            if (id <= 0)
                throw new ArgumentException("ID должен быть положительным числом");

            return _repository.GetById(id);
        }

        public void UpdateEmployeeField(int employeeId, string fieldName, string fieldValue)
        {
            if (employeeId <= 0)
                throw new ArgumentException("ID должен быть положительным числом");

            if (string.IsNullOrWhiteSpace(fieldName))
                throw new ArgumentException("Имя поля не может быть пустым");

            if (string.IsNullOrWhiteSpace(fieldValue))
                throw new ArgumentException("Значение не может быть пустым");

            ValidateFieldValue(fieldName, fieldValue);
            _repository.UpdateField(employeeId, fieldName, fieldValue);
        }

        public int DeleteEmployee(int employeeId)
        {
            if (employeeId <= 0)
                throw new ArgumentException("ID должен быть положительным числом");

            return _repository.Delete(employeeId);
        }

        private void ValidateEmployee(Employee employee)
        {
            if (employee == null)
                throw new ArgumentNullException(nameof(employee));

            if (string.IsNullOrWhiteSpace(employee.FirstName))
                throw new ArgumentException("Имя не может быть пустым");

            if (string.IsNullOrWhiteSpace(employee.LastName))
                throw new ArgumentException("Фамилия не может быть пустой");

            if (string.IsNullOrWhiteSpace(employee.Email))
                throw new ArgumentException("Email не может быть пустым");

            if (employee.DateOfBirth > DateTime.Now)
                throw new ArgumentException("Дата рождения не может быть в будущем");

            if (employee.Salary < 0)
                throw new ArgumentException("Зарплата не может быть отрицательной");
        }

        private void ValidateFieldValue(string fieldName, string fieldValue)
        {
            if (fieldName == "DateOfBirth")
            {
                if (!DateTime.TryParse(fieldValue, out DateTime date))
                    throw new ArgumentException("Неверный формат даты");

                if (date > DateTime.Now)
                    throw new ArgumentException("Дата рождения не может быть в будущем");
            }

            if (fieldName == "Salary")
            {
                if (!decimal.TryParse(fieldValue, out decimal salary) || salary < 0)
                    throw new ArgumentException("Неверное значение зарплаты");
            }
        }
    }


}
