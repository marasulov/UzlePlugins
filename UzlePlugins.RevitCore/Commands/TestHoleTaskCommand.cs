using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using Autodesk.Revit.DB.Plumbing;

namespace UzlePlugins.RevitCore.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class TestHoleTaskCommand : IExternalCommand
    {
        private const string FAMILYNAME = "Пересечение_Стена_Круглое";
        private const string FAMILYTYPENAME = "ОВ1";
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var uiapp = commandData.Application;
            var uidoc = uiapp.ActiveUIDocument;
            var doc = uidoc.Document;

            var t = new Transaction(doc,
                "Insert structural stiffener family instance");

            t.Start();

            // Получите все семейства в проекте с заданным именем и типом
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            List<FamilySymbol> foundElements = collector
                .OfClass(typeof(FamilySymbol))
                .WhereElementIsElementType()
                .Cast<FamilySymbol>()
                .Where(symbol => symbol.Family != null && symbol.Family.Name == FAMILYNAME)
                .ToList();

            if (foundElements.Any())
            {
                var ov1 = foundElements.Where(x => x.Name == FAMILYTYPENAME);
              
                // Выведите информацию о найденных семействах
                foreach (var element in foundElements)
                {
                    if (element.Name == FAMILYTYPENAME)
                    {
                        if (element != null)
                        {

                            foreach (var point in GetIntersectionPoints(uidoc,element))
                            {
                                //FamilyInstance familyInstance = doc.Create.NewFamilyInstance(point, element, StructuralType.NonStructural);    
                                Debug.Print("Success", $"Семейство успешно вставлено в {point}.");
                            }

                            // Создание экземпляра семейства в документе
                            

                            Debug.Print("Семейство успешно вставлено в проект.");
                        }
                        else
                        {
                            TaskDialog.Show("Error", "Не удалось получить символ семейства.");
                        }
                    }
                }
            }
            else
            {
                TaskDialog.Show("Not Found", $"Семейство с именем '{FAMILYNAME}' не найдено в проекте.");
            }

            // Получите семейство в проекте по его имени

            //if (familyId != null)
            //{
            //    // Получите символы семейства
            //    List<FamilySymbol> symbols = new FilteredElementCollector(doc, familyId)
            //        .OfClass(typeof(FamilySymbol))
            //        .Cast<FamilySymbol>()
            //        .ToList();

            //    // Выберите нужный символ (например, первый)
            //    FamilySymbol selectedSymbol = symbols.FirstOrDefault();

            //    // Если символ найден, вставьте экземпляр семейства
            //    if (selectedSymbol != null)
            //    {
            //        // Указание места вставки семейства
            //        XYZ insertionPoint = new XYZ(0, 0, 0);

            //        // Создание экземпляра семейства в документе
            //        FamilyInstance familyInstance = doc.Create.NewFamilyInstance(insertionPoint, selectedSymbol, StructuralType.NonStructural);

            //        TaskDialog.Show("Success", "Семейство успешно вставлено в проект.");
            //    }
            //    else
            //    {
            //        TaskDialog.Show("Error", "Не удалось получить символ семейства.");
            //    }
            //}
            //else
            //{
            //    TaskDialog.Show("Error", "Семейство с заданным именем не найдено в проекте.");
            //}


            t.Commit();

            return Result.Succeeded;
        }

        public List<XYZ> GetIntersectionPoints(UIDocument uidoc, FamilySymbol symbol)
        {
            Document doc = uidoc.Document;

            Reference pipeRef = uidoc.Selection.PickObject(
              ObjectType.Element);

            Element pipeElem = doc.GetElement(pipeRef);

            LocationCurve lc = pipeElem.Location as LocationCurve;
            XYZ startPoint = lc.Curve.GetEndPoint(0) as XYZ;
            XYZ endPoint = lc.Curve.GetEndPoint(1) as XYZ;
            // Shoot intersector along element.

            XYZ rayDirection = endPoint.Subtract(
                startPoint).Normalize();

            Curve curve = lc.Curve;

            ReferenceComparer reference1 = new ReferenceComparer();

            ElementFilter filter = new ElementCategoryFilter(
              BuiltInCategory.OST_Walls);

            FilteredElementCollector collector
              = new FilteredElementCollector(doc);

            Func<View3D, bool> isNotTemplate = v3 => !(v3.IsTemplate);
            View3D view3D = collector
              .OfClass(typeof(View3D))
              .Cast<View3D>()
              .First<View3D>(isNotTemplate);

            ReferenceIntersector refIntersector
              = new ReferenceIntersector(
                filter, FindReferenceTarget.Element, view3D);

            refIntersector.FindReferencesInRevitLinks = true;
            IList<ReferenceWithContext> referenceWithContext
              = refIntersector.Find(
                startPoint,
                rayDirection);
            List<XYZ> intersectPoints = new List<XYZ>();

            IList<Reference> references
              = referenceWithContext
                .Select(p => p.GetReference())
                .Distinct(reference1)
                .Where(p => p.GlobalPoint.DistanceTo(
                  curve.GetEndPoint(0)) < curve.Length)
                .ToList();
          
            foreach (Reference rc in references)
            {
                RevitLinkInstance instance = doc.GetElement(rc) as RevitLinkInstance;
                Document linkDoc = instance.GetLinkDocument();
                var element = linkDoc.GetElement(rc.LinkedElementId) as Wall;
                var width = element.WallType.Width;

                var wallOrientation = element.Orientation;

                var intersectPoint = rc.GlobalPoint - (width/2 * rayDirection);
                FamilyInstance fi = doc.Create.NewFamilyInstance(intersectPoint, symbol, StructuralType.NonStructural);
                var basisY = fi.GetTransform().BasisY;
                var angle = basisY.AngleTo(rayDirection);

                Line axis = Line.CreateBound(intersectPoint, intersectPoint + XYZ.BasisZ);
                ElementTransformUtils.RotateElement(doc,fi.Id,axis, -angle);
                intersectPoints.Add(intersectPoint);
            }

            return intersectPoints;
        }

        public class ReferenceComparer : IEqualityComparer<Reference>
        {
            public bool Equals(Reference x, Reference y)
            {
                if (x.ElementId == y.ElementId)
                {
                    if (x.LinkedElementId == y.LinkedElementId)
                    {
                        return true;
                    }
                    return false;
                }
                return false;
            }

            public int GetHashCode(Reference obj)
            {
                int hashName = obj.ElementId.GetHashCode();
                int hashId = obj.LinkedElementId.GetHashCode();
                return hashId ^ hashId;
            }
        }

        public Dictionary<Reference, XYZ>  GetIntersectPoints(
            Document doc,
            Element intersect)
        {
            // Find a 3D view to use for the 
            // ReferenceIntersector constructor.

            FilteredElementCollector collector
                = new FilteredElementCollector(doc);

            Func<View3D, bool> isNotTemplate = v3
                => !(v3.IsTemplate);

            View3D view3D = collector
                .OfClass(typeof(View3D))
                .Cast<View3D>()
                .First<View3D>(isNotTemplate);

            // Use location point as start point for intersector.

            LocationCurve lp = intersect.Location as LocationCurve;
            XYZ startPoint = lp.Curve.GetEndPoint(0) as XYZ;
            XYZ endPoint = lp.Curve.GetEndPoint(1) as XYZ;

            // Shoot intersector along element.

            XYZ rayDirection = endPoint.Subtract(
                startPoint).Normalize();

            List<BuiltInCategory> builtInCats
                = new List<BuiltInCategory>();

            builtInCats.Add(BuiltInCategory.OST_Roofs);
            builtInCats.Add(BuiltInCategory.OST_Ceilings);
            builtInCats.Add(BuiltInCategory.OST_Floors);
            builtInCats.Add(BuiltInCategory.OST_Walls);

            ElementMulticategoryFilter intersectFilter
                = new ElementMulticategoryFilter(builtInCats);

            ReferenceIntersector refIntersector
                = new ReferenceIntersector(intersectFilter,
                    FindReferenceTarget.Element, view3D);

            refIntersector.FindReferencesInRevitLinks = true;
            //todo
            IList<ReferenceWithContext> referencesWithContext
                = refIntersector.Find(startPoint,
                    rayDirection);
            
            List<XYZ> intersectPoints = new List<XYZ>();

            IList<Reference> intersectRefs
                = new List<Reference>();

            Dictionary<Reference, XYZ> dictProvisionForVoidRefs
                = new Dictionary<Reference, XYZ>();

            FilteredElementCollector a
                = new FilteredElementCollector(doc)
                    .OfClass(typeof(Family));

            Family family = a.FirstOrDefault<Element>(e => e.Name.Equals("ОВ1")) as Family;

            ReferenceComparer reference1 = new ReferenceComparer();

            var newref = referencesWithContext.Distinct(new ReferenceWithContextElementEqualityComparer());

            foreach (ReferenceWithContext r in
                     newref)
            {
                var intersectPoint = r.GetReference().GlobalPoint;
                intersectPoints.Add(intersectPoint);
                dictProvisionForVoidRefs.Add(r.GetReference(),
                    intersectPoint);

            }
            return dictProvisionForVoidRefs;
        }

        void CreateNurseCallDomeOnWall(Autodesk.Revit.DB.Document document, Wall wall)
        {
            FilteredElementCollector collector = new FilteredElementCollector(document);
            collector.OfClass(typeof(FamilySymbol)).OfCategory(BuiltInCategory.OST_NurseCallDevices);

            FamilySymbol symbol = collector.FirstElement() as FamilySymbol;

            // Get interior face of wall
            IList<Reference> sideFaces = HostObjectUtils.GetSideFaces(wall, ShellLayerType.Interior);
            Reference interiorFaceRef = sideFaces[0];

            XYZ location = new XYZ(4, 2, 8);
            XYZ refDir = new XYZ(0, 0, 1);

            FamilyInstance instance = document.Create.NewFamilyInstance(interiorFaceRef, location, refDir, symbol);
        }

        public void PickPoint(UIDocument uidoc)
        {
            ObjectSnapTypes snapTypes = ObjectSnapTypes.Endpoints | ObjectSnapTypes.Intersections;
            XYZ point = uidoc.Selection.PickPoint(snapTypes, "Select an end point or intersection");

            string strCoords = "Selected point is " + point.ToString();

            TaskDialog.Show("Revit", strCoords);
        }

        /// <summary>
        /// Поиск элемента
        /// по его типу и наименованию
        /// </summary>
        public static Element FindElementByName(Document doc, Type targetType, string targetName)
        {
            return new FilteredElementCollector(doc)
                .OfClass(targetType)
                .FirstOrDefault<Element>(
                    e => e.Name.Equals(targetName));
        }
    }
}
