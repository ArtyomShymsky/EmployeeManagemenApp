
namespace EmployeeManagementApp.Application.Interfaces
{
    using EmployeeManagementApp.Domain;
    using System;
    using System.Collections.Generic;


    public interface IEmployeeRepository
    {
        int Add(Employee employee);
        IEnumerable<Employee> GetAll();
        Employee GetById(int id);
        void UpdateField(int employeeId, string fieldName, string fieldValue);
        int Delete(int employeeId);
    }

}

