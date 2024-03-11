using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using System.Collections.Generic;
using Autodesk.Revit.DB.Mechanical;
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
        public List<HoleFamilyModel> HolesProps { get; set; } = new ();

        public void GetHoles(XYZ point, UIDocument uidoc)
        {
            int i = 0;

            if (IntersectedSourceElement == null) return;

            if (_doc.GetElement(IntersectedSourceElement.ElementId) is not RevitLinkInstance link) return;

            var ldoc = link.GetLinkDocument();

            Element el = ldoc.GetElement(IntersectedSourceElement.LinkedElementId) as Wall;
            
            var holeFamily = GetHoleProps(point, el, IntersectingElement, uidoc);
            HolesProps.Add(holeFamily);
        }

        public HoleFamilyModel GetHoleProps(XYZ intPoint, Element sourceElement, Element element, UIDocument uidoc)
        {

            var elType = element.GetType();
            double pipeSize = default;
            string elName = default;
            switch (elType.Name)
            {
                //TODO 
                case "Duct":
                {
                    var intersectElement = element as Duct;
                    elName = intersectElement.MEPSystem.Name;
                    pipeSize = intersectElement.get_Parameter(BuiltInParameter.RBS_CALCULATED_SIZE).AsDouble();
                    break;
                }
                case "Pipe":
                {
                    var intersectElement = element as Pipe;
                    elName = intersectElement.PipeType.Name;
                    pipeSize = intersectElement.get_Parameter(BuiltInParameter.RBS_PIPE_OUTER_DIAMETER).AsDouble();
                    break;
                }
            }

            var sourceElementThickness = (sourceElement as Wall).Width;
            //(element as Floor).get_Parameter();

            return new HoleFamilyModel(uidoc, intPoint, element, elName,elType.Name, pipeSize,
                sourceElement.Name, sourceElementThickness,true, 20, true,sourceElementThickness);
        }
    }
}
