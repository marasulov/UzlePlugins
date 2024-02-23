using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UzlePlugins.Settings
{

    public class Circled
    {
        public FamilyNames FamilyNames { get; set; }
        public List<string> FamilyParameters { get; set; }
    }

    public class FamilyNames
    {
        public string WallType { get; set; }
        public string FloorType { get; set; }
    }

    public class FamilyTypes
    {
        public Circled Circled { get; set; }
        public Rectangled Rectangled { get; set; }
    }

    public class Rectangled
    {
        public FamilyNames FamilyNames { get; set; }
        public List<string> FamilyParameters { get; set; }
    }

    public class HoleFamilyTypes
    {
        public FamilyTypes FamilyTypes { get; set; }
    }

}
