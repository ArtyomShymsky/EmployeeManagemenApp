
namespace EmployeeManagementApp.Presentation
{
    using System;
    using System.Linq;
    using EmployeeManagemenApp.Application.Services;
    using EmployeeManagemenApp.Domain;

    public class ConsoleUI
    {
        private readonly EmployeeService _employeeService;

        public ConsoleUI(EmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        public void Run()
        {
            bool exit = false;

            Console.WriteLine("╔════════════════════════════════════════════════════════╗");
            Console.WriteLine("║   Система управления информацией о сотрудниках         ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════╝");

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
                        case "1": AddEmployee(); break;
                        case "2": ViewAllEmployees(); break;
                        case "3": UpdateEmployee(); break;
                        case "4": DeleteEmployee(); break;
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

        private void AddEmployee()
        {
            Console.WriteLine("\n" + new string('═', 56));
            Console.WriteLine("ДОБАВЛЕНИЕ НОВОГО СОТРУДНИКА");
            Console.WriteLine(new string('═', 56));

            try
            {
                var employee = new Employee
                {
                    FirstName = ReadInput("Введите имя: "),
                    LastName = ReadInput("Введите фамилию: "),
                    Email = ReadInput("Введите email: "),
                    DateOfBirth = ReadDate("Введите дату рождения (ГГГГ-ММ-ДД): "),
                    Salary = ReadDecimal("Введите зарплату: ")
                };

                int newId = _employeeService.AddEmployee(employee);
                Console.WriteLine($"\n✓ Сотрудник успешно добавлен с ID: {newId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n✗ Ошибка: {ex.Message}");
            }
        }

        private void ViewAllEmployees()
        {
            Console.WriteLine("\n" + new string('═', 56));
            Console.WriteLine("СПИСОК ВСЕХ СОТРУДНИКОВ");
            Console.WriteLine(new string('═', 56));

            try
            {
                var employees = _employeeService.GetAllEmployees().ToList();

                if (!employees.Any())
                {
                    Console.WriteLine("\nНет данных о сотрудниках в базе данных.");
                    return;
                }

                foreach (var emp in employees)
                {
                    Console.WriteLine($"\n[ID: {emp.EmployeeID}]");
                    Console.WriteLine($"  Имя: {emp.GetFullName()}");
                    Console.WriteLine($"  Email: {emp.Email}");
                    Console.WriteLine($"  Дата рождения: {emp.DateOfBirth:dd.MM.yyyy}");
                    Console.WriteLine($"  Зарплата: {emp.Salary:N2} руб.");
                }

                Console.WriteLine($"\n{new string('─', 56)}");
                Console.WriteLine($"Всего сотрудников: {employees.Count}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n✗ Ошибка: {ex.Message}");
            }
        }

        private void UpdateEmployee()
        {
            Console.WriteLine("\n" + new string('═', 56));
            Console.WriteLine("ОБНОВЛЕНИЕ ИНФОРМАЦИИ О СОТРУДНИКЕ");
            Console.WriteLine(new string('═', 56));

            try
            {
                int employeeId = ReadInt("Введите ID сотрудника для обновления: ");
                var employee = _employeeService.GetEmployeeById(employeeId);

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

                var (fieldName, prompt) = GetFieldInfo(fieldChoice);
                if (string.IsNullOrEmpty(fieldName))
                {
                    Console.WriteLine("✗ Неверный выбор поля.");
                    return;
                }

                string newValue = ReadInput(prompt);
                _employeeService.UpdateEmployeeField(employeeId, fieldName, newValue);
                Console.WriteLine("\n✓ Информация успешно обновлена.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n✗ Ошибка: {ex.Message}");
            }
        }

        private void DeleteEmployee()
        {
            Console.WriteLine("\n" + new string('═', 56));
            Console.WriteLine("УДАЛЕНИЕ СОТРУДНИКА");
            Console.WriteLine(new string('═', 56));

            try
            {
                int employeeId = ReadInt("Введите ID сотрудника для удаления: ");
                var employee = _employeeService.GetEmployeeById(employeeId);

                if (employee == null)
                {
                    Console.WriteLine($"✗ Сотрудник с ID {employeeId} не найден.");
                    return;
                }

                Console.WriteLine($"\nСотрудник: {employee.GetFullName()}");
                Console.Write("\nВы уверены, что хотите удалить этого сотрудника? (да/нет): ");
                string confirmation = Console.ReadLine()?.ToLower().Trim();

                if (confirmation != "да" && confirmation != "yes")
                {
                    Console.WriteLine("Удаление отменено.");
                    return;
                }

                int rowsAffected = _employeeService.DeleteEmployee(employeeId);

                if (rowsAffected > 0)
                    Console.WriteLine("\n✓ Сотрудник успешно удален.");
                else
                    Console.WriteLine("\n✗ Не удалось удалить сотрудника.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n✗ Ошибка: {ex.Message}");
            }
        }

        private string ReadInput(string prompt)
        {
            Console.Write(prompt);
            return Console.ReadLine()?.Trim();
        }

        private int ReadInt(string prompt)
        {
            Console.Write(prompt);
            if (int.TryParse(Console.ReadLine(), out int result))
                return result;
            throw new FormatException("Неверный формат числа");
        }

        private DateTime ReadDate(string prompt)
        {
            Console.Write(prompt);
            if (DateTime.TryParse(Console.ReadLine(), out DateTime result))
                return result;
            throw new FormatException("Неверный формат даты");
        }

        private decimal ReadDecimal(string prompt)
        {
            Console.Write(prompt);
            if (decimal.TryParse(Console.ReadLine(), out decimal result))
                return result;
            throw new FormatException("Неверный формат числа");
        }

        private (string fieldName, string prompt) GetFieldInfo(string choice)
        {
            return choice switch
            {
                "1" => ("FirstName", "Введите новое имя: "),
                "2" => ("LastName", "Введите новую фамилию: "),
                "3" => ("Email", "Введите новый email: "),
                "4" => ("DateOfBirth", "Введите новую дату рождения (ГГГГ-ММ-ДД): "),
                "5" => ("Salary", "Введите новую зарплату: "),
                _ => (null, null)
            };
        }
    }

}

