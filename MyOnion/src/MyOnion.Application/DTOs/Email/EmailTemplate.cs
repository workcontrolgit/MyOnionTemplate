namespace MyOnion.Application.DTOs.Email
{
    /// <summary>
    /// Represents an email template with subject, body, and variable definitions
    /// </summary>
    public class EmailTemplate
    {
        /// <summary>
        /// Unique identifier for the template
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Display name for the template
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Description of when to use this template
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Category for organizing templates (e.g., "Transactional", "Marketing")
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Subject line template with Scriban syntax (e.g., "Welcome, {{ user.first_name }}!")
        /// </summary>
        public string SubjectTemplate { get; set; }

        /// <summary>
        /// HTML body template with Scriban syntax
        /// </summary>
        public string BodyTemplate { get; set; }

        /// <summary>
        /// Default sender email address (optional, falls back to MailSettings if not provided)
        /// </summary>
        public string From { get; set; }

        /// <summary>
        /// List of required variables for this template
        /// </summary>
        public List<string> RequiredVariables { get; set; } = new();

        /// <summary>
        /// Example variables for documentation and testing
        /// </summary>
        public Dictionary<string, string> ExampleVariables { get; set; } = new();
    }
}
