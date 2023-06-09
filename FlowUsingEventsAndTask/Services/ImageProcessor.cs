class ImageProcessor
{
    public event EventHandler<ImageProcessedEventArgs> ImageProcessed;

    public async Task Process(object sender, ImageGrabbedEventArgs e)
    {
        // Simulating image processing
        await Task.Delay(500); // Simulating processing delay
        Console.WriteLine($"Processed frame: {e.Frame}");
        OnImageProcessed(new ImageProcessedEventArgs(e.Frame));
    }

    protected virtual void OnImageProcessed(ImageProcessedEventArgs e)
    {
        ImageProcessed?.Invoke(this, e);
    }
}
