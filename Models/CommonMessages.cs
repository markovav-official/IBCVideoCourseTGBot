namespace IBCVideoCourseTGBot.Models;

public class CommonMessages
{
    public string[] Messages { get; set; } = null!;
    public string ButtonText { get; set; } = null!;
}

public class CommonMessage
{
    public string Message { get; set; } = null!;
    public string ButtonText { get; set; } = null!;
}

public static class Helper
{
    public static CommonMessage GetRandom(this CommonMessages messages)
    {
        var random = new Random();
        var index = random.Next(messages.Messages.Length);
        return new CommonMessage
        {
            Message = messages.Messages[index],
            ButtonText = messages.ButtonText
        };
    }
}