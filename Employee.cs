namespace EmployeeManagementApp.Domain
{
    using System;

    public class Employee
    {
        public int EmployeeID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public DateTime DateOfBirth { get; set; }
        public decimal Salary { get; set; }

        public string GetFullName() => $"{FirstName} {LastName}";
    }
}