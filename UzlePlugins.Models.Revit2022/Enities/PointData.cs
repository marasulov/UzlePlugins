using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;

namespace UzlePlugins.Models.Revit2022.Enities
{
    public class PointData
    {
        public ElementId Id { get; set; }
        public XYZ Point { get; set; }

        public PointData(ElementId id, XYZ point)
        {
            Id = id;
            Point = point;
        }
    }
}
