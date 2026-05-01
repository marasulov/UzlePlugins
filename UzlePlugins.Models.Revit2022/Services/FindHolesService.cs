using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using UzlePlugins.Contracts;
using UzlePlugins.Contracts.DTOs;
using UzlePlugins.Models.Revit2022.Enities;
using UzlePlugins.RevitCore.Models;
using UzlePlugins.RevitCore.Services;
using UzlePlugins.Settings;

namespace UzlePlugins.Models.Revit2022.Services
{
    public class FindHolesService : IFindHoleService
    {
        private readonly UIDocument _uiDocument;
        private EntityToDtoConverter _entityToDTOConverter;

        public FindHolesService(UIDocument uiDocument, EntityToDtoConverter entityToDtoConverter)
        {
            _uiDocument = uiDocument;
            _entityToDTOConverter = entityToDtoConverter;
        }

        public AllHolesDto FindHoles()
        {
            var doc = _uiDocument.Document;
            var offset = 1.1;
            double pipeDiametrForFilter = 50;
            var settingsReader = new SettingsReader();
            var names = settingsReader.GetFamilyNames();


            //var recFamilyNames = settingsReader.GetFamilyTypes().FamilyTypes.Rectangled.FamilyNames;
            //var circledFamilyNames = settingsReader.GetFamilyTypes().FamilyTypes.Circled.FamilyNames;
            //var linkedDocs = GetLinkedDocuments(doc);

            var familyNames = new HashSet<string?>(names);
            var familyInstances = new FilteredElementCollector(doc)
                .WhereElementIsNotElementType()
                .OfClass(typeof(FamilyInstance))
                .Cast<FamilyInstance>()
                .Where(fi => familyNames.Contains(
                    fi.get_Parameter(BuiltInParameter.ELEM_FAMILY_PARAM)?
                        .AsValueString()))
                .ToArray();

            using TransactionGroup transactionGroup = new TransactionGroup(doc);

            //var collector3dView = new FilteredElementCollector(doc);
            //Func<View3D, bool> isNotTemplate = v3 => !(v3.IsTemplate);
            //View3D view3D = collector3dView
            //    .OfClass(typeof(View3D))
            //    .Cast<View3D>()
            //    .FirstOrDefault(isNotTemplate);

            //if (view3D == null) TaskDialog.Show("3d view is absent", "Create 3d view");

            View3D view3D = new FilteredElementCollector(doc)
    .OfClass(typeof(View3D))
    .Cast<View3D>()
    .FirstOrDefault(v => v.Name == "Hole_Check" && !v.IsTemplate);

            if (view3D == null)
            {
                TaskDialog.Show("Ошибка", "Создайте 3D вид с именем 'Hole_Check', отключите на нем границу подрезки (Section Box) и включите все связанные файлы!");
                return null; // Прерываем работу, так как без вида луч не полетит
            }


            List<PointData> pointDatas = familyInstances.Select(familyInstance => new PointData(familyInstance.Id, (familyInstance.Location as LocationPoint).Point)).ToList();

            var smallestDiametr = UnitUtils.ConvertToInternalUnits(pipeDiametrForFilter, UnitTypeId.Millimeters);

            var pipeCollector = new FilteredElementCollector(doc).OfClass(typeof(Pipe)).Cast<Pipe>()
                .Where(w => w.Diameter > smallestDiametr);

            List<HoleFamilyModel> wallHoles = new List<HoleFamilyModel>();

            //исходные точки
            var newIntersections = new List<XYZ>();

            foreach (Element pipeElement in pipeCollector)
            {
                ReferenceIntersectionFinder refFinder = new ReferenceIntersectionFinder(doc, pipeElement, view3D);
                refFinder.TestThickestLayerLogic(BuiltInCategory.OST_Walls);

                refFinder.GetStructuralReferences(BuiltInCategory.OST_Walls);

                if (refFinder.WallReferences.Count > 0)
                {
                    // Получаем список наших объектов IntersecResult
                    var intersectResults = refFinder.GetIntersectionsPoints(refFinder.WallReferences);

                    // ИСПРАВЛЕНИЕ 1: Извлекаем только координаты (XYZ) для списка newIntersections
                    newIntersections.AddRange(intersectResults.Select(r => r.Point));

                    var holes = new List<HoleFamilyModel>();

                    // ИСПРАВЛЕНИЕ 2: Перебираем наши готовые результаты, а не просто точки!
                    foreach (var result in intersectResults)
                    {
                        // Берем элемент-основу прямо из нашего результата (это будет та самая несущая стена 400 мм)
                        var sourceElement = result.SourceReference;
                        if (sourceElement == null) continue;

                        // Берем правильную центральную точку из результата
                        var exactPoint = result.Point;

                        // Передаем правильные данные в твой класс заполнения свойств
                        var holeFiller = new HolePropertiesFiller(doc, pipeElement, sourceElement, exactPoint);
                        holeFiller.GetHoles(exactPoint, _uiDocument, refFinder.Normal);

                        holes = holeFiller.HolesProps;
                        wallHoles.AddRange(holes);
                    }
                }

                //    refFinder.GetStructuralReferences(BuiltInCategory.OST_Floors);
                //    if (refFinder.FloorReferences.Count > 0)
                //    {
                //        var points = refFinder.GetIntersectionsPoints(refFinder.FloorReferences);
                //        newIntersections.AddRange(points);

                //        int i = 0;

                //        var holes = new List<HoleFamilyModel>();
                //        foreach (var point in points)
                //        {
                //            var sourceElement = refFinder.FloorReferences[i];
                //            if (sourceElement == null) continue;
                //            var holeFiller = new HolePropertiesFiller(doc, pipeElement, sourceElement, point);
                //            holeFiller.GetHoles(point, _uiDocument, refFinder.Normal);
                //            holes = holeFiller.HolesProps;
                //            wallHoles.AddRange(holes);
                //        }

                //    }
                //}

                //TODO Last
                var allductCollector = new FilteredElementCollector(doc).OfClass(typeof(Duct)).Cast<Duct>();
                var ductCollector = new List<Duct>();

                foreach (var duct in allductCollector)
                {
                    if (duct.DuctType.Shape == ConnectorProfileType.Round)
                    {
                        if (duct.Diameter > smallestDiametr)
                            ductCollector.Add(duct);
                    }
                    else
                    {
                        if (duct.Width > smallestDiametr)
                            ductCollector.Add(duct);
                    }
                }

                //.Where(w => w.Width > smallestDiametr);

                //из коллекции труб создаем отверстия

                //foreach (Element pipeElement in ductCollector)
                //{
                //    ReferenceIntersectionFinder refFinder = new ReferenceIntersectionFinder(doc, pipeElement, view3D);

                //    refFinder.GetStructuralReferences(BuiltInCategory.OST_Walls);

                //    if (refFinder.WallReferences.Count > 0)
                //    {
                //        var points = refFinder.GetIntersectionsPoints(refFinder.WallReferences);

                //        newIntersections.AddRange(points);
                //        int i = 0;

                //        var holes = new List<HoleFamilyModel>();
                //        foreach (var point in points)
                //        {
                //            var sourceElement = refFinder.WallReferences[i];
                //            if (sourceElement == null) continue;
                //            var holeFiller = new HolePropertiesFiller(doc, pipeElement, sourceElement, point);
                //            holeFiller.GetHoles(point, _uiDocument, refFinder.Normal);
                //            holes = holeFiller.HolesProps;
                //            wallHoles.AddRange(holes);
                //        }

                //    }

                //    refFinder.GetStructuralReferences(BuiltInCategory.OST_Floors);
                //    if (refFinder.FloorReferences.Count > 0)
                //    {
                //        var points = refFinder.GetIntersectionsPoints(refFinder.FloorReferences);
                //        newIntersections.AddRange(points);

                //        var i = 0;

                //        var holes = new List<HoleFamilyModel>();
                //        foreach (var point in points)
                //        {
                //            var sourceElement = refFinder.FloorReferences[i];
                //            if (sourceElement == null) continue;
                //            var holeFiller = new HolePropertiesFiller(doc, pipeElement, sourceElement, point);
                //            holeFiller.GetHoles(point, _uiDocument, refFinder.Normal);
                //            holes = holeFiller.HolesProps;
                //            wallHoles.AddRange(holes);
                //        }

                //    }
            }

            List<ActualHoleModelDto> actualHoles = new();
            List<NewHolesDto> newHoles = new();
            List<OutdatedFamilyDto> outdatedFamilies = new();

            // если есть семейства значит не первый раз вставляются семейства отверстий

            if (pointDatas.Count > 0)
            {
                var familyPoints = pointDatas.Select(x => x.Point);
                List<PointData> actualPoints =
                    (from intersection in newIntersections
                     from pointData in pointDatas
                     where intersection.IsAlmostEqualTo(pointData.Point)
                     select pointData)
                    .ToList();

                //var actualPoints = pointDatas.Where(pd => newIntersections.Any(xyz =>xyz.Equals(pd.Point)));

                //var actualPoints = newIntersections.Intersect(familyPoints, new XYZComparer()).ToArray();

                var newPoints = newIntersections.Except(familyPoints, new XYZComparer()).ToArray(); // newIntersections.Except(familyPoints)
                var deletedPoints = familyPoints.Except(newIntersections, new XYZComparer()).ToArray(); // familyPoints.Except(newIntersections)

                foreach (var actualPoint in actualPoints)
                {
                    var actualModels = wallHoles.Where(holeFamilyModel =>
                        actualPoint.Point.IsAlmostEqualTo(holeFamilyModel.IntersectionPoint));

                    actualHoles.AddRange(actualModels.Select(actualModel =>
                        new ActualHoleModelDto(
                            actualPoint.Id.IntegerValue,
                            new PointDTO(actualModel.IntersectionPoint.X, actualModel.IntersectionPoint.Y, actualModel.IntersectionPoint.Z),
                            actualModel.IntersectingElementName,
                            actualModel.IntersectingElementType,
                            actualModel.SourceType,
                            UnitUtils.ConvertFromInternalUnits(actualModel.IntersectingElementTypeSize, UnitTypeId.Millimeters),
                            actualModel.SourceType,
                            true,
                            20,
                            false,
                            actualModel.SourceThickness,
                            new PointDTO(actualModel.IntersectionPoint.X, actualModel.IntersectionPoint.Y, actualModel.IntersectionPoint.Z),
                            actualModel.IntersectingElementWidth,
                            actualModel.SourceName
                        )));
                }

                foreach (var newPoint in newPoints)
                {
                    var newModels = wallHoles.Where(holeFamilyModel =>
                        newPoint == holeFamilyModel.IntersectionPoint);

                    newHoles.AddRange(
                        newModels.Select(actualModel =>
                            new NewHolesDto(
                                actualModel.IntersectingElement.Id.IntegerValue,
                                new PointDTO(actualModel.IntersectionPoint.X, actualModel.IntersectionPoint.Y, actualModel.IntersectionPoint.Z),
                                actualModel.IntersectingElementType,
                                actualModel.IntersectingElementName,
                                actualModel.SourceType,
                                UnitUtils.ConvertFromInternalUnits(actualModel.IntersectingElementTypeSize, UnitTypeId.Millimeters),
                                actualModel.SourceType,
                                "Square",
                                20,
                                true,
                                actualModel.SourceThickness,
                                new PointDTO(actualModel.Normal.X, actualModel.Normal.Y, actualModel.Normal.Z),
                                actualModel.SourceThickness,
                                actualModel.SourceName,
                                actualModel.IntersectingElementHeight,
                                actualModel.IntersectingElementWidth)));
                }

                foreach (var deletedPoint in deletedPoints)
                {
                    foreach (var family in familyInstances)
                    {
                        var loc = family.Location as LocationPoint;

                        if (deletedPoint.IsAlmostEqualTo(loc?.Point))
                        {

                            outdatedFamilies.Add(_entityToDTOConverter.Convert(family));
                        }
                    }
                }
            }
            else
            {
                //TODO have to delete family not intersecting object
                foreach (var hole in wallHoles)
                {
                    var holeModel = new NewHolesDto(
                        hole.IntersectingElement.Id.IntegerValue,
                        new PointDTO(hole.IntersectionPoint.X, hole.IntersectionPoint.Y, hole.IntersectionPoint.Z),
                        hole.IntersectingElementType,
                        hole.IntersectingElementName,
                        "",
                        UnitUtils.ConvertFromInternalUnits(hole.IntersectingElementTypeSize, UnitTypeId.Millimeters),
                        hole.SourceType,
                        "Square",
                        hole.HoleOffset,
                        hole.IsInsert,
                        hole.SourceThickness,
                        new PointDTO(hole.Normal.X, hole.Normal.Y, hole.Normal.Z),
                        hole.SourceThickness,
                        hole.SourceName,
                        hole.IntersectingElementHeight,
                        hole.IntersectingElementWidth
                    );

                    newHoles.Add(holeModel);

                }
            }

            return new AllHolesDto(newHoles, actualHoles, outdatedFamilies);
        }

