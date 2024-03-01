using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UzlePlugins.Contracts;
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

            // список семейств
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
                            holeFiller.GetHoles(point, uidoc);
                            holes = holeFiller.WallHoles;
                        }
                        wallHoles.AddRange(holes);
                    }

                    refFinder.GetStructuralReferences(BuiltInCategory.OST_Floors);
                    if (refFinder.FloorReferences.Count > 0)
                    {
                        newIntersections.AddRange(refFinder.GetIntersectionsPoints(refFinder.FloorReferences));
                    }
                }

                List<IHoleModel> actualHoles = new();
                List<IHoleModel> newHoles = new();

                var outdatedFamilies = new List<IOutdatedFamily>();
                
                // если есть семейства значит не первый раз вставляются семейства отверстий

                if (familyPoints.Count > 0)
                {
                    var actualPoints = newIntersections.Intersect(familyPoints, new XYZComparer()).ToArray();
                    var newPoints = newIntersections.Except(familyPoints, new XYZComparer()).ToArray(); // newIntersections.Except(familyPoints)
                    var deletedPoints = familyPoints.Except(newIntersections, new XYZComparer()).ToArray(); // familyPoints.Except(newIntersections)
                    var typeOfSource = "Wall";

                    foreach (var actualPoint in actualPoints)
                    {
                        var actualModels = wallHoles.Where(holeFamilyModel =>
                            actualPoint == holeFamilyModel.IntersectionPoint);

                        actualHoles.AddRange(actualModels.Select(actualModel => 
                            new HoleModel(
                                actualModel.IntersectingElement.Id.IntegerValue, 
                                actualModel.IntersectionPoint.ToString(), 
                                actualModel.IntersectingElementName, 
                                actualModel.IntersectedSourceType, 
                                actualModel.IntersectedSourceType, 
                                actualModel.IntersectingElementTypeSize, 
                                actualModel.IntersectedSourceType, 
                                true, 
                                UnitUtils.ConvertToInternalUnits(20, UnitTypeId.Millimeters), 
                                true)));
                    }

                    foreach (var newPoint in newPoints)
                    {
                        var newModels = wallHoles.Where(holeFamilyModel =>
                            newPoint == holeFamilyModel.IntersectionPoint);

                        newHoles.AddRange(
                            newModels.Select(actualModel => 
                                new HoleModel(
                                    actualModel.IntersectingElement.Id.IntegerValue, 
                                    actualModel.IntersectionPoint.ToString(), 
                                    actualModel.IntersectingElementName, 
                                    actualModel.IntersectedSourceType, 
                                    actualModel.IntersectedSourceType, 
                                    actualModel.IntersectingElementTypeSize, 
                                    actualModel.IntersectedSourceType, 
                                    true, 
                                    UnitUtils.ConvertToInternalUnits(20, UnitTypeId.Millimeters), 
                                    true)));
                    }

                    foreach (var deletedPoint in deletedPoints)
                    {
                        foreach (var family in familyInstances)
                        {
                            var loc = family.Location as LocationPoint;
                            
                            if (deletedPoint.IsAlmostEqualTo(loc?.Point))
                            {
                                var familyName = family.Symbol.Family.Name;
                                outdatedFamilies.Add(new OutdatedFamily(family.Id.IntegerValue, familyName, loc.Point.ToString()));
                            }
                        }
                    }
                    
                }
                else
                {
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
                }
                
                Debug.Print($"{newIntersections.Count} точек {wallHoles.Count} отверстий");

                HolesVm holesVm = new HolesVm(newHoles,actualHoles, outdatedFamilies, wallHoles[0]);
                HoleTaskView view = new HoleTaskView(holesVm);
                view.Show();

                if (!holesVm.ButtonClicked) return Result.Cancelled;

                var selectedHoles = holesVm.Holes.Where(x => x.IsInsert);

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

        private List<Element> GetOutdatedFamiliesSource(XYZ basePnt, Document doc, string objectType)
        {
            //var basePnt = familyInstance.Location as LocationPoint;

            BoundingBoxContainsPointFilter containsPointFilter =
                new BoundingBoxContainsPointFilter(basePnt); // inverted filter

            var collector = new FilteredElementCollector(doc);
            List<Element> elements = new List<Element>();
            IList<Element> containFounds = new List<Element>();
            if (objectType == "Wall")
            {
                containFounds = collector.OfClass(typeof(Wall)).WherePasses(containsPointFilter).ToElements();
                elements.AddRange(containFounds.Select(containFound => containFound as Wall).Cast<Element>());
            }

            if (objectType != "Floor") return elements;

            containFounds = collector.OfClass(typeof(Floor)).WherePasses(containsPointFilter).ToElements();
            elements.AddRange(containFounds.Select(containFound => containFound as Floor).Cast<Element>());

            return elements;
        }


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

        //    Family family = a.FirstOrDefault<Element>(e => e.IntersectingElementName.Equals("ОВ1")) as Family;

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


    }
}
