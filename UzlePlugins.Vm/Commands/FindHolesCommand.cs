using System;
using UzlePlugins.Contracts;
using UzlePlugins.Contracts.DTOs;

namespace UzlePlugins.Vm.Commands
{
    public class FindHolesCommand : CommandBase
    {
        private readonly IFindHoleService _findHoleService;

        public FindHolesCommand(IFindHoleService findHoleService)
        {
            _findHoleService = findHoleService;
        }

        public event EventHandler? CanExecuteChanged;

        public event Action<AllHolesDto> ResultObtained;

        public override void Execute(object parameter)
        {
            var res = _findHoleService.FindHoles();

            ResultObtained?.Invoke(res);
        }

    }
}
