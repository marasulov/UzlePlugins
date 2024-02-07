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
using UzlePlugins.RevitCore.Services;

namespace UzlePlugins.RevitCore.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class TestHoleTaskCommand : IExternalCommand
    {
        private const string Familyname = "Пересечение_Стена_Круглое";
        private const string Familytypename = "ОВ1";
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var uiapp = commandData.Application;
            var uidoc = uiapp.ActiveUIDocument;
            var doc = uidoc.Document;
            var offset = 0.1;
            var t = new Transaction(doc, "Insert structural stiffener family instance");

            t.Start();

            var pipeRef = uidoc.Selection.PickObject(ObjectType.Element);
            var pipe = doc.GetElement(pipeRef) as Pipe;

            ReferenceIntersectionFinder refFinder = new ReferenceIntersectionFinder(doc, pipeRef);

            List<XYZ> intersectPoints = refFinder.GetIntersectionsPoint();

            if (intersectPoints.Count > 0)
            {
                var symbol = GetFamilySymbolToPlace(doc, Familyname, Familytypename);
                foreach (var intersectPoint in intersectPoints)
                {
                    FamilyInstance fi = doc.Create.NewFamilyInstance(intersectPoint, symbol, StructuralType.NonStructural);
                    var basisY = fi.GetTransform().BasisY;
                    var angle = basisY.AngleTo(refFinder.Normal);

                    Line axis = Line.CreateBound(intersectPoint, intersectPoint + XYZ.BasisZ);
                    ElementTransformUtils.RotateElement(doc, fi.Id, axis, -angle);
                    foreach (var parameter in fi.GetOrderedParameters())
                    {
                        if (parameter.Definition.Name == "ADSK_Размер_Диаметр")
                        {
                            Debug.Print($"parameter - before {parameter.AsDouble()}");
                            var outerDiameter = pipe.get_Parameter(BuiltInParameter.RBS_PIPE_OUTER_DIAMETER);
                            parameter.Set(outerDiameter.AsDouble() + offset);
                            
                        }
                        if (parameter.Definition.Name == "ADSK_Размер_Толщина")
                        {
                            Debug.Print($"parameter толщина - before {parameter.AsDouble()}");
                            var thickness = refFinder.StartPoint.DistanceTo(refFinder.EndPoint);
                            parameter.Set(refFinder.Thickness + offset);
                            
                        }
                    }

                    
                }
                
            }
            
            t.Commit();

            return Result.Succeeded;
        }

        private FamilySymbol GetFamilySymbolToPlace(Document doc, string familyName, string familyTypeName)
        {
            FamilySymbol symbol = null;
            foreach (FamilySymbol fSym in new FilteredElementCollector(doc).OfClass(typeof(FamilySymbol))
                    .Cast<FamilySymbol>()) 
            {
                if (fSym.FamilyName == familyName && fSym.Name == familyTypeName) 
                {
                    symbol = fSym;
                    Debug.Print("family selected");
                    break;
                }
            }
            return symbol;
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
