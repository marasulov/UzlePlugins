using System.Windows;
using UzlePlugins.Contracts;

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
            var holes1 = new List<IHoleModel>();
            
            //for (int i = 0; i < 40; i++)
            //{
            //    var actualHoles = new ActualHoleModelDto(i,"AAA"+i,"324"+i,"32432"+i, "3242"+i, i,"asdsad", true, 20, true);   
            //    holes1.Add(actualHoles);
            //}
            //ActualHoleModelDto outdatedHoles = new ActualHoleModelDto(1,"98797","sadsad","sadsad", "sadsad", 55,"asdsad", true, 20, true);
            //var newHoles = new OutdatedFamily(1,"243","вавы");
            
            //var holes2 = new List<IHoleModel>{outdatedHoles};
            //var holes3 = new List<IOutdatedFamily>{newHoles};
            //IIntersectionPointZoom zoom = new ;
            //var vm = new HolesVm(holes1, holes2, holes3, zoom);

            //vm.CloseAction ??= new Action(this.Close);
            ////vm.Initialize();
            //DataContext = vm;
        }
    }
}