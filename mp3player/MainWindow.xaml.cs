using System;
using System.Windows;
using System.Windows.Media;

using System.IO;
using System.Timers;
using System.Threading;
using TagLib;
using System.Windows.Input;
using System.Xaml; 

namespace mp3player
{
    public partial class MainWindow : Window
    {
        public MediaPlayer mediaPlayer;
        bool Paused = false;
        bool CodeEvent = false;

        bool insideVolumeSlider = false;
        bool insideBalanceSlider = false;
        bool insideSeek = false;

        System.Timers.Timer aTimer;
        Playlist Playlist;
        bool CountUp = false;
        bool Stopped = true;
        string Txb_File_Text = "";

        bool PauseFromClose = false;
        int PauseBlink = 0;
        int PauseBlinkMax = 12;
        double PauseTotal = 0;
        double PausePosition = 0;
        const string FileNameSettings = "InitSettings.txt";

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            mediaPlayer = new MediaPlayer();

            aTimer = new System.Timers.Timer();
            aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            aTimer.Interval = 100;

            Playlist = new Playlist(this);


            Top = 100;

            Left = 200;

            Playlist.Top = 800;

            Playlist.Left = 800;

            Playlist.Show();

            InitOnStart();
        }

        private void Mp3_Play_Activated(object sender, EventArgs e)
        {
            //MessageBox.Show("a");
            if (Playlist != null)
            {
                Playlist.Focus();
                // this.Focus();
            }
        }

        private void Mp3_Play_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Playlist.WriteCloseM3U();
            WriteSettings();

            // MessageBox.Show("shutdown now");

