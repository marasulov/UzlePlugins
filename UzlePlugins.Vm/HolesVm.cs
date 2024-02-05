using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using UzlePlugins.Models;
using UzlePlugins.Vm.Base;

namespace UzlePlugins.Vm
{
    public class HolesVm : BaseViewModel
    {
       
        private ObservableCollection<HoleModel> _holes;

        public ObservableCollection<HoleModel> Holes
        {
            get => _holes;
            set
            {
                Set(ref _holes, value);
            }
        }

        public HolesVm()
        {
            
        }

        public void Initialize()
        {
            Holes = new ObservableCollection<HoleModel>
            {
                new HoleModel("first","first description","circle", 25, "wall", "wall material", true, 10, true),
                new HoleModel("second","second description","circle", 25, "wall", "wall material", true, 10, true),
                new HoleModel("third","third description","circle", 25, "wall", "wall material", true, 10, true),
                new HoleModel("fourth","fourth description","circle", 25, "wall", "wall material", true, 10, true)
            };
        }
    }
}
