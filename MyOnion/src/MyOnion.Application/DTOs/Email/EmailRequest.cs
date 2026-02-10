namespace MyOnion.Application.DTOs.Email
{
    // Represents an email request with properties for recipient addresses, subject line, message body, and sender address
    public class EmailRequest
    {
        // Primary recipient addresses (To)
        public List<string> To { get; set; } = new();

        // Carbon copy recipient addresses (CC)
        public List<string> Cc { get; set; } = new();

        // Blind carbon copy recipient addresses (BCC)
        public List<string> Bcc { get; set; } = new();

        // Subject line of the email
        public string Subject { get; set; }

        // Message body of the email
        public string Body { get; set; }

        // Sender address of the email
        public string From { get; set; }
    }
}