namespace EmployeeManagementConsoleApp
{
    using EmployeeManagementApp.Application.Interfaces;
    using EmployeeManagementApp.Application.Services;
    using EmployeeManagementApp.Presentation;
    using Infrastructure.Data;
    using Infrastructure.Interfaces;
    using Infrastructure.Repositories;
    using System;
    public class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            // Конфигурация
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            // Прямое указание строки подключения (без IConfiguration)
            string masterConnectionString = "Server=(localdb)\\MSSQLLocalDB;Database=master;Integrated Security=true;TrustServerCertificate=true;";
            string defaultConnectionString = "Server=(localdb)\\MSSQLLocalDB;Database=EmployeeManagementDB;Integrated Security=true;TrustServerCertificate=true;";

            // Инициализация базы данных
            var dbInitializer = new DatabaseInitializer(masterConnectionString);
            if (!dbInitializer.Initialize())
            {
                Console.WriteLine("\nНажмите любую клавишу для выхода...");
                Console.ReadKey();
                return;
            }

            Console.WriteLine("✓ База данных успешно инициализирована.");
            Console.WriteLine("✓ Подключение к базе данных успешно установлено.");

            // Dependency Injection (ручная композиция)
            IEmployeeRepository repository = new EmployeeRepository(dbInitializer.GetConnectionString());
            IEmployeeService service = new EmployeeService(repository);
            ConsoleUI ui = new ConsoleUI(service);

            // Запуск приложения
            ui.Run();
        }
    }
}
