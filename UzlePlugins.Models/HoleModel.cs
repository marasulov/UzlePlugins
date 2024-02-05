using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UzlePlugins.Models
{
    public class HoleModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string HoleType { get; set; }
        public double HoleSize { get; set; }
        public string HoleSourceType { get; set; }
        public string HoleSourceMaterial { get; set; }
        public bool IsHoleCircled { get; set; }
        public double HoleOffset { get; set; }
        public bool IsInsert { get; set; }

        public HoleModel(string name, string description, string holeType, double holeSize, string holeSourceType, string holeSourceMaterial, bool isHoleCircled, double holeOffset, bool isInsert)
        {
            Name = name;
            Description = description;
            HoleType = holeType;
            HoleSize = holeSize;
            HoleSourceType = holeSourceType;
            HoleSourceMaterial = holeSourceMaterial;
            IsHoleCircled = isHoleCircled;
            HoleOffset = holeOffset;
            IsInsert = isInsert;
        }
    }
}
