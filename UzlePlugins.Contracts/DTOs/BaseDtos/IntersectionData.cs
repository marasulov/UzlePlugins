using System;
using System.Collections.Generic;
using System.Text;

namespace UzlePlugins.Contracts.DTOs.BaseDtos
{
    public class IntersectionData
    {
        /// <summary>
        /// Инфо о перечесении
        /// </summary>
        /// <param name="intersectionPoint">Точка пересечения</param>
        /// <param name="intersectingElementName">Имя пересекаемого элемента</param>
        /// <param name="intersectingElementType">Тип пересекаемого элемента</param>
        /// <param name="intersectingElementTypeSize">Размер типа пересекаемого элемента</param>
        /// <param name="intersectionNormal">Нормаль пересечения</param>
        /// <param name="intersectingElementShape">Форма пересекаемого элемента</param>
        public IntersectionData(PointDTO intersectionPoint, string intersectingElementName, string intersectingElementType, double intersectingElementTypeSize, PointDTO intersectionNormal, string intersectingElementShape)
        {
            
            IntersectionPoint = intersectionPoint;
            IntersectingElementName = intersectingElementName;
            IntersectingElementType = intersectingElementType;
            IntersectingElementTypeSize = intersectingElementTypeSize;
            IntersectionNormal = intersectionNormal;
            IntersectingElementShape = intersectingElementShape;
        }

        /// <summary>
        /// Точка пересечения
        /// </summary>
        public PointDTO IntersectionPoint { get; set; }
    
        /// <summary>
        /// Имя пересекаемого элемента
        /// </summary>
        public string IntersectingElementName { get; set; }
    
        /// <summary>
        /// Тип пересекаемого элемента
        /// </summary>
        public string IntersectingElementType { get; set; }
    
        /// <summary>
        /// Размер типа пересекаемого элемента
        /// </summary>
        public double IntersectingElementTypeSize { get; set; }
    
        /// <summary>
        /// Нормаль пересечения
        /// </summary>
        public PointDTO IntersectionNormal { get; set; }
    
        /// <summary>
        /// Форма пересекаемого элемента
        /// </summary>
        public string IntersectingElementShape { get; set; }

    }
}
