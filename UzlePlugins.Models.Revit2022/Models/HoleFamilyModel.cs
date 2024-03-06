using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using UzlePlugins.Contracts;

namespace UzlePlugins.RevitCore.Models
{


    public record HoleFamilyModel<T> : IIntersectionPointZoom
    {
        private readonly UIDocument _uidoc;


        public HoleFamilyModel(UIDocument uidoc, XYZ intersectionPoint, Element intersectingElement)
        {
            _uidoc = uidoc;

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
            UIDocument uidoc,
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
            _uidoc = uidoc;
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

        public void FamilyZoom(int id)
        {
            //XYZ point = new XYZ();// TODO
            //Options opt = new Options();
            //opt.View = _uidoc.ActiveView;
            //GeometryElement geoEle = point.get_Geometry(opt);

            //_uidoc.ActiveView = view
            //UIView.ZoomAndCenterRectangle(m, n);

            _uidoc.ShowElements(new ElementId(id));
        }
    }

}
