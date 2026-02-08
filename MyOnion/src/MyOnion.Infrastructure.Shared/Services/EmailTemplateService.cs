using System.IO;
using System.Text.Json;
using Scriban;
using MyOnion.Application.DTOs.Email;
using MyOnion.Application.Interfaces;

namespace MyOnion.Infrastructure.Shared.Services
{
    /// <summary>
    /// Service for loading and rendering email templates from JSON files
    /// </summary>
    public class EmailTemplateService : IEmailTemplateService
    {
        private readonly string _templatesPath;
        private Dictionary<string, EmailTemplate> _templateCache;
        private readonly object _cacheLock = new object();

        public EmailTemplateService()
        {
            // Templates are stored relative to the application base directory
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            _templatesPath = Path.Combine(baseDirectory, "email-templates");

            // If running in development, check the project root
            if (!Directory.Exists(_templatesPath))
            {
                var projectRoot = Directory.GetCurrentDirectory();
                _templatesPath = Path.Combine(projectRoot, "email-templates");
            }

            _templateCache = new Dictionary<string, EmailTemplate>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Gets a template by ID, loading from file system if not cached
        /// </summary>
        public async Task<EmailTemplate> GetTemplateAsync(string templateId)
        {
            // Check cache first
            if (_templateCache.TryGetValue(templateId, out var cachedTemplate))
            {
                return cachedTemplate;
            }

            // Load from file system
            var templateFile = Path.Combine(_templatesPath, $"{templateId}.json");

            if (!File.Exists(templateFile))
            {
                return null;
            }

            var json = await File.ReadAllTextAsync(templateFile);
            var template = JsonSerializer.Deserialize<EmailTemplate>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            // Cache for future use
            lock (_cacheLock)
            {
                _templateCache[templateId] = template;
            }

            return template;
        }

        /// <summary>
        /// Gets all templates from the templates directory
        /// </summary>
        public async Task<List<EmailTemplate>> GetAllTemplatesAsync()
        {
            var templates = new List<EmailTemplate>();

            if (!Directory.Exists(_templatesPath))
            {
                return templates;
            }

            var templateFiles = Directory.GetFiles(_templatesPath, "*.json");

            foreach (var file in templateFiles)
            {
                try
                {
                    var json = await File.ReadAllTextAsync(file);
                    var template = JsonSerializer.Deserialize<EmailTemplate>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (template != null)
                    {
                        templates.Add(template);
                    }
                }
                catch
                {
                    // Skip invalid template files
                    continue;
                }
            }

            return templates;
        }

        /// <summary>
        /// Renders a template using Scriban with provided variables
        /// </summary>
        public async Task<(string Subject, string Body)> RenderTemplateAsync(
            EmailTemplate template,
            Dictionary<string, object> variables)
        {
            // Parse and render subject template
            var subjectTemplate = Template.Parse(template.SubjectTemplate);
            var renderedSubject = await subjectTemplate.RenderAsync(variables);

            // Parse and render body template
            var bodyTemplate = Template.Parse(template.BodyTemplate);
            var renderedBody = await bodyTemplate.RenderAsync(variables);

            return (renderedSubject, renderedBody);
        }

        /// <summary>
        /// Validates that all required variables are provided
        /// </summary>
        public List<string> ValidateRequiredVariables(
            EmailTemplate template,
            Dictionary<string, object> variables)
        {
            var missingVariables = new List<string>();

            if (template.RequiredVariables == null || !template.RequiredVariables.Any())
            {
                return missingVariables;
            }

            foreach (var requiredVar in template.RequiredVariables)
            {
                // Check if variable exists and is not null/empty
                if (!variables.ContainsKey(requiredVar) ||
                    variables[requiredVar] == null ||
                    (variables[requiredVar] is string str && string.IsNullOrWhiteSpace(str)))
                {
                    missingVariables.Add(requiredVar);
                }
            }

            return missingVariables;
        }
    }
}
