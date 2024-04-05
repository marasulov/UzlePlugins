using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace UzlePlugins.Contracts.DTOs;

public class OffsetsDto
{
    public OffsetsDto(ObservableCollection<OffsetRanges> duct, ObservableCollection<OffsetRanges> pipe)
    {
        Duct = duct;
        Pipe = pipe;
    }

    public ObservableCollection<OffsetRanges> Duct { get; set; }
    public ObservableCollection<OffsetRanges> Pipe { get; set; }
}