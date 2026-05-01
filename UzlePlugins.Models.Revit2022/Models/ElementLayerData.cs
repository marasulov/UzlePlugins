using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UzlePlugins.Models.Revit2022.Models
{
    public class ElementLayerData
    {
        public ElementId Id { get; set; }
        public double StartDist { get; set; }
        public double EndDist { get; set; }
        public double Thickness { get; set; }
        public Reference FirstRef { get; set; }
        public Reference LastRef { get; set; }
    }
}
