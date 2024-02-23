using System.Windows;
using UzlePlugins.Vm;

namespace UzlePlugins.Views
{
    /// <summary>
    /// Логика взаимодействия для HoleTaskView.xaml
    /// </summary>
    public partial class HoleTaskView : Window
    {
        public HoleTaskView(HolesVm holesVm)
        {
            InitializeComponent();
            
            DataContext = holesVm;
            holesVm.CloseAction ??= new Action(this.Close);
        }
    }

   
}
