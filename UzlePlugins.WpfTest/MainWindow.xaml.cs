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
            HoleModel holeModel = new HoleModel(1,"asdsa","sadsad","sadsad", "sadsad", 55,"asdsad", true, 20, true);
            List<HoleModel> holes = new List<HoleModel>{holeModel};
            HolesVm vm = new HolesVm(holes);
            vm.Title = "a5465456";

            vm.CloseAction ??= new Action(this.Close);
            //vm.Initialize();
            DataContext = vm;
        }
    }
}