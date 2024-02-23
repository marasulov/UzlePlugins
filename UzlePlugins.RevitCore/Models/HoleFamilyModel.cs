using Autodesk.Revit.DB;
using System.Collections.Generic;

namespace UzlePlugins.RevitCore.Models
{
    

    public record HoleFamilyModel<T>
    {
        public HoleFamilyModel(XYZ intersectionPoint, Element intersectingElement)
        {
            IntersectionPoint = intersectionPoint;
            IntersectingElement = intersectingElement;
        }

        /// <summary>
        /// Информация для семейства отверстия
        /// </summary>
        /// <param name="intersectionPoint"></param>
        /// <param name="intersectingElement"></param>
        /// <param name="intersectingElementName"></param>
        /// <param name="intersectingElementType"></param>
        /// <param name="intersectingElementTypeSize"></param>
        /// <param name="intersectedSourceType"></param>
        /// <param name="intersectedSourceMaterial"></param>
        /// <param name="holeFormType"></param>
        /// <param name="holeOffset"></param>
        /// <param name="isInsert"></param>
        public HoleFamilyModel(
            XYZ intersectionPoint, 
            Element intersectingElement, 
            string intersectingElementName, 
            string intersectingElementType,
            double intersectingElementTypeSize, 
            string intersectedSourceType,
            bool isHoleRectangled, 
            double holeOffset, 
            bool isInsert)
        {
            IntersectionPoint = intersectionPoint;
            IntersectingElement = intersectingElement;
            IntersectingElementName = intersectingElementName;
            IntersectingElementType = intersectingElementType;
            IntersectingElementTypeSize = intersectingElementTypeSize;
            IntersectedSourceType = intersectedSourceType;
            IsHoleRectangled = isHoleRectangled;
            HoleOffset = holeOffset;
            IsInsert = isInsert;
        }

        /// <summary>
        /// Точка пересечения
        /// </summary>
        public XYZ IntersectionPoint { get; set; }

        /// <summary>
        /// Пересекающийся элемент
        /// </summary>
        public Element IntersectingElement { get; set; }

        /// <summary>
        /// Название пересекающего элемента
        /// </summary>
        public string IntersectingElementName{ get; set; }

        /// <summary>
        /// Тип пересекающего элемента
        /// </summary>
        public string IntersectingElementType { get; set; }

        /// <summary>
        /// Размер пересекающего элемента
        /// </summary>
        public double IntersectingElementTypeSize { get; set; }
        /// <summary>
        /// Тип материала пересеченной основы
        /// </summary>
        public string IntersectedSourceType { get; set; }

        /// <summary>
        /// Форма проема
        /// </summary>
        public bool IsHoleRectangled { get; set; }

        /// <summary>
        /// Смещение
        /// </summary>
        public double HoleOffset { get; set; } 

        /// <summary>
        /// Вставка
        /// </summary>
        public bool IsInsert { get; set; }
    }

}
