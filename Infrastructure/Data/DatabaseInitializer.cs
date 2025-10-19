using Dapper;
using EmployeeManagementApp.Application.Interfaces;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Data
{
    public class DatabaseInitializer : IDatabaseInitializer
    {
        private readonly string _masterConnectionString;
        private readonly string _databaseName;

        public DatabaseInitializer(string masterConnectionString, string databaseName = "EmployeeManagementDB")
        {
            _masterConnectionString = masterConnectionString;
            _databaseName = databaseName;
        }

        public bool Initialize()
        {
            try
            {
                CreateDatabaseIfNotExists();
                CreateTablesIfNotExist();
                SeedInitialData();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка инициализации базы данных: {ex.Message}");
                return false;
            }
        }

        public string GetConnectionString()
        {
            // Создаем connection string на основе MasterConnection
            var builder = new SqlConnectionStringBuilder(_masterConnectionString)
            {
                InitialCatalog = _databaseName
            };
            return builder.ConnectionString;
        }

        private void CreateDatabaseIfNotExists()
        {
            using var connection = new SqlConnection(_masterConnectionString);

            var databaseExists = connection.ExecuteScalar<bool>(
                "SELECT COUNT(*) FROM sys.databases WHERE name = @DatabaseName",
                new { DatabaseName = _databaseName });

            if (!databaseExists)
            {
                connection.Execute($"CREATE DATABASE [{_databaseName}]");
                Console.WriteLine($"База данных '{_databaseName}' создана успешно.");
            }
            else
            {
                Console.WriteLine($"База данных '{_databaseName}' уже существует.");
            }
        }

        private void CreateTablesIfNotExist()
        {
            using var connection = new SqlConnection(GetConnectionString());

            var createEmployeesTable = @"
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Employees' AND xtype='U')
                CREATE TABLE Employees (
                    EmployeeID INT PRIMARY KEY IDENTITY(1,1),
                    FirstName NVARCHAR(50) NOT NULL,
                    LastName NVARCHAR(50) NOT NULL,
                    Email NVARCHAR(100) UNIQUE NOT NULL,
                    DateOfBirth DATETIME2 NOT NULL,
                    Salary DECIMAL(18,2) NOT NULL,
                    DepartmentId INT NULL,
                    Position NVARCHAR(100),
                    HireDate DATETIME2 DEFAULT GETUTCDATE(),
                    TerminationDate DATETIME2 NULL,
                    IsActive BIT DEFAULT 1,
                    CreatedDate DATETIME2 DEFAULT GETUTCDATE(),
                    ModifiedDate DATETIME2 DEFAULT GETUTCDATE()
                )";

            connection.Execute(createEmployeesTable);
            Console.WriteLine("Таблицы созданы/проверены успешно.");
        }

        private void SeedInitialData()
        {
            using var connection = new SqlConnection(GetConnectionString());

            var employeesCount = connection.ExecuteScalar<int>("SELECT COUNT(*) FROM Employees");

            if (employeesCount == 0)
            {
                var employees = new[]
                {
                    new {
                        FirstName = "Иван",
                        LastName = "Иванов",
                        Email = "ivan.ivanov@company.com",
                        DateOfBirth = new DateTime(1985, 5, 15),
                        Salary = 75000.00m,
                        Position = "Разработчик",
                        HireDate = DateTime.UtcNow.AddYears(-2)
                    },
                    new {
                        FirstName = "Петр",
                        LastName = "Петров",
                        Email = "petr.petrov@company.com",
                        DateOfBirth = new DateTime(1990, 8, 22),
                        Salary = 65000.00m,
                        Position = "Менеджер",
                        HireDate = DateTime.UtcNow.AddYears(-1)
                    }
                };

                connection.Execute(@"
                    INSERT INTO Employees (FirstName, LastName, Email, DateOfBirth, Salary, Position, HireDate)
                    VALUES (@FirstName, @LastName, @Email, @DateOfBirth, @Salary, @Position, @HireDate)",
                    employees);

                Console.WriteLine("Тестовые данные добавлены успешно.");
            }
        }
    }
}
