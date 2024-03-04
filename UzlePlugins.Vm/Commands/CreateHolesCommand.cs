using System;
using System.Windows.Input;
using UzlePlugins.Contracts;
using UzlePlugins.Models;

namespace UzlePlugins.Vm.Commands
{
    public class CreateHolesCommand : CommandBase
    {
        private readonly IFamilyInsertService _familyInsertService;

        public CreateHolesCommand()
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
            _familyInsertService.InsertFamily((HolesVm) parameter);
        }


    }
}

