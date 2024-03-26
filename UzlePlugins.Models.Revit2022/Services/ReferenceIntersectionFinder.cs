using System;
using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

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
            if (lc == null) return;
            StartPoint = lc.Curve.GetEndPoint(0);
            EndPoint = lc.Curve.GetEndPoint(1);
            Normal = EndPoint.Subtract(StartPoint).Normalize();

            _curve = lc.Curve;
        }

        public IList<Reference> WallReferences { get; } = new List<Reference>();
        public IList<Reference> FloorReferences { get; } = new List<Reference>();

        public double Thickness { get; private set; }

        public XYZ StartPoint;
        public XYZ EndPoint;
        public XYZ Normal { get; set; }


        //public List<HoleFamilyModel<Wall>> HolesProps { get; } = new List<HoleFamilyModel<Wall>>();
        //public List<HoleFamilyModel<Floor>> FloorHoles { get; } = new List<HoleFamilyModel<Floor>>();

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
            var intersectPoints = new List<XYZ>();

            if (references.Count <= 0) return intersectPoints;

            var firstFaceRef = references[0];
            var secondFaceRef = references[0 + 1];

            if (firstFaceRef.ElementId != secondFaceRef.ElementId) return intersectPoints;
            Thickness = firstFaceRef.GlobalPoint.DistanceTo(secondFaceRef.GlobalPoint);
            var intPoint = new XYZ(
                (firstFaceRef.GlobalPoint.X + secondFaceRef.GlobalPoint.X) / 2,
                (firstFaceRef.GlobalPoint.Y + secondFaceRef.GlobalPoint.Y) / 2,
                firstFaceRef.GlobalPoint.Z);
            intersectPoints.Add(intPoint);

            return intersectPoints;
        }

        public void GetStructuralReferences(BuiltInCategory builtInCategory)
        {
            var tempReferences = GetAllReferences(builtInCategory);
            if (tempReferences.Count <= 0) return;
            foreach (var r in tempReferences)
            {
                if (r == null) return;

                if (_document.GetElement(r.ElementId) is not RevitLinkInstance link) return;

                var ldoc = link.GetLinkDocument();

                Element el = ldoc.GetElement(r.LinkedElementId) as HostObject;
                double structuralParameter;

                Dictionary<BuiltInCategory, BuiltInParameter> builtInParameters =
                    new Dictionary<BuiltInCategory, BuiltInParameter>()
                    {
                        {BuiltInCategory.OST_Walls,  BuiltInParameter.WALL_STRUCTURAL_SIGNIFICANT},
                        {BuiltInCategory.OST_Floors, BuiltInParameter.FLOOR_PARAM_IS_STRUCTURAL}

                    };


                //structuralParameter = el.get_Parameter(builtInParameters[builtInCategory])
                //    .AsInteger();
                //if (structuralParameter != 1) continue;
                //WallReferences.Add(r);

                switch (builtInCategory)
                {
                    case BuiltInCategory.OST_Walls:
                        {
                            structuralParameter = el.get_Parameter(BuiltInParameter.WALL_STRUCTURAL_SIGNIFICANT)
                                .AsInteger();
                            if (structuralParameter != 1) continue;
                            WallReferences.Add(r);

                            break;
                        }
                    case BuiltInCategory.OST_Floors:
                        {
                            structuralParameter =
                                el.get_Parameter(BuiltInParameter.FLOOR_PARAM_IS_STRUCTURAL).AsInteger();
                            if (structuralParameter != 1) continue;

                            FloorReferences.Add(r);
                            break;
                        }
                }
            }
        }
    }
}
