using System.Windows;
using UzlePlugins.Models;
using UzlePlugins.Vm;

namespace UzlePlugins.WpfTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            HoleModel actualHoles = new HoleModel(1,"5464","324","32432", "3242", 55,"asdsad", true, 20, true);
            HoleModel outdatedHoles = new HoleModel(1,"98797","sadsad","sadsad", "sadsad", 55,"asdsad", true, 20, true);
            HoleModel newHoles = new HoleModel(1,"243","вавы","232", "313", 55,"asdsad", true, 20, true);
            List<HoleModel> holes1 = new List<HoleModel>{actualHoles};
            List<HoleModel> holes2 = new List<HoleModel>{outdatedHoles};
            List<HoleModel> holes3 = new List<HoleModel>{newHoles};
            HolesVm vm = new HolesVm(holes1, holes2, holes3);

            vm.CloseAction ??= new Action(this.Close);
            //vm.Initialize();
            DataContext = vm;
        }
    }
}