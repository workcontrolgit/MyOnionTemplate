namespace MyOnion.Application.Features.Positions.DTOs
{
    /// <summary>
    /// Lightweight projection for listing or shaping position data.
    /// </summary>
    public class PositionSummaryDto
    {
        public Guid Id { get; set; }

        public string PositionTitle { get; set; }

        public string PositionNumber { get; set; }

        public string PositionDescription { get; set; }

        public Department Department { get; set; }

        public SalaryRange SalaryRange { get; set; }
    }
}
