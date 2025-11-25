using HelperSylas;
using HelperSylas.ViewModels;
using System.Windows;

namespace HelperSylas
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // 依赖注入 (Dependency Injection)
            ILcuService service = new LcuService();
            this.DataContext = new MainViewModel(service);
        }
    }
}