using System.ComponentModel.DataAnnotations.Schema;
using MyOnion.Domain.ValueObjects;

namespace MyOnion.Domain.Entities
{
    public class Employee : AuditableBaseEntity // Inheriting from a base entity that includes audit information such as created/modified dates and user IDs.
    {
        public PersonName Name { get; set; } // The employee's name.
        [NotMapped]
        public string FirstName => Name?.FirstName;
        [NotMapped]
        public string MiddleName => Name?.MiddleName;
        [NotMapped]
        public string LastName => Name?.LastName;
        [NotMapped]
        public string FullName => Name?.FullName;
        // Foreign Key for Position
        public Guid PositionId { get; set; } // A unique identifier for the position that the employee holds.
        // Navigation Property for Position
        public virtual Position Position { get; set; } // A reference to the Position entity that the employee is associated with. This allows you to retrieve information about the employee's position without having to make additional database queries.
        // Foreign Key for Home Department
        public Guid DepartmentId { get; set; } // A unique identifier for the employee's home department.
        // Navigation Property for Home Department
        public virtual Department Department { get; set; } // A reference to the Department entity that represents the employee's home organization.
        // Salary of the Employee
        public decimal Salary { get; set; } // The monthly salary of the employee.

        public DateTime Birthday { get; set; } // The date of birth for the employee.
        public string Email { get; set; } // The email address for the employee.
        public Gender Gender { get; set; } // An enumeration representing the gender of the employee.
        public string EmployeeNumber { get; set; } // A unique identifier for the employee within your organization.
        public string Prefix { get; set; } // Any prefixes or titles that should be displayed before the employee's name, such as "Dr." or "Mr."
        public string Phone { get; set; } // The phone number for the employee.
    }
}
