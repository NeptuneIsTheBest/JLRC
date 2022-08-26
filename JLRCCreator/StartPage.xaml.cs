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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AWFCreator
{
    /// <summary>
    /// StartPage.xaml 的交互逻辑
    /// </summary>
    public partial class StartPage : Page
    {
        private MainWindow ParentWindow;
        public StartPage(MainWindow parentWindow)
        {
            InitializeComponent();

            ParentWindow = parentWindow;
            ParentWindow.Drop += StartPage_Drop;
        }

        private void StartPage_Drop(object sender, DragEventArgs e)
        {
            try
            {
                string filePath = (string)((Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0);
                filePath = String.IsNullOrEmpty(filePath) ? throw new ArgumentNullException("File path is null or empty.") : filePath;
                string fileExt = System.IO.Path.GetExtension(filePath);

                if (fileExt == ".mp3" || fileExt == "flac")
                {
                    ParentWindow.MainWindowFrame.Content = new InformationInput(ParentWindow, filePath);
                    ParentWindow.Drop -= StartPage_Drop;
                }
            }
            catch { }
        }
    }
}
