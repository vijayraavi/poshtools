using PowerShellTools.HostService.ServiceManagement.Debugging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PowerShellTools.HostService
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Windows API imports:
        [DllImport("Kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();
        [DllImport("Kernel32.dll")]
        private static extern bool FreeConsole();
        [DllImport("Kernel32.dll")]
        private static extern bool AllocConsole();


        public MainWindow()
        {
            InitializeComponent();

            this.Visibility = System.Windows.Visibility.Hidden;
            this.Topmost = true;
            this.ShowInTaskbar = false;
            //ConsoleManager.Show();
            //Console.WriteLine("Hello");
            //Console.ReadLine();
            //PowerShellDebuggingService dbgService = new PowerShellDebuggingService();
            //dbgService.Execute(". \"C:\\Users\\zhiyd\\Documents\\visual studio 2013\\Projects\\ConsoleApplication2\\ConsoleApplication2\\bin\\Debug\\ConsoleApplication2.exe\"");
        }
    }
}
