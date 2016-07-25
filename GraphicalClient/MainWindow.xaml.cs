using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
using bhelper;

namespace PokemonGo.RocketAPI.GUI
{
    public partial class MainWindow : Window
    {
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        private const int GWL_STYLE = -16;
        private const int WS_MAXIMIZEBOX = 0x10000;

        private static Logger logger;

        public static MainWindow main;

        #region Manipulate MainForm gui controls
        public string SetMainFormTitle
        {
            get { return Title.ToString(); }
            set { Dispatcher.Invoke(new Action(() => { Title = value; })); }
        }

        public bool SeStartButtonStatus
        {
            get { return buttonStart.IsEnabled; }
            set { Dispatcher.Invoke(new Action(() => { buttonStart.IsEnabled = value; })); }
        }
        public bool SeStopButtonStatus
        {
            get { return buttonStop.IsEnabled; }
            set { Dispatcher.Invoke(new Action(() => { buttonStop.IsEnabled = value; })); }
        }

        public string SetVersionLabel
        {
            get { return lbl_Version.Content.ToString(); }
            set { Dispatcher.Invoke(new Action(() => { lbl_Version.Content = value; })); }
        }
        #endregion


        public MainWindow()
        {
            InitializeComponent();
            main = this;

            logger = new Logger(Output);
            Console.SetOut(logger);
            Console.SetError(logger);
            
            SetVersionLabel = "^v." + Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        private async void buttonStart_Click(object sender, RoutedEventArgs e)
        {
            await Task.Run(() =>
            {
                try
                {

                    var client = new Client(new bhelper.Settings());
                    Program._hero = new Hero(client);

                    if (Program._hero.ClientSettings.Language == "System")
                    {
                        switch (System.Globalization.CultureInfo.InstalledUICulture.Name)
                        {
                            case "de_DE":
                            case "de_AT":
                            case "de_CH":
                                {
                                    bhelper.Language.LoadLanguageFile("de_DE");
                                    break;
                                }
                            default:
                                {
                                    bhelper.Language.LoadLanguageFile("en_EN");
                                    break;
                                }
                        }
                        bhelper.Main.ColoredConsoleWrite(ConsoleColor.White, $"[{DateTime.Now.ToString("HH:mm:ss")}] " + bhelper.Language.GetPhrases()["detected_sys_language"] + System.Globalization.CultureInfo.InstalledUICulture.DisplayName);
                    }
                    else
                        bhelper.Language.LoadLanguageFile(Program._hero.ClientSettings.Language);
                    bhelper.Main.ColoredConsoleWrite(ConsoleColor.White, $"[{DateTime.Now.ToString("HH:mm:ss")}] " + bhelper.Language.GetPhrases()["loaded_language"] + bhelper.Language.LanguageFile);

                    //if we are on the newest version we should be fine running the bot
                    if (bhelper.Main.CheckVersion(Assembly.GetExecutingAssembly().GetName()))
                    {
                        Program._hero.AllowedToRun = true;
                    }

                    //lets get rolling
                    Program.Execute();
                    //change the button status around
                    SeStartButtonStatus = false;
                    SeStopButtonStatus = true;
                }
                catch (Exceptions.PtcOfflineException) { bhelper.Main.ColoredConsoleWrite(ConsoleColor.Red, "PTC Servers are probably down OR your credentials are wrong. Try google"); }
                catch (System.IO.FileNotFoundException) { bhelper.Main.ColoredConsoleWrite(ConsoleColor.Red, $"Use an existing language!"); }
                catch (Exception ex)
                {
                    bhelper.Main.ColoredConsoleWrite(ConsoleColor.Red, $"[{DateTime.Now.ToString("HH:mm:ss")}] Unhandled exception: {ex}");
                }
            });
        }
        private void buttonStop_Click(object sender, RoutedEventArgs e)
        {
            Program._hero.AllowedToRun = false;
            SeStartButtonStatus = true;
            SeStopButtonStatus = false;
        }

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            var hwnd = new System.Windows.Interop.WindowInteropHelper((Window)sender).Handle;
            var value = GetWindowLong(hwnd, GWL_STYLE);
            SetWindowLong(hwnd, GWL_STYLE, (int)(value & ~WS_MAXIMIZEBOX));
        }

        
    }
}
