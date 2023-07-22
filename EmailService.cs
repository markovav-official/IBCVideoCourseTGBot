using System.Net;
using System.Net.Mail;

namespace IBCVideoCourseTGBot;

public class EmailService
{
    private readonly string _server;
    private readonly int _port;
    private readonly string _username;
    private readonly string _password;

    private static EmailService _emailService = null!;

    private EmailService(string server, int port, string username, string password)
    {
        _server = server;
        _port = port;
        _username = username;
        _password = password;
    }

    public static void InitEmailService(string server, int port, string username, string password)
    {
        _emailService = new EmailService(server, port, username, password);
    }

    public static EmailService GetInstance()
    {
        return _emailService;
    }

    public void SendEmail(string toEmail, string subject, string body)
    {
        using var mail = new MailMessage();

        mail.Subject = subject;
        mail.Body = body;
        mail.From = new MailAddress(_username);
        mail.To.Add(new MailAddress(toEmail));
        mail.IsBodyHtml = true;


        using var smtpClient = new SmtpClient(_server, _port);

        smtpClient.UseDefaultCredentials = false;
        smtpClient.Credentials = new NetworkCredential(_username, _password);
        smtpClient.EnableSsl = true;
        smtpClient.Send(mail);
    }
}