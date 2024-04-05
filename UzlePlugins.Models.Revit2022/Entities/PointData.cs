using Autodesk.Revit.DB;

namespace UzlePlugins.Models.Revit2022.Entities
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
