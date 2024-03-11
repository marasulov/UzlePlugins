using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.DB.Structure;
using UzlePlugins.Contracts;

namespace UzlePlugins.RevitCore.Services
{
    public class FamilyInsertService : IFamilyInsertService
    {
        private readonly Document _doc;
        private readonly FamilySymbol _familySymbol;
        private List<XYZ> _intersectPoints;
        //private readonly List<string> _familyParameters;
        private readonly ReferenceIntersectionFinder _refFinder;

        public FamilyInsertService(Document doc, FamilySymbol familySymbol/*, List<string> familyParameters*/)
        {
            _doc = doc;
            _familySymbol = familySymbol;
            //_familyParameters = familyParameters;
        }


        public void InsertFamily(FamilySymbol symbol, double offset, double thickness, double diametr, bool isRectancgled)
        {
            //var elementType = pipeElement.GetType().Name;
            //_refFinder.GetStructuralReferences(builtInCategory);
            
            //if (builtInCategory == BuiltInCategory.OST_Floors)
            //{
            //    _intersectPoints = _refFinder.GetIntersectionsPoints(_refFinder.FloorReferences);
            //}
            //else
            //{
            //    _intersectPoints = _refFinder.GetIntersectionsPoints(_refFinder.WallReferences);
            //}

            foreach (var intersectPoint in _intersectPoints)
            {
                FamilyInstance fi = _doc.Create.NewFamilyInstance(intersectPoint, symbol, StructuralType.NonStructural);
                var basisY = fi.GetTransform().BasisY;
                var angle = basisY.AngleTo(_refFinder.Normal);

                Line axis = Line.CreateBound(intersectPoint, intersectPoint + XYZ.BasisZ);
                ElementTransformUtils.RotateElement(_doc, fi.Id, axis, -angle);
                var parameters = fi.GetOrderedParameters();
                if (!isRectancgled)
                {
                    SetCircledFamilyParameter(parameters, "Depth", "Diameter", diametr, offset, thickness);
                }
                else
                {
                    SetRectFamilyParameter(parameters, "Depth", "Height", "Width", diametr, offset,
                        thickness);
                }
            }
        }

        //TODO 
        private void SetCircledFamilyParameter(IList<Parameter> parameters, string familyLength, string familyWidth, double diametr, double offset, double thickness)
        {
            foreach (var parameter in parameters)
            {

                if (parameter.Definition.Name == familyWidth)
                {
                    //var outerDiameter = (element as Pipe).get_Parameter(BuiltInParameter.RBS_PIPE_OUTER_DIAMETER);
                    parameter.Set(diametr + offset);
                }

                if (parameter.Definition.Name != familyLength) continue;
                parameter.Set(thickness + (thickness * offset));


            }
        }

        private void SetRectFamilyParameter(IList<Parameter> parameters, string familyLength, string familyHeight, string familyWidth, double diametr,  double offset, double thickness)
        {
            foreach (var parameter in parameters)
            {
                //var outerDiameter = (element as Pipe).get_Parameter(BuiltInParameter.RBS_PIPE_OUTER_DIAMETER);

                if (parameter.Definition.Name == familyWidth)
                {
                    parameter.Set(diametr + offset);
                }

                if (parameter.Definition.Name == familyHeight)
                {
                    parameter.Set(diametr + offset);
                }

                if (parameter.Definition.Name != familyLength) continue;
                parameter.Set(thickness + (thickness * offset));


            }
        }

        public void InsertFamily(object parameter)
        {
            throw new NotImplementedException();
        }
    }


}
