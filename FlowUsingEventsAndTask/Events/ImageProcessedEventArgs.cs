class ImageProcessedEventArgs : EventArgs
{
    public int Frame { get; }

    public ImageProcessedEventArgs(int frame)
    {
        Frame = frame;
    }
}
