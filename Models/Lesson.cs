using Newtonsoft.Json;

namespace IBCVideoCourseTGBot.Models;

public class Lesson
{
    public string Id { get; set; }
    public List<Step> Steps { get; set; }

    public Lesson(string path)
    {
        Id = path.Split(Path.DirectorySeparatorChar).Last().Split("-")[0];
        Steps = new List<Step>();
        Console.WriteLine("Reading lesson " + path);
        foreach (var stepPath in Directory.GetFiles(path))
        {
            Console.WriteLine("  Reading step " + stepPath);
            var stepFileName = stepPath.Split(Path.DirectorySeparatorChar).Last();
            var stepType = stepFileName.Split(".")[0].Split("-")[1];
            var step = stepType switch
            {
                "question" => JsonConvert.DeserializeObject<Question>(File.ReadAllText(stepPath))! as Step,
                "message" => JsonConvert.DeserializeObject<Message>(File.ReadAllText(stepPath))!,
                "video" => JsonConvert.DeserializeObject<Video>(File.ReadAllText(stepPath))!,
                "international" => JsonConvert.DeserializeObject<International>(File.ReadAllText(stepPath))!,
                "email" => JsonConvert.DeserializeObject<Email>(File.ReadAllText(stepPath))!,
                _ => throw new Exception("Unknown file type")
            };
            step.Id = stepFileName.Split("-")[0];
            step.Lesson = this;
            Steps.Add(step);
        }
    }
}