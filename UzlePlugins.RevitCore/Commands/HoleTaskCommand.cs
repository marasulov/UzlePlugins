using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using System.Collections.Generic;
using System.Linq;


namespace UzlePlugins.RevitCore.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class HoleTaskCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var uiapp = commandData.Application;
            var uidoc = uiapp.ActiveUIDocument;
            var doc = uidoc.Document;

            var collector = new FilteredElementCollector(doc);

            //var pipe = (Pipe)collector
            //    .OfClass(typeof(Pipe))
            //    .FirstElement();

            // Получаем все элементы труб в проекте
            FilteredElementCollector pipeCollector = new FilteredElementCollector(doc)
                .OfClass(typeof(Pipe))
                .WhereElementIsNotElementType();

            TaskDialog.Show("dev", $"Found {pipeCollector.Count()} pipes");

            FilteredElementCollector ductCollector = new FilteredElementCollector(doc)
                .OfClass(typeof(Duct))
                .WhereElementIsNotElementType();

            TaskDialog.Show("dev", $"Found {ductCollector.Count()} ducts");

            //FilteredElementCollector CableTrayCollector = new FilteredElementCollector(doc)
            //    .OfClass(typeof(CableTray))
            //    .WhereElementIsNotElementType();

            //TaskDialog.Show("dev", $"Found {CableTrayCollector.Count()} pipes");

            List<ReferenceWithContext> intersections = new();
            
            foreach (var pipe in pipeCollector)
            {
                var pipeLocation = (LocationCurve)pipe.Location;

                var pipeLine = (Line)pipeLocation.Curve;

                var referenceIntersector = new ReferenceIntersector(new ElementClassFilter(typeof(Wall)), FindReferenceTarget.Element, (View3D)uidoc.ActiveGraphicalView)
                {
                    FindReferencesInRevitLinks = true
                };

                var origin = pipeLine.GetEndPoint(0);

                List<ReferenceWithContext> curIntersections = referenceIntersector.Find(origin, pipeLine.Direction)
                    .Where(x => x.Proximity <= pipeLine.Length)
                    .Distinct(new ReferenceWithContextElementEqualityComparer())
                    .ToList();

                if (curIntersections.Count > 0)
                {
                    intersections?.AddRange(curIntersections);
                }

                
            }

            TaskDialog.Show("dev", $"Found {intersections?.Count} intersections");

            return Result.Succeeded;

        }
    }

    public class ReferenceWithContextElementEqualityComparer : IEqualityComparer<ReferenceWithContext>
    {
        public bool Equals(ReferenceWithContext x, ReferenceWithContext y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(null, x)) return false;
            if (ReferenceEquals(null, y)) return false;

            var xReference = x.GetReference();

            var yReference = y.GetReference();

            return xReference.LinkedElementId == yReference.LinkedElementId
                       && xReference.ElementId == yReference.ElementId;
        }

        public int GetHashCode(ReferenceWithContext obj)
        {
            var reference = obj.GetReference();

            unchecked
            {
                return (reference.LinkedElementId.GetHashCode() * 397) ^ reference.ElementId.GetHashCode();
            }
        }
    }
}
