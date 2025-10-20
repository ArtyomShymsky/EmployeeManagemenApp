namespace EmployeeManagementConsoleApp
{
    using EmployeeManagementApp.Application.Interfaces;
    using EmployeeManagementApp.Application.Services;
    using EmployeeManagementApp.Presentation;
    using Infrastructure.Data;
    using Infrastructure.Interfaces;
    using Infrastructure.Repositories;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using System;
    public class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var services = new ServiceCollection();
            services.AddSingleton<IConfiguration>(configuration);

            services.AddTransient<DatabaseInitializer>(provider =>
                           new DatabaseInitializer(configuration.GetConnectionString("MasterConnection")));

            services.AddTransient<IEmployeeRepository>(provider =>
                new EmployeeRepository(configuration.GetConnectionString("DefaultConnection")));

            services.AddTransient<IEmployeeService, EmployeeService>();
            services.AddTransient<ConsoleUI>();

            var serviceProvider = services.BuildServiceProvider();


            var dbInitializer = serviceProvider.GetRequiredService<DatabaseInitializer>();
            if (!dbInitializer.Initialize())
            {
                Console.WriteLine("\nНажмите любую клавишу для выхода...");
                Console.ReadKey();
                return;
            }

            Console.WriteLine("База данных успешно инициализирована.");
            Console.WriteLine("Подключение к базе данных успешно установлено.");

            var ui = serviceProvider.GetRequiredService<ConsoleUI>();
            ui.Run();

        }
    }
}
