using Microsoft.Extensions.Configuration;

namespace IBCVideoCourseTGBot;

public static class Program
{
    public static async Task Main()
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        JsonsReader.GetCourse();

        EmailService.InitEmailService(
            config["SMTP:SERVER"]!,
            int.Parse(config["SMTP:PORT"]!),
            config["SMTP:USERNAME"]!,
            config["SMTP:PASSWORD"]!
        );
        
        await new TgBot(config["BOT_TOKEN"]).Run();
    }
}