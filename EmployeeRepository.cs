namespace EmployeeManagementApp.Infrastructure
{
    using System;
    using System.Collections.Generic;

    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly string _connectionString;

        public EmployeeRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public int Add(Employee employee)
        {
            using (IDbConnection conn = new SqlConnection(_connectionString))
            {
                var parameters = new
                {
                    employee.FirstName,
                    employee.LastName,
                    employee.Email,
                    employee.DateOfBirth,
                    employee.Salary
                };

                return conn.QuerySingle<int>(
                    "sp_AddEmployee",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );
            }
        }

        public IEnumerable<Employee> GetAll()
        {
            using (IDbConnection conn = new SqlConnection(_connectionString))
            {
                return conn.Query<Employee>(
                    "sp_GetAllEmployees",
                    commandType: CommandType.StoredProcedure
                ).ToList();
            }
        }

        public Employee GetById(int id)
        {
            using (IDbConnection conn = new SqlConnection(_connectionString))
            {
                return conn.QueryFirstOrDefault<Employee>(
                    "sp_GetEmployeeById",
                    new { EmployeeID = id },
                    commandType: CommandType.StoredProcedure
                );
            }
        }

        public void UpdateField(int employeeId, string fieldName, string fieldValue)
        {
            using (IDbConnection conn = new SqlConnection(_connectionString))
            {
                conn.Execute(
                    "sp_UpdateEmployeeField",
                    new
                    {
                        EmployeeID = employeeId,
                        FieldName = fieldName,
                        FieldValue = fieldValue
                    },
                    commandType: CommandType.StoredProcedure
                );
            }
        }

        public int Delete(int employeeId)
        {
            using (IDbConnection conn = new SqlConnection(_connectionString))
            {
                return conn.QuerySingle<int>(
                    "sp_DeleteEmployee",
                    new { EmployeeID = employeeId },
                    commandType: CommandType.StoredProcedure
                );
            }
        }
    }

}


