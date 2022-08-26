using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using System.IO.Compression;

namespace AWFPlayer
{
    public partial class MainWindow : Window
    {
        public MediaPlayer mediaPlayer = new MediaPlayer();

        double InitMousePositionX, InitMousePositionY;

        int LineIndex, TextIndex;
        double TotalTextWidth;

        int MainFontSize = 36;
        int SecondFontSize = 28;

        double LastMediaTime = 0.0;

        List<dynamic> JAWF = new List<dynamic>();
        Dictionary<string, dynamic> JLRC = new Dictionary<string, dynamic>();

        AWFCharacterLine MainCharacterLine;
        AWFCharacterLine SecondCharacterLine;

        DispatcherTimer DisplayFlashTimer = new DispatcherTimer();
        DispatcherTimer TextFlashTimer = new DispatcherTimer();

        string LoadType = "";

        bool IsMouseDown = false;

        bool mediaLoop = true;

        public MainWindow()
        {
            InitializeComponent();

            mediaPlayer.MediaEnded += MediaPlayer_MediaEnded;

            this.MainCharacterLine = new AWFCharacterLine();
            this.SecondCharacterLine = new AWFCharacterLine();

            DisplayFlashTimer.Tick += DisplayFlashTimer_Tick;
            TextFlashTimer.Tick += TextFlashTimer_Tick;

            PlayerInit();
        }

        private void MediaPlayer_MediaEnded(object? sender, EventArgs e)
        {
            if (mediaLoop)
            {
                mediaPlayer.Position = new TimeSpan(0, 0, 0);
                mediaPlayer.Play();
            }
        }

        private void PlayerInit()
        {
            DisplayFlashTimer.Interval = new TimeSpan(0, 0, 0, 0, 50);
            DisplayFlashTimer.Stop();

            TextFlashTimer.Interval = new TimeSpan(0, 0, 0, 0, 10);
            TextFlashTimer.Stop();

            LineIndex = 0;
            TextIndex = 0;
            TotalTextWidth = 0;
            LastMediaTime = 0;

            MainColorLine.Items.Clear();
            MainBackLine.Items.Clear();

            SecondBackLine.Items.Clear();

            JAWF.Clear();

            string tipString = "请放入文件";

            var characters = new ObservableCollection<AWFCharacter>();
            foreach (char c in tipString)
            {
                var lineCharacters = new AWFCharacter(c, string.Empty);
                characters.Add(lineCharacters);
            }
            MainBackLineCanvas.Margin = new Thickness(0, 0, 0, 0);
            MainBackLine.Items.Clear();
            MainBackLine.Items.Add(new AWFCharacterLine(characters));
        }

