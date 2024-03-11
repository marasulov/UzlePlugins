using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using SimpleInjector;
using UzlePlugins.Contracts;
using UzlePlugins.Contracts.DTOs;
using UzlePlugins.Models.Revit2022.Models;
using UzlePlugins.Models.Revit2022.Services;
using UzlePlugins.Views;
using UzlePlugins.Vm;
using UzlePlugins.Vm.Commands;

namespace UzlePlugins.Models.Revit2022.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class CreateHoleCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var uiDocument = commandData
                .Application
                .ActiveUIDocument;

            var app = commandData
                .Application
                .Application;

            var container = ConfigureUI();

            container.RegisterInstance(app);
            container.RegisterInstance(uiDocument);
            //container.Register<RevitRepository>();

            container.Register<IZoomEntity, ZoomElementService>();
            
            container.Register<CreateHoleModel>();
           
            var holeInserter = container
                .GetInstance<CreateHoleModel>();

            holeInserter.CreateHole();



            //container.Register<ISettingsReader, SettingsReader>();

            //container.Register<IGetEntities, GetElementsService>();
            //container.Register<IPickEntities, PickElementsService>();

            //container.Register<IWatchDocument, WatchDocumentService>();
            //container.Register<IDeleteEnitity, DeleteElementService>();
            //container.Register<ElementToDTOConverter>();

            var window = container
                .GetInstance<HoleTaskView>();

            window.ShowDialog();

            return Result.Succeeded;
        }


        public static Container ConfigureUI()
        {
            var container = new Container();
            container.Register<CreateHolesCommand>();
            container.Register<ZoomToPointCommand>();
            //container.Register<ZoomEntityCommand>();
            container.Register<HolesVm>();
            container.Register<HoleTaskView>();
            return container;
        }
    }
}
