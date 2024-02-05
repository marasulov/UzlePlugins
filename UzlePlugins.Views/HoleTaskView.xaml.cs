using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
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
        }
    }
}