        private void TextFlash()
        {
            int currentLine = LineIndex > 0 ? LineIndex - 1 : 0;
            if (LoadType == "JAWF")
            {
                if (mediaPlayer.Position.TotalMilliseconds > JAWF[currentLine][TextIndex]["starttime"].Value)
                {
                    double startTime = JAWF[currentLine][TextIndex]["starttime"].Value;
                    double endTime = JAWF[currentLine][TextIndex]["endtime"].Value;
                    double timeInterval = endTime - startTime;
                    timeInterval = timeInterval > 0 ? timeInterval : 1;

                    string textString = JAWF[currentLine][TextIndex]["text"].Value;
                    string textAnnotation = JAWF[currentLine][TextIndex]["kana"] == null ? string.Empty : JAWF[currentLine][TextIndex]["kana"].Value;

                    double stringWidth = 0;

                    if (!string.IsNullOrEmpty(textString))
                    {
                        var characters = new ObservableCollection<AWFCharacter>();
                        var annotation = textAnnotation;
                        foreach (char c in textString)
                        {
                            var lineCharacters = new AWFCharacter(c, annotation);
                            characters.Add(lineCharacters);
                            annotation = string.Empty;
                        }

                        GetTextWidthLine.Items.Clear();
                        GetTextWidthLine.Items.Add(new AWFCharacterLine(characters));
                        UpdateLayout();
                        stringWidth = GetTextWidthLine.ActualWidth;
                    }
                    MainColorLine.BeginAnimation(WidthProperty, null);
                    MainColorLine.Width = TotalTextWidth;

                    TotalTextWidth += stringWidth;

                    DoubleAnimation ChangeWidthAnimation = new DoubleAnimation(TotalTextWidth, new Duration(TimeSpan.FromMilliseconds(timeInterval)));
                    MainColorLine.BeginAnimation(WidthProperty, ChangeWidthAnimation);

                    TextIndex++;
                }
                if (MainBackLine.ActualWidth > MainLineGrid.ActualWidth && MainBackLine.ActualWidth + MainBackLineCanvas.Margin.Left > MainLineGrid.ActualWidth)
                {
                    if (MainColorLine.Width > MainLineGrid.ActualWidth / 2)
                    {
                        double leftToMove = MainLineGrid.ActualWidth / 2 - MainColorLine.Width;
                        MainBackLineCanvas.Margin = new Thickness(leftToMove, 0, 0, 0);
                        MainColorLineCanvas.Margin = new Thickness(leftToMove, 0, 0, 0);
                    }
                }
                if (SecondBackLine.ActualWidth > SecondLineGrid.ActualWidth && SecondBackLine.ActualWidth + SecondBackCanvas.Margin.Left > SecondLineGrid.ActualWidth)
                {
                    double leftToMove = (SecondLineGrid.ActualWidth - SecondBackLine.ActualWidth) * (MainColorLine.ActualWidth / MainBackLine.ActualWidth);
                    SecondBackCanvas.Margin = new Thickness(leftToMove, 0, 0, 0);
                }
                if (TextIndex >= JAWF[currentLine].Count)
                {
                    TextFlashTimer.Stop();
                }
            }else if(LoadType == "JLRC")
            {
                if (mediaPlayer.Position.TotalMilliseconds > JLRC["lyric"]["text"][currentLine][TextIndex]["starttime"].Value)
                {
                    double startTime = JLRC["lyric"]["text"][currentLine][TextIndex]["starttime"].Value;
                    double endTime = JLRC["lyric"]["text"][currentLine][TextIndex]["endtime"].Value;
                    double timeInterval = endTime - startTime;
                    timeInterval = timeInterval > 0 ? timeInterval : 1;

                    string textString = JLRC["lyric"]["text"][currentLine][TextIndex]["text"].Value;
                    string textAnnotation = JLRC["lyric"]["text"][currentLine][TextIndex]["annotation"] == null ? string.Empty : JLRC["lyric"]["text"][currentLine][TextIndex]["annotation"].Value;

                    double stringWidth = 0;

                    if (!string.IsNullOrEmpty(textString))
                    {
                        var characters = new ObservableCollection<AWFCharacter>();
                        var annotation = textAnnotation;
                        foreach (char c in textString)
                        {
                            var lineCharacters = new AWFCharacter(c, annotation);
                            characters.Add(lineCharacters);
                            annotation = string.Empty;
                        }

                        GetTextWidthLine.Items.Clear();
                        GetTextWidthLine.Items.Add(new AWFCharacterLine(characters));
                        UpdateLayout();
                        stringWidth = GetTextWidthLine.ActualWidth;
                    }
                    MainColorLine.BeginAnimation(WidthProperty, null);
                    MainColorLine.Width = TotalTextWidth;

                    TotalTextWidth += stringWidth;

                    DoubleAnimation ChangeWidthAnimation = new DoubleAnimation(TotalTextWidth, new Duration(TimeSpan.FromMilliseconds(timeInterval)));
                    MainColorLine.BeginAnimation(WidthProperty, ChangeWidthAnimation);

                    TextIndex++;
                }
                if (MainBackLine.ActualWidth > MainLineGrid.ActualWidth && MainBackLine.ActualWidth + MainBackLineCanvas.Margin.Left > MainLineGrid.ActualWidth)
                {
                    if (MainColorLine.Width > MainLineGrid.ActualWidth / 2)
                    {
                        double leftToMove = MainLineGrid.ActualWidth / 2 - MainColorLine.Width;
                        MainBackLineCanvas.Margin = new Thickness(leftToMove, 0, 0, 0);
                        MainColorLineCanvas.Margin = new Thickness(leftToMove, 0, 0, 0);
                    }
                }
                if (SecondBackLine.ActualWidth > SecondLineGrid.ActualWidth && SecondBackLine.ActualWidth + SecondBackCanvas.Margin.Left > SecondLineGrid.ActualWidth)
                {
                    double leftToMove = (SecondLineGrid.ActualWidth - SecondBackLine.ActualWidth) * (MainColorLine.ActualWidth / MainBackLine.ActualWidth);
                    SecondBackCanvas.Margin = new Thickness(leftToMove, 0, 0, 0);
                }
                if (TextIndex >= JLRC["lyric"]["text"][currentLine].Count)
                {
                    TextFlashTimer.Stop();
                }
            }
        }

        private void TextFlashTimer_Tick(object? sender, EventArgs e)
        {
            TextFlash();
        }

        private void Display()
        {
            if (LoadType == "JAWF")
            {
                if (LineIndex < JAWF.Count && mediaPlayer.Position.TotalMilliseconds.CompareTo(JAWF[LineIndex][0]["starttime"].Value) > 0)
                {
                    TotalTextWidth = 0;
                    TextIndex = 0;

                    StringBuilder MainLineBuilder = new StringBuilder();
                    for (int i = 0; i < JAWF[LineIndex].Count; i++)
                    {
                        MainLineBuilder.Append(JAWF[LineIndex][i]["text"].Value);
                    }
                    string MainLineString = MainLineBuilder.ToString();
                    using var stringReader = new StringReader(MainLineString);
                    string? line = string.Empty;
                    while ((line = stringReader.ReadLine()) != null)
                    {
                        if (string.IsNullOrEmpty(line))
                        {
                            MainCharacterLine = AWFCharacterLine.Blank;
                            continue;
                        }

                        var characters = new ObservableCollection<AWFCharacter>();

                        for (int j = 0; j < JAWF[LineIndex].Count; j++)
                        {
                            string annotation = JAWF[LineIndex][j]["kana"] == null
                                ? string.Empty
                                : JAWF[LineIndex][j]["kana"].Value;
                            foreach (char c in JAWF[LineIndex][j]["text"].Value)
                            {
                                var lineCharacter = new AWFCharacter(c, annotation);
                                characters.Add(lineCharacter);
                                annotation = string.Empty;
                            }
                        }

                        MainCharacterLine = new AWFCharacterLine(characters);
                        MainColorLine.Items.Clear();
                        MainColorLine.Items.Add(MainCharacterLine);
                        MainBackLine.Items.Clear();
                        MainBackLine.Items.Add(MainCharacterLine);
                        UpdateLayout();
                        if (MainBackLine.ActualWidth < MainLineGrid.ActualWidth)
                        {
                            MainBackLineCanvas.Margin = new Thickness((MainLineGrid.ActualWidth - MainBackLine.ActualWidth) / 2, 0, 0, 0);
                            MainColorLineCanvas.Margin = new Thickness((MainLineGrid.ActualWidth - MainBackLine.ActualWidth) / 2, 0, 0, 0);
                        }
                        else
                        {
                            MainBackLineCanvas.Margin = new Thickness(0, 0, 0, 0);
                            MainColorLineCanvas.Margin = new Thickness(0, 0, 0, 0);
                        }
                        MainColorLine.BeginAnimation(WidthProperty, null);
                        MainColorLine.Width = 0;
                    }
                    if (JAWF[0][0]["Translation"] != null && LineIndex < JAWF[0][0]["Translation"].Count)
                    {
                        StringBuilder SecondLineBuilder = new StringBuilder();
                        SecondLineBuilder.Append(JAWF[0][0]["Translation"][LineIndex].Value);
                        string SecondLineString = SecondLineBuilder.ToString();
                        using var secondStringReader = new StringReader(SecondLineString);
                        string? secondLine = string.Empty;
                        while ((secondLine = secondStringReader.ReadLine()) != null)
                        {
                            if (string.IsNullOrEmpty(secondLine))
                            {
                                SecondCharacterLine = AWFCharacterLine.Blank;
                                continue;
                            }

                            var characters = new ObservableCollection<AWFCharacter>();

                            foreach (char character in secondLine)
                            {
                                string annotation = String.Empty;

                                var lineCharacter = new AWFCharacter(character, annotation);
                                characters.Add(lineCharacter);
                            }

                            SecondCharacterLine = new AWFCharacterLine(characters);
                            SecondBackLine.Items.Clear();
                            SecondBackLine.Items.Add(SecondCharacterLine);
                            UpdateLayout();
                            if (SecondBackLine.ActualWidth < SecondLineGrid.ActualWidth)
                            {
                                SecondBackCanvas.Margin = new Thickness((SecondLineGrid.ActualWidth - SecondBackLine.ActualWidth) / 2, 0, 0, 0);
                            }
                            else
                            {
                                SecondBackCanvas.Margin = new Thickness(0, 0, 0, 0);
                            }
                        }
                    }
                    TextFlashTimer.Start();
                    while (LineIndex < JAWF.Count && mediaPlayer.Position.TotalMilliseconds.CompareTo(JAWF[LineIndex][0]["starttime"].Value) > 0)
                    {
                        LineIndex++;
                    }
                }
                if (mediaPlayer.Position.TotalMilliseconds.CompareTo(LastMediaTime) < 0)
                {
                    PlayerInit();
                }
                else
                {
                    LastMediaTime = mediaPlayer.Position.TotalMilliseconds;
                }
            }else if(LoadType == "JLRC")
            {
                if (LineIndex < JLRC["lyric"]["text"].Count && mediaPlayer.Position.TotalMilliseconds.CompareTo(JLRC["lyric"]["text"][LineIndex][0]["starttime"].Value) > 0)
                {
                    TotalTextWidth = 0;
                    TextIndex = 0;

                    StringBuilder MainLineBuilder = new StringBuilder();
                    for (int i = 0; i < JLRC["lyric"]["text"][LineIndex].Count; i++)
                    {
                        MainLineBuilder.Append(JLRC["lyric"]["text"][LineIndex][i]["text"].Value);
                    }
                    string MainLineString = MainLineBuilder.ToString();
                    using var stringReader = new StringReader(MainLineString);
                    string? line = string.Empty;
                    while ((line = stringReader.ReadLine()) != null)
                    {
                        if (string.IsNullOrEmpty(line))
                        {
                            MainCharacterLine = AWFCharacterLine.Blank;
                            continue;
                        }

                        var characters = new ObservableCollection<AWFCharacter>();

                        for (int j = 0; j < JLRC["lyric"]["text"][LineIndex].Count; j++)
                        {
                            string annotation = JLRC["lyric"]["text"][LineIndex][j]["annotation"] == null
                                ? string.Empty
                                : JLRC["lyric"]["text"][LineIndex][j]["annotation"].Value;
                            foreach (char c in JLRC["lyric"]["text"][LineIndex][j]["text"].Value)
                            {
                                var lineCharacter = new AWFCharacter(c, annotation);
                                characters.Add(lineCharacter);
                                annotation = string.Empty;
                            }
                        }

                        MainCharacterLine = new AWFCharacterLine(characters);
                        MainColorLine.Items.Clear();
                        MainColorLine.Items.Add(MainCharacterLine);
                        MainBackLine.Items.Clear();
                        MainBackLine.Items.Add(MainCharacterLine);
                        UpdateLayout();
                        if (MainBackLine.ActualWidth < MainLineGrid.ActualWidth)
                        {
                            MainBackLineCanvas.Margin = new Thickness((MainLineGrid.ActualWidth - MainBackLine.ActualWidth) / 2, 0, 0, 0);
                            MainColorLineCanvas.Margin = new Thickness((MainLineGrid.ActualWidth - MainBackLine.ActualWidth) / 2, 0, 0, 0);
                        }
                        else
                        {
                            MainBackLineCanvas.Margin = new Thickness(0, 0, 0, 0);
                            MainColorLineCanvas.Margin = new Thickness(0, 0, 0, 0);
                        }
                        MainColorLine.BeginAnimation(WidthProperty, null);
                        MainColorLine.Width = 0;
                    }
                    if (JLRC["lyric"]["Translation"] != null && LineIndex < JLRC["lyric"]["Translation"].Count)
                    {
                        StringBuilder SecondLineBuilder = new StringBuilder();
                        SecondLineBuilder.Append(JLRC["lyric"]["Translation"][LineIndex].Value);
                        string SecondLineString = SecondLineBuilder.ToString();
                        using var secondStringReader = new StringReader(SecondLineString);
                        string? secondLine = string.Empty;
                        while ((secondLine = secondStringReader.ReadLine()) != null)
                        {
                            if (string.IsNullOrEmpty(secondLine))
                            {
                                SecondCharacterLine = AWFCharacterLine.Blank;
                                continue;
                            }

                            var characters = new ObservableCollection<AWFCharacter>();

                            foreach (char character in secondLine)
                            {
                                string annotation = String.Empty;

                                var lineCharacter = new AWFCharacter(character, annotation);
                                characters.Add(lineCharacter);
                            }

                            SecondCharacterLine = new AWFCharacterLine(characters);
                            SecondBackLine.Items.Clear();
                            SecondBackLine.Items.Add(SecondCharacterLine);
                            UpdateLayout();
                            if (SecondBackLine.ActualWidth < SecondLineGrid.ActualWidth)
                            {
                                SecondBackCanvas.Margin = new Thickness((SecondLineGrid.ActualWidth - SecondBackLine.ActualWidth) / 2, 0, 0, 0);
                            }
                            else
                            {
                                SecondBackCanvas.Margin = new Thickness(0, 0, 0, 0);
                            }
                        }
                    }
                    TextFlashTimer.Start();
                    while (LineIndex < JLRC["lyric"]["text"].Count && mediaPlayer.Position.TotalMilliseconds.CompareTo(JLRC["lyric"]["text"][LineIndex][0]["starttime"].Value) > 0)
                    {
                        LineIndex++;
                    }
                }
                if (mediaPlayer.Position.TotalMilliseconds.CompareTo(LastMediaTime) < 0)
                {
                    PlayerInit();
                }
                else
                {
                    LastMediaTime = mediaPlayer.Position.TotalMilliseconds;
                }
            }
        }

