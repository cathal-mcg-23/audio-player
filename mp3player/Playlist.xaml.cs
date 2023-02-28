using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
using System.Windows.Shapes;

namespace mp3player
{

    public class Track
    {
        public String Path;
        public String Filename;
        public String Title;
        public String Artist;
        
        public double LengthSeconds { get; set; }
        public string ListString { get; set; }
        public string Colour 
        { 
            get
            {
                if( IsPlaying )
                {
                    return "White";
                }
                return "#FF18D90C";
            }
            set
            { 
            }
        }


        public bool IsPlaying { get; set; } = false;


        public string LengthMins
        {
            get
            {
                return MainWindow.SecondsToText((int)LengthSeconds) ;
            } 
        }
        

        public Track()
        {
        }

        public Track(string s)
        {
            Path = s;
            //Filename = System.IO.Path.GetFileName(Path);


            var tfile = TagLib.File.Create(s);
            string title = tfile.Tag.Title;
            TimeSpan duration = tfile.Properties.Duration;
            //MessageBox.Show("cccccc" + title);

            LengthSeconds = duration.TotalSeconds;
            Title = tfile.Tag.Title;
            Artist = tfile.Tag.FirstPerformer;
            ListString = Artist + " - " + Title;

            // MessageBox.Show("cc " + tfile.Properties.AudioBitrate + " ww " + tfile.Properties.AudioSampleRate + " ww " + tfile.Properties.AudioChannels + " ww " );
        }


        public Track(string path, int seconds, string listString)
        {
            Path = path;
            ListString = listString;
            LengthSeconds = seconds;
        }

        public override string ToString()
        {
            string x = MainWindow.SecondsToText((int)Math.Floor(LengthSeconds));
            return x + "\t" + ListString;
        }
    }

    public partial class Playlist : Window
    {
        MainWindow MainWindow;
        public List<Track> Tracks = new List<Track>();
        public Track NowPlaying;
        public bool PlaylistChanged = false;
        private const string PlaylistName = "playlist.m3u";

        public Playlist(MainWindow mainWindow)
        {
            InitializeComponent();
            MainWindow = mainWindow;

            // MessageBox.Show("new pls");

            //Lsb_Pl.Items.Refresh();

            Lsb_Pl.ItemsSource = Tracks;
            MakeTracksFromM3U();
        }

        public void InitOnStart(int selected, int playing, double pausePosition, double pauseTotal)
        {
            /*
            Lsb_Pl.SelectedIndex = selected;

            Track t = Tracks.ElementAt(playing);
            MainWindow.LoadTrack(t);
            try
            {
                MainWindow.SeekPauseFromClose(pausePosition, pauseTotal);
            }
            catch( Exception e1 )
            {
                MessageBox.Show("skip :" + e1 );
            }
            */
        }


        public int GetIndexOfNowPlaying()
        {
            return Tracks.IndexOf(NowPlaying);
            // return 0;
        }

        public void WriteCloseM3U()
        {
            //MessageBox.Show("WriteCloseM3U");
            if (PlaylistChanged == false)
            {
                return;
            }

            string s = "#EXTM3U";
            s += "\r\n";

            foreach (Track t in Tracks)
            {
                s += "#EXTINF:";
                s += (int)t.LengthSeconds;
                s += ",";
                s += t.ListString;
                s += "\r\n";
                s += "file" + ":///";
                s += t.Path;
                s += "\r\n";
            }

            // string s = "Hello and Welcome3" + Environment.NewLine;
            File.WriteAllText(PlaylistName, s);
            
        }



        public void MakeTracksFromM3U()
        {
            string m3u = "";
            try
            {
                m3u = File.ReadAllText(PlaylistName);
            }
            catch( FileNotFoundException  )
            {
                return;
            }


            string[] lines = m3u.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            //MessageBox.Show(s111[0]);

            string s = "#EXTM3U";

            if (lines.Length > 1 && lines[0] != s)
            {
                // MessageBox.Show("!!!" + s111[0] + "!!!");
                return;
            }

            // MessageBox.Show(s111[1]);

            for (int i = 1; i + 1 < lines.Length;)
            {
                //MessageBox.Show( "start for " + i );
                string lineA = lines[i];
                string[] lineAByColon = lineA.Split(':');
                //MessageBox.Show("a " + a2[0]);
                if (lineAByColon[0] == "#EXTINF")
                {
                    //MessageBox.Show("a ok" + a2[0]);

                    string[] lineARightByComma = lineAByColon[1].Split(',');
                    try
                    {
                        int seconds = int.Parse(lineARightByComma[0]);
                        string listString = lineARightByComma[1];
                        //MessageBox.Show(seconds + "___" + filename);
                        string path = lines[i + 1];
                        path = path.Substring( 8, path.Length - 8);
                        Track t = new Track( path, seconds, listString );
                        Tracks.Add(t);
                        i++;
                        i++;
                        //MessageBox.Show("end for " + i);
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.Message + "exc " + lineARightByComma[0] + i);
                        i++;
                    }
                }
                else
                {
                    //MessageBox.Show( lineAByColon[0] + "   not # " + i + " " + lineAByColon[0].Length);

                    if (lineAByColon[0] == "#EXTINF")
                    {
                        MessageBox.Show("!!!" + lineAByColon[0] + "!!!");
                    }
                    i++;
                }
            }
        }


        private void ListBox_Drop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

            foreach( string s in files )
            {
                Track t = new Track(s);
                Tracks.Add(t);
            }

            Lsb_Pl.ItemsSource = Tracks;
            Lsb_Pl.Items.Refresh();

            PlaylistChanged = true;
        }

        private void Lsb_Files_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // MessageBox.Show("Lsb_Files_Sel change" + Dg_Playlist.SelectedIndex);
        }

        private void Lsb_Files_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Track t = (Track)Lsb_Pl.SelectedItem;

                MainWindow.PlayTrack(t);
            }
            catch( Exception ex )
            {
                MessageBox.Show("Lsb_Files_MouseDoubleClick err " + ex.Message);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //MessageBox.Show("new Window_Closing");

            e.Cancel = true;
            Hide();
        }

        public void BackTrack()
        {
            int nowPlaying = Tracks.IndexOf(NowPlaying);
            if (nowPlaying > 0 )
            {
                Track t = Tracks.ElementAt(nowPlaying - 1);
                MainWindow.PlayTrack(t);
            }
        }
        public void NextTrack()
        {
            int nowPlaying = Tracks.IndexOf(NowPlaying);
            if (nowPlaying < Tracks.Count - 1)
            {
                Track t = Tracks.ElementAt(nowPlaying + 1);
                MainWindow.PlayTrack(t);
            }
        }

        public Track TrackOne()
        {
            if ( Tracks.Count > 0 )
            {
                Track t = Tracks.ElementAt( 0 );
                return t;
                //MainWindow.PlayTrack(t);
            }
            return null;
        }

        private void Lsb_Pl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Track t = (Track)Lsb_Pl.SelectedItem;

                MainWindow.PlayTrack(t);
                Lsb_Pl.Items.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lsb_Files_MouseDoubleClick err " + ex.Message);
            }

        }


        private void Grid_1_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Grid g = (Grid)sender;
            string s = g.Children.ToString();

            TextBlock t = (TextBlock)g.Children[0];
            TextBlock t2 = (TextBlock)g.Children[1];

            // MessageBox.Show(t.Text + " + " + t.ActualWidth + " + " + t2.ActualWidth + "_" +  g.ActualWidth  );
            double great = g.ActualWidth - t2.ActualWidth - 10;

            t.Width = (great > 0) ? great : 0;
        }


    }
}