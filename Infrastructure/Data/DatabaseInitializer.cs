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
                    FirstName NVARCHAR(50) COLLATE Cyrillic_General_CI_AS NOT NULL,
                    LastName NVARCHAR(50) COLLATE Cyrillic_General_CI_AS NOT NULL,
                    Email NVARCHAR(100) UNIQUE NOT NULL,
                    DateOfBirth DATETIME2 NOT NULL,
                    Salary DECIMAL(18,2) NOT NULL,
                    DepartmentId INT NULL,
                    Position NVARCHAR(100) COLLATE Cyrillic_General_CI_AS,
                    HireDate DATETIME2 DEFAULT GETUTCDATE(),
                    TerminationDate DATETIME2 NULL,
                    IsActive BIT DEFAULT 1,
                    CreatedDate DATETIME2 DEFAULT GETUTCDATE(),
                    ModifiedDate DATETIME2 DEFAULT GETUTCDATE()
                )";

            connection.Execute(createEmployeesTable);
            Console.WriteLine("Таблицы созданы/проверены успешно.");
        }
    }
}
