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
using System.Text.Json;
using System.IO;
using System.IO.Compression;

namespace AWFCreator
{
    /// <summary>
    /// Make.xaml 的交互逻辑
    /// </summary>
    public partial class Make : Page
    {
        private MainWindow ParentWindow;
        private Dictionary<string, dynamic> JLRC;

        int CurrentRow = 0, CurrentColumn = 0;
        List<TextBlock> BackTextBlocks = new List<TextBlock>();
        List<TextBlock> FrontTextBlocks = new List<TextBlock>();

        MediaPlayer Player = new MediaPlayer();
        bool PlayerPause = true;

        string MediaPath;

        public Make(MainWindow parentWindow, string mediaPath, Dictionary<string, dynamic> jLRC)
        {
            InitializeComponent();

            ParentWindow = parentWindow;
            JLRC = jLRC;

            processJLRC(JLRC);

            Player.Open(new Uri(mediaPath));
            MeidaNameTextBlock.Text = System.IO.Path.GetFileName(mediaPath);
            MediaPath = mediaPath;

            BackTextBlocks[CurrentRow].Foreground = Brushes.LightSkyBlue;

            ParentWindow.KeyDown += ParentWindow_KeyDown;
        }

        private void LineUp()
        {
            double LineHeightOffset = LyricItemsControlScrollViewer.ExtentHeight / (double)LyricItemsControl.Items.Count;
            if (CurrentRow > 0)
            {
                BackTextBlocks[CurrentRow].Foreground = Brushes.Black;
                CurrentRow--;
                CurrentColumn = 0;
                BackTextBlocks[CurrentRow].Foreground = Brushes.LightSkyBlue;
                FrontTextBlocks[CurrentRow].Text = String.Empty;
            }
            else
            {
                InformationTextBlock.Text = "已经是第一句";
            }
            LyricItemsControlScrollViewer.ScrollToVerticalOffset(CurrentRow * LineHeightOffset);
        }

        private void LineDown()
        {
            double LineHeightOffset = LyricItemsControlScrollViewer.ExtentHeight / (double)LyricItemsControl.Items.Count;
            if (CurrentRow < JLRC["lyric"]["text"].Count - 1)
            {
                BackTextBlocks[CurrentRow].Foreground = Brushes.Black;
                CurrentRow++;
                CurrentColumn = 0;
                BackTextBlocks[CurrentRow].Foreground = Brushes.LightSkyBlue;
                FrontTextBlocks[CurrentRow].Text = String.Empty;
            }
            else
            {
                InformationTextBlock.Text = "已经是最后一句";
            }
            LyricItemsControlScrollViewer.ScrollToVerticalOffset(CurrentRow * LineHeightOffset);
        }

        private void ParentWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (Key.W == e.Key)
            {
                LineUp();
            }
            else if (Key.S == e.Key)
            {
                LineDown();
            }
            else if (Key.A == e.Key)
            {
                FrontTextBlocks[CurrentRow].Text = "";
                CurrentColumn = 0;
                if (CurrentRow > 0)
                {
                    if (JLRC["lyric"]["text"][CurrentRow][CurrentColumn]["starttime"] != 0.0)
                    {
                        Player.Position = TimeSpan.FromMilliseconds(JLRC["lyric"]["text"][CurrentRow][CurrentColumn]["starttime"]);
                    }
                }
            }
            else if (Key.D == e.Key)
            {
                if (CurrentColumn < JLRC["lyric"]["text"][CurrentRow].Count)
                {
                    if (CurrentColumn > 0)
                    {
                        JLRC["lyric"]["text"][CurrentRow][CurrentColumn - 1]["endtime"] = Player.Position.TotalMilliseconds;
                    }
                    JLRC["lyric"]["text"][CurrentRow][CurrentColumn]["starttime"] = Player.Position.TotalMilliseconds;
                    FrontTextBlocks[CurrentRow].Text += JLRC["lyric"]["text"][CurrentRow][CurrentColumn]["text"];
                    CurrentColumn++;
                }
                else
                {
                    JLRC["lyric"]["text"][CurrentRow][CurrentColumn - 1]["endtime"] = Player.Position.TotalMilliseconds;
                    LineDown();
                }
            }
            else if (Key.B == e.Key)
            {
                if (PlayerPause)
                {
                    Player.Play();
                    InformationTextBlock.Text = "歌曲播放";
                }
                else
                {
                    Player.Pause();
                    InformationTextBlock.Text = "歌曲暂停";
                }
                PlayerPause = !PlayerPause;
            }
            else if (Key.Z == e.Key)
            {
                Player.Position -= new TimeSpan(0, 0, 5);
            }
            else if (Key.X == e.Key)
            {
                Player.Position += new TimeSpan(0, 0, 5);
            }
        }

        private void OutputButton_Click(object sender, RoutedEventArgs e)
        {
            string json = JsonSerializer.Serialize(JLRC);
            File.WriteAllBytes(System.IO.Path.GetDirectoryName(MediaPath) + "\\" + System.IO.Path.GetFileNameWithoutExtension(MediaPath) + ".jlrc", Compress(json));
            Player.Stop();
            ParentWindow.KeyDown -= ParentWindow_KeyDown;
            ParentWindow.MainWindowFrame.Content = new StartPage(ParentWindow);
        }

        private byte[] Compress(string s)
        {
            var bytes = Encoding.UTF8.GetBytes(s);

            using(var msi = new MemoryStream(bytes))
                using(var mso = new MemoryStream())
            {
                using(var bs = new BrotliStream(mso, CompressionMode.Compress))
                {
                    msi.CopyTo(bs);
                }
                return mso.ToArray();
            }
        }

        private void processJLRC(Dictionary<string, dynamic> jLRC)
        {
            for (int i = 0; i < jLRC["lyric"]["text"].Count; i++)
            {
                BackTextBlocks.Add(new TextBlock());
                FrontTextBlocks.Add(new TextBlock());
                BackTextBlocks[i].FontSize = 25;
                FrontTextBlocks[i].FontSize = 25;
                FrontTextBlocks[i].Foreground = Brushes.Yellow;
                foreach (Dictionary<string, dynamic> word in jLRC["lyric"]["text"][i])
                {
                    BackTextBlocks[i].Text += word["text"];
                }
                Grid grid = new Grid();
                grid.Children.Add(BackTextBlocks[i]);
                grid.Children.Add(FrontTextBlocks[i]);
                LyricItemsControl.Items.Add(grid);
            }
        }
    }
}
