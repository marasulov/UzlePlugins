using System.Collections.Generic;
using System.Runtime.Serialization.Configuration;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using UzlePlugins.Contracts;
using UzlePlugins.Models.Revit2022.Entities;

namespace UzlePlugins.Models.Revit2022.Services
{
    public class HolePropertiesFiller
    {
        private readonly Document _doc;

        public HolePropertiesFiller(
            Document doc, 
            Element intersectingElement, 
            Reference intersectedSourceElement, 
            XYZ intersectionPoint)
        {
            _doc = doc;
            IntersectingElement = intersectingElement;
            IntersectedSourceElement = intersectedSourceElement;
            IntersectionPoint = intersectionPoint;
        }

        public Element IntersectingElement { get; set; }
        public Reference IntersectedSourceElement { get; set; }
        public XYZ IntersectionPoint { get; set; }
        public List<HoleFamilyEntity> HolesProps { get; set; } = new();

        public void GetHoles(XYZ point, XYZ normal)
        {
            if (IntersectedSourceElement == null) return;

            if (_doc.GetElement(IntersectedSourceElement.ElementId) is not RevitLinkInstance link) return;

            var ldoc = link.GetLinkDocument();

            Element el = ldoc.GetElement(IntersectedSourceElement.LinkedElementId);
            Element sourceElement = el.GetType().Name switch
            {
                "Wall" => el as Wall,
                "Floor" => el as Floor,
                _ => default
            };

            if (sourceElement == null) return;
            var holeFamily = GetHoleProps(point, sourceElement, IntersectingElement, normal);
            HolesProps.Add(holeFamily);
        }

        

        private HoleFamilyEntity GetHoleProps(
            XYZ intPoint, 
            Element sourceElement, 
            Element element, 
            XYZ normal)
        {

            var elType = element.GetType();
            double pipeSize = default;
            string elName = default;

            double width = default;
            double height = default;
            string shape = default;

            switch (elType.Name)
            {
                //TODO 
                case "Duct":
                    {
                        var intersectElement = element as Duct;
                        elName = intersectElement.MEPSystem.Name;
                        var elShape = intersectElement.DuctType.Shape;
                        shape = elShape.ToString();
                        if (elShape == ConnectorProfileType.Round)
                        {
                            width = intersectElement.Diameter;
                            height = width;
                        }
                        else
                        {
                            width = intersectElement.Width;
                            height = intersectElement.Height;
                        }
                        
                        pipeSize = width;
                        break;
                    }
                case "Pipe":
                    {
                        var intersectElement = element as Pipe;
                        elName = intersectElement.PipeType.Name;
                        shape = intersectElement.PipeType.Shape.ToString();
                        pipeSize = intersectElement.get_Parameter(BuiltInParameter.RBS_PIPE_OUTER_DIAMETER).AsDouble();
                        break;
                    }
            }

            var sourceType = sourceElement.GetType();
            var sourceThickness = sourceType.Name switch
            {
                "Wall" => (sourceElement as Wall).Width,
                "Floor" => (sourceElement as Floor).get_Parameter(BuiltInParameter.FLOOR_ATTR_THICKNESS_PARAM)
                    .AsDouble(),
                _ => default
            };

            var intersectionParams = new IntersectionParameters(intPoint, element,elName,elType.Name, normal,pipeSize,height, width, shape);
            var sourceProps = new SourceParameters(sourceElement.Name, sourceType.Name, sourceThickness);
            var holeFamilyModel = HoleFamilyEntity.CreateInstance(intersectionParams, sourceProps, true, true);

            return holeFamilyModel;
        }

       
    }
}
