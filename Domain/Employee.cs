namespace EmployeeManagementApp.Domain
{
    using System;
    public record Employee
    {
        public int EmployeeID { get; init; }
        public string FirstName { get; init; }
        public string LastName { get; init; }
        public string Email { get; init; }
        public DateTime DateOfBirth { get; init; }
        public decimal Salary { get; init; }
        public int? DepartmentId { get; init; }
        public string? Position { get; init; }
        public DateTime HireDate { get; init; }

        public string GetFullName() => $"{FirstName} {LastName}";
    }
}