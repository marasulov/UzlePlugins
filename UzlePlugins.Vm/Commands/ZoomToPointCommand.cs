using System;
using UzlePlugins.Contracts;

namespace UzlePlugins.Vm.Commands
{
    public class ZoomToPointCommand : CommandBase
    {
        private readonly IIntersectionPointZoom _intersectionPointZoom;

        public ZoomToPointCommand(IIntersectionPointZoom intersectionPointZoom)
        {
            _intersectionPointZoom = intersectionPointZoom;
        }

        public override void Execute(object parameter)
        {
            var id = Convert.ToInt32(parameter);
            _intersectionPointZoom.FamilyZoom(id);
        }
    }
}
