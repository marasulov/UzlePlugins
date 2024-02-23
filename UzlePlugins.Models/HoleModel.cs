using System.Drawing;

namespace UzlePlugins.Models
{
    public class HoleModel
    {
        public int Id { get; set; }
        public string IntersectionPoint { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string HoleType { get; set; }
        public double HoleSize { get; set; }
        public string HoleSourceType { get; set; }
        
        public bool IsHoleCircled { get; set; }
        public double HoleOffset { get; set; }
        public bool IsInsert { get; set; }

        public HoleModel(int id, string point,string name, string description, string holeType, double holeSize, string holeSourceType, bool isHoleCircled, double holeOffset, bool isInsert)
        {
            Id = id;
            IntersectionPoint = point;
            Name = name;
            Description = description;
            HoleType = holeType;
            HoleSize = holeSize;
            HoleSourceType = holeSourceType;
            IsHoleCircled = isHoleCircled;
            HoleOffset = holeOffset;
            IsInsert = isInsert;

        }
    }
}
