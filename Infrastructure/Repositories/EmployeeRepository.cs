using Dapper;
using EmployeeManagementApp.Application.Interfaces;
using EmployeeManagementApp.Domain;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly IConfiguration _configuration;

        public EmployeeRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // Конструктор для обратной совместимости
        public EmployeeRepository(string connectionString)
        {
            var inMemorySettings = new Dictionary<string, string> {
                {"ConnectionStrings:DefaultConnection", connectionString}
            };

            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();
        }

        private string GetConnectionString()
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("DefaultConnection string is not configured");
            }

            return connectionString;
        }

        public int Add(Employee employee)
        {
            using var connection = new SqlConnection(GetConnectionString());

            var sql = @"
                INSERT INTO Employees (
                    FirstName, LastName, Email, DateOfBirth, Salary, 
                    DepartmentId, Position, HireDate, CreatedDate, ModifiedDate
                )
                OUTPUT INSERTED.EmployeeID
                VALUES (
                    @FirstName, @LastName, @Email, @DateOfBirth, @Salary,
                    @DepartmentId, @Position, @HireDate, @CreatedDate, @ModifiedDate
                )";

            var parameters = new
            {
                employee.FirstName,
                employee.LastName,
                employee.Email,
                employee.DateOfBirth,
                employee.Salary,
                //DepartmentId = employee.DepartmentId ?? (object)DBNull.Value,
                //Position = employee.Position ?? (object)DBNull.Value,
                //HireDate = employee.HireDate,
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow
            };

            var employeeId = connection.ExecuteScalar<int>(sql, parameters);
            return employeeId;
        }

        public IEnumerable<Employee> GetAll()
        {
            using var connection = new SqlConnection(GetConnectionString());

            var sql = @"
                SELECT * FROM Employees 
                WHERE IsActive = 1 
                ORDER BY LastName, FirstName";

            return connection.Query<Employee>(sql);
        }

        public Employee GetById(int id)
        {
            using var connection = new SqlConnection(GetConnectionString());

            var sql = @"
                SELECT * FROM Employees 
                WHERE EmployeeID = @EmployeeID AND IsActive = 1";

            return connection.QueryFirstOrDefault<Employee>(sql, new { EmployeeID = id });
        }

        public void UpdateField(int employeeId, string fieldName, string fieldValue)
        {
            using var connection = new SqlConnection(GetConnectionString());

            // Валидация имени поля для защиты от SQL-инъекций
            var allowedFields = new[] { "FirstName", "LastName", "Email", "Position", "Salary", "DepartmentId" };
            if (!allowedFields.Contains(fieldName))
            {
                throw new ArgumentException($"Invalid field name: {fieldName}");
            }

            object convertedValue = fieldValue;

            // Конвертация значений в правильный тип данных
            if (fieldName == "Salary")
            {
                if (decimal.TryParse(fieldValue, out decimal salaryValue))
                    convertedValue = salaryValue;
                else
                    throw new ArgumentException("Invalid salary format");
            }
            else if (fieldName == "DepartmentId")
            {
                if (int.TryParse(fieldValue, out int departmentIdValue))
                    convertedValue = departmentIdValue;
                else if (string.IsNullOrEmpty(fieldValue))
                    convertedValue = DBNull.Value;
                else
                    throw new ArgumentException("Invalid department ID format");
            }
            else if (fieldName == "DateOfBirth")
            {
                if (DateTime.TryParse(fieldValue, out DateTime dateValue))
                    convertedValue = dateValue;
                else
                    throw new ArgumentException("Invalid date format");
            }

            var sql = $@"
                UPDATE Employees 
                SET {fieldName} = @FieldValue, 
                    ModifiedDate = @ModifiedDate
                WHERE EmployeeID = @EmployeeID AND IsActive = 1";

            var parameters = new
            {
                EmployeeID = employeeId,
                FieldValue = convertedValue,
                ModifiedDate = DateTime.UtcNow
            };

            var affectedRows = connection.Execute(sql, parameters);

            if (affectedRows == 0)
            {
                throw new InvalidOperationException($"Employee with ID {employeeId} not found or not active");
            }
        }

        public int Delete(int employeeId)
        {
            using var connection = new SqlConnection(GetConnectionString());

            var sql = @"
                UPDATE Employees 
                SET IsActive = 0, 
                    TerminationDate = @TerminationDate,
                    ModifiedDate = @ModifiedDate
                WHERE EmployeeID = @EmployeeID AND IsActive = 1";

            var parameters = new
            {
                EmployeeID = employeeId,
                TerminationDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow
            };

            var affectedRows = connection.Execute(sql, parameters);
            return affectedRows;
        }
    }
}
