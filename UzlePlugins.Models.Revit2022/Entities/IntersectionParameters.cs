using Autodesk.Revit.DB;

namespace UzlePlugins.Models.Revit2022.Entities
{
    /// <summary>
    /// Параметры пересечения
    /// </summary>
    public class IntersectionParameters
    {
        /// <summary>
        /// Точка пересечения
        /// </summary>
        public XYZ IntersectionPoint { get; }
    
        /// <summary>
        /// Элемент, с которым произошло пересечение
        /// </summary>
        public Element IntersectingElement { get; }
    
        /// <summary>
        /// Имя элемента, с которым произошло пересечение
        /// </summary>
        public string IntersectingElementName { get; }
    
        /// <summary>
        /// Тип элемента, с которым произошло пересечение
        /// </summary>
        public string IntersectingElementType { get; }
    
        /// <summary>
        /// Нормаль к плоскости пересечения
        /// </summary>
        public XYZ Normal { get; }
    
        /// <summary>
        /// Размер типа элемента, с которым произошло пересечение
        /// </summary>
        public double IntersectingElementTypeSize { get; }
    
        /// <summary>
        /// Высота элемента, с которым произошло пересечение
        /// </summary>
        public double IntersectingElementHeight { get; }
    
        /// <summary>
        /// Ширина элемента, с которым произошло пересечение
        /// </summary>
        public double IntersectingElementWidth { get; }

        public string intersectingElementShape { get; }

        public IntersectionParameters(
            XYZ intersectionPoint, 
            Element intersectingElement, 
            string intersectingElementName, 
            string intersectingElementType, 
            XYZ normal, 
            double intersectingElementTypeSize, 
            double intersectingElementHeight, 
            double intersectingElementWidth, string intShape)
        {
            IntersectionPoint = intersectionPoint;
            IntersectingElement = intersectingElement;
            IntersectingElementName = intersectingElementName;
            IntersectingElementType = intersectingElementType;
            Normal = normal;
            IntersectingElementTypeSize = intersectingElementTypeSize;
            IntersectingElementHeight = intersectingElementHeight;
            IntersectingElementWidth = intersectingElementWidth;
            intersectingElementShape = intShape;
        }

        
    }

}
