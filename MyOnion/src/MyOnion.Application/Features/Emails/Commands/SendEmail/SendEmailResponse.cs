namespace MyOnion.Application.Features.Emails.Commands.SendEmail;

public class SendEmailResponse
{
    public bool Sent { get; set; }
    public string MessageId { get; set; }
    public DateTime SentAtUtc { get; set; }
    public int TotalRecipients { get; set; }
    public int ToCount { get; set; }
    public int CcCount { get; set; }
    public int BccCount { get; set; }
}
