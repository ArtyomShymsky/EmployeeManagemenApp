using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using Microsoft.Data.SqlClient;

namespace EmployeeManagemenConsoleApp
{
    // Модель Employee
    public class Employee
    {
        public int EmployeeID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public DateTime DateOfBirth { get; set; }
        public decimal Salary { get; set; }
    }

    class Program
    {
        // Строка подключения к базе данных
        // Измените на вашу строку подключения
        private static string connectionString = "Server=localhost;Database=master;Integrated Security=true;";

        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            bool exit = false;

            Console.WriteLine("╔════════════════════════════════════════════════════════╗");
            Console.WriteLine("║   Система управления информацией о сотрудниках         ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════╝");

            // Инициализация базы данных
            if (!InitializeDatabase())
            {
                Console.WriteLine("\nНажмите любую клавишу для выхода...");
                Console.ReadKey();
                return;
            }

            // Проверка подключения к БД
            if (!TestConnection())
            {
                Console.WriteLine("\nНажмите любую клавишу для выхода...");
                Console.ReadKey();
                return;
            }

            while (!exit)
            {
                try
                {
                    Console.WriteLine("\n" + new string('─', 56));
                    Console.WriteLine("ГЛАВНОЕ МЕНЮ:");
                    Console.WriteLine("1. Добавить нового сотрудника");
                    Console.WriteLine("2. Просмотреть всех сотрудников");
                    Console.WriteLine("3. Обновить информацию о сотруднике");
                    Console.WriteLine("4. Удалить сотрудника");
                    Console.WriteLine("5. Выйти из приложения");
                    Console.WriteLine(new string('─', 56));
                    Console.Write("Выберите опцию (1-5): ");

                    string choice = Console.ReadLine();

                    switch (choice)
                    {
                        case "1":
                            AddEmployee();
                            break;
                        case "2":
                            ViewAllEmployees();
                            break;
                        case "3":
                            UpdateEmployee();
                            break;
                        case "4":
                            DeleteEmployee();
                            break;
                        case "5":
                            exit = true;
                            Console.WriteLine("\n✓ Завершение работы приложения...");
                            break;
                        default:
                            Console.WriteLine("\n✗ Ошибка: Неверный выбор. Пожалуйста, выберите опцию от 1 до 5.");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\n✗ Произошла ошибка: {ex.Message}");
                    Console.WriteLine("Приложение продолжит работу...");
                }
            }
        }

