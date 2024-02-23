using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using UzlePlugins.RevitCore.Models;


namespace UzlePlugins.RevitCore.Services
{
    public class HolePropertiesFiller
    {
        private readonly Document _doc;

        public HolePropertiesFiller(Document doc, Element intersectingElement, Reference intersectedSourceElement, XYZ intersectionPoint)
        {
            _doc = doc;
            IntersectingElement = intersectingElement;
            IntersectedSourceElement = intersectedSourceElement;
            IntersectionPoint = intersectionPoint;
        }

        public Element IntersectingElement { get; set; }
        public Reference IntersectedSourceElement { get; set; }
        public XYZ IntersectionPoint { get; set; }
        public List<HoleFamilyModel<Wall>> WallHoles { get; set; } = new List<HoleFamilyModel<Wall>>();

        public void GetHoles(XYZ point)
        {
            int i = 0;

            if (IntersectedSourceElement == null) return;

            if (_doc.GetElement(IntersectedSourceElement.ElementId) is not RevitLinkInstance link) return;

            var ldoc = link.GetLinkDocument();

            Element el = ldoc.GetElement(IntersectedSourceElement.LinkedElementId) as Wall;

            var holeFamily = GetHoleProps(point, el, IntersectingElement);
            WallHoles.Add(holeFamily);
        }

        public HoleFamilyModel<Wall> GetHoleProps(XYZ intPoint, Element sourceElement, Element element)
        {
            var elType = element.GetType();
            var pipeElement = element as Pipe;
            var elName = pipeElement.PipeType.Name;
            var pipeSize = pipeElement.get_Parameter(BuiltInParameter.RBS_PIPE_OUTER_DIAMETER).AsDouble();

            return new HoleFamilyModel<Wall>(intPoint, element, elType.Name, elName, pipeSize,
                sourceElement.Name, true, 20, true);
        }
    }
}