        public void GetWalls(Document document, Reference pipeRef, Element pipeElem)
        {

            LocationCurve lc = pipeElem.Location as LocationCurve;
            Curve curve = lc.Curve;

            ReferenceComparer reference1 = new ReferenceComparer();

            ElementFilter filter = new
                ElementCategoryFilter(BuiltInCategory.OST_Walls);

            FilteredElementCollector collector = new FilteredElementCollector(document);
            Func<View3D, bool> isNotTemplate = v3 => !(v3.IsTemplate);
            View3D view3D = collector.OfClass(typeof(View3D)).Cast<View3D>()
                .First<View3D>(isNotTemplate);

            ReferenceIntersector refIntersector = new ReferenceIntersector(filter, FindReferenceTarget.Element, view3D);
            refIntersector.FindReferencesInRevitLinks = true;
            IList<ReferenceWithContext> referenceWithContext = refIntersector.Find(curve.GetEndPoint(0), (curve as Line).Direction);
            IList<Reference> references = referenceWithContext.Select(p => p.GetReference()).Distinct(reference1).Where(p => p.GlobalPoint.DistanceTo(curve.GetEndPoint(0)) < curve.Length).ToList();
            IList<Element> walls = new List<Element>();
            foreach (Reference reference in references)
            {
                RevitLinkInstance instance = document.GetElement(reference) as RevitLinkInstance;
                Document linkDoc = instance.GetLinkDocument();
                Element element = linkDoc.GetElement(reference.LinkedElementId);
                walls.Add(element);
            }
            TaskDialog.Show("Count of wall", walls.Count.ToString());
        }
    }
}