        // Инициализация базы данных и создание всех объектов
        static bool InitializeDatabase()
        {
            try
            {
                using (IDbConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Проверяем существование базы данных
                    string checkDbSql = @"
                        IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'EmployeeDB')
                        BEGIN
                            CREATE DATABASE EmployeeDB;
                        END";

                    conn.Execute(checkDbSql);
                }

                // Подключаемся к созданной базе данных
                string employeeDbConnection = connectionString.Replace("Database=master", "Database=EmployeeDB");

                using (IDbConnection conn = new SqlConnection(employeeDbConnection))
                {
                    conn.Open();

                    // Создаем таблицу Employees
                    string createTableSql = @"
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
                        END";

                    conn.Execute(createTableSql);

                    // Создаем хранимую процедуру для добавления сотрудника
                    string createSpAddEmployee = @"
                        IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'sp_AddEmployee')
                            DROP PROCEDURE sp_AddEmployee;";

                    conn.Execute(createSpAddEmployee);

                    string createSpAddEmployee2 = @"
                        CREATE PROCEDURE sp_AddEmployee
                            @FirstName NVARCHAR(50),
                            @LastName NVARCHAR(50),
                            @Email NVARCHAR(100),
                            @DateOfBirth DATE,
                            @Salary DECIMAL(18,2)
                        AS
                        BEGIN
                            INSERT INTO Employees (FirstName, LastName, Email, DateOfBirth, Salary)
                            VALUES (@FirstName, @LastName, @Email, @DateOfBirth, @Salary);
                            
                            SELECT CAST(SCOPE_IDENTITY() AS INT) AS NewEmployeeID;
                        END";

                    conn.Execute(createSpAddEmployee2);

                    // Создаем хранимую процедуру для получения всех сотрудников
                    conn.Execute("IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'sp_GetAllEmployees') DROP PROCEDURE sp_GetAllEmployees;");

                    string createSpGetAllEmployees = @"
                        CREATE PROCEDURE sp_GetAllEmployees
                        AS
                        BEGIN
                            SELECT EmployeeID, FirstName, LastName, Email, DateOfBirth, Salary
                            FROM Employees
                            ORDER BY EmployeeID;
                        END";

                    conn.Execute(createSpGetAllEmployees);

                    // Создаем хранимую процедуру для получения сотрудника по ID
                    conn.Execute("IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'sp_GetEmployeeById') DROP PROCEDURE sp_GetEmployeeById;");

                    string createSpGetEmployeeById = @"
                        CREATE PROCEDURE sp_GetEmployeeById
                            @EmployeeID INT
                        AS
                        BEGIN
                            SELECT EmployeeID, FirstName, LastName, Email, DateOfBirth, Salary
                            FROM Employees
                            WHERE EmployeeID = @EmployeeID;
                        END";

                    conn.Execute(createSpGetEmployeeById);

                    // Создаем хранимую процедуру для обновления поля
                    conn.Execute("IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'sp_UpdateEmployeeField') DROP PROCEDURE sp_UpdateEmployeeField;");

                    string createSpUpdateEmployeeField = @"
                        CREATE PROCEDURE sp_UpdateEmployeeField
                            @EmployeeID INT,
                            @FieldName NVARCHAR(50),
                            @FieldValue NVARCHAR(100)
                        AS
                        BEGIN
                            IF @FieldName = 'FirstName'
                                UPDATE Employees SET FirstName = @FieldValue WHERE EmployeeID = @EmployeeID;
                            ELSE IF @FieldName = 'LastName'
                                UPDATE Employees SET LastName = @FieldValue WHERE EmployeeID = @EmployeeID;
                            ELSE IF @FieldName = 'Email'
                                UPDATE Employees SET Email = @FieldValue WHERE EmployeeID = @EmployeeID;
                            ELSE IF @FieldName = 'DateOfBirth'
                                UPDATE Employees SET DateOfBirth = CAST(@FieldValue AS DATE) WHERE EmployeeID = @EmployeeID;
                            ELSE IF @FieldName = 'Salary'
                                UPDATE Employees SET Salary = CAST(@FieldValue AS DECIMAL(18,2)) WHERE EmployeeID = @EmployeeID;
                        END";

                    conn.Execute(createSpUpdateEmployeeField);

                    // Создаем хранимую процедуру для удаления сотрудника
                    conn.Execute("IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'sp_DeleteEmployee') DROP PROCEDURE sp_DeleteEmployee;");

                    string createSpDeleteEmployee = @"
                        CREATE PROCEDURE sp_DeleteEmployee
                            @EmployeeID INT
                        AS
                        BEGIN
                            DELETE FROM Employees WHERE EmployeeID = @EmployeeID;
                            SELECT @@ROWCOUNT AS RowsAffected;
                        END";

                    conn.Execute(createSpDeleteEmployee);
                }

                // Обновляем строку подключения для дальнейшей работы
                connectionString = connectionString.Replace("Database=master", "Database=EmployeeDB");

