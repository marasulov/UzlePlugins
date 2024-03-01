using System.Diagnostics;
using System.Drawing;
using UzlePlugins.Contracts;

namespace UzlePlugins.Models
{
    public class HoleModel : IHoleModel, IIntersectionPointZoom

    {
        public int Id { get; set; }
        public string IntersectionPoint { get; set; }
        public string IntersectingElementName { get; set; }
        public string Description { get; set; }
        public string HoleType { get; set; }
        public double IntersectingElementTypeSize { get; set; }
        public string IntersectedSourceType { get; set; }
        public bool IsHoleRectangled { get; set; }
        public double HoleOffset { get; set; }
        public bool IsInsert { get; set; }

        public HoleModel(int id, string point, string intersectingElementName, string description, string holeType,
            double intersectingElementTypeSize, string intersectedSourceType, bool isHoleRectangled, double holeOffset,
            bool isInsert)
        {
            Id = id;
            IntersectionPoint = point;
            IntersectingElementName = intersectingElementName;
            Description = description;
            HoleType = holeType;
            IntersectingElementTypeSize = intersectingElementTypeSize;
            IntersectedSourceType = intersectedSourceType;
            IsHoleRectangled = isHoleRectangled;
            HoleOffset = holeOffset;
            IsInsert = isInsert;

        }

        public void FamilyZoom(int id)
        {
            Debug.Print($"id {id}");
        }
    }
}
