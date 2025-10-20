namespace EmployeeManagementApp.Application.Interfaces
{
    public interface IDatabaseInitializer
    {
        bool Initialize();
        string GetConnectionString();
    }
}



