using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using UzlePlugins.Models;
using UzlePlugins.Vm.Base;

namespace UzlePlugins.Vm
{
    public class HolesVm : BaseViewModel
    {
       
        private List<HoleModel> _holes;
        private List<HoleModel> _outdatedHoles;
        private List<HoleModel> _newHoles;

        private ICommand _createHolesCommand;
        private bool _buttonClicked;
        private bool _canExecute = true;

        public List<HoleModel> Holes
        {
            get => _holes;
            set => Set(ref _holes, value);
        }

        public List<HoleModel> OutdatedHoles
        {
            get => _outdatedHoles;
            set => Set(ref _outdatedHoles, value);
        }

        public List<HoleModel> NewHoles
        {
            get => _newHoles;
            set => Set(ref _newHoles, value);
        }
        
        public HolesVm(List<HoleModel> holes, List<HoleModel> outdatedHoles, List<HoleModel> newHoles)
        {
            _holes = holes;
            _outdatedHoles = outdatedHoles;
            _newHoles = newHoles;
            CreateHolesCommand = new RelayCommand(CloseWindow, t => this._canExecute);
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

        public void Initialize()
        {
            Holes = new List<HoleModel>
            {
                //new HoleModel(new PointModel(),"first","first description","circle", 25, "wall", "wall material", true, 10, true),
                //new HoleModel(2,"second","second description","circle", 25, "wall", "wall material", true, 10, true),
                //new HoleModel(3,"third","third description","circle", 25, "wall", "wall material", true, 10, true),
                //new HoleModel(4,"fourth","fourth description","circle", 25, "wall", "wall material", true, 10, true)
            };
        }
        public Action CloseAction  { get; set;}
        /// <summary>
        /// Команда для перенумерования
        /// </summary>
        public ICommand CreateHolesCommand
        {
            get => _createHolesCommand;
            set => Set(ref _createHolesCommand, value);
        }

        public void CloseWindow(object parameter)
        {
            string clicked = parameter as string;
            if (clicked == "Clicked")
            {
                _buttonClicked = true;
                CloseAction();
                // OnRequestClose(this, new EventArgs());
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
