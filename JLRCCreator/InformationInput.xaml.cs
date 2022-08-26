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
    /// InformationInput.xaml 的交互逻辑
    /// </summary>
    public partial class InformationInput : Page
    {
        private MainWindow ParentWindow;
        private string MediaPath;

        public InformationInput(MainWindow parentWindow, string mediaPath, string title = "", string album = "", string artist = "", double offset = 0, string lrcText = "", bool isSplit = false)
        {
            InitializeComponent();

            ParentWindow = parentWindow;
            MediaPath = mediaPath;

            TitleTextBox.Text = title;
            AlbumTextBox.Text = album;
            ArtistTextBox.Text = artist;
            OffsetTextBox.Text = offset.ToString();
            LyricTextTextBox.Text = lrcText.ToString();
            IsSplitSpaceCheckBox.IsChecked = isSplit;
        }

        private void NextStepButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(LyricTextTextBox.Text))
            {
                MessageBox.Show("未填写歌词！", "错误");
                return;
            }
            string title, album, artist;
            double offset;
            title = string.IsNullOrEmpty(TitleTextBox.Text) ? "无" : TitleTextBox.Text;
            album = string.IsNullOrEmpty(AlbumTextBox.Text) ? "无" : AlbumTextBox.Text;
            artist = string.IsNullOrEmpty(ArtistTextBox.Text) ? "无" : ArtistTextBox.Text;
            offset = string.IsNullOrEmpty(OffsetTextBox.Text) ? 0 : double.Parse(OffsetTextBox.Text);
            string lyricText = LyricTextTextBox.Text;
            bool isSplitBySpace = IsSplitSpaceCheckBox.IsChecked.GetValueOrDefault();
            ParentWindow.MainWindowFrame.Content = new CheckInformation(ParentWindow, MediaPath, title, album, artist, offset, lyricText, isSplitBySpace);
        }
    }
}