                Console.WriteLine("\n✓ База данных успешно инициализирована.");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n✗ Ошибка инициализации базы данных: {ex.Message}");
                return false;
            }
        }

        // Проверка подключения к базе данных
        static bool TestConnection()
        {
            try
            {
                using (IDbConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    Console.WriteLine("✓ Подключение к базе данных успешно установлено.");
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n✗ Ошибка подключения к базе данных: {ex.Message}");
                Console.WriteLine("Проверьте строку подключения и доступность SQL Server.");
                return false;
            }
        }

        // Добавление нового сотрудника
        static void AddEmployee()
        {
            Console.WriteLine("\n" + new string('═', 56));
            Console.WriteLine("ДОБАВЛЕНИЕ НОВОГО СОТРУДНИКА");
            Console.WriteLine(new string('═', 56));

            try
            {
                Console.Write("Введите имя: ");
                string firstName = Console.ReadLine()?.Trim();
                if (string.IsNullOrEmpty(firstName))
                {
                    Console.WriteLine("✗ Имя не может быть пустым.");
                    return;
                }

                Console.Write("Введите фамилию: ");
                string lastName = Console.ReadLine()?.Trim();
                if (string.IsNullOrEmpty(lastName))
                {
                    Console.WriteLine("✗ Фамилия не может быть пустой.");
                    return;
                }

                Console.Write("Введите email: ");
                string email = Console.ReadLine()?.Trim();
                if (string.IsNullOrEmpty(email))
                {
                    Console.WriteLine("✗ Email не может быть пустым.");
                    return;
                }

                Console.Write("Введите дату рождения (ГГГГ-ММ-ДД): ");
                string dobInput = Console.ReadLine();
                DateTime dateOfBirth;
                if (!DateTime.TryParse(dobInput, out dateOfBirth))
                {
                    Console.WriteLine("✗ Неверный формат даты. Используйте формат ГГГГ-ММ-ДД.");
                    return;
                }

                Console.Write("Введите зарплату: ");
                string salaryInput = Console.ReadLine();
                decimal salary;
                if (!decimal.TryParse(salaryInput, out salary) || salary < 0)
                {
                    Console.WriteLine("✗ Неверное значение зарплаты. Введите положительное число.");
                    return;
                }

                using (IDbConnection conn = new SqlConnection(connectionString))
                {
                    var parameters = new
                    {
                        FirstName = firstName,
                        LastName = lastName,
                        Email = email,
                        DateOfBirth = dateOfBirth,
                        Salary = salary
                    };

                    int newId = conn.QuerySingle<int>(
                        "sp_AddEmployee",
                        parameters,
                        commandType: CommandType.StoredProcedure
                    );

                    Console.WriteLine($"\n✓ Сотрудник успешно добавлен с ID: {newId}");
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"\n✗ Ошибка базы данных: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n✗ Ошибка: {ex.Message}");
            }
        }

        // Просмотр всех сотрудников
        static void ViewAllEmployees()
        {
            Console.WriteLine("\n" + new string('═', 56));
            Console.WriteLine("СПИСОК ВСЕХ СОТРУДНИКОВ");
            Console.WriteLine(new string('═', 56));

            try
            {
                using (IDbConnection conn = new SqlConnection(connectionString))
                {
                    var employees = conn.Query<Employee>(
                        "sp_GetAllEmployees",
                        commandType: CommandType.StoredProcedure
                    ).ToList();

                    if (!employees.Any())
                    {
                        Console.WriteLine("\nНет данных о сотрудниках в базе данных.");
                        return;
                    }

                    foreach (var emp in employees)
                    {
                        Console.WriteLine($"\n[ID: {emp.EmployeeID}]");
                        Console.WriteLine($"  Имя: {emp.FirstName} {emp.LastName}");
                        Console.WriteLine($"  Email: {emp.Email}");
                        Console.WriteLine($"  Дата рождения: {emp.DateOfBirth:dd.MM.yyyy}");
                        Console.WriteLine($"  Зарплата: {emp.Salary:N2} руб.");
                    }

                    Console.WriteLine($"\n{new string('─', 56)}");
                    Console.WriteLine($"Всего сотрудников: {employees.Count}");
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"\n✗ Ошибка базы данных: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n✗ Ошибка: {ex.Message}");
            }
        }

        // Обновление информации о сотруднике
        static void UpdateEmployee()
        {
            Console.WriteLine("\n" + new string('═', 56));
            Console.WriteLine("ОБНОВЛЕНИЕ ИНФОРМАЦИИ О СОТРУДНИКЕ");
            Console.WriteLine(new string('═', 56));

            try
            {
                Console.Write("Введите ID сотрудника для обновления: ");
                string idInput = Console.ReadLine();
                int employeeId;

                if (!int.TryParse(idInput, out employeeId))
                {
                    Console.WriteLine("✗ Неверный формат ID.");
                    return;
                }

                using (IDbConnection conn = new SqlConnection(connectionString))
                {
                    // Получаем информацию о сотруднике
                    var employee = conn.QueryFirstOrDefault<Employee>(
                        "sp_GetEmployeeById",
                        new { EmployeeID = employeeId },
                        commandType: CommandType.StoredProcedure
                    );

                    if (employee == null)
                    {
                        Console.WriteLine($"✗ Сотрудник с ID {employeeId} не найден.");
                        return;
                    }

                    Console.WriteLine("\nТекущая информация о сотруднике:");
                    Console.WriteLine($"  1. Имя: {employee.FirstName}");
                    Console.WriteLine($"  2. Фамилия: {employee.LastName}");
                    Console.WriteLine($"  3. Email: {employee.Email}");
                    Console.WriteLine($"  4. Дата рождения: {employee.DateOfBirth:dd.MM.yyyy}");
                    Console.WriteLine($"  5. Зарплата: {employee.Salary:N2} руб.");

                    Console.WriteLine("\nКакое поле вы хотите обновить?");
                    Console.Write("Введите номер поля (1-5) или 0 для отмены: ");
                    string fieldChoice = Console.ReadLine();

                    if (fieldChoice == "0")
                    {
                        Console.WriteLine("Обновление отменено.");
                        return;
                    }

                    string fieldName = "";
                    string prompt = "";

                    switch (fieldChoice)
                    {
                        case "1":
                            fieldName = "FirstName";
                            prompt = "Введите новое имя: ";
                            break;
                        case "2":
                            fieldName = "LastName";
                            prompt = "Введите новую фамилию: ";
                            break;
                        case "3":
                            fieldName = "Email";
                            prompt = "Введите новый email: ";
                            break;
                        case "4":
                            fieldName = "DateOfBirth";
                            prompt = "Введите новую дату рождения (ГГГГ-ММ-ДД): ";
                            break;
                        case "5":
                            fieldName = "Salary";
                            prompt = "Введите новую зарплату: ";
                            break;
                        default:
                            Console.WriteLine("✗ Неверный выбор поля.");
                            return;
                    }

                    Console.Write(prompt);
                    string newValue = Console.ReadLine()?.Trim();

                    if (string.IsNullOrEmpty(newValue))
                    {
                        Console.WriteLine("✗ Значение не может быть пустым.");
                        return;
                    }

                    // Валидация для даты
                    if (fieldName == "DateOfBirth")
                    {
                        DateTime tempDate;
                        if (!DateTime.TryParse(newValue, out tempDate))
                        {
                            Console.WriteLine("✗ Неверный формат даты.");
                            return;
                        }
                    }

                    // Валидация для зарплаты
                    if (fieldName == "Salary")
                    {
                        decimal tempSalary;
                        if (!decimal.TryParse(newValue, out tempSalary) || tempSalary < 0)
                        {
                            Console.WriteLine("✗ Неверное значение зарплаты.");
                            return;
                        }
                    }

                    // Обновляем поле
                    conn.Execute(
                        "sp_UpdateEmployeeField",
                        new
                        {
                            EmployeeID = employeeId,
                            FieldName = fieldName,
                            FieldValue = newValue
                        },
                        commandType: CommandType.StoredProcedure
                    );

                    Console.WriteLine("\n✓ Информация успешно обновлена.");
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"\n✗ Ошибка базы данных: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n✗ Ошибка: {ex.Message}");
            }
        }

        // Удаление сотрудника
        static void DeleteEmployee()
        {
            Console.WriteLine("\n" + new string('═', 56));
            Console.WriteLine("УДАЛЕНИЕ СОТРУДНИКА");
            Console.WriteLine(new string('═', 56));

            try
            {
                Console.Write("Введите ID сотрудника для удаления: ");
                string idInput = Console.ReadLine();
                int employeeId;

                if (!int.TryParse(idInput, out employeeId))
                {
                    Console.WriteLine("✗ Неверный формат ID.");
                    return;
                }

                using (IDbConnection conn = new SqlConnection(connectionString))
                {
                    // Проверяем существование сотрудника
                    var employee = conn.QueryFirstOrDefault<Employee>(
                        "sp_GetEmployeeById",
                        new { EmployeeID = employeeId },
                        commandType: CommandType.StoredProcedure
                    );

                    if (employee == null)
                    {
                        Console.WriteLine($"✗ Сотрудник с ID {employeeId} не найден.");
                        return;
                    }

                    Console.WriteLine($"\nСотрудник: {employee.FirstName} {employee.LastName}");
                    Console.Write("\nВы уверены, что хотите удалить этого сотрудника? (да/нет): ");
                    string confirmation = Console.ReadLine()?.ToLower().Trim();

                    if (confirmation != "да" && confirmation != "yes")
                    {
                        Console.WriteLine("Удаление отменено.");
                        return;
                    }

                    // Удаляем сотрудника
                    int rowsAffected = conn.QuerySingle<int>(
                        "sp_DeleteEmployee",
                        new { EmployeeID = employeeId },
                        commandType: CommandType.StoredProcedure
                    );

                    if (rowsAffected > 0)
                    {
                        Console.WriteLine("\n✓ Сотрудник успешно удален.");
                    }
                    else
                    {
                        Console.WriteLine("\n✗ Не удалось удалить сотрудника.");
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"\n✗ Ошибка базы данных: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n✗ Ошибка: {ex.Message}");
            }
        }
    }
}