            System.Windows.Application.Current.Shutdown();
        }


        private void InitOnStart()
        {
            
            ReadSettings();
            // Playlist.InitOnStart(2, 4, 30, 120);
        }


        private void ReadSettings()
        {
            String settings;
            try
            {
                settings = System.IO.File.ReadAllText(FileNameSettings);
            }
            catch (FileNotFoundException)
            {
                return;
            }
            string[] lines = settings.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            try
            {
                double volVal = double.Parse(lines[1]);
                Slider_Vol.Value = volVal;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return;
            }

            try
            {
                double playListTop = double.Parse(lines[3]);
                double playListLeft = double.Parse(lines[5]);
                Playlist.Top = playListTop;
                Playlist.Left = playListLeft;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return;
            }

            try
            {
                double windowTop = double.Parse(lines[7]);
                double windowLeft = double.Parse(lines[9]);
                Top = windowTop;
                Left = windowLeft;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return;
            }


           // MessageBox.Show("" + lines[1] +  lines[2] + lines[3]);
            
        }


        private void WriteSettings()
        {
            string s = "Volume:";
            s += "\r\n";
            s += "" +  Slider_Vol.Value;
            s += "\r\n";
            s += "\r\n";

            s += "PlaylistTop:";
            s += "\r\n";
            s += "" + Playlist.Top;
            s += "\r\n";
            s += "\r\n";

            s += "PlaylistLeft:";
            s += "\r\n";
            s += "" + Playlist.Left;
            s += "\r\n";
            s += "\r\n";

            s += "WindowTop:";
            s += "\r\n";
            s += "" + Top;
            s += "\r\n";
            s += "\r\n";

            s += "WindowLeft:";
            s += "\r\n";
            s += "" + Left;
            s += "\r\n";
            s += "\r\n";

            s += "pause_position:";
            s += "\r\n";
            s += "" + mediaPlayer.Position.TotalSeconds;
            s += "\r\n";
            s += "\r\n";

            s += "pause_total:";
            s += "\r\n";
            s += "" + 120.0;
            s += "\r\n";
            s += "\r\n";

            s += "NowPlaying:";
            s += "\r\n";
            s += 2;
            s += "\r\n";
            s += "\r\n";

            s += "NowSelected:";
            s += "\r\n";
            s += 4;
            s += "\r\n";
            s += "\r\n";

            s += "file" + ":///";

            s += "\r\n";

            // string s = "Hello and Welcome3" + Environment.NewLine;
            try 
            {

                System.IO.File.WriteAllText(FileNameSettings, s);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return;
            }
        }

        private void Btn_Play_Click(object sender, RoutedEventArgs e)
        {
            if( Paused )
            {
                UnPause();
                return;
            }
            Open();
            Play();
        }


        public void Open(string file)
        {
            mediaPlayer.Open(new System.Uri(file));
        }

        private void Open()
        {
            Track t = Playlist.NowPlaying;
            if (t == null)
            {
                t = Playlist.TrackOne();
                if (t == null)
                {
                    return;
                }
            }
            PlayTrack(t);
        }


        public void LoadTrack(Track t)
        {
            if (Playlist.NowPlaying != null)
            {
                Playlist.NowPlaying.IsPlaying = false;
            }
            Open(t.Path);

            Txb_File.Text = t.ToString();
            t.IsPlaying = true;
            Playlist.NowPlaying = t;

            Playlist.Lsb_Pl.Items.Refresh();
        }

        public void PlayTrack(Track t)
        {
            LoadTrack(t);
            Play();
        }



        public void Play()
        {
            mediaPlayer.Play();
            aTimer.Enabled = true;
            Stopped = false;

            // mediaPlayer.SpeedRatio = 1.8;
            // MessageBox.Show("Play"  );
        }


        private void Btn_Stop_Click(object sender, RoutedEventArgs e)
        {
            aTimer.Enabled = false;

            mediaPlayer.Stop();
            Stopped = true;
            PauseFromClose = Paused = false;

            TxBk_Info.Text = " :  ";
            Prg_Bar.Value = 0;
            SetSliderPosition( 0);

            // UpdateInfo();
        }

        private void Btn_Pause_Click(object sender, RoutedEventArgs e)
        {
            if( Paused )
            {
                UnPause();
            }
            else
            {
                mediaPlayer.Pause();
                PausePosition = mediaPlayer.Position.TotalSeconds;
                PauseTotal = mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds;
                Paused = true;
            }
            UpdateInfo();
        }

        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            try
            {
                Dispatcher.Invoke(() => {
                    // Code causing the exception or requires UI thread access
                    // https://stackoverflow.com/questions/21306810/the-calling-thread-cannot-access-this-object-because-a-different-thread-owns-it
                    UpdateInfo();
                });
            }
            catch (Exception)
            {
                // MessageBox.Show(eee.Message);
            }
        }


        private void UpdateInfoPause()
        {
            string countString = "";
            if ((PauseBlink * 2) < PauseBlinkMax)
            {
                countString = " :  ";
            }
            else
            {
                if (CountUp == true)
                {
                    countString = (SecondsToText((int)PausePosition));
                }
                else
                {
                    countString = (SecondsToText((int)(PauseTotal - PausePosition)));
                }
            }

            TxBk_Info.Text = countString;

            PauseBlink += 1;
            // PauseBlink %= PauseBlinkMax;
            PauseBlink = PauseBlink % PauseBlinkMax;
        }

        private void UpdateInfoPlaying(Duration naturalDuration)
        {
            try
            {
                double total = naturalDuration.TimeSpan.TotalSeconds;
                double Position = mediaPlayer.Position.TotalSeconds;
                double remaining = total - Position;
                // Display the time remaining in the current media.

                if (remaining == 0)
                {
                    // MessageBox.Show("ending");
                    Playlist.NextTrack();
                }

                string countString = "";

                if (CountUp == true)
                {
                    countString = (SecondsToText((int)Math.Floor(Position)));
                }
                else
                {
                    countString = (SecondsToText((int)Math.Floor(remaining)));
                }

                TxBk_Info.Text = countString;

                Prg_Bar.Value = 100 / (total / Position);   //  100 / ( 60 / 30 )  100 / 2 = 50
                SetSliderPosition(100 / (total / Position));
            }
            catch (Exception)
            {
                // MessageBox.Show(eee.Message);
            }

        }

        public void UpdateInfo()
        {
            if ( Paused )
            {
                UpdateInfoPause();
                return;
            }

            Duration naturalDuration = mediaPlayer.NaturalDuration;

            if (naturalDuration.HasTimeSpan == false ) 
            {
                TxBk_Info.Text = ("0:00");
                return;
            }

            UpdateInfoPlaying( naturalDuration );

        }



        private void UnPause()
        {
            if( PauseFromClose )
            {
                mediaPlayer.Position = TimeSpan.FromSeconds(PausePosition);
                PauseFromClose = false;
            }
            
            mediaPlayer.Play();
            Paused = false;
        }


        private void SetSliderPosition( double progressPerCent )
        {
            CodeEvent = true;
            Sld_Position.Value = progressPerCent;
            CodeEvent = false;
        }

        private void Sld_Position_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if( CodeEvent )
            {
                return;
            }
            Seek(Sld_Position.Value);
        }

        public void Seek(double slideProgressPerCent)
        {
            // if stopped Sld_Position.Value = 0, return
            if (Stopped == true)
            {
                SetSliderPosition(0);
                return;
            }

            if( Paused == false)
            {
                double total = mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds;
                double newPos = 0.01 * slideProgressPerCent * total;
                mediaPlayer.Position = TimeSpan.FromSeconds(newPos);
            }
            else
            {
                PausePosition = PauseTotal * slideProgressPerCent * 0.01;
            }

            Prg_Bar.Value = slideProgressPerCent;
            SetSliderPosition(slideProgressPerCent);
        }

        public void skipSeek(double seconds)
        {
            double total = mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds;
            double position = mediaPlayer.Position.TotalSeconds;
            double newPosition = position + seconds;
            mediaPlayer.Position = TimeSpan.FromSeconds(newPosition);

            //double Position = mediaPlayer.Position.TotalSeconds;

            Prg_Bar.Value = 100 / (total / newPosition);
            SetSliderPosition(100 / (total / newPosition));
        }

        public void SeekPauseFromClose(double pausePosition, double pauseTotal)
        {
            PauseFromClose = true;
            PauseTotal = pauseTotal;
            PausePosition = pausePosition;

            int progressPerCent = (int)(PausePosition * 100 / PauseTotal);
            Prg_Bar.Value = progressPerCent;
            SetSliderPosition(progressPerCent);
            Stopped = false;
            Paused = true;
            aTimer.Enabled = true;
        }




        private void TxBk_Info_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            CountUp = !CountUp;
        }

        private void Btn_Next_Click(object sender, RoutedEventArgs e)
        {
            Playlist.NextTrack();
        }

        private void Btn_Back_Click(object sender, RoutedEventArgs e)
        {
            Playlist.BackTrack();
        }


        private void Mp3_Play_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            int valueOfWheel = 0;

            // If the mouse wheel delta is positive
            if (e.Delta > 0)
            {
                valueOfWheel = 1;
            }

            // If the mouse wheel delta is negative
            if (e.Delta < 0)
            {
                valueOfWheel = -1;
            }

            if (insideVolumeSlider)
            {
                Slider_Vol.Value = Slider_Vol.Value + (  valueOfWheel );
                return;
            }
            if (insideBalanceSlider)
            {
                SliderStereo.Value = SliderStereo.Value + (10 * valueOfWheel);
                return;
            }
            if (insideSeek)
            {
                skipSeek( 5 * valueOfWheel);
                return;
            }

            Slider_Vol.Value = Slider_Vol.Value + ( 5 * valueOfWheel);
        }



        private void SliderStereo_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (mediaPlayer == null)
            {
                return;
            }
            try
            {
                mediaPlayer.Balance = SliderStereo.Value / 100;
                Txb_File.Text = "balance: " + (mediaPlayer.Balance * 100);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }


        private void Slider_Vol_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (mediaPlayer == null)
            {
                return;
            }
            try
            {
                mediaPlayer.Volume = Slider_Vol.Value / 100;
                if (insideVolumeSlider)
                {
                    Txb_File.Text = "volume: " + (int)(mediaPlayer.Volume * 100);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }




        private void Slider_Vol_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Txb_File_Text = Txb_File.Text;
            Txb_File.Text = "volume: " + (int)(mediaPlayer.Volume * 100);
            insideVolumeSlider = true;
        }

        private void Slider_Vol_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Txb_File.Text = Txb_File_Text;
            insideVolumeSlider = false;
        }

        private void SliderStereo_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Txb_File_Text = Txb_File.Text;
            Txb_File.Text = "balance: " + (int)(mediaPlayer.Balance * 100);
            insideBalanceSlider = true;
        }

        private void SliderStereo_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Txb_File.Text = Txb_File_Text;
            insideBalanceSlider = false;
        }

        private void Prg_Bar_MouseEnter(object sender, MouseEventArgs e)
        {
            insideSeek = true;
        }

        private void Prg_Bar_MouseLeave(object sender, MouseEventArgs e)
        {
            insideSeek = false;
        }

        private void Slider_Vol_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Slider_Vol.Value = 100;
        }

        private void Slider_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SliderStereo.Value = 0;
        }

        private void Mp3_Play_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void Btn_Playlist_Click(object sender, RoutedEventArgs e)
        {
            if (Playlist != null)
            {
                if (Playlist.IsVisible)
                {
                    Playlist.Hide();
                }
                else
                {
                    Playlist.Show();
                }
            }
            else
            {
                Playlist.Show();
            }
        }

        public static string SecondsToText(int seconds)
        {
            int seconds2 = seconds % 60;
            if (seconds2 < 10)
            {
                return "" + seconds / 60 + ":0" + seconds2;
            }
            return "" + seconds / 60 + ":" + seconds2;
        }

        private void Prg_Bar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {

                double q = e.GetPosition(Prg_Bar).X;

                // MessageBox.Show(q + "");
            }
            catch (Exception w)
            {

                MessageBox.Show(w.Message);
            }
        }
    }
}

