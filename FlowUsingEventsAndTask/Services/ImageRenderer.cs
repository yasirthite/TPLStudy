class ImageRenderer
{
    private static readonly object lockObject = new object();
    private static int currentFrame = 1;

    public event EventHandler<ImageRenderedEventArgs> ImageRendered;

    public async Task Render(object sender, ImageProcessedEventArgs e)
    {
        // Simulating image rendering
        await Task.Delay(200); // Simulating rendering delay

        lock (lockObject)
        {
            if (e.Frame == currentFrame)
            {
                Console.WriteLine($"Rendered frame: {e.Frame}");
                currentFrame++;
                OnImageRendered(new ImageRenderedEventArgs(e.Frame));
            }
        }
    }

    protected virtual void OnImageRendered(ImageRenderedEventArgs e)
    {
        ImageRendered?.Invoke(this, e);
    }
}
