class ImageGrabbedEventArgs : EventArgs
{
    public int Frame { get; }

    public ImageGrabbedEventArgs(int frame)
    {
        Frame = frame;
    }
}
