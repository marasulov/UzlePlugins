using System.Windows;
using UzlePlugins.Contracts;
using UzlePlugins.Contracts.DTOs;
using UzlePlugins.Vm;
using UzlePlugins.Vm.Commands;

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
            var holes1 = new List<ActualHoleModelDto>();
            var holes2 = new List<NewHolesDto>();
            var holes3 = new List<OutdatedFamilyDto>();

            for (int i = 0; i < 40; i++)
            {
                var actualHoles = new ActualHoleModelDto(i, new PointDTO(1,2,3), "AAA" + i, "324" + i, "32432" + i, 3242 + i, "asdsad", true, 20, true,20, new PointDTO(i+1,i+2,i+3),50, "aaa");
                holes1.Add(actualHoles);
            }

            for (int i = 0; i < 40; i++)
            {
                var actualHoles = new ActualHoleModelDto(i, new PointDTO(1,2,3), "AAA" + i, "324" + i, "32432" + i, 3242 + i, "asdsad", true, 20, true,20, new PointDTO(i+1,i+2,i+3),50, "aaa");
                holes1.Add(actualHoles);
            }

            for (int i = 0; i < 40; i++)
            {
                var actualHoles = new ActualHoleModelDto(i, new PointDTO(1,2,3), "AAA" + i, "324" + i, "32432" + i, 3242 + i, "asdsad", true, 20, true,20, new PointDTO(i+1,i+2,i+3),50, "aaa");
                holes1.Add(actualHoles);
            }
          
            //FindHolesCommand findHolesCommand = new FindHolesCommand()
            //var vm = new HolesVm(holes1, holes2, holes3);

            //vm.CloseAction ??= new Action(this.Close);
            ////vm.Initialize();
            //DataContext = vm;
        }
    }
}