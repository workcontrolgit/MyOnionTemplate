// Define a namespace for organizing code and avoiding naming conflicts
namespace MyOnion.Infrastructure.Shared.Services
{
    // Define a public class named DateTimeService that implements the IDateTimeService interface
    public class DateTimeService : IDateTimeService
    {
        // Implement a read-only property NowUtc that returns the current date and time in UTC
        public DateTime NowUtc => DateTime.UtcNow;
    }
}