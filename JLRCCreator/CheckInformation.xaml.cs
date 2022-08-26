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
using System.Text.RegularExpressions;

namespace AWFCreator
{
    /// <summary>
    /// CheckInformation.xaml 的交互逻辑
    /// </summary>
    public partial class CheckInformation : Page
    {
        private MainWindow ParentWindow;
        string MediaPath;

        private string JLRCTitle, JLRCAlbum, JLRCArtist;
        private double JLRCOffset;
        private string JLRCText;
        private bool JLRCIsSplitBySpace;

        private Dictionary<string, dynamic> JLRC;

        private Dictionary<string, dynamic> ProcessJLRC(string lrcText, bool isSplitBySpace, string title, string album, string artist, double offset, Dictionary<string, dynamic> extend)
        {
            Dictionary<string, dynamic> jlrc = new Dictionary<string, dynamic>();

            Dictionary<string, dynamic> info = new Dictionary<string, dynamic>();

            info.Add("title", title);
            info.Add("album", album);
            info.Add("artist", artist);
            info.Add("offset", offset);

            info.Add("extend", extend);

            jlrc.Add("info", info);
            jlrc.Add("lyric", ProcessText(lrcText, isSplitBySpace, jlrc));

            return jlrc;
        }

        private Dictionary<string, dynamic> ProcessText(string lrcText, bool isSplitBySpace, Dictionary<string, dynamic> jlrc)
        {
            Dictionary<string, dynamic> lyric = new Dictionary<string, dynamic>();
            List<List<Dictionary<string, dynamic>>> text = new List<List<Dictionary<string, dynamic>>>();

            string[] lines = lrcText.Split("\r\n");

            foreach (string line in lines)
            {
                if (string.IsNullOrEmpty(line) || string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                List<Dictionary<string, dynamic>> processedWords = new List<Dictionary<string, dynamic>>();

                if (isSplitBySpace)
                {
                    string[] words = line.Split(' ');
                    for (int i = 0; i < words.Length - 1; i++)
                    {
                        words[i] = words[i] + " ";
                    }
                    foreach (string word in words)
                    {
                        Dictionary<string, dynamic> processedWord = new Dictionary<string, dynamic>();
                        processedWord.Add("text", word);
                        processedWord.Add("starttime", 0.0);
                        processedWord.Add("endtime", 0.0);

                        processedWords.Add(processedWord);
                    }
                }
                else
                {
                    Regex letterNumberRegex = new Regex(@"[0-9A-Za-z]");
                    Regex puncRegex = new Regex(@"\p{P}");

                    for (int i = 0; i < line.Length; i++)
                    {
                        string letter = line[i].ToString();
                        if (!letterNumberRegex.IsMatch(letter))
                        {
                            Dictionary<string, dynamic> processedWord = new Dictionary<string, dynamic>();
                            processedWord.Add("text", letter);
                            processedWord.Add("starttime", 0.0);
                            processedWord.Add("endtime", 0.0);

                            processedWords.Add(processedWord);
                        }
                        else
                        {
                            int startNum = i;
                            int endNum;
                            for (endNum = i; endNum < line.Length; endNum++)
                            {
                                if (!letterNumberRegex.IsMatch(line[endNum].ToString()) && !puncRegex.IsMatch(line[endNum].ToString()) && line[endNum].ToString() != " ")
                                {
                                    break;
                                }
                            }
                            string[] words = line.Substring(startNum, endNum - startNum).Split(' ');
                            for (int j = 0; j < words.Length; j++)
                            {
                                string word = words[j];
                                if (words.Length > 1 && j < words.Length - 1)
                                {
                                    word += " ";
                                }

                                Dictionary<string, dynamic> processedWord = new Dictionary<string, dynamic>();
                                processedWord.Add("text", word);
                                processedWord.Add("starttime", 0.0);
                                processedWord.Add("endtime", 0.0);

                                processedWords.Add(processedWord);
                            }
                            i = endNum - 1;
                        }
                    }
                }

                text.Add(processedWords);
            }

            lyric.Add("text", text);

            return lyric;
        }

        private void NextStepButton_Click(object sender, RoutedEventArgs e)
        {
            JLRC = ProcessJLRC(JLRCText, JLRCIsSplitBySpace, JLRCTitle, JLRCAlbum, JLRCArtist, JLRCOffset, new Dictionary<string, dynamic>());
            ParentWindow.MainWindowFrame.Content = new Make(ParentWindow, MediaPath, JLRC);
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            ParentWindow.MainWindowFrame.Content = new InformationInput(ParentWindow, MediaPath, JLRCTitle, JLRCAlbum, JLRCArtist, JLRCOffset, JLRCText, JLRCIsSplitBySpace);
        }

        public CheckInformation(MainWindow parentWindow, string mediaPath, string title, string album, string artist, double offset, string text, bool isSplitBySpace)
        {
            InitializeComponent();
            ParentWindow = parentWindow;
            MediaPath = mediaPath;

            JLRCTitle = title;
            JLRCAlbum = album;
            JLRCArtist = artist;
            JLRCOffset = offset;
            JLRCText = text;
            JLRCIsSplitBySpace = isSplitBySpace;

            TitleTextBlock.Text = "标题：" + title;
            AlbumTextBlock.Text = "专辑：" + album;
            ArtistTextBlock.Text = "歌手：" + artist;
            OffsetTextBlock.Text = "偏移量：" + offset.ToString();
            LyricTextTextBox.Text = text.ToString();

            if (isSplitBySpace)
            {
                NextStepButton.Content = "强制按空格分割单词并开始制作";
            }
            else
            {
                NextStepButton.Content = "自动分割单词并开始制作";
            }
        }
    }
}
