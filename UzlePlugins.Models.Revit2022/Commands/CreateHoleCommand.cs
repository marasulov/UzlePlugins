using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using SimpleInjector;
using UzlePlugins.Contracts;
using UzlePlugins.Models.Revit2022.Services;
using UzlePlugins.Settings;
using UzlePlugins.Views;
using UzlePlugins.Vm;
using UzlePlugins.Vm.Commands;

namespace UzlePlugins.Models.Revit2022.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class CreateHoleCommand : IExternalCommand
    {
        private static HoleTaskView? _windowInstanse;

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

            container.Register<IFamilyInsertService, FamilyInsertService>();
            container.Register<IZoomEntity, ZoomElementService>();
            container.Register<IFindHoleService, FindHolesService>();
            container.Register<ISettingsReader, SettingsReader>();
            container.Register<EntityToDtoConverter>();

            var window = container
                .GetInstance<HoleTaskView>();

            var context = (HolesVm)window.DataContext;
            context.FindHolesCommand.Execute(null);

            if (_windowInstanse == null)
            {
                _windowInstanse = window;
                window.ShowDialog();
            }
            else
            {
                window.Activate();
            }

            

            return Result.Succeeded;
        }


        public static Container ConfigureUI()
        {
            var container = new Container();
            container.Register<CreateHolesCommand>();
            container.Register<ZoomToPointCommand>();
            container.Register<FindHolesCommand>();
            container.Register<HolesVm>();
            container.Register<HoleTaskView>();
            return container;
        }
    }
}
