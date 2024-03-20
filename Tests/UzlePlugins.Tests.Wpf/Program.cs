using Mocks;
using SimpleInjector;
using System;
using UzlePlugins.Contracts;
using UzlePlugins.Views;
using UzlePlugins.Vm;
using UzlePlugins.Vm.Commands;

namespace UzlePlugins.Tests.Wpf
{
    internal class Program
    {
        [STAThread()]
        static void Main(string[] args)
        {
            var container = new Container();
            container.Register<CreateHolesCommand>();
            container.Register<ZoomToPointCommand>();
            container.Register<FindHolesCommand>();
            container.Register<HolesVm>();
            container.Register<HoleTaskView>();

            container.Register<IFamilyInsertService, FamilyInsertService>();
            container.Register<IZoomEntity, ZoomElementService>();
            container.Register<IFindHoleService, FindHolesService>();

            var window = container
                .GetInstance<HoleTaskView>();

            var context = (HolesVm)window.DataContext;
            context.FindHolesCommand.Execute(null);

            window.ShowDialog();
        }
    }
}
