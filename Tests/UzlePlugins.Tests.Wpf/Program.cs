using Mocks;
using SimpleInjector;
using System;
using UzlePlugins.Contracts;
using UzlePlugins.Settings;
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
            container.Register<FillOffsetSettingsCommand>();
            container.Register<SaveOffsetsCommand>();

            container.Register<HolesVm>();
            container.Register<HoleTaskView>();

            container.Register<IFamilyInsertService, FamilyInsertService>();
            container.Register<IZoomEntity, ZoomElementService>();
            container.Register<IFindHoleService, FindHolesService>();
            container.Register<IOffsetManagerService, OffsetManagerService>();

            var window = container
                .GetInstance<HoleTaskView>();

            var context = (HolesVm)window.DataContext;
            context.FindHolesCommand.Execute(null);
            context.FillOffsetSettingsCommand.Execute(null);
            context.SelectedOffsets = context.DuctOffsets;

            window.ShowDialog();
        }
    }
}
