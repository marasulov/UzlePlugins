using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using System.Collections.Generic;
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Controls;
using UzlePlugins.Contracts;
using UzlePlugins.Contracts.DTOs;
using UzlePlugins.RevitCore.Models;
using UzlePlugins.RevitCore.Services;
using UzlePlugins.Settings;
using UzlePlugins.Views;
using UzlePlugins.Vm;

namespace UzlePlugins.Models.Revit2022.Models
{
    public class CreateHoleModel : ICreateHoleModel
    {
        private readonly UIDocument _uiDocument;
        //private readonly RevitRepository _revitRepository;

        //public CreateHoleModel(RevitRepository revitRepository)
        //{
        //    _revitRepository = revitRepository;
        //}

        public CreateHoleModel(UIDocument uiDocument)
        {
            _uiDocument = uiDocument;
        }

        //public Document Document => _revitRepository.Document;


        public void CreateHole()
        {
            var doc = _uiDocument.Document;
            var offset = 1.1;
            double pipeDiametrForFilter = 50;
            var settingsReader = new SettingsReader();
            var names = settingsReader.GetFamilyNames().FamilyNames;
            //var recFamilyNames = settingsReader.GetFamilyTypes().FamilyTypes.Rectangled.FamilyNames;
            //var circledFamilyNames = settingsReader.GetFamilyTypes().FamilyTypes.Circled.FamilyNames;
            //var linkedDocs = GetLinkedDocuments(doc);

            var familyNames = new HashSet<string>(names);
            var familyInstances = new FilteredElementCollector(doc)
                .WhereElementIsNotElementType()
                .OfClass(typeof(FamilyInstance))
                .Cast<FamilyInstance>()
                .Where(fi => familyNames.Contains(
                    fi.get_Parameter(BuiltInParameter.ELEM_FAMILY_PARAM)?
                        .AsValueString()))
                .ToArray();

            using TransactionGroup transactionGroup = new TransactionGroup(doc);

            var collector3dView = new FilteredElementCollector(doc);
            Func<View3D, bool> isNotTemplate = v3 => !(v3.IsTemplate);
            View3D view3D = collector3dView
                .OfClass(typeof(View3D))
                .Cast<View3D>()
                .First(isNotTemplate);
                
            var familyPoints = familyInstances.Where(w => w.Location is LocationPoint)
                .Select(f => f.Location as LocationPoint)
                .Select(loc => loc.Point).ToList();

            var smallestDiametr = UnitUtils.ConvertToInternalUnits(pipeDiametrForFilter, UnitTypeId.Millimeters);
            var pipeCollector = new FilteredElementCollector(doc).OfClass(typeof(Pipe)).Cast<Pipe>()
                .Where(w => w.Diameter > smallestDiametr);

            var ductCollector = new FilteredElementCollector(doc).OfClass(typeof(Duct)).Cast<Duct>()
                .Where(w => w.Width > smallestDiametr);

            //Todo : надо сделать чтобы созлись список моделей которые содержат инфу о точках пересечения

            List<HoleFamilyModel> wallHoles = new List<HoleFamilyModel>();
                
            //исходные точки
            var newIntersections = new List<XYZ>();

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

                    var holes = new List<HoleFamilyModel>();
                    foreach (var point in points)
                    {
                        var sourceElement = refFinder.WallReferences[i];
                        if (sourceElement == null) continue;
                        var holeFiller = new HolePropertiesFiller(doc, pipeElement, sourceElement, point);
                        holeFiller.GetHoles(point, _uiDocument);
                        holes = holeFiller.HolesProps;
                    }
                    wallHoles.AddRange(holes);
                }

                refFinder.GetStructuralReferences(BuiltInCategory.OST_Floors);
                if (refFinder.FloorReferences.Count > 0)
                {
                    newIntersections.AddRange(refFinder.GetIntersectionsPoints(refFinder.FloorReferences));
                }
            }

