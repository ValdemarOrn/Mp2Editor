using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
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
using LowProfile.Core.Extensions;
using Path = System.IO.Path;

namespace Mp2Editor
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();

            Application.Current.DispatcherUnhandledException += (s, e) => { OnUnhandledException(e.Exception); e.Handled = true; };
            AppDomain.CurrentDomain.UnhandledException += (s, e) => { OnUnhandledException(e.ExceptionObject as Exception); };

			DataContext = new MainViewModel();
		}

        public static void OnUnhandledException(Exception e)
        {
            try
            {
                var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                dir = Path.Combine(dir, "Error Logs");
                Directory.CreateDirectory(dir);
                var filename = string.Format("ErrorLog-{0:yyyyMMddHHmmss}.log", DateTime.Now);
                File.WriteAllText(Path.Combine(dir, filename), e.GetTrace());
            }
            catch
            {
                MessageBox.Show("Failed to write error log to disk", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            MessageBox.Show(e.Message, "An Error has occurred", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
