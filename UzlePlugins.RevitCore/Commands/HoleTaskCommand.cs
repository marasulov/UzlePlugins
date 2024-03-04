using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            var t = new Transaction(doc, "Insert structural stiffener family instance");
            
            t.Start();

            // Получаем все элементы труб в проекте
            //FilteredElementCollector pipeCollector = new FilteredElementCollector(doc)
            //    .OfClass(typeof(Pipe))
            //    .WhereElementIsNotElementType();

            var linkedDocs = GetLinkedDocuments(doc);

            Debug.Print($"Found {linkedDocs.Count()} linked docs");

            FilteredElementCollector collector = new FilteredElementCollector(doc);

            IList<Element> rvtLinks = collector
                .OfCategory(BuiltInCategory.OST_RvtLinks)
                .OfClass(typeof(RevitLinkInstance))
                .ToElements();
            Transform totalTransform = default;
            foreach (Element e in rvtLinks)
            {
                var linkInstance = e as RevitLinkInstance;
                if (linkInstance != null)
                {
                    TaskDialog.Show($"Origin", $" {linkInstance.GetTransform().Origin}");
                    Debug.Print($"origin {linkInstance.GetTransform().Origin}");
                    totalTransform = linkInstance.GetTotalTransform();
                }
            }

            int counter = 0;
            foreach (Document linkedDoc in uiapp.Application.Documents)
            {
                var linkedWalls =
                    new FilteredElementCollector(linkedDoc)
                        .OfCategoryId(new ElementId(-2000011))
                        .OfClass(typeof(Wall)).WhereElementIsNotElementType()
                        .Cast<Wall>().Where<Wall>((Func<Wall, bool>)(w => w.CurtainGrid == null))
                        .Where<Wall>((Func<Wall, bool>)(w => w.StructuralUsage == StructuralWallUsage.Bearing));

                Debug.Print($"Found {linkedWalls.Count()} linked walls");

                foreach (Wall wall in linkedWalls)
                {
                    var wallThickness = wall.WallType.Width;
                    var transformedWall
                        = wall.get_Geometry(new Options()).GetTransformed(totalTransform);

                    var bb = wall.get_BoundingBox(null);
                    //var bb = transformedWall.GetBoundingBox();

                    Options opt = new Options();
                    Solid solid = null;
                    foreach (GeometryObject geomObj in transformedWall)
                    {
                        solid = geomObj as Solid;
                        if (solid != null) break;
                    }
                    Solid transformed = SolidUtils.CreateTransformed(solid, totalTransform);

                    Outline outline = new Outline(bb.Min, bb.Max);
                    BoundingBoxIntersectsFilter bFilter = new BoundingBoxIntersectsFilter(outline);

                    ElementIntersectsSolidFilter
                        elementFilter = new ElementIntersectsSolidFilter(transformed);

                    var pipeCollector = new FilteredElementCollector(doc)
                        .OfClass(typeof(Pipe))
                        .WherePasses(bFilter)
                        .WherePasses(elementFilter)
                        .ToElements();

                    var extReferences = HostObjectUtils.GetSideFaces(wall, ShellLayerType.Exterior);
                    Face extFace = linkedDoc.GetElement(extReferences[0].ElementId).GetGeometryObjectFromReference(extReferences[0]) as Face;

                    var intReferences = HostObjectUtils.GetSideFaces(wall, ShellLayerType.Interior);
                    Face intFace = linkedDoc.GetElement(intReferences[0].ElementId).GetGeometryObjectFromReference(intReferences[0]) as Face;

                    foreach (var element in pipeCollector)
                    {
                        var pipe = element as Pipe;
                        if (pipe != null)
                        {
                            var lc = pipe.Location as LocationCurve;
                            var line = lc.Curve as Line;

                            if (line != null)
                            {
                                //Transform transform = new Transform()
                                //line.CreateTransformed()
                                ////var dist = origin.DistanceTo(doc.Origin);
                                //var curve = line.CreateOffset(dist,
                                //    line.Direction);
                            }

                            var pipeStartPoint = lc.Curve.GetEndPoint(0);
                            var pipeEndPoint = lc.Curve.GetEndPoint(1);

                            //var movedStartPoint = new XYZ(pipeStartPoint.X - origin.X, pipeStartPoint.Y - origin.Y,
                            //    pipeStartPoint.Z - origin.Z);
                            //var movedEndPoint = new XYZ(pipeEndPoint.X - origin.X, pipeEndPoint.Y - origin.Y,
                            //    pipeEndPoint.Z - origin.Z);

                            var pipeNormal = pipeEndPoint.Subtract(pipeStartPoint).Normalize();

                            // access the side face
                            IntersectionResultArray intResults = new IntersectionResultArray();
                            IntersectionResultArray extResults = new IntersectionResultArray();

                            var intAr = intFace.Intersect(lc.Curve, out intResults);
                            var extAr = extFace.Intersect(lc.Curve, out extResults);

                            XYZ intPoint = default;
                            XYZ extPoint = default;

                            if (intAr == SetComparisonResult.Overlap & extAr == SetComparisonResult.Overlap)
                            {
                                intPoint = intResults.get_Item(0).XYZPoint;
                                extPoint = extResults.get_Item(0).XYZPoint;

                                //else if (intResults == null & extResults != null)
                                //{
                                //    extPoint = extResults.get_Item(0).XYZPoint; ;
                                //    intPoint = extPoint - (wallThickness / 2 * pipeNormal);
                                //}

                                var intersectionPoint = new XYZ((intPoint.X + extPoint.X) / 2,
                                    (intPoint.Y + extPoint.Y) / 2,
                                    intPoint.Z);

                                //var intPointFromOrigin = new XYZ(intersectionPoint.X + origin.X, intersectionPoint.Y + origin.Y,
                                //    intersectionPoint.Z + origin.Z);

                                //Debug.Print($"point {intersectionPoint} - {intPointFromOrigin}");
                                //var symbol =
                                //    TestHoleTaskCommand.GetFamilySymbolToPlace(doc, Familyname);
                                //FamilyInstance fi = doc.Create.NewFamilyInstance(intersectionPoint, symbol,
                                //    StructuralType.NonStructural);
                                //var basisY = fi.GetTransform().BasisY;

                                //var angle = basisY.AngleTo(pipeNormal);

                                //Line axis = Line.CreateBound(intersectionPoint, intersectionPoint + XYZ.BasisZ);
                                //ElementTransformUtils.RotateElement(doc, fi.Id, axis, -angle);

                                //foreach (var parameter in fi.GetOrderedParameters())
                                //{
                                //    if (parameter.Definition.IntersectingElementName == "ADSK_Размер_Диаметр")
                                //    {
                                //        Debug.Print($"parameter - before {parameter.AsDouble()}");
                                //        var outerDiameter =
                                //            pipe.get_Parameter(BuiltInParameter.RBS_PIPE_OUTER_DIAMETER);
                                //        parameter.Set(outerDiameter.AsDouble() + offset);

                                //    }

                                //    if (parameter.Definition.IntersectingElementName == "ADSK_Размер_Толщина")
                                //    {
                                //        Debug.Print($"parameter толщина - before {parameter.AsDouble()}");

                                //        parameter.Set(wallThickness + offset + 2);

                                //    }
                                //}
                            }
                        }
                        Debug.Print($"стена {wall.Id} - труба {element.Id}");
                        Debug.Print($"{wall.Name} - {wall.Id} - {wall.WallType} - {wall.Width}");
                        counter++;
                    }
                }
                //Debug.Print($"Wall Level {linkedDoc.PathName + " : " + s}");
            }

            Debug.Print($"intersections {counter}");

            t.Commit();

            return Result.Succeeded;

        }




        //public void ConvertCodeToCSharp(Document doc)
        //{
        //    // Setting IncludeNonVisibleObjects property to true
        //    // to retrieve hidden geometry`
        //    Options g_options = new Options();
        //    g_options.IncludeNonVisibleObjects = true;

        //    // Selecting surfaces located in the middle of walls
        //    Dictionary<Wall, Face> center_planar_face = new Dictionary<Wall, Face>();
        //    foreach (Wall wall in new FilteredElementCollector(doc).OfClass(typeof(Wall)))
        //    {
        //        // Iterating through solids of hidden elements
        //        foreach (Solid solid in wall.get_Geometry(g_options))
        //        {
        //            // Getting FaceArray from the solid
        //            foreach (Face planar_face in solid.Faces)
        //            {
        //                // Checking for intersection between the surface and wall location line
        //                // at the Subset value. The location line is always in the center with any settings
        //                if (planar_face.Intersect(wall.Location.Curve) == SetComparisonResult.Subset)
        //                {
        //                    // Selecting only vertical surfaces
        //                    if (!(planar_face.XVector.Z == 0 && planar_face.YVector.Z == 0))
        //                    {
        //                        // Only one surface will be in the dictionary no matter how many surfaces there are
        //                        center_planar_face[wall] = planar_face;
        //                    }
        //                }
        //            }
        //        }
        //    }

        //    // Getting transformation from the link instance
        //    Dictionary<string, object> mep_link_data = GetLinkData(doc, "IronPython_4408_Q_Intersection");
        //    List<Curve> duct_curves = new List<Curve>();
        //    foreach (Duct duct in new FilteredElementCollector((Document)mep_link_data["doc"]).OfClass(typeof(Mechanical.Duct)))
        //    {
        //        // Applying transformation from link instance to the duct location line
        //        // to get the placement of lines in the coordinates of the main file
        //        duct_curves.Add(((Curve)duct.Location.Curve).CreateTransformed(((Transform)mep_link_data["instances"][0]).GetTotalTransform());
        //    }

        //    // Typed variable
        //    Reference<IntersectionResultArray> result_2 = new Reference<IntersectionResultArray>();
        //}

        /// <summary>
        ///     Predicate to determine whether the given
        ///     real number should be considered equal to
        ///     zero, adding fuzz according to the specified
        ///     tolerance
        /// </summary>
        public static bool IsZero(
            double a,
            double tolerance = _eps)
        {
            return tolerance > Math.Abs(a);
        }
        /// <summary>
        ///     Default tolerance used to add fuzz
        ///     for real number equality detection
        /// </summary>
        public const double _eps = 1.0e-9;
        public static XYZ LinePlaneIntersection(Line line, Plane plane, out double lineParameter)
        {
            XYZ planePoint = plane.Origin;
            XYZ planeNormal = plane.Normal;

            XYZ linePoint = line.GetEndPoint(0);

            XYZ lineDirection = (line.GetEndPoint(1) - linePoint);

            // Is the line parallel to the plane, i.e.,
            // perpendicular to the plane normal?

            if (IsZero(planeNormal.DotProduct(lineDirection)))
            {
                lineParameter = double.NaN;
                return null;
            }

            lineParameter = (planeNormal.DotProduct(planePoint) - planeNormal.DotProduct(linePoint)) / planeNormal.DotProduct(lineDirection);
            lineParameter = lineParameter * line.Length; ////

            double start = line.GetEndParameter(0);
            double end = line.GetEndParameter(1);

            //test whether the lineparameter is inside the line using the "isInside" method.
            ////
            if (!line.IsInside(lineParameter))
            {
                lineParameter = double.NaN;
                return null;
            }
            ////
            return linePoint + lineParameter * lineDirection;
        }

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

        //public static Transform PlanarFaceTransform(PlanarFace face)
        //{
        //    return TransformByVectors(XYZ.BasisX, XYZ.BasisY, XYZ.BasisZ, XYZ.Zero, face.Vector(0), face.Vector(1), face.Normal, face.Origin);
        //}
        public static Transform TransformByVectors(XYZ oldX, XYZ oldY, XYZ oldZ, XYZ oldOrigin, XYZ newX, XYZ newY, XYZ newZ, XYZ newOrigin)
        {

            // [new vector] = [transform]*[old vector]
            // [3x1] = [3x4] * [4x1]
            // 
            // [v'x]   [ i*i'  j*i'  k*i'  translationX' ]   [vx]
            // [v'y] = [ i*j'  j*j'  k*j'  translationY' ] * [vy]
            // [v'z]   [ i*k'  j*k'  k*k'  translationZ' ]   [vz]
            // [1 ]
            Transform t = Transform.Identity;

            double xx = oldX.DotProduct(newX);
            double xy = oldX.DotProduct(newY);
            double xz = oldX.DotProduct(newZ);

            double yx = oldY.DotProduct(newX);
            double yy = oldY.DotProduct(newY);
            double yz = oldY.DotProduct(newZ);

            double zx = oldZ.DotProduct(newX);
            double zy = oldZ.DotProduct(newY);
            double zz = oldZ.DotProduct(newZ);

            t.BasisX = new XYZ(xx, xy, xz);
            t.BasisY = new XYZ(yx, yy, yz);
            t.BasisZ = new XYZ(zx, zy, zz);

            // The movement of the origin point 
            // in the old coordinate system

            XYZ translation = newOrigin - oldOrigin;

            // Convert the translation into coordinates 
            // in the new coordinate system

            double translationNewX = xx * translation.X
                                     + yx * translation.Y
                                     + zx * translation.Z;

            double translationNewY = xy * translation.X
                                     + yy * translation.Y
                                     + zy * translation.Z;

            double translationNewZ = xz * translation.X
                                     + yz * translation.Y
                                     + zz * translation.Z;

            t.Origin = new XYZ(-translationNewX, -translationNewY, -translationNewZ);

            return t;
        }

    }


}
