using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;

namespace NBATracker;

internal class EmailService
{
    private string? _userName;
    private string? _password;

    private const string EmailRegex =
            @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z";

    public EmailService()
    {
        try
        {
            using TextReader reader = new StreamReader("emailConfig.txt");

            string? email = reader.ReadLine(), password = reader.ReadLine();
            if (email is null || password is null) return;
            _userName = email;
            _password = password;
        }

        catch (FileNotFoundException) { }
    }

    public void SetEmailConfig()
    {
        if (_userName is not null && _password is not null) 
            return;

        Console.WriteLine(@"
Please enter your email and password.
It will be saved, so you don't need to re-enter your credentials again.");

        Console.Write("Your Email: ");
        string email = Console.ReadLine()!;

        Console.Write("Password: ");
        string password = Console.ReadLine()!;


        if (IsEmailValid(email)) _userName = email;

        else
        {
            Console.WriteLine("Please enter a valid email.");
            return;
        }

        _password = password;

        using TextWriter writer = new StreamWriter("emailConfig.txt");
        writer.WriteLine(email);
        writer.WriteLine(password);
    }

    public static bool IsEmailValid(string email) => 
        Regex.IsMatch(email, EmailRegex, RegexOptions.IgnoreCase);

    public void SendEmail(string subject, string body)
    {
        try
        {
            using var client = new SmtpClient("smtp.gmail.com", 587);
            client.EnableSsl = true;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = false;
            client.Credentials = new NetworkCredential(_userName, _password);
            MailMessage email = new();
            email.From = new MailAddress(_userName!);
            email.To.Add(_userName!);
            email.Subject = subject;
            email.Body = body;
            email.IsBodyHtml = true;
            client.Send(email);
            Console.WriteLine("Successfully sent your email!");
        }

        catch
        {
            Console.WriteLine("An unknown error occurred whlie sending your email.");
        }
    }
}