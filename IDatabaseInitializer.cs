namespace EmployeeManagementApp.Application.Interfaces
{
    using System;

    public interface IDatabaseInitializer
    {
        bool Initialize();
        string GetConnectionString();
    }
}



