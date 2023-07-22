using IBCVideoCourseTGBot.Models;
using Newtonsoft.Json;

namespace IBCVideoCourseTGBot;

public static class JsonsReader
{
    private static List<Lesson>? _course;

    public static List<Lesson> GetCourse()
    {
        if (_course is not null) return _course;

        _course = new List<Lesson>();
        const string basePath = "course";
        foreach (var lessonPath in Directory.GetDirectories(basePath))
        {
            var lesson = new Lesson(lessonPath);
            _course.Add(lesson);
        }

        return _course;
    }

    private static CommonMessages? _successMessages;

    public static CommonMessages GetSuccessMessages()
    {
        return _successMessages ??=
            JsonConvert.DeserializeObject<CommonMessages>(File.ReadAllText("course/success_messages.json"))!;
    }

    private static CommonMessages? _wrongMessages;

    public static CommonMessages GetWrongMessages()
    {
        return _wrongMessages ??=
            JsonConvert.DeserializeObject<CommonMessages>(File.ReadAllText("course/wrong_messages.json"))!;
    }
}