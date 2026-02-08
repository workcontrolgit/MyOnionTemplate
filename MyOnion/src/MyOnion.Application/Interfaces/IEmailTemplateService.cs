using MyOnion.Application.DTOs.Email;

namespace MyOnion.Application.Interfaces
{
    /// <summary>
    /// Service for loading and rendering email templates
    /// </summary>
    public interface IEmailTemplateService
    {
        /// <summary>
        /// Gets a template by its ID
        /// </summary>
        /// <param name="templateId">Unique identifier of the template</param>
        /// <returns>Email template or null if not found</returns>
        Task<EmailTemplate> GetTemplateAsync(string templateId);

        /// <summary>
        /// Gets all available templates
        /// </summary>
        /// <returns>List of all email templates</returns>
        Task<List<EmailTemplate>> GetAllTemplatesAsync();

        /// <summary>
        /// Renders a template with the provided variables
        /// </summary>
        /// <param name="template">The template to render</param>
        /// <param name="variables">Variables to substitute in the template</param>
        /// <returns>Rendered subject and body</returns>
        Task<(string Subject, string Body)> RenderTemplateAsync(EmailTemplate template, Dictionary<string, object> variables);

        /// <summary>
        /// Validates that all required variables are provided
        /// </summary>
        /// <param name="template">The template to validate against</param>
        /// <param name="variables">Variables provided by the caller</param>
        /// <returns>List of missing variable names (empty if all provided)</returns>
        List<string> ValidateRequiredVariables(EmailTemplate template, Dictionary<string, object> variables);
    }
}
