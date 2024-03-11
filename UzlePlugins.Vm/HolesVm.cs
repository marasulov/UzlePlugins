using System;
using System.Collections.Generic;
using System.Windows.Input;
using UzlePlugins.Contracts;
using UzlePlugins.Contracts.DTOs;
using UzlePlugins.Vm.Base;
using UzlePlugins.Vm.Commands;

namespace UzlePlugins.Vm
{
    public class HolesVm : BaseViewModel
    {
        private List<ActualHoleModelDto> _holes;
        private List<OutdatedFamilyDto> _outdatedHoles;
        private List<ActualHoleModelDto> _newHoles;

        private ICommand _createHolesCommand;
        private bool _buttonClicked;
        private bool _canExecute = true;
        private ICommand _zoomToPointCommand;

        public HolesVm(
            List<ActualHoleModelDto> newHoles, 
            List<ActualHoleModelDto> holes, 
            List<OutdatedFamilyDto> outdatedHoles, 
            ZoomToPointCommand zoomToPointCommand)
        {
            _newHoles = newHoles;
            _holes = holes;
            _outdatedHoles = outdatedHoles;

            CreateHolesCommand = new CreateHolesCommand();
            ZoomToPointCommand = zoomToPointCommand;

        }

        public List<ActualHoleModelDto> Holes
        {
            get => _holes;
            set => Set(ref _holes, value);
        }

        public List<OutdatedFamilyDto> OutdatedHoles
        {
            get => _outdatedHoles;
            set => Set(ref _outdatedHoles, value);
        }

        public List<ActualHoleModelDto> NewHoles
        {
            get => _newHoles;
            set => Set(ref _newHoles, value);
        }




        public bool CanExecute
        {
            get => this._canExecute;

            set
            {
                if (_canExecute == value)
                {
                    return;
                }

                this._canExecute = value;
            }
        }

        public Action CloseAction { get; set; }


        public ICommand CreateHolesCommand
        {
            get => _createHolesCommand;
            set => Set(ref _createHolesCommand, value);
        }

        public ICommand ZoomToPointCommand
        {
            get => _zoomToPointCommand;
            set => Set(ref _zoomToPointCommand, value);
        }

        public void CloseWindow(object parameter)
        {
            string clicked = parameter as string;
            if (clicked == "Clicked")
            {
                _buttonClicked = true;
                CloseAction();
                OnRequestClose(this, new EventArgs());
            }
        }

        public event EventHandler OnRequestClose;

        public bool ButtonClicked
        {
            get => _buttonClicked;
            set => Set(ref _buttonClicked, value);
        }

    }
}
