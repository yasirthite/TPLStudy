using System;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        var imageGrabber = new ImageGrabber();
        var imageProcessor = new ImageProcessor();
        var imageRenderer = new ImageRenderer();

        imageGrabber.GrabImage += async (sender, e) =>
        {
            await imageGrabber.Grab();
        };

        imageGrabber.ImageGrabbed += async (sender, e) =>
        {
            await imageProcessor.Process(sender, e);
        };

        imageProcessor.ImageProcessed += async (sender, e) =>
        {
            await imageRenderer.Render(sender, e);
        };

        // Subscribe to ImageRenderer's ImageRendered event to know when all frames are rendered
        imageRenderer.ImageRendered += (sender, e) =>
        {
            if (e.Frame == 10)
            {
                Console.WriteLine("All frames processed and rendered.");
            }
        };

        // Raise the GrabImage event to trigger image grabbing
        await imageGrabber.Grab();
    }
}
