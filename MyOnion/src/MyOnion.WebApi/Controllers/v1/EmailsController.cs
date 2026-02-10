using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyOnion.Application.Common.Results;
using MyOnion.Application.DTOs.Email;
using MyOnion.Application.Features.Emails.Commands.SendEmail;
using MyOnion.Application.Features.Emails.Commands.SendTemplatedEmail;
using MyOnion.Application.Interfaces;

namespace MyOnion.WebApi.Controllers.v1;

[ApiVersion("1.0")]
public class EmailsController : BaseApiController
{
    private readonly IEmailTemplateService _templateService;

    public EmailsController(IEmailTemplateService templateService)
    {
        _templateService = templateService;
    }
    /// <summary>
    /// Send an email to one or more recipients with optional CC and BCC
    /// </summary>
    /// <param name="command">Email details including recipients (To, CC, BCC), subject, and body</param>
    /// <returns>Email send result with message ID, timestamp, and recipient counts</returns>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(Result<SendEmailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Send(SendEmailCommand command)
    {
        var result = await Mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Send an email using a predefined template with variable substitution
    /// </summary>
    /// <param name="command">Template ID, variables, and recipient details</param>
    /// <returns>Email send result with message ID, timestamp, and recipient counts</returns>
    [HttpPost("templated")]
    [Authorize]
    [ProducesResponseType(typeof(Result<SendEmailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SendTemplated(SendTemplatedEmailCommand command)
    {
        var result = await Mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Get all available email templates
    /// </summary>
    /// <returns>List of email templates</returns>
    [HttpGet("templates")]
    [Authorize]
    [ProducesResponseType(typeof(List<EmailTemplate>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetTemplates()
    {
        var templates = await _templateService.GetAllTemplatesAsync();
        return Ok(templates);
    }

    /// <summary>
    /// Get a specific email template by ID
    /// </summary>
    /// <param name="templateId">Template identifier</param>
    /// <returns>Email template details</returns>
    [HttpGet("templates/{templateId}")]
    [Authorize]
    [ProducesResponseType(typeof(EmailTemplate), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetTemplate(string templateId)
    {
        var template = await _templateService.GetTemplateAsync(templateId);

        if (template == null)
        {
            return NotFound(new { message = $"Template '{templateId}' not found" });
        }

        return Ok(template);
    }
}
