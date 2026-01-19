namespace MyOnion.Application.Features.Employees.Queries.GetEmployeeById
{
    public class EmployeeDetailsDto
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public DateTime Birthday { get; set; }
        public string Email { get; set; }
        public Gender Gender { get; set; }
        public string EmployeeNumber { get; set; }
        public string Prefix { get; set; }
        public string Phone { get; set; }
        public decimal Salary { get; set; }
        public Guid PositionId { get; set; }
        public string PositionTitle { get; set; }
        public Guid DepartmentId { get; set; }
        public string DepartmentName { get; set; }
    }
}
