using SkiaSharp;

namespace IBCVideoCourseTGBot;

public static class ImageToTextConverter
{
    public static MemoryStream ConvertTextToImage(string text,
        float textSize = 48, string bgColor = "0071CE", string textColor = "FFFFFF")
    {
        // Define the image dimensions
        const int width = 1080;
        const int height = 350;
        const int xMargin = 200;

        // Create a new SKSurface with the specified dimensions
        using var surface = SKSurface.Create(new SKImageInfo(width, height));
        // Get the canvas from the surface
        var canvas = surface.Canvas;

        // Set the background color
        var backgroundColor = SKColor.Parse(bgColor);
        canvas.Clear(backgroundColor);

        // Create the text paint
        var textPaint = new SKPaint
        {
            Color = SKColor.Parse(textColor),
            TextSize = textSize,
            Typeface = SKTypeface.FromFamilyName("Montserrat", SKFontStyle.Normal),
            TextAlign = SKTextAlign.Center,
            IsAntialias = true // Enable antialiasing
        };

        // Wrap the text into separate lines
        var wrappedLines = WrapTextToLines(text, textPaint, width - xMargin);

        // Calculate the total height of the wrapped text with additional spacing between rows
        var lineHeight = textPaint.FontSpacing * 1.2f; // Adjust the line spacing as desired
        var totalTextHeight = wrappedLines.Length * lineHeight;

        // Calculate the starting Y position for the wrapped text
        var startY = (height - totalTextHeight) / 2;

        // Draw the wrapped text on the canvas
        for (var i = 0; i < wrappedLines.Length; i++)
        {
            var textY = startY + (i + 1) * lineHeight - textPaint.FontMetrics.Descent;
            canvas.DrawText(wrappedLines[i], width / 2, textY, textPaint);
        }

        // Create an SKImage from the surface
        var image = surface.Snapshot();

        // Encode the image to a PNG stream
        var stream = new MemoryStream();
        image.Encode(SKEncodedImageFormat.Png, 100).SaveTo(stream);
        stream.Position = 0;
        return stream;
    }

    private static string[] WrapTextToLines(string text, SKPaint textPaint, int maxWidth)
    {
        var words = text.Split(' ');
        var lines = new List<string>();
        var currentLine = string.Empty;

        foreach (var word in words)
        {
            var tempLine = currentLine.Length > 0 ? currentLine + " " + word : word;
            var wordBounds = new SKRect();
            textPaint.MeasureText(tempLine, ref wordBounds);

            if (wordBounds.Width <= maxWidth)
            {
                currentLine = tempLine;
            }
            else
            {
                lines.Add(currentLine);
                currentLine = word;
            }
        }

        if (!string.IsNullOrEmpty(currentLine))
        {
            lines.Add(currentLine);
        }

        return lines.ToArray();
    }
}