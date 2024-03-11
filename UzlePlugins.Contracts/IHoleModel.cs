using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UzlePlugins.Contracts
{
    public interface IHoleModel //: IIntersectionPointZoom
    {
        int Id { get; set; }
        string IntersectionPoint { get; set; }
        string IntersectingElementName { get; set; }
        string Description { get; set; }
        string HoleType { get; set; }
        double IntersectingElementTypeSize { get; set; }
        string IntersectedSourceType { get; set; }
        double SourceElementWidth { get; set; }
        bool IsHoleRectangled { get; set; }
        double HoleOffset { get; set; }
        bool IsInsert { get; set; }

    }
}
