namespace UzlePlugins.Contracts.DTOs;

public class OffsetRanges
{
    public OffsetRanges(int from, int to, int offset)
    {
        From = from;
        To = to;
        Offset = offset;
    }

    public int From { get; set; }
    public int To { get; set; }
    public int Offset { get; set; }

    public bool IsDelete { get; set; }
}