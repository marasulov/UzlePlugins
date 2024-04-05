using System.Collections.ObjectModel;
using System.Linq;
using Autodesk.Revit.DB;
using UzlePlugins.Contracts.DTOs;

namespace UzlePlugins.Models.Revit2022.Entities
{
    /// <summary>
    /// Модель отверстия
    /// </summary>
    public class HoleFamilyEntity
    {
        /// <summary>
        /// Параметры пересечения
        /// </summary>
        public IntersectionParameters IntersectionParameters { get; }
    
        /// <summary>
        /// Параметры источника
        /// </summary>
        public SourceParameters SourceParameters { get; }
    
        /// <summary>
        /// Флаг, указывающий, является ли отверстие прямоугольным
        /// </summary>
        public bool IsHoleRectangled { get; }

        /// <summary>
        /// Флаг, указывающий, является ли отверстие вставленным
        /// </summary>
        public bool IsInsert { get; }

        private HoleFamilyEntity(IntersectionParameters intersectionParameters, SourceParameters sourceParameters, bool isHoleRectangled, bool isInsert)
        {
            IntersectionParameters = intersectionParameters;
            SourceParameters = sourceParameters;
            IsHoleRectangled = isHoleRectangled;
            IsInsert = isInsert;
        }

        public static HoleFamilyEntity CreateInstance(IntersectionParameters intersectionParameters, SourceParameters sourceParameters, bool isHoleRectangled, bool isInsert)
        {
            return new HoleFamilyEntity(intersectionParameters, sourceParameters, isHoleRectangled, isInsert);
        }

        public double GetOffset(ObservableCollection<OffsetRanges> offsets)
        {
            var convertedSize = UnitUtils.ConvertFromInternalUnits(IntersectionParameters.IntersectingElementTypeSize, UnitTypeId.Millimeters);
            
            return (from offset in offsets
                    where convertedSize > offset.From
                          && convertedSize < offset.To
                    select offset.Offset)
                .FirstOrDefault();
        }
    }

}
