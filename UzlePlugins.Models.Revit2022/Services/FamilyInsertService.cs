using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using UzlePlugins.Contracts;
using UzlePlugins.Contracts.DTOs;
using UzlePlugins.RevitCore.Services;

namespace UzlePlugins.Models.Revit2022.Services
{
    public class FamilyInsertService : IFamilyInsertService
    {
        private readonly Document _doc;

        //private readonly List<string> _familyParameters;

        public FamilyInsertService(UIDocument uiDocument)
        {
            _doc = uiDocument.Document;
        }


        public void InsertFamily(FamilySymbol symbol, NewHolesDto dto)
        {
            var point = new XYZ(dto.IntersectionPoint.X, dto.IntersectionPoint.Y, dto.IntersectionPoint.Z);
            FamilyInstance fi = _doc.Create.NewFamilyInstance(point, symbol, StructuralType.NonStructural);
            var basisY = fi.GetTransform().BasisY;
            var angle = basisY.AngleTo(new XYZ(dto.IntersectionNormal.X, dto.IntersectionNormal.Y, dto.IntersectionNormal.Z));

            Line axis = Line.CreateBound(point, point + XYZ.BasisZ);
            ElementTransformUtils.RotateElement(_doc, fi.Id, axis, -angle);
            var parameters = fi.GetOrderedParameters();

            var offset = UnitUtils.ConvertToInternalUnits(dto.HoleOffset, UnitTypeId.Millimeters);

            switch (dto.Shape)
            {
                case "Circle":
                    SetCircledFamilyParameter(parameters, "Depth", "Diameter", dto.IntersectingElementTypeSize, offset, dto.SourceThickness);
                    break;
                case "Square" when dto.IntersectingElementType == "Duct" && dto.Width !=0:
                    SetSquaredFamilyParameter(parameters, "Depth", "Height", "Width", dto.Height, dto.Width,
                        offset, dto.SourceThickness);
                    break;
                default:
                    SetRectFamilyParameter(parameters, "Depth", "Height", "Width", dto.IntersectingElementTypeSize,offset, dto.SourceThickness);
                    break;
            }
            //}
        }

        private static void SetCircledFamilyParameter(IList<Parameter> parameters, string familyLength, string familyWidth, double diameter, double offset, double thickness)
        {
            foreach (var parameter in parameters)
            {

                if (parameter.Definition.Name == familyWidth)
                {
                    parameter.Set(diameter + offset * 2);
                }

                if (parameter.Definition.Name != familyLength) continue;
                parameter.Set(thickness + (thickness * offset));

            }
        }

        private static void SetRectFamilyParameter(IList<Parameter> parameters, string familyLength, string familyHeight, string familyWidth, double diameter, double offset, double thickness)
        {
            foreach (var parameter in parameters)
            {

                if (parameter.Definition.Name == familyWidth)
                {
                    parameter.Set(diameter + offset * 2);
                }

                if (parameter.Definition.Name == familyHeight)
                {
                    parameter.Set(diameter + offset * 2);
                }

                if (parameter.Definition.Name != familyLength) continue;
                parameter.Set(thickness + thickness * offset);


            }
        }

        private void SetSquaredFamilyParameter(IList<Parameter> parameters, string familyLength, string familyHeight, string familyWidth, double height, double width, double offset, double thickness)
        {
            foreach (var parameter in parameters)
            {

                if (parameter.Definition.Name == familyWidth)
                {
                    parameter.Set(width + offset * 2);
                }

                if (parameter.Definition.Name == familyHeight)
                {
                    parameter.Set(height + offset * 2);
                }

                if (parameter.Definition.Name != familyLength) continue;
                parameter.Set(thickness + thickness * offset);


            }
        }

        public void InsertFamily(AllHolesDto allHoles)
        {

            var newHoles = allHoles.NewFamiliesDtos.Where(i => i.IsInsert);
            var actualHoles = allHoles.ActualFamiliesDtos.Where(i => i.IsDelete).ToArray();
            var outdatedHoles = allHoles.OutdatedFamiliesDtos.Where(i => i.IsDelete);

            using var t = new Transaction(_doc);
            t.Start("Hole task for walls and floors with pipes");

            foreach (var hole in newHoles)
            {
                FamilyTypeFinderService familyTypeFinderService = new FamilyTypeFinderService();

                var wallsFamilyName = hole.SourceType switch
                {
                    "Wall" => familyTypeFinderService.GetFamilyType(BuiltInCategory.OST_Walls, hole.Shape),
                    "Floor" => familyTypeFinderService.GetFamilyType(BuiltInCategory.OST_Floors, hole.Shape),
                    _ => default
                };

                var wallsFamilyType = familyTypeFinderService.FamilyParameters;

                FamilySymbol symbol = familyTypeFinderService.GetFamilySymbolToPlace(_doc, wallsFamilyName);

                //FamilyInsertService insertService = new FamilyInsertService(_doc, symbol);
                //Reference r = new Reference(pipeElement);
                //ReferenceIntersectionFinder refFinder = new ReferenceIntersectionFinder(doc, pipeElement, view3D);

                
                InsertFamily(symbol, hole);
            }

            var actualHolesId = actualHoles.Select(x => new ElementId(x.Id)).ToArray();

            foreach (var id in actualHolesId)
            {
                _doc.Delete(id);
            }

            var outdatedHolesids = outdatedHoles.Select(x => x.Id).ToArray();

            
            foreach (var id in outdatedHolesids)
            {
                var i = int.Parse(id);
                _doc.Delete(new ElementId(i));
            }

            t.Commit();
        }

        private void DeleteFamily(ICollection<ElementId> deletedElementIds)
        {
            ICollection<ElementId> deletedIdSet = _doc.Delete(deletedElementIds);

            if (0 != deletedIdSet.Count)
            {
                throw new Exception("Deleting the selected elements in Revit failed.");
            }

            TaskDialog.Show("Revit", "The selected element has been removed.");
        }
    }


}
