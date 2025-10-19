
namespace EmployeeManagementApp.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using Dapper;
    using EmployeeManagemenApp.Application.Interfaces;

    public class DatabaseInitializer : IDatabaseInitializer
    {
        private readonly string _masterConnectionString;
        private string _employeeDbConnectionString;

        public DatabaseInitializer(string masterConnectionString)
        {
            _masterConnectionString = masterConnectionString;
        }

        public bool Initialize()
        {
            try
            {
                CreateDatabaseIfNotExists();
                _employeeDbConnectionString = _masterConnectionString.Replace("Database=master", "Database=EmployeeDB");
                CreateTableAndProcedures();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Ошибка инициализации базы данных: {ex.Message}");
                return false;
            }
        }

        public string GetConnectionString() => _employeeDbConnectionString;

        private void CreateDatabaseIfNotExists()
        {
            using (IDbConnection conn = new SqlConnection(_masterConnectionString))
            {
                conn.Open();
                string checkDbSql = @"
                    IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'EmployeeDB')
                    BEGIN
                        CREATE DATABASE EmployeeDB;
                    END";
                conn.Execute(checkDbSql);
            }
        }

        private void CreateTableAndProcedures()
        {
            using (IDbConnection conn = new SqlConnection(_employeeDbConnectionString))
            {
                conn.Open();

                // Создаем таблицу
                conn.Execute(@"
                    IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Employees')
                    BEGIN
                        CREATE TABLE Employees (
                            EmployeeID INT PRIMARY KEY IDENTITY(1,1),
                            FirstName NVARCHAR(50) NOT NULL,
                            LastName NVARCHAR(50) NOT NULL,
                            Email NVARCHAR(100) NOT NULL,
                            DateOfBirth DATE NOT NULL,
                            Salary DECIMAL(18,2) NOT NULL
                        );
                    END");

                // Создаем хранимые процедуры
                CreateStoredProcedures(conn);
            }
        }

        private void CreateStoredProcedures(IDbConnection conn)
        {
            var procedures = new Dictionary<string, string>
            {
                ["sp_AddEmployee"] = @"
                    CREATE PROCEDURE sp_AddEmployee
                        @FirstName NVARCHAR(50), @LastName NVARCHAR(50), @Email NVARCHAR(100),
                        @DateOfBirth DATE, @Salary DECIMAL(18,2)
                    AS BEGIN
                        INSERT INTO Employees (FirstName, LastName, Email, DateOfBirth, Salary)
                        VALUES (@FirstName, @LastName, @Email, @DateOfBirth, @Salary);
                        SELECT CAST(SCOPE_IDENTITY() AS INT) AS NewEmployeeID;
                    END",

                ["sp_GetAllEmployees"] = @"
                    CREATE PROCEDURE sp_GetAllEmployees
                    AS BEGIN
                        SELECT EmployeeID, FirstName, LastName, Email, DateOfBirth, Salary
                        FROM Employees ORDER BY EmployeeID;
                    END",

                ["sp_GetEmployeeById"] = @"
                    CREATE PROCEDURE sp_GetEmployeeById @EmployeeID INT
                    AS BEGIN
                        SELECT EmployeeID, FirstName, LastName, Email, DateOfBirth, Salary
                        FROM Employees WHERE EmployeeID = @EmployeeID;
                    END",

                ["sp_UpdateEmployeeField"] = @"
                    CREATE PROCEDURE sp_UpdateEmployeeField
                        @EmployeeID INT, @FieldName NVARCHAR(50), @FieldValue NVARCHAR(100)
                    AS BEGIN
                        IF @FieldName = 'FirstName' UPDATE Employees SET FirstName = @FieldValue WHERE EmployeeID = @EmployeeID;
                        ELSE IF @FieldName = 'LastName' UPDATE Employees SET LastName = @FieldValue WHERE EmployeeID = @EmployeeID;
                        ELSE IF @FieldName = 'Email' UPDATE Employees SET Email = @FieldValue WHERE EmployeeID = @EmployeeID;
                        ELSE IF @FieldName = 'DateOfBirth' UPDATE Employees SET DateOfBirth = CAST(@FieldValue AS DATE) WHERE EmployeeID = @EmployeeID;
                        ELSE IF @FieldName = 'Salary' UPDATE Employees SET Salary = CAST(@FieldValue AS DECIMAL(18,2)) WHERE EmployeeID = @EmployeeID;
                    END",

                ["sp_DeleteEmployee"] = @"
                    CREATE PROCEDURE sp_DeleteEmployee @EmployeeID INT
                    AS BEGIN
                        DELETE FROM Employees WHERE EmployeeID = @EmployeeID;
                        SELECT @@ROWCOUNT AS RowsAffected;
                    END"
            };

            foreach (var proc in procedures)
            {
                conn.Execute($"IF EXISTS (SELECT * FROM sys.procedures WHERE name = '{proc.Key}') DROP PROCEDURE {proc.Key};");
                conn.Execute(proc.Value);
            }
        }
    }
}
   
