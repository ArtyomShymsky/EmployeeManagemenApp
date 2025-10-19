using EmployeeManagementApp.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Interfaces
{
    public interface IEmployeeService
    {
        int AddEmployee(Employee employee);
        IEnumerable<Employee> GetAllEmployees();
        Employee GetEmployeeById(int id);
        void UpdateEmployeeField(int employeeId, string fieldName, string fieldValue);
        bool DeleteEmployee(int employeeId);
    }
}
