using System;
using System.Windows.Input;
using UzlePlugins.Contracts;
using UzlePlugins.Contracts.DTOs;

namespace UzlePlugins.Vm.Commands
{
    public class CreateHolesCommand : CommandBase
    {
        private readonly IFamilyInsertService _familyInsertService;

        public CreateHolesCommand(IFamilyInsertService familyInsertService)
        {
            _familyInsertService = familyInsertService;
        }

        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public bool CanExecute(object parameter)
        {
            return parameter is HolesVm;
        }

        public override void Execute(object parameter)
        {
            var holesVm = (HolesVm)parameter;
            holesVm.CloseAction?.Invoke();
            var allHoles = new AllHolesDto(holesVm.NewHoles, holesVm.ActualHoles, holesVm.OutdatedHoles);
            
            _familyInsertService?.InsertFamily(allHoles);
        }


    }
}

