namespace UzlePlugins.Models.Revit2022.Services
{
    using UzlePlugins.Contracts.DTOs;
    using Autodesk.Revit.DB;


    public class ElementToDtoConverter
    {
        public EntityDTO Convert(Element element)
        {
            var res = new EntityDTO(
                element.Id.IntegerValue.ToString(),
                element.Name,
                TryGetLocation(element));

            return res;
        }

        private PointDTO? TryGetLocation(Element e)
        {
            return e.Location is LocationPoint lp ?
                new PointDTO(lp.Point.X, lp.Point.X, lp.Point.X) :
                null;
        }
    }
}
