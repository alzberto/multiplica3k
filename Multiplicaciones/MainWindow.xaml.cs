using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Multiplicaciones
{
    public enum Sonidos
    {
        [Description("empezar el juego.wav")]
        Empiece,
        [Description("bien.wav")]
        Bien,
        [Description("fallas.wav")]
        Fallo,
        [Description("fallas 2.wav")]
        Fallo2,
        [Description("fallas 3.wav")]
        Fallo3,
        [Description("10 bien seguidas.wav")]
        DiezSeguidas,
        [Description("fin de juego bien.wav")]
        FinDeJuegoBien,
        [Description("ultima vida.wav")]
        UltimaVida,
        [Description("game over.wav")]
        GameOver,
        [Description("cuco.wav")]
        Cuco,
        [Description("bostezo.wav")]
        Bostezo
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        string MEDIAPATH = Properties.Settings.Default.Path;

        [DllImport("winmm.dll")]
        static extern Int32 mciSendString(string command, StringBuilder buffer, int bufferSize, IntPtr hwndCallback);


        private ICommand _checkCommand;
        public ICommand CheckCommand
        {
            get
            {
                return _checkCommand ?? (_checkCommand = new CommandHandler(() =>
                {
                    CheckResult();
                }));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public string LoQueLlevas { get; set; }

        public int Racha { get; set; }

        private int mal;
        private int bien;
        public string Total { get; set; }

        private int mult1;
        private int mult2;

        private string _paraMultiplicar;
        public string ParaMultiplicar
        {
            get
            {
                return _paraMultiplicar;
            }
            set
            {
                _paraMultiplicar = value;
                OnPropertyChanged("ParaMultiplicar");
            }
        }

        public bool Vida1 { get; set; } = true;
        public bool Vida2 { get; set; } = true;
        public bool Vida3 { get; set; } = true;

        private DispatcherTimer _timer;
        private DateTime _time;

        private TimeSpan _best { get; set; } = TimeSpan.MaxValue;
        public string Best { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            this.Loaded += MainWindow_Loaded;

            //MediaPlayer myPlayer = new MediaPlayer();
            //myPlayer.Open(new System.Uri(Path.Combine(MEDIAPATH, Sonidos.Empiece.DescriptionAttr())));
            //myPlayer.Play();

            Portada.Content = Properties.Settings.Default.Title;

            CleanStart();

            System.Threading.Thread.Sleep(1500);

            var backgroundMusic = Path.Combine(Properties.Settings.Default.Path, Properties.Settings.Default.BackgroundMusic);
            mciSendString($"open {backgroundMusic} type waveaudio alias tolrato", null, 0, IntPtr.Zero);
            mciSendString(@"play tolrato", null, 0, IntPtr.Zero);
        }

        private void t_Tick(object sender, EventArgs e)
        {
            var time = (DateTime.Now - _time);
            TimeLabel.Content = time.ToString("mm':'ss");

            if (time.Seconds == 57)
            {
                EscucharSonido(Sonidos.Bostezo);
            }
            else if (time.Seconds == 28)
            {
                EscucharSonido(Sonidos.Cuco);
            }
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            NuevaCuenta();
        }

        private void NuevaCuenta()
        {
            var random = new Random();

            var pre1 = mult1;
            mult1 = random.Next(1, 10);

            var pre2 = mult2;
            mult2 = random.Next(1, 10);

            if (pre1 == mult1 && pre2 == mult2)
            {
                //NuevaCuenta();
                mult1 = random.Next(1, 10);
                mult2 = random.Next(1, 10);
            }
            ParaMultiplicar = mult1 + " * " + mult2;
        }

        private void TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                CheckResult();
            }
        }

        private void CheckResult()
        {
            if (string.IsNullOrEmpty(RestTextBox.Text.Trim()))
            {
                return;
            }

            if (RestTextBox.Text.Trim() == (mult1 * mult2).ToString())
            {
                //MessageBox.Show("Biennnn");
                LoQueLlevas = "YEAH" + Environment.NewLine + LoQueLlevas;
                NuevaCuenta();
                Racha++;
                bien++;
                EscucharSonido(Sonidos.Bien);

                if (Racha % 10 == 0)
                {
                    EscucharSonido(Sonidos.DiezSeguidas);
                }
                if (bien + mal == 25)
                {
                    _timer.Stop();

                    var tiempoTotal = (DateTime.Now - _time);

                    EscucharSonido(Sonidos.FinDeJuegoBien);

                    if (Vida3)
                    {
                        MessageBox.Show($"HAS TERMINADO!{Environment.NewLine}Tu tiempo es {tiempoTotal.Minutes} minutos y {tiempoTotal.Seconds} segundos" +
                            $"{Environment.NewLine}{Environment.NewLine}" +
                            $"TE HAS PASADO EL JUEGO! ENHORABUENA!! Ahora intenta mejorar tu tiempo ^_^");
                    }
                    else if (Vida2)
                    {
                        MessageBox.Show($"HAS TERMINADO!{Environment.NewLine}Tu tiempo es {tiempoTotal.Minutes} minutos y {tiempoTotal.Seconds} segundos" +
                            $"{(Vida3 ? string.Empty : $"{Environment.NewLine}(15 segundos extra por fallo)")}{Environment.NewLine}{Environment.NewLine}" +
                            $"CASI! REVISA LA QUE FALLASTE" + Environment.NewLine + Environment.NewLine + LoQueLlevas.Replace($"YEAH{Environment.NewLine}", string.Empty));
                    }
                    else
                    {
                        MessageBox.Show($"HAS TERMINADO!{Environment.NewLine}Tu tiempo es {tiempoTotal.Minutes} minutos y {tiempoTotal.Seconds} segundos" +
                            $"{(Vida3 ? string.Empty : $"{Environment.NewLine}(15 segundos extra por fallo)")}{Environment.NewLine}{Environment.NewLine}" +
                            $"BUEEEENO! REVISA LAS QUE FALLASTE" + Environment.NewLine + Environment.NewLine + LoQueLlevas.Replace($"YEAH{Environment.NewLine}", string.Empty));
                    }

                    CleanStart();

                    if (tiempoTotal < _best)
                    {
                        Best = $"{tiempoTotal.Minutes} minutos y {tiempoTotal.Seconds} segundos";
                        OnPropertyChanged("Best");
                        _best = tiempoTotal;
                    }
                }
            }
            else
            {
                _time = _time.Subtract(new TimeSpan(0, 0, 15));

                //MessageBox.Show("Fatal!!!!");
                LoQueLlevas = $"MAL!! {ParaMultiplicar} NO ES {RestTextBox.Text}" + Environment.NewLine + LoQueLlevas;
                Racha = 0;
                mal++;

                var random = new Random();
                switch (random.Next(1, 4))
                {
                    case 1:
                        EscucharSonido(Sonidos.Fallo);
                        break;
                    case 2:
                        EscucharSonido(Sonidos.Fallo2);
                        break;
                    case 3:
                        EscucharSonido(Sonidos.Fallo3);
                        break;
                }

                if (Vida3)
                {
                    Vida3 = false;
                    OnPropertyChanged("Vida3");
                }
                else if (Vida2)
                {
                    Vida2 = false;
                    //EscucharSonido(Sonidos.UltimaVida);
                    OnPropertyChanged("Vida2");
                }
                else if (Vida1)
                {
                    Vida1 = false;
                    EscucharSonido(Sonidos.GameOver);
                    MessageBox.Show("GAME OVER! ERES TORPE!!!!" + Environment.NewLine + Environment.NewLine + LoQueLlevas.Replace($"YEAH{Environment.NewLine}", string.Empty));
                    CleanStart();
                }
            }
            Total = bien + " de " + (bien + mal);

            OnPropertyChanged("Total");
            OnPropertyChanged("Racha");
            OnPropertyChanged("LoQueLlevas");
            RestTextBox.Text = string.Empty;
        }

        private void CleanStart()
        {
            LoQueLlevas = string.Empty;
            Total = string.Empty;
            Racha = 0;
            bien = 0;
            mal = 0;
            Vida2 = true;
            Vida3 = true;
            OnPropertyChanged("Vida2");
            OnPropertyChanged("Vida3");
            TimeLabel.Content = "00:00";

            EscucharSonido(Sonidos.Empiece);

            _time = DateTime.Now;
            _timer = new DispatcherTimer(new TimeSpan(0, 0, 0, 0, 100), DispatcherPriority.Background,
                t_Tick, Dispatcher.CurrentDispatcher);
            _timer.IsEnabled = true;

            RestTextBox.Focus();
        }

        private void EscucharSonido(Sonidos sonido)
        {
            System.Media.SoundPlayer player = new System.Media.SoundPlayer(Path.Combine(MEDIAPATH, sonido.DescriptionAttr()));
            player.Play();
        }

        private void RestTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
    }

}
