using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UzlePlugins.Models;
using UzlePlugins.RevitCore.Models;
using UzlePlugins.RevitCore.Services;
using UzlePlugins.RevitCore.Settings;
using UzlePlugins.Views;
using UzlePlugins.Vm;

namespace UzlePlugins.RevitCore.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class TestHoleTaskCommand : IExternalCommand
    {
        public static IEnumerable<ExternalFileReference> GetLinkedFileReferences(Document _document)
        {
            var collector = new FilteredElementCollector(
                _document);

            var linkedElements = collector
                .OfClass(typeof(RevitLinkType))
                .Select(x => x.GetExternalFileReference())
                .ToList();

            return linkedElements;
        }
        public static IEnumerable<Document> GetLinkedDocuments(Document _document)
        {
            var linkedfiles = GetLinkedFileReferences(
                _document);

            var linkedFileNames = linkedfiles
                .Select(x => ModelPathUtils
                    .ConvertModelPathToUserVisiblePath(
                        x.GetAbsolutePath())).ToList();

            return _document.Application.Documents
                .Cast<Document>()
                .Where(doc => linkedFileNames.Any(
                    fileName => doc.PathName.Equals(fileName)));
        }


        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var uiapp = commandData.Application;
            var uidoc = uiapp.ActiveUIDocument;
            var doc = uidoc.Document;
            var offset = 1.1;
            double pipeDiametrForFilter = 50;

            var settingsReader = new SettingsReader();
            var recFamilyNames = settingsReader.GetFamilyTypes().FamilyTypes.Rectangled.FamilyNames;
            var circledFamilyNames = settingsReader.GetFamilyTypes().FamilyTypes.Circled.FamilyNames;
            var linkedDocs = GetLinkedDocuments(doc);

            var familyNames = new List<string>()
            {
                recFamilyNames.FloorType, recFamilyNames.WallType, circledFamilyNames.FloorType,
                circledFamilyNames.WallType
            };

            List<FamilyInstance> familyInstances = new List<FamilyInstance>();
            foreach (var familyName in familyNames)
            {
                var result = new FilteredElementCollector(doc)
                    .OfClass(typeof(FamilyInstance))
                    .Cast<FamilyInstance>()
                    .Where(x => x.get_Parameter(BuiltInParameter.ELEM_FAMILY_PARAM).AsValueString() == familyName);
                familyInstances.AddRange(result);
            }

            Debug.Print($"{familyInstances.Count} exist");


            using (TransactionGroup transactionGroup = new TransactionGroup(doc))
            {
                var collector3dView = new FilteredElementCollector(doc);
                Func<View3D, bool> isNotTemplate = v3 => !(v3.IsTemplate);
                View3D view3D = collector3dView
                    .OfClass(typeof(View3D))
                    .Cast<View3D>()
                    .First(isNotTemplate);

                //исходные точки
                var newIntersections = new List<XYZ>();
                var familyPoints = familyInstances.Where(w => w.Location is LocationPoint)
                    .Select(f => f.Location as LocationPoint)
                    .Select(loc => loc.Point).ToList();

                
                

                var smallestDiametr = UnitUtils.ConvertToInternalUnits(pipeDiametrForFilter, UnitTypeId.Millimeters);
                var pipeCollector = new FilteredElementCollector(doc).OfClass(typeof(Pipe)).Cast<Pipe>()
                    .Where(w => w.Diameter > smallestDiametr);

                var ductCollector = new FilteredElementCollector(doc).OfClass(typeof(Duct)).Cast<Duct>()
                    .Where(w => w.Width > smallestDiametr);

                //Todo : надо сделать чтобы созлись список моделей которые содержат инфу о точках пересечения

                List<HoleFamilyModel<Wall>> wallHoles = new List<HoleFamilyModel<Wall>>();

                //из коллекции труб создаем отверстия
                foreach (Element pipeElement in pipeCollector)
                {
                    ReferenceIntersectionFinder refFinder = new ReferenceIntersectionFinder(doc, pipeElement, view3D);

                    refFinder.GetStructuralReferences(BuiltInCategory.OST_Walls);

                    if (refFinder.WallReferences.Count > 0)
                    {
                        var points = refFinder.GetIntersectionsPoints(refFinder.WallReferences);
                        newIntersections.AddRange(points);
                        int i = 0;
                        var holes = new List<HoleFamilyModel<Wall>>();
                        foreach (var point in points)
                        {
                            var sourceElement = refFinder.WallReferences[i];
                            if (sourceElement == null) continue;
                            var holeFiller = new HolePropertiesFiller(doc, pipeElement, sourceElement, point);
                            holeFiller.GetHoles(point);
                            holes = holeFiller.WallHoles;
                        }
                        wallHoles.AddRange(holes);
                    }

                    //Debug.Print($"references {refFinder.WallReferences.Count}-{wallHoles.Count}");

                    refFinder.GetStructuralReferences(BuiltInCategory.OST_Floors);
                    if (refFinder.FloorReferences.Count > 0)
                    {
                        newIntersections.AddRange(refFinder.GetIntersectionsPoints(refFinder.FloorReferences));
                    }

                }

                var intPoints = newIntersections.Intersect(familyPoints, new XYZComparer()).ToArray();
                var newPoints = newIntersections.Except(familyPoints, new XYZComparer()).ToArray(); // newIntersections.Except(familyPoints)
                var deletedPoints = familyPoints.Except(newIntersections, new XYZComparer()).ToArray(); // familyPoints.Except(newIntersections)

                List<HoleModel> actualHoles = new();
                List<HoleModel> outdatedHoles = new();
                List<HoleModel> newHoles = new();


                var j = 0;
                foreach (var hole in wallHoles)
                {
                    var holeModel = new HoleModel(
                           j,
                           hole.IntersectionPoint.ToString(),
                           hole.IntersectingElementName,
                           hole.IntersectedSourceType,
                           hole.IntersectingElementType,
                           hole.IntersectingElementTypeSize,
                           "", hole.IsHoleRectangled, hole.HoleOffset, hole.IsInsert);

                    actualHoles.Add(holeModel);
                    j++;
                }

                Debug.Print($"{newIntersections.Count} точек {wallHoles.Count} отверстий");

                HolesVm holesVm = new HolesVm(actualHoles, outdatedHoles, newHoles);
                HoleTaskView view = new HoleTaskView(holesVm);
                view.ShowDialog();

                if (!holesVm.ButtonClicked) return Result.Cancelled;

                var selectedHoles = holesVm.Holes.Where(x => x.IsInsert);
                Debug.Print($"для печати {selectedHoles.Count().ToString()}");

                transactionGroup.Start("Hole trask for walls with pipes");

                using (Transaction t = new Transaction(doc))
                {
                    t.Start("Hole task for walls and floors with pipes");

                    FamilyTypeFinder familyTypeFinder = new FamilyTypeFinder();
                    familyTypeFinder.GetFamilyType(BuiltInCategory.OST_Walls, true);

                    var wallsFamilyName = familyTypeFinder.FamilyName;
                    var wallsFamilyType = familyTypeFinder.FamilyParameters;

                    FamilySymbol symbol = familyTypeFinder.GetFamilySymbolToPlace(doc, wallsFamilyName);

                    foreach (Element pipeElement in pipeCollector)
                    {
                        //Reference r = new Reference(pipeElement);
                        ReferenceIntersectionFinder refFinder = new ReferenceIntersectionFinder(doc, pipeElement, view3D);
                        FamilyInserter inserter = new FamilyInserter(symbol, wallsFamilyType, refFinder);
                        inserter.InsertFamily(doc, pipeElement, offset, BuiltInCategory.OST_Walls, symbol, true);

                    }


                    familyTypeFinder.GetFamilyType(BuiltInCategory.OST_Floors, true);
                    var floorsFamilyName = familyTypeFinder.FamilyName;
                    var floorsFamilyType = familyTypeFinder.FamilyParameters;
                    symbol = familyTypeFinder.GetFamilySymbolToPlace(doc, floorsFamilyName);
                    if (symbol != null)
                        foreach (Element pipeElement in pipeCollector)
                        {
                            //InsertFamily(doc, pipeElement, view3D, 1, BuiltInCategory.OST_Floors, symbol, true);
                            //Reference r = new Reference(pipeElement);
                            ReferenceIntersectionFinder refFinder = new ReferenceIntersectionFinder(doc, pipeElement, view3D);
                            FamilyInserter inserter = new FamilyInserter(symbol, floorsFamilyType, refFinder);
                            inserter.InsertFamily(doc, pipeElement, offset, BuiltInCategory.OST_Floors, symbol, true);

                        }

                    t.Commit();
                }
                //using (Transaction t = new Transaction(doc))
                //{
                //    t.Start("Hole task for walls and floors with ducts");

                //    foreach (Element ductElement in ductCollector)
                //    {

                //        InsertFamily(doc, ductElement, view3D, 1, BuiltInCategory.OST_Walls, true);
                //        InsertFamily(doc, ductElement, view3D, 1, BuiltInCategory.OST_Floors, true);

                //    }

                //    t.Commit();
                //}

                transactionGroup.Assimilate();
            }

            return Result.Succeeded;
        }


        //private void GetObjectsByFamilyLocations()
        //{
        //    // поиск свойств стены по точке расположения семейства
        //    foreach (var familyInstance in familyInstances)
        //    {
               

        //        Debug.Print($"{containFounds.Count}");
        //    }
        //}

        //private Element GetObjectByFamilyLocation(FamilyInstance familyInstance, Document doc)
        //{
        //    LocationPoint basePnt = familyInstance.Location as LocationPoint;

        //    BoundingBoxContainsPointFilter containsPointFilter =
        //        new BoundingBoxContainsPointFilter(basePnt.Point); // inverted filter

        //    var collector = new FilteredElementCollector(doc);
        //    IList<Element> containFounds =
        //        collector.OfClass(typeof(Wall)).WherePasses(containsPointFilter).ToElements();

        //    Element element;

        //    foreach (var containFound in containFounds)
        //    {
        //        element = containFound as Wall;

        //        Debug.Print($"{element.WallType.Name} тип стены");
        //    }
        //}

        //private void InsertFamily(Document doc, Element ductElement, View3D view3D, double offset,
        //    BuiltInCategory builtInCategory, bool isCircled)
        //{
        //    familyTypeFinder.GetFamilyType(BuiltInCategory.OST_Floors, true);
        //    var floorsFamilyName = familyTypeFinder.FamilyName;
        //    var floorsFamilyType = familyTypeFinder.FamilyParameters;
        //    symbol = familyTypeFinder.GetFamilySymbolToPlace(doc, floorsFamilyName);
        //    if (symbol != null)
        //        foreach (Element pipeElement in pipeCollector)
        //        {
        //            //InsertFamily(doc, pipeElement, view3D, 1, BuiltInCategory.OST_Floors, symbol, true);
        //            Reference r = new Reference(pipeElement);
        //            ReferenceIntersectionFinder refFinder = new ReferenceIntersectionFinder(doc, r, view3D);
        //            FamilyInserter inserter = new FamilyInserter(symbol, floorsFamilyType, refFinder);
        //            inserter.InsertFamily(doc, pipeElement, 1, BuiltInCategory.OST_Floors, symbol, true);

        //        }
        //}


        //private void InsertFamily(Document doc, Element pipeElement, View3D view3D, double offset, BuiltInCategory builtInCategory, FamilySymbol symbol, bool isCircled)
        //{
        //    Reference r = new Reference(pipeElement);
        //    ReferenceIntersectionFinder refFinder = new ReferenceIntersectionFinder(doc, r, view3D);

        //    var elementType = pipeElement.GetType().Name;
        //    refFinder.GetStructuralReferences(builtInCategory);
        //    List<XYZ> intersectPoints = new();

        //    if (builtInCategory == BuiltInCategory.OST_Floors)
        //    {
        //        intersectPoints = refFinder.GetIntersectionsPoints(refFinder.FloorReferences);
        //    }
        //    else
        //    {
        //        intersectPoints = refFinder.GetIntersectionsPoints(refFinder.WallReferences);
        //    }

        //    foreach (var intersectPoint in intersectPoints)
        //    {
        //        FamilyInstance fi = doc.Create.NewFamilyInstance(intersectPoint, symbol, StructuralType.NonStructural);
        //        var basisY = fi.GetTransform().BasisY;
        //        var angle = basisY.AngleTo(refFinder.Normal);

        //        Line axis = Line.CreateBound(intersectPoint, intersectPoint + XYZ.BasisZ);
        //        ElementTransformUtils.RotateElement(doc, fi.Id, axis, -angle);
        //        var parameters = fi.GetOrderedParameters();
        //        if (isCircled)
        //        {
        //            SetCircledFamilyParameter(parameters, "Depth", "Diameter", pipeElement, offset, refFinder);
        //        }
        //        else
        //        {
        //            SetRectFamilyParameter(parameters, "Depth", "Height", "Width", pipeElement, offset,
        //                refFinder);
        //        }
        //    }
        //    //}

        //}
        //TODO 
        private void SetCircledFamilyParameter(IList<Parameter> parameters, string familyLength, string familyWidth, Element element, double offset, ReferenceIntersectionFinder refFinder)
        {
            foreach (var parameter in parameters)
            {

                if (parameter.Definition.Name == familyWidth)
                {
                    var outerDiameter = (element as Pipe).get_Parameter(BuiltInParameter.RBS_PIPE_OUTER_DIAMETER);
                    parameter.Set(outerDiameter.AsDouble() + offset);
                }

                if (parameter.Definition.Name != familyLength) continue;
                parameter.Set(refFinder.Thickness + (refFinder.Thickness * offset));


            }
        }

        private void SetRectFamilyParameter(IList<Parameter> parameters, string familyLength, string familyHeight, string familyWidth, Element element, double offset, ReferenceIntersectionFinder refFinder)
        {
            foreach (var parameter in parameters)
            {

                if (parameter.Definition.Name == familyWidth)
                {
                    var outerDiameter = (element as Pipe).get_Parameter(BuiltInParameter.RBS_PIPE_OUTER_DIAMETER);

                    parameter.Set(outerDiameter.AsDouble() + offset);
                }

                if (parameter.Definition.Name == familyHeight)
                {
                    var outerDiameter = (element as Pipe).get_Parameter(BuiltInParameter.RBS_PIPE_OUTER_DIAMETER);
                    parameter.Set(outerDiameter.AsDouble() + offset);
                }

                if (parameter.Definition.Name != familyLength) continue;
                parameter.Set(refFinder.Thickness + (refFinder.Thickness * offset));


            }
        }


        //public Dictionary<Reference, XYZ>  GetIntersectPoints(
        //    Document doc,
        //    Element intersect)
        //{
        //    // Find a 3D view to use for the 
        //    // ReferenceIntersector constructor.

        //    FilteredElementCollector collector
        //        = new FilteredElementCollector(doc);

        //    Func<View3D, bool> isNotTemplate = v3
        //        => !(v3.IsTemplate);

        //    View3D view3D = collector
        //        .OfClass(typeof(View3D))
        //        .Cast<View3D>()
        //        .First<View3D>(isNotTemplate);

        //    // Use location point as start point for intersector.

        //    LocationCurve lp = intersect.Location as LocationCurve;
        //    XYZ startPoint = lp.Curve.GetEndPoint(0) as XYZ;
        //    XYZ endPoint = lp.Curve.GetEndPoint(1) as XYZ;

        //    // Shoot intersector along element.

        //    XYZ rayDirection = endPoint.Subtract(
        //        startPoint).Normalize();

        //    List<BuiltInCategory> builtInCats
        //        = new List<BuiltInCategory>();

        //    builtInCats.Add(BuiltInCategory.OST_Roofs);
        //    builtInCats.Add(BuiltInCategory.OST_Ceilings);
        //    builtInCats.Add(BuiltInCategory.OST_Floors);
        //    builtInCats.Add(BuiltInCategory.OST_Walls);

        //    ElementMulticategoryFilter intersectFilter
        //        = new ElementMulticategoryFilter(builtInCats);

        //    ReferenceIntersector refIntersector
        //        = new ReferenceIntersector(intersectFilter,
        //            FindReferenceTarget.Element, view3D);

        //    refIntersector.FindReferencesInRevitLinks = true;
        //    //todo
        //    IList<ReferenceWithContext> referencesWithContext
        //        = refIntersector.Find(startPoint,
        //            rayDirection);

        //    List<XYZ> intersectPoints = new List<XYZ>();

        //    IList<Reference> intersectRefs
        //        = new List<Reference>();

        //    Dictionary<Reference, XYZ> dictProvisionForVoidRefs
        //        = new Dictionary<Reference, XYZ>();

        //    FilteredElementCollector a
        //        = new FilteredElementCollector(doc)
        //            .OfClass(typeof(Family));

        //    Family family = a.FirstOrDefault<Element>(e => e.Name.Equals("ОВ1")) as Family;

        //    ReferenceComparer reference1 = new ReferenceComparer();

        //    var newref = referencesWithContext.Distinct(new ReferenceWithContextElementEqualityComparer());

        //    foreach (ReferenceWithContext r in
        //             newref)
        //    {
        //        var intersectPoint = r.GetReference().GlobalPoint;
        //        intersectPoints.Add(intersectPoint);
        //        dictProvisionForVoidRefs.Add(r.GetReference(),
        //            intersectPoint);

        //    }
        //    return dictProvisionForVoidRefs;
        //}

        /// <summary>
        /// Default tolerance used to add fuzz 
        /// for real number equality detection
        /// </summary>
        public const double _eps = 1.0e-9;

        /// <summary>
        /// Predicate to determine whether the given 
        /// real number should be considered equal to
        /// zero, adding fuzz according to the specified 
        /// tolerance
        /// </summary>
        public static bool IsZero(
            double a,
            double tolerance = _eps)
        {
            return tolerance > Math.Abs(a);
        }

        /// <summary>
        /// Predicate to determine whether the two given 
        /// real numbers should be considered equal, adding 
        /// fuzz according to the specified tolerance
        /// </summary>
        public static bool IsEqual(
            double a,
            double b,
            double tolerance = _eps)
        {
            return IsZero(b - a, tolerance);
        }

        /// <summary>
        /// Comparison method for two real numbers
        /// returning 0 if they are to be considered equal,
        /// -1 if the first is smaller and +1 otherwise
        /// </summary>
        public static int Compare(
            double a,
            double b,
            double tolerance = _eps)
        {
            return IsEqual(a, b, tolerance)
                ? 0
                : (a < b ? -1 : 1);
        }

        /// <summary>
        /// Comparison method for two XYZ objects
        /// returning 0 if they are to be considered equal,
        /// -1 if the first is smaller and +1 otherwise
        /// </summary>
        public static int Compare(
            XYZ p,
            XYZ q,
            double tolerance = _eps)
        {
            int d = Compare(p.X, q.X, tolerance);

            if (0 == d)
            {
                d = Compare(p.Y, q.Y, tolerance);

                if (0 == d)
                {
                    d = Compare(p.Z, q.Z, tolerance);
                }
            }
            return d;
        }
    }
    public class XYZComparer : IEqualityComparer<XYZ>
    {
        public bool Equals(XYZ pt1, XYZ pt2) => pt1.IsAlmostEqualTo(pt2);


        public int GetHashCode(XYZ obj)
        {
            return (int)obj.X ^ (int)obj.Y ^ (int)obj.Z;
        }
    }

}
