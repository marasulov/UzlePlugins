using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using UzlePlugins.Contracts;
using UzlePlugins.Contracts.DTOs;
using UzlePlugins.Contracts.DTOs.BaseDtos;
using UzlePlugins.Models.Revit2022.Entities;
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
            double pipeDiametrForFilter = 50;
            var settingsReader = new SettingsReader<HoleFamilyNames>();
            var names = settingsReader.Read("Settings.json");

            //var recFamilyNames = settingsReader.GetFamilyTypes().FamilyTypes.Rectangled.FamilyNames;
            //var circledFamilyNames = settingsReader.GetFamilyTypes().FamilyTypes.Circled.FamilyNames;
            //var linkedDocs = GetLinkedDocuments(doc);

            var familyNames = new HashSet<string?>(names.FamilyNames);
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
                .FirstOrDefault(isNotTemplate);

            if (view3D == null) TaskDialog.Show("3d view is absent", "Create 3d view");

            List<PointData> pointDatas = familyInstances.Select(familyInstance => new PointData(familyInstance.Id, (familyInstance.Location as LocationPoint).Point)).ToList();

            var smallestDiametr = UnitUtils.ConvertToInternalUnits(pipeDiametrForFilter, UnitTypeId.Millimeters);

            var pipeCollector = new FilteredElementCollector(doc).OfClass(typeof(Pipe)).Cast<Pipe>()
                .Where(w => w.Diameter > smallestDiametr);

            List<HoleFamilyEntity> wallHoles = new List<HoleFamilyEntity>();

            //исходные точки
            var newIntersections = new List<XYZ>();

            foreach (Element pipeElement in pipeCollector)
            {
                ReferenceIntersectionFinder refFinder = new ReferenceIntersectionFinder(doc, pipeElement, view3D);

                refFinder.GetStructuralReferences(BuiltInCategory.OST_Walls);

                if (refFinder.WallReferences.Count > 0)
                {
                    var points = refFinder.GetIntersectionsPoints(refFinder.WallReferences);

                    newIntersections.AddRange(points);
                    //int i = 0;

                    var holes = new List<HoleFamilyEntity>();
                    foreach (var point in points)
                    {
                        var sourceElement = refFinder.WallReferences[0];
                        if (sourceElement == null) continue;
                        var holeFiller = new HolePropertiesFiller(doc, pipeElement, sourceElement, point);
                        holeFiller.GetHoles(point, refFinder.Normal);
                        holes = holeFiller.HolesProps;
                        wallHoles.AddRange(holes);
                    }

                }

                refFinder.GetStructuralReferences(BuiltInCategory.OST_Floors);
                if (refFinder.FloorReferences.Count <= 0) continue;
                {
                    var points = refFinder.GetIntersectionsPoints(refFinder.FloorReferences);
                    newIntersections.AddRange(points);

                    //int i = 0;

                    var holes = new List<HoleFamilyEntity>();
                    foreach (var point in points)
                    {
                        var sourceElement = refFinder.FloorReferences[0];
                        if (sourceElement == null) continue;
                        var holeFiller = new HolePropertiesFiller(doc, pipeElement, sourceElement, point);
                        holeFiller.GetHoles(point, refFinder.Normal);
                        holes = holeFiller.HolesProps;
                        wallHoles.AddRange(holes);
                    }
                }
            }

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

            //из коллекции воздуховодов создаем отверстия
            IOffsetManagerService manager = new OffsetManagerService();
            var offsets = manager.Read();

            foreach (Element pipeElement in ductCollector)
            {
                ReferenceIntersectionFinder refFinder = new ReferenceIntersectionFinder(doc, pipeElement, view3D);

                refFinder.GetStructuralReferences(BuiltInCategory.OST_Walls);

                if (refFinder.WallReferences.Count > 0)
                {
                    var points = refFinder.GetIntersectionsPoints(refFinder.WallReferences);

                    newIntersections.AddRange(points);
                    
                    var holes = new List<HoleFamilyEntity>();
                    foreach (var point in points)
                    {
                        var sourceElement = refFinder.WallReferences[0];
                        if (sourceElement == null) continue;
                        var holeFiller = new HolePropertiesFiller(doc, pipeElement, sourceElement, point);
                        holeFiller.GetHoles(point, refFinder.Normal);
                        holes = holeFiller.HolesProps;
                        wallHoles.AddRange(holes);
                    }

                }

                refFinder.GetStructuralReferences(BuiltInCategory.OST_Floors);
                if (refFinder.FloorReferences.Count > 0)
                {
                    var points = refFinder.GetIntersectionsPoints(refFinder.FloorReferences);
                    newIntersections.AddRange(points);

                    var i = 0;

                    var holes = new List<HoleFamilyEntity>();
                    foreach (var point in points)
                    {
                        var sourceElement = refFinder.FloorReferences[i];
                        if (sourceElement == null) continue;
                        var holeFiller = new HolePropertiesFiller(doc, pipeElement, sourceElement, point);
                        holeFiller.GetHoles(point, refFinder.Normal);
                        holes = holeFiller.HolesProps;
                        wallHoles.AddRange(holes);
                    }

                }
            }

            List<ActualHoleDto> actualHoles = new();
            List<NewHoleDto> newHoles = new();
            List<OutdatedFamilyDto> outdatedFamilies = new();

            // если есть семейства значит не первый раз вставляются семейства отверстий
            var pipeOffsets = offsets.Pipe;
            var ductOffsets = offsets.Duct;
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
                        actualPoint.Point.IsAlmostEqualTo(holeFamilyModel.IntersectionParameters.IntersectionPoint));

                    actualHoles.AddRange(actualModels.Select(actualModel =>
                    {
                        var intParams = actualModel.IntersectionParameters;
                        var intersectionPoint = intParams.IntersectionPoint;
                        return new ActualHoleDto(
                            actualPoint.Id.IntegerValue,
                            new IntersectionData(
                                XYZPointToDtoConverter.ConvertToDTO(intersectionPoint),
                                intParams.IntersectingElementName,
                                intParams.IntersectingElementType,
                                UnitUtils.ConvertFromInternalUnits(intParams.IntersectingElementTypeSize, UnitTypeId.Millimeters),
                                XYZPointToDtoConverter.ConvertToDTO(actualModel.IntersectionParameters.Normal), "Square"
                            ),
                            new HoleData("", actualModel.GetOffset(pipeOffsets), true),
                            new HoleSourceData(
                                actualModel.SourceParameters.SourceType,
                                actualModel.SourceParameters.SourceThickness,
                                0,
                                actualModel.SourceParameters.SourceName
                            ),
                            false);


                    }));

                }

                foreach (var newPoint in newPoints)
                {
                    var newModels = wallHoles.Where(holeFamilyModel =>
                        newPoint == holeFamilyModel.IntersectionParameters.IntersectionPoint);

                    newHoles.AddRange(
                        newModels.Select(newModel =>
                        {
                            var intParams = newModel.IntersectionParameters;
                            var intersectionPoint = intParams.IntersectionPoint;
                            return new NewHoleDto(
                                intParams.IntersectingElement.Id.IntegerValue,
                                new IntersectionData(XYZPointToDtoConverter.ConvertToDTO(intersectionPoint),
                                    intParams.IntersectingElementName,
                                    intParams.IntersectingElementType,
                                    UnitUtils.ConvertFromInternalUnits(intParams.IntersectingElementTypeSize, UnitTypeId.Millimeters),
                                    XYZPointToDtoConverter.ConvertToDTO(intParams.Normal),"Square"
                                    ),
                                new HoleData("", newModel.GetOffset(pipeOffsets), true),
                                new HoleSourceData(
                                    newModel.SourceParameters.SourceType,
                                    newModel.SourceParameters.SourceThickness,
                                    0,
                                    newModel.SourceParameters.SourceName
                                ),
                                "Square",
                                intParams.IntersectingElementHeight,
                                intParams.IntersectingElementWidth,
                                true
                            );
                        }));

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
                newHoles.AddRange(wallHoles.Select(hole =>
                {
                    var intParams = hole.IntersectionParameters;
                    var intersectionPoint = intParams.IntersectionPoint;
                    return new NewHoleDto(
                        intParams.IntersectingElement.Id.IntegerValue,
                        new IntersectionData(XYZPointToDtoConverter.ConvertToDTO(intersectionPoint),
                            intParams.IntersectingElementName,
                            intParams.IntersectingElementType,
                            UnitUtils.ConvertFromInternalUnits(intParams.IntersectingElementTypeSize, UnitTypeId.Millimeters),
                            XYZPointToDtoConverter.ConvertToDTO(intParams.Normal),"Square"
                        ),
                        new HoleData("", hole.GetOffset(pipeOffsets), true),
                        new HoleSourceData(
                            hole.SourceParameters.SourceType,
                            hole.SourceParameters.SourceThickness,
                            0,
                            hole.SourceParameters.SourceName
                        ),
                        "Square",
                        intParams.IntersectingElementHeight,
                        intParams.IntersectingElementWidth,
                        true
                    );
                }));
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
