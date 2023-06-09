class ImageRenderedEventArgs : EventArgs
{
    public int Frame { get; }

    public ImageRenderedEventArgs(int frame)
    {
        Frame = frame;
    }
}
