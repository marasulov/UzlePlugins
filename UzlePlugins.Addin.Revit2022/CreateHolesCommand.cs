using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using UzlePlugins.Contracts;
using SimpleInjector;
using UzlePlugins.Models.Revit2022.Models;
using UzlePlugins.Settings;
using UzlePlugins.Views;

namespace UzlePlugins.Addin.Revit2022
{

    [Transaction(TransactionMode.Manual)]
    public class CreateHolesCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var uiDocument = commandData
                .Application
                .ActiveUIDocument;

            var app = commandData
                .Application
                .Application;

            var container = new Container();
            container.RegisterInstance(app);
            container.RegisterInstance(uiDocument);
            //container.Register<RevitRepository>();  
            container.Register<CreateHoleModel>();
            
            var holeInserter = container
                .GetInstance<CreateHoleModel>();

            holeInserter.CreateHole();



            //container.Register<ISettingsReader, SettingsReader>();

            //container.Register<IGetEntities, GetElementsService>();
            //container.Register<IPickEntities, PickElementsService>();
            //container.Register<IZoomEntity, ZoomElementService>();
            //container.Register<IWatchDocument, WatchDocumentService>();
            //container.Register<IDeleteEnitity, DeleteElementService>();
            //container.Register<ElementToDTOConverter>();

            //var window = container
            //    .GetInstance<MainWindow>();

            //window.ShowDialog();

            return Result.Succeeded;
        }


        //public static Container ConfigureUI()
        //{
        //    var container = new Container();
        //    container.Register<GetEntitiesCommand>();
        //    container.Register<PickEntitiesCommand>();
        //    container.Register<ZoomEntityCommand>();
        //    container.Register<MainViewModel>();
        //    container.Register<HoleTaskView>();
        //    return container;
        //}
    }
}