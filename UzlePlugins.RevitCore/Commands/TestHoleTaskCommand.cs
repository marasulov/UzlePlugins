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
using System.Collections.ObjectModel;

namespace UzlePlugins.RevitCore.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class TestHoleTaskCommand : IExternalCommand
    {
        private const string WallFamilyName = "Пересечение_Стена_Круглое";
        private const string FloorFamilyName = "Пересечение_Плита_Круглое";
        private const string FamilytypeOv1 = "ОВ1";
        private const string FamilytypeOv2 = "ОВ2";
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var uiapp = commandData.Application;
            var uidoc = uiapp.ActiveUIDocument;
            var doc = uidoc.Document;
            var offset = 1.1;
            var pipeDiametrForFilter = 50;

            using (TransactionGroup transactionGroup = new TransactionGroup(doc))
            {
                var collector = new FilteredElementCollector(doc);
                Func<View3D, bool> isNotTemplate = v3 => !(v3.IsTemplate);
                View3D view3D = collector
                    .OfClass(typeof(View3D))
                    .Cast<View3D>()
                    .First<View3D>(isNotTemplate);

                
                var pipeCollector = new FilteredElementCollector(doc).OfClass(typeof(Pipe)).Cast<Pipe>()
                    .Where(w => w.Diameter > pipeDiametrForFilter / 304.8);
                transactionGroup.Start("Hole trask for walls with pipes");
                using (Transaction t = new Transaction(doc))
                {
                    t.Start("Hole trask for walls with pipes");




                    // Iterate through each pipe to get references
                    foreach (Element pipeElement in pipeCollector)
                    {
                        var pipe = pipeElement as Pipe;

                        Reference r = new Reference(pipeElement);
                        ReferenceIntersectionFinder refFinder = new ReferenceIntersectionFinder(doc, r, view3D);
                        List<XYZ> intersectPoints = refFinder.GetIntersectionsPoint();

                        if (intersectPoints.Count <= 0) continue;
                        var symbol = GetFamilySymbolToPlace(doc, WallFamilyName, FamilytypeOv1);
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

                                if (parameter.Definition.Name != "ADSK_Размер_Толщина") continue;
                                Debug.Print($"parameter толщина - before {parameter.AsDouble()}");

                                parameter.Set(refFinder.Thickness + (refFinder.Thickness * 1.2));
                            }
                        }
                    }

                    t.Commit();
                }

                using (Transaction t = new Transaction(doc))
                {
                    t.Start("Hole trask for walls with floors");

                    //var pipeCollector = new FilteredElementCollector(doc).OfClass(typeof(Pipe)).Cast<Pipe>()
                    //    .Where<Pipe>((Func<Pipe, bool>)(w =>
                    //        w.Diameter > pipeDiametrForFilter / 304.8));

                    // Iterate through each pipe to get references
                    foreach (Element pipeElement in pipeCollector)
                    {
                        var pipe = pipeElement as Pipe;

                        Reference r = new Reference(pipeElement);
                        ReferenceIntersectionFinder refFinder = new ReferenceIntersectionFinder(doc, r, view3D);
                        List<XYZ> intersectPoints = refFinder.GetIntersectionsPointWithFloors();

                        if (intersectPoints.Count <= 0) continue;
                        var symbol = GetFamilySymbolToPlace(doc, FloorFamilyName, FamilytypeOv2);
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

                                if (parameter.Definition.Name != "ADSK_Размер_Толщина") continue;
                                Debug.Print($"parameter толщина - before {parameter.AsDouble()}");

                                parameter.Set(1);
                            }
                        }
                    }

                    t.Commit();
                }

                transactionGroup.Assimilate();
            }

            return Result.Succeeded;
        }

        public static FamilySymbol GetFamilySymbolToPlace(Document doc, string familyName, string familyTypeName)
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



    }
}
