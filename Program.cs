using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.Fonts;

namespace CatImageDownloader
{
    class Program
    {
        private static readonly HttpClient client = new HttpClient();

        static async Task Main(string[] args)
        {
            var outputFileOption = new Option<string>(
                new[] { "-o", "--outputFilepath" },
                "Path to the output file where the image will be saved")
            {
                IsRequired = true
            };

            var textToOverlayOption = new Option<string>(
                new[] { "-t", "--textToOverlay" },
                "Text to overlay on the image (optional)");

            var rootCommand = new RootCommand
            {
                outputFileOption,
                textToOverlayOption
            };

            rootCommand.Description = "Application to download and save cat images";

            rootCommand.SetHandler<string, string>(async (outputFilepath, textToOverlay) =>
            {
                await DownloadCatImage(outputFilepath, textToOverlay);
            }, outputFileOption, textToOverlayOption);

            await rootCommand.InvokeAsync(args);
        }

        private static async Task DownloadCatImage(string outputFilepath, string textToOverlay)
        {
            try
            {
                string url = "https://cataas.com/cat";
                if (!string.IsNullOrEmpty(textToOverlay))
                {
                    url += $"?text={Uri.EscapeDataString(textToOverlay)}";
                }
                Console.WriteLine($"Generated URL: {url}");

                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                byte[] imageBytes = await response.Content.ReadAsByteArrayAsync();

                using (Image<Rgba32> image = Image.Load<Rgba32>(imageBytes))
                {
                    if (!string.IsNullOrEmpty(textToOverlay))
                    {
                        var font = SystemFonts.CreateFont("Arial", 24);
                        var textGraphicsOptions = new DrawingOptions
                        {
                            GraphicsOptions = new GraphicsOptions
                            {
                                Antialias = true
                            }
                        };

                        var textColor = Color.White;
                        var textPosition = new PointF(image.Width / 2, image.Height / 2);
                      
                        image.Mutate(ctx => ctx.DrawText(textGraphicsOptions, textToOverlay, font, textColor, textPosition));
                    }
                 
                    await image.SaveAsync(outputFilepath);
                }

                Console.WriteLine($"Image saved to: {outputFilepath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
    }
}