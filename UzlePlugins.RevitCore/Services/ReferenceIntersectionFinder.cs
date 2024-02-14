using Autodesk.Revit.DB;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Autodesk.Revit.DB.Structure;

namespace UzlePlugins.RevitCore.Services
{
    public class ReferenceIntersectionFinder
    {
        private readonly Document _document;
        private readonly Curve _curve;
        private readonly View3D _view3D;

        public ReferenceIntersectionFinder(Document document, Reference reference,View3D view3D)
        {
            _document = document;
            _view3D = view3D;
            Element pipeElem = _document.GetElement(reference);
            LocationCurve lc = pipeElem.Location as LocationCurve;

            StartPoint = lc.Curve.GetEndPoint(0);
            EndPoint = lc.Curve.GetEndPoint(1);
            Normal = EndPoint.Subtract(StartPoint).Normalize();

            _curve = lc.Curve;
        }


        public void GetReferences(BuiltInCategory builtInCategory)
        {
            ElementFilter filter = new ElementCategoryFilter(builtInCategory);

            ReferenceIntersector refIntersector
                = new ReferenceIntersector(filter, FindReferenceTarget.Element, _view3D);

            refIntersector.FindReferencesInRevitLinks = true;
            IList<ReferenceWithContext> referenceWithContext
                = refIntersector.Find(
                    StartPoint,
                    Normal);

            var tempReferences = referenceWithContext
                .Select(p => p.GetReference())
                //.Distinct(referenceComparer)
                .Where(p => p.GlobalPoint.DistanceTo(
                    _curve.GetEndPoint(0)) < _curve.Length)
                .ToList();

            foreach (var r in tempReferences)
            {
                if (r == null) return;

                if (_document.GetElement(r.ElementId) is not RevitLinkInstance link) return;
                
                var ldoc =   link.GetLinkDocument();

                Element el;
                string structuralParameter;
                if (builtInCategory== BuiltInCategory.OST_Walls)
                {
                   el = ldoc.GetElement(r.LinkedElementId) as Wall;
                   structuralParameter = el.get_Parameter(BuiltInParameter.WALL_STRUCTURAL_SIGNIFICANT).AsValueString();
                   if (structuralParameter != "Yes") continue;
                   Debug.Print(structuralParameter);
                   WallReferences.Add(r);
                }
                if(builtInCategory == BuiltInCategory.OST_Floors)
                {
                    el = ldoc.GetElement(r.LinkedElementId) as Floor;
                    structuralParameter = el.get_Parameter(BuiltInParameter.FLOOR_PARAM_IS_STRUCTURAL).AsValueString();
                    if (structuralParameter != "Yes") continue;
                    Debug.Print(structuralParameter);
                    FloorReferences.Add(r);
                }
            }
        }

        public double Thickness { get; private set; }

        public XYZ StartPoint;
        public XYZ EndPoint;


        public XYZ Normal { get; set; }

        public IList<Reference> WallReferences { get; } = new List<Reference>();
        public IList<Reference> FloorReferences { get; } = new List<Reference>();

        public List<XYZ> GetIntersectionsPoint()
        {
            List<XYZ> intersectPoints = new List<XYZ>();

            if(WallReferences.Count<=0) return intersectPoints;

            for (int i = 0; i < WallReferences.Count; i += 2)
            {

                var firstFaceRef = WallReferences[i] as Reference;
                var secondFaceRef = WallReferences[i + 1] as Reference;

                if (firstFaceRef.ElementId == secondFaceRef.ElementId)
                {
                    Thickness = firstFaceRef.GlobalPoint.DistanceTo(secondFaceRef.GlobalPoint);
                    intersectPoints.Add(new XYZ(
                        (firstFaceRef.GlobalPoint.X + secondFaceRef.GlobalPoint.X) / 2,
                        (firstFaceRef.GlobalPoint.Y + secondFaceRef.GlobalPoint.Y) / 2,
                        firstFaceRef.GlobalPoint.Z));

                }

            }

            return intersectPoints;
        }

        public List<XYZ> GetIntersectionsPointWithFloors()
        {
            List<XYZ> intersectPoints = new List<XYZ>();

            if(FloorReferences.Count<=0 && FloorReferences.Count%2!=0) return intersectPoints;
            bool isDiv = FloorReferences.Count % 2 == 0;
            if (!isDiv)
            {
                for (int i = 0; i < FloorReferences.Count; i++)
                {

                    var firstFaceRef = FloorReferences[i] as Reference;
                    //var secondFaceRef = FloorReferences[i + 1] as Reference;



                    intersectPoints.Add(new XYZ(
                        (firstFaceRef.GlobalPoint.X + Thickness / 2),
                        (firstFaceRef.GlobalPoint.Y + Thickness / 2),
                        firstFaceRef.GlobalPoint.Z + Thickness));


                }

                return intersectPoints;
            }
            for (int i = 0; i < FloorReferences.Count; i += 2)
            {

                var firstFaceRef = FloorReferences[i] as Reference;
                var secondFaceRef = FloorReferences[i + 1] as Reference;

                if (firstFaceRef.ElementId == secondFaceRef.ElementId)
                {
                    Thickness = firstFaceRef.GlobalPoint.DistanceTo(secondFaceRef.GlobalPoint);
                    intersectPoints.Add(new XYZ(
                        (firstFaceRef.GlobalPoint.X + secondFaceRef.GlobalPoint.X) / 2,
                        (firstFaceRef.GlobalPoint.Y + secondFaceRef.GlobalPoint.Y) / 2,
                        firstFaceRef.GlobalPoint.Z + Thickness));

                }



            }

            return intersectPoints;
        }

    }
}
