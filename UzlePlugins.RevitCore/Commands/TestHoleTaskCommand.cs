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

            var t = new Transaction(doc, "Insert structural stiffener family instance");

            t.Start();

            var pipeRef = uidoc.Selection.PickObject(ObjectType.Element);

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

        private double ConvertToRadians(double angle)
        {
            return (Math.PI / 180) * angle;
        }

        public List<XYZ> GetIntersectionPoints(UIDocument uidoc, FamilySymbol symbol)
        {
            Document doc = uidoc.Document;
            Reference pipeRef = uidoc.Selection.PickObject(ObjectType.Element);

            ReferenceIntersectionFinder refFinder = new ReferenceIntersectionFinder(doc, pipeRef);
           
            List<XYZ> intersectPoints = new List<XYZ>();
            foreach (Reference rc in refFinder.References)
            {
                RevitLinkInstance instance = doc.GetElement(rc) as RevitLinkInstance;
                Document linkDoc = instance.GetLinkDocument();
                var element = linkDoc.GetElement(rc.LinkedElementId) as Wall;
                var width = element.WallType.Width;

                //var wallOrientation = element.Orientation;

                var intersectPoint = rc.GlobalPoint - (width/2 * refFinder.Normal);
                FamilyInstance fi = doc.Create.NewFamilyInstance(intersectPoint, symbol, StructuralType.NonStructural);
                var basisY = fi.GetTransform().BasisY;
                var angle = basisY.AngleTo(refFinder.Normal);

                Line axis = Line.CreateBound(intersectPoint, intersectPoint + XYZ.BasisZ);
                ElementTransformUtils.RotateElement(doc,fi.Id,axis, -angle);
                intersectPoints.Add(intersectPoint);
            }

            return intersectPoints;
        }

        //private void CreateFamily()
        //{
        //    FamilyInstance fi = doc.Create.NewFamilyInstance(intersectPoint, symbol, StructuralType.NonStructural);
        //    var basisY = fi.GetTransform().BasisY;
        //    var angle = basisY.AngleTo(refFinder.Normal);

        //    Line axis = Line.CreateBound(intersectPoint, intersectPoint + XYZ.BasisZ);
        //    ElementTransformUtils.RotateElement(doc,fi.Id,axis, -angle);
        //}

        //private Reference GetIntersectionReference(Document doc, Reference pipeRef)
        //{
        //    Element pipeElem = doc.GetElement(pipeRef);
        //    ReferenceIntersectionFinder refFinder = new ReferenceIntersectionFinder(doc, pipeRef);
        //    //LocationCurve lc = pipeElem.Location as LocationCurve;

        //    //XYZ startPoint = lc.Curve.GetEndPoint(0);
        //    //XYZ endPoint = lc.Curve.GetEndPoint(1);

        //    //XYZ normal = endPoint.Subtract(
        //    //    startPoint).Normalize();

        //    //Curve curve = lc.Curve;
        //    //ReferenceComparer reference1 = new ReferenceComparer();
            
        //    //ElementFilter filter = new ElementCategoryFilter(
        //    //  BuiltInCategory.OST_Walls);

        //    //FilteredElementCollector collector = new FilteredElementCollector(doc);

        //    //Func<View3D, bool> isNotTemplate = v3 => !(v3.IsTemplate);
        //    //View3D view3D = collector
        //    //  .OfClass(typeof(View3D))
        //    //  .Cast<View3D>()
        //    //  .First<View3D>(isNotTemplate);

        //    //ReferenceIntersector refIntersector
        //    //  = new ReferenceIntersector(
        //    //    filter, FindReferenceTarget.Element, view3D);

        //    //refIntersector.FindReferencesInRevitLinks = true;
        //    //IList<ReferenceWithContext> referenceWithContext
        //    //  = refIntersector.Find(
        //    //    startPoint,
        //    //    normal);
        //    List<XYZ> intersectPoints = new List<XYZ>();

        //    //IList<Reference> references
        //    //  = referenceWithContext
        //    //    .Select(p => p.GetReference())
        //    //    .Distinct(reference1)
        //    //    .Where(p => p.GlobalPoint.DistanceTo(
        //    //      curve.GetEndPoint(0)) < curve.Length)
        //    //    .ToList();

          
        //    //foreach (Reference rc in refFinder.References)
        //    //{
        //    //    RevitLinkInstance instance = doc.GetElement(rc) as RevitLinkInstance;
        //    //    Document linkDoc = instance.GetLinkDocument();
        //    //    var element = linkDoc.GetElement(rc.LinkedElementId) as Wall;
        //    //    var width = element.WallType.Width;

        //    //    var wallOrientation = element.Orientation;

        //    //    var intersectPoint = rc.GlobalPoint - (width/2 * refFinder.Normal);
        //    //    FamilyInstance fi = doc.Create.NewFamilyInstance(intersectPoint, StructuralType.NonStructural);
        //    //    var basisY = fi.GetTransform().BasisY;
        //    //    var angle = basisY.AngleTo(refFinder.Normal);

        //    //    Line axis = Line.CreateBound(intersectPoint, intersectPoint + XYZ.BasisZ);
        //    //    ElementTransformUtils.RotateElement(doc,fi.Id,axis, -angle);
        //    //    intersectPoints.Add(intersectPoint);
        //    //}
        //}



        

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
