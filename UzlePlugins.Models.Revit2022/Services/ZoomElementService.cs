using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using UzlePlugins.Contracts;
using UzlePlugins.Contracts.DTOs;

namespace UzlePlugins.Models.Revit2022.Services
{
    public class ZoomElementService : IZoomEntity
    {
        private UIDocument _uiDocument;

        public ZoomElementService(UIDocument uiDocument)
        {
            _uiDocument = uiDocument;
        }



        //public void Zoom(EntityDTO element)
        //{
        //    var ids = new ElementId[]
        //    {
        //        new ElementId(int.Parse(element.Id))
        //    };

        //    _uiDocument?.ShowElements(ids);
        //}
        public void Zoom(int id)
        {
            _uiDocument?.ShowElements(new ElementId(id));
        }
    }
}