            List<ActualHoleModelDto> actualHoles = new();
            List<ActualHoleModelDto> newHoles = new();

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
                        new ActualHoleModelDto(
                            actualModel.IntersectingElement.Id.IntegerValue,
                            actualModel.IntersectionPoint.ToString(),
                            actualModel.IntersectingElementName,
                            actualModel.IntersectedSourceType,
                            actualModel.IntersectedSourceType,
                            actualModel.IntersectingElementTypeSize,
                            actualModel.IntersectedSourceType,
                            true,
                            UnitUtils.ConvertToInternalUnits(20, UnitTypeId.Millimeters),
                            true,
                            actualModel.IntersectedSourceThickness)));
                }

                var i = 1;
                foreach (var newPoint in newPoints)
                {
                    var newModels = wallHoles.Where(holeFamilyModel =>
                        newPoint == holeFamilyModel.IntersectionPoint);

                    newHoles.AddRange(
                        newModels.Select(actualModel =>
                            new ActualHoleModelDto(
                                i,
                                actualModel.IntersectionPoint.ToString(),
                                actualModel.IntersectingElementName,
                                actualModel.IntersectedSourceType,
                                actualModel.IntersectedSourceType,
                                actualModel.IntersectingElementTypeSize,
                                actualModel.IntersectedSourceType,
                                true,
                                UnitUtils.ConvertToInternalUnits(20, UnitTypeId.Millimeters),
                                true,
                                actualModel.IntersectedSourceThickness)));
                    i++;
                }

                foreach (var deletedPoint in deletedPoints)
                {
                    foreach (var family in familyInstances)
                    {
                        var loc = family.Location as LocationPoint;

                        if (deletedPoint.IsAlmostEqualTo(loc?.Point))
                        {
                            var familyName = family.Symbol.Family.Name;
                            outdatedFamilies.Add(new OutdatedFamilyDto(family.Id.IntegerValue, familyName));
                        }
                    }
                }
            }
            else
            {
                var j = 0;
                foreach (var hole in wallHoles)
                {
                    var holeModel = new ActualHoleModelDto(
                        j,
                        hole.IntersectionPoint.ToString(),
                        hole.IntersectingElementName,
                        hole.IntersectedSourceType,
                        hole.IntersectingElementType,
                        hole.IntersectingElementTypeSize,
                        "", hole.IsHoleRectangled, hole.HoleOffset, hole.IsInsert, hole.IntersectedSourceThickness);

                    actualHoles.Add(holeModel);
                    j++;
                }
            }

            Debug.Print($"{newIntersections.Count} точек {wallHoles.Count} отверстий");


            //TODO mvvm as in example

            //HolesVm holesVm = new HolesVm(newHoles, actualHoles, outdatedFamilies);
            //HoleTaskView view = new HoleTaskView(holesVm);
            //view.Show();

            //if (!holesVm.ButtonClicked) return;

            //var selectedHoles = holesVm.Holes.Where(x => x.IsInsert);

            //transactionGroup.Start("Hole trask for walls with pipes");

            //using (Transaction t = new Transaction(doc))
            //{
            //    t.Start("Hole task for walls and floors with pipes");

            //    FamilyTypeFinder familyTypeFinder = new FamilyTypeFinder();
            //    familyTypeFinder.GetFamilyType(BuiltInCategory.OST_Walls, true);

            //    var wallsFamilyName = familyTypeFinder.FamilyName;
            //    var wallsFamilyType = familyTypeFinder.FamilyParameters;

            //    FamilySymbol symbol = familyTypeFinder.GetFamilySymbolToPlace(doc, wallsFamilyName);

            //    FamilyInsertService insertService = new FamilyInsertService(doc, symbol);

            //    foreach (var hole in selectedHoles)
            //    {
            //        //Reference r = new Reference(pipeElement);
            //        //ReferenceIntersectionFinder refFinder = new ReferenceIntersectionFinder(doc, pipeElement, view3D);

                        

            //        insertService.InsertFamily(symbol, offset, hole.SourceElementWidth, hole.IntersectingElementTypeSize, true);
            //    }

            //    //foreach (var pipeElement in pipeCollector)
            //    //{
            //    //    //Reference r = new Reference(pipeElement);
            //    //    ReferenceIntersectionFinder refFinder = new ReferenceIntersectionFinder(doc, pipeElement, view3D);
            //    //    FamilyInsertService insertService = new FamilyInsertService(symbol, wallsFamilyType, refFinder);
            //    //    insertService.InsertFamily(pipeElement, offset, BuiltInCategory.OST_Walls, symbol, true);
            //    //}

            //    //familyTypeFinder.GetFamilyType(BuiltInCategory.OST_Floors, true);
            //    //var floorsFamilyName = familyTypeFinder.FamilyName;
            //    //var floorsFamilyType = familyTypeFinder.FamilyParameters;
            //    //symbol = familyTypeFinder.GetFamilySymbolToPlace(doc, floorsFamilyName);
            //    //if (symbol != null)
            //    //    foreach (var pipeElement in pipeCollector)
            //    //    {
            //    //        //InsertFamily(doc, pipeElement, view3D, 1, BuiltInCategory.OST_Floors, symbol, true);
            //    //        //Reference r = new Reference(pipeElement);
            //    //        ReferenceIntersectionFinder refFinder = new ReferenceIntersectionFinder(doc, pipeElement, view3D);
            //    //        FamilyInsertService insertService = new FamilyInsertService(doc, symbol, floorsFamilyType, refFinder);
            //    //        insertService.InsertFamily(pipeElement, offset, BuiltInCategory.OST_Floors, symbol, true);

            //    //    }

            //    t.Commit();
            //}
            ////using (Transaction t = new Transaction(doc))
            ////{
            ////    t.Start("Hole task for walls and floors with ducts");

            ////    foreach (Element ductElement in ductCollector)
            ////    {

            ////        InsertFamily(doc, ductElement, view3D, 1, BuiltInCategory.OST_Walls, true);
            ////        InsertFamily(doc, ductElement, view3D, 1, BuiltInCategory.OST_Floors, true);

            ////    }

            ////    t.Commit();
            ////}

            //transactionGroup.Assimilate();
        }

    }
}
