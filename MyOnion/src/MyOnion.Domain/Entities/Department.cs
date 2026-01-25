using MyOnion.Domain.ValueObjects;

namespace MyOnion.Domain.Entities
{
    // Represents a department in the organization.
    public class Department : AuditableBaseEntity
    {
        // Gets or sets the name of the department.
        public DepartmentName Name { get; set; }

        // Navigation property for related positions.
        // This property allows for easy access to all positions associated with this department.
        public virtual ICollection<Position> Positions { get; set; }

        // Initializes a new instance of the Department class, creating an empty collection for positions.
        public Department()
        {
            Positions = new HashSet<Position>();
        }

        // Additional properties (e.g., Name, ManagerId, etc.) can be added here
    }
}