        private void DisplayFlashTimer_Tick(object? sender, EventArgs e)
        {
            Display();
        }

        private void AWFPlayerWindow_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Link;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
        }

        private void LoadJAWF(string Path)
        {
            try
            {
                PlayerInit();

                string JAWFText = File.ReadAllText(Path, System.Text.Encoding.UTF8);
                JAWF = JsonConvert.DeserializeObject<List<dynamic>>(JAWFText);

                DisplayFlashTimer.Start();
            }
            catch (Exception ex)
            {
                System.Console.Write(ex);
            }

        }

        private string Decompress(byte[] bytes)
        {
            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var bs = new BrotliStream(msi, CompressionMode.Decompress))
                {
                    bs.CopyTo(mso);
                }

                return Encoding.UTF8.GetString(mso.ToArray());
            }
        }

        private void LoadJLRC(string Path)
        {
            try
            {
                PlayerInit();

                string JSON = Decompress(File.ReadAllBytes(Path));
                JLRC = JsonConvert.DeserializeObject<Dictionary<string,dynamic>>(JSON);

                DisplayFlashTimer.Start();
            }
            catch (Exception ex)
            {
                System.Console.Write(ex);
            }

        }

        private void AWFPlayerWindow_Drop(object sender, DragEventArgs e)
        {
            try
            {
                string FileName = (string)((Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0);

                if (System.IO.Path.GetExtension(FileName) == ".jawf")
                {
                    LoadType = "JAWF";
                    LoadJAWF(FileName);
                }
                else if (System.IO.Path.GetExtension(FileName) == ".jlrc")
                {
                    LoadType = "JLRC";
                    LoadJLRC(FileName);
                }
                else
                {
                    mediaPlayer.Open(new Uri(FileName));
                    mediaPlayer.Play();
                }
            }
            catch (Exception ex)
            {
                System.Console.Write(ex);
            }
        }

        private void AWFPlayerWindow_MouseEnter(object sender, MouseEventArgs e)
        {
            AWFPlayerWindow.Background = new SolidColorBrush(Color.FromArgb(125, 0, 0, 0));
        }

        private void AWFPlayerWindow_MouseLeave(object sender, MouseEventArgs e)
        {
            AWFPlayerWindow.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
            IsMouseDown = false;
        }

        private void AWFPlayerWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            InitMousePositionX = Mouse.GetPosition(AWFPlayerWindow).X;
            InitMousePositionY = Mouse.GetPosition(AWFPlayerWindow).Y;

            IsMouseDown = true;
        }

        private void AWFPlayerWindow_MouseUp(object sender, MouseButtonEventArgs e)
        {
            IsMouseDown = false;
        }

        private void AWFPlayerWindow_MouseMove(object sender, MouseEventArgs e)
        {
            if (IsMouseDown)
            {
                AWFPlayerWindow.Top += Mouse.GetPosition(AWFPlayerWindow).Y - InitMousePositionY;
                AWFPlayerWindow.Left += Mouse.GetPosition(AWFPlayerWindow).X - InitMousePositionX;
            }
        }
    }
}
