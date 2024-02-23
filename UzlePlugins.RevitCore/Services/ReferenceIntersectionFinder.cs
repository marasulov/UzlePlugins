using Autodesk.Revit.DB;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Autodesk.Revit.DB.Structure;
using UzlePlugins.RevitCore.Models;
using System.Runtime.Serialization.Configuration;
using Autodesk.Revit.DB.Plumbing;

namespace UzlePlugins.RevitCore.Services
{
    public class ReferenceIntersectionFinder
    {
        private readonly Document _document;
        private readonly Curve _curve;
        private readonly View3D _view3D;
        private readonly Element _intersectingElement;

        public ReferenceIntersectionFinder(Document document, Element intersectingElement, View3D view3D)
        {
            _intersectingElement = intersectingElement;
            Reference reference = new Reference(intersectingElement);
            _document = document;
            _view3D = view3D;
            Element element = _document.GetElement(reference);
            LocationCurve lc = element.Location as LocationCurve;
            if(lc == null) return;
            StartPoint = lc.Curve.GetEndPoint(0);
            EndPoint = lc.Curve.GetEndPoint(1);
            Normal = EndPoint.Subtract(StartPoint).Normalize();

            _curve = lc.Curve;
        }

        public double Thickness { get; private set; }

        public XYZ StartPoint;
        public XYZ EndPoint;


        public XYZ Normal { get; set; }

        public IList<Reference> WallReferences { get; } = new List<Reference>();
        public IList<Reference> FloorReferences { get; } = new List<Reference>();
        public List<HoleFamilyModel<Wall>> WallHoles { get; } = new List<HoleFamilyModel<Wall>>();
        public List<HoleFamilyModel<Floor>> FloorHoles { get; } = new List<HoleFamilyModel<Floor>>();

         private List<Reference> GetAllReferences(BuiltInCategory builtInCategory)
        {
            ElementFilter filter = new ElementCategoryFilter(builtInCategory);

            ReferenceIntersector refIntersector
                = new ReferenceIntersector(filter, FindReferenceTarget.Element, _view3D);

            refIntersector.FindReferencesInRevitLinks = true;
            IList<ReferenceWithContext> referenceWithContext
                = refIntersector.Find(
                    StartPoint,
                    Normal);

            var references = referenceWithContext
                .Select(p => p.GetReference())
                .Where(p => p.GlobalPoint.DistanceTo(
                    _curve.GetEndPoint(0)) < _curve.Length)
                .ToList();

            return references;
        }

        public List<XYZ> GetIntersectionsPoints(IList<Reference> references)
        {
            List<XYZ> intersectPoints = new List<XYZ>();

            if (references.Count <= 0) return intersectPoints;

            var firstFaceRef = references[0];
            var secondFaceRef = references[0 + 1];

            if (firstFaceRef.ElementId == secondFaceRef.ElementId)
            {
                Thickness = firstFaceRef.GlobalPoint.DistanceTo(secondFaceRef.GlobalPoint);
                var intPoint = new XYZ(
                    (firstFaceRef.GlobalPoint.X + secondFaceRef.GlobalPoint.X) / 2,
                    (firstFaceRef.GlobalPoint.Y + secondFaceRef.GlobalPoint.Y) / 2,
                    firstFaceRef.GlobalPoint.Z);
                intersectPoints.Add(intPoint);
                
            }

            return intersectPoints;
        }



        public XYZ GetIntersectionPoint(IList<Reference> references)
        {

            var intPoint = new XYZ();
            if (references.Count <= 0) ;

            var firstFaceRef = references[0];
            var secondFaceRef = references[0 + 1];

            if (firstFaceRef.ElementId == secondFaceRef.ElementId)
            {
                Thickness = firstFaceRef.GlobalPoint.DistanceTo(secondFaceRef.GlobalPoint);
                intPoint = new XYZ(
                    (firstFaceRef.GlobalPoint.X + secondFaceRef.GlobalPoint.X) / 2,
                    (firstFaceRef.GlobalPoint.Y + secondFaceRef.GlobalPoint.Y) / 2,
                    firstFaceRef.GlobalPoint.Z);

            }

            return intPoint;
        }

        public void GetStructuralReferences(BuiltInCategory builtInCategory)
        {
            var tempReferences = GetAllReferences(builtInCategory);
            if (tempReferences.Count > 0)
            {
                foreach (var r in tempReferences)
                {
                    if (r == null) return;

                    if (_document.GetElement(r.ElementId) is not RevitLinkInstance link) return;

                    var ldoc = link.GetLinkDocument();

                    Element el;
                    string structuralParameter;

                    if (builtInCategory == BuiltInCategory.OST_Walls)
                    {
                        el = ldoc.GetElement(r.LinkedElementId) as Wall;
                        structuralParameter = el.get_Parameter(BuiltInParameter.WALL_STRUCTURAL_SIGNIFICANT)
                            .AsValueString();
                        if (structuralParameter != "Yes") continue;
                        WallReferences.Add(r);

                    }

                    if (builtInCategory == BuiltInCategory.OST_Floors)
                    {
                        el = ldoc.GetElement(r.LinkedElementId) as Floor;
                        structuralParameter =
                            el.get_Parameter(BuiltInParameter.FLOOR_PARAM_IS_STRUCTURAL).AsValueString();
                        if (structuralParameter != "Yes") continue;

                        FloorReferences.Add(r);
                    }
                }
            }
        }

        public void GetHoles()
        {
            if (WallReferences.Count <= 0) return;
            var points = GetIntersectionsPoints(WallReferences);
                
            int i = 0;
            foreach (var point in points)
            {
                var sourceElement = WallReferences[i];
                if (sourceElement == null) continue;

                if (_document.GetElement(sourceElement.ElementId) is not RevitLinkInstance link) continue;

                var ldoc = link.GetLinkDocument();

                Element el = ldoc.GetElement(sourceElement.LinkedElementId) as Wall;

                var holeFamily = GetHoleProps(point, el, _intersectingElement);
                WallHoles.Add(holeFamily);
                i++;
            }
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
