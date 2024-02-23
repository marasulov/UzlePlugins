using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.DB.Structure;

namespace UzlePlugins.RevitCore.Services
{
    public class FamilyInserter
    {
        private readonly FamilySymbol _familySymbol;
        private List<XYZ> _intersectPoints;
        private readonly List<string> _familyParameters;
        private readonly ReferenceIntersectionFinder _refFinder;

        public FamilyInserter(FamilySymbol familySymbol, List<string> familyParameters, ReferenceIntersectionFinder refFinder )
        {
            _familySymbol = familySymbol;
            _familyParameters = familyParameters;
            _refFinder = refFinder;
        }


        public void InsertFamily(Document doc, Element pipeElement, double offset, BuiltInCategory builtInCategory, FamilySymbol symbol, bool isCircled)
        {
            
            var elementType = pipeElement.GetType().Name;
            _refFinder.GetStructuralReferences(builtInCategory);
            
            if (builtInCategory == BuiltInCategory.OST_Floors)
            {
                _intersectPoints = _refFinder.GetIntersectionsPoints(_refFinder.FloorReferences);
            }
            else
            {
                _intersectPoints = _refFinder.GetIntersectionsPoints(_refFinder.WallReferences);
            }
            

            foreach (var intersectPoint in _intersectPoints)
            {
                FamilyInstance fi = doc.Create.NewFamilyInstance(intersectPoint, symbol, StructuralType.NonStructural);
                var basisY = fi.GetTransform().BasisY;
                var angle = basisY.AngleTo(_refFinder.Normal);

                Line axis = Line.CreateBound(intersectPoint, intersectPoint + XYZ.BasisZ);
                ElementTransformUtils.RotateElement(doc, fi.Id, axis, -angle);
                var parameters = fi.GetOrderedParameters();
                if (isCircled)
                {
                    SetCircledFamilyParameter(parameters, "Depth", "Diameter", pipeElement, offset, _refFinder);
                }
                else
                {
                    SetRectFamilyParameter(parameters, "Depth", "Height", "Width", pipeElement, offset,
                        _refFinder);
                }
            }
            
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

    }


}
