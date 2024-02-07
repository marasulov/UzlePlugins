using Autodesk.Revit.DB;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB.Structure;

namespace UzlePlugins.RevitCore.Services
{
    public class ReferenceIntersectionFinder
    {
        private readonly Document _document;

        public ReferenceIntersectionFinder(Document document, Reference reference)
        {
            _document = document;

            Element pipeElem = _document.GetElement(reference);
            LocationCurve lc = pipeElem.Location as LocationCurve;

            
            StartPoint = lc.Curve.GetEndPoint(0);
            EndPoint = lc.Curve.GetEndPoint(1);
            Normal = EndPoint.Subtract(StartPoint).Normalize();

            Curve curve = lc.Curve;
            //ReferenceComparer referenceComparer = new ReferenceComparer();

            ElementFilter filter = new ElementCategoryFilter(BuiltInCategory.OST_Walls);
            ElementFilter filter2 = new StructuralInstanceUsageFilter(StructuralInstanceUsage.Wall);
            var fi = new LogicalAndFilter(filter, filter2);
            //().Where<Wall>((Func<Wall, bool>)(w=>w.StructuralUsage == StructuralWallUsage.Bearing));
            
            var collector = new FilteredElementCollector(_document);
                
            List<Wall> walls = ((IEnumerable) new FilteredElementCollector(document).OfCategoryId(new ElementId( -2000011)).OfClass(typeof (Wall)).WhereElementIsNotElementType()).Cast<Wall>().Where<Wall>((Func<Wall, bool>) (w => w.CurtainGrid == null)).Where<Wall>((Func<Wall, bool>) (w => worksetList.FirstOrDefault<Workset>((Func<Workset, bool>) (ws => WorksetId.op_Equality(((WorksetPreview) ws).Id, ((Element) w).WorksetId))) == null)).Where<Wall>((Func<Wall, bool>) (w => ((Element) w.WallType)[(BuiltInParameter) -1010109] != null)).Where<Wall>((Func<Wall, bool>) (w => ((Element) w.WallType)[(BuiltInParameter) -1010109].AsString() != "Отделка стен")).ToList<Wall>();

            Func<View3D, bool> isNotTemplate = v3 => !(v3.IsTemplate);
            View3D view3D = collector
                .OfClass(typeof(View3D))
                .Cast<View3D>()
                .First<View3D>(isNotTemplate);

            ReferenceIntersector refIntersector
                = new ReferenceIntersector(filter, FindReferenceTarget.Element, view3D);
            
            //refIntersector.SetFilter(filter2);

            refIntersector.FindReferencesInRevitLinks = true;
            IList<ReferenceWithContext> referenceWithContext
                = refIntersector.Find(
                    StartPoint,
                    Normal);

            References = referenceWithContext
                    .Select(p => p.GetReference())
                    //.Distinct(referenceComparer)
                    .Where(p => p.GlobalPoint.DistanceTo(
                        curve.GetEndPoint(0)) < curve.Length)
                    .ToList();

            

        }

        public double Thickness { get; private set; }

        public XYZ StartPoint;
        public XYZ EndPoint;

        public XYZ Normal { get; set; }

        public IList<Reference> References { get; private set; }

        public List<XYZ> GetIntersectionsPoint()
        {
            List<XYZ> intersectPoints = new List<XYZ>();

            for (int i = 0; i < References.Count; i += 2)
            {

                var firstFaceRef = References[i] as Reference;
                var secondFaceRef = References[i + 1] as Reference;

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


    }
}
