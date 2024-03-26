using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using UzlePlugins.Contracts;

namespace UzlePlugins.Models.Revit2022.Services
{
    public class ZoomElementService : IZoomEntity
    {
        private UIDocument _uiDocument;

        public ZoomElementService(UIDocument uiDocument)
        {
            _uiDocument = uiDocument;
        }

        public void Zoom(int id)
        {
            _uiDocument?.ShowElements(new ElementId(id));
        }
    }
}
