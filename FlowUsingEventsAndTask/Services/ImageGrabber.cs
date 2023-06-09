class ImageGrabber
{
    public event EventHandler<ImageGrabbedEventArgs> ImageGrabbed;
    public event EventHandler GrabImage;

    public async Task Grab()
    {
        // Simulating image grabbing
        for (int i = 1; i <= 10; i++)
        {
            await Task.Delay(100); // Simulating grabbing delay
            Console.WriteLine($"Grabbed frame: {i}");
            OnImageGrabbed(new ImageGrabbedEventArgs(i));
        }
    }

    protected virtual void OnImageGrabbed(ImageGrabbedEventArgs e)
    {
        ImageGrabbed?.Invoke(this, e);
    }

    public virtual void OnGrabImage()
    {
        GrabImage?.Invoke(this, EventArgs.Empty);
    }
}
