using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Hpl.HrmDatabase.Services;
using Hpl.HrmDatabase.ViewModels;
using Microsoft.Extensions.Options;
using Serilog;
using Unosquare.PassCore.Common;
using Unosquare.PassCore.PasswordProvider;

namespace Hpl.Acm
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static PasswordChangeOptions _options;
        private static ILogger _logger;
        private static IPasswordChangeProvider _passwordChangeProvider;

        //Declare a _timer of type System.Threading
        private static Timer _timer;

        //Local Variable of type int
        private static int _count = 1;

        public MainWindow(ILogger logger, IOptions<PasswordChangeOptions> options, IPasswordChangeProvider passwordChangeProvider)
        {
            InitializeComponent();

            _logger = logger;
            _options = options.Value;
            _passwordChangeProvider = passwordChangeProvider;

            _logger.Information("MainWindow");
            //Initialization of _timer 
            _timer = new Timer(async x => { await CallTimerMethode(); }, null, Timeout.Infinite, Timeout.Infinite);
            SetupTimer();
        }

        private static void SetupTimer()
        {
            DateTime currentTime = DateTime.Now.AddDays(1);
            DateTime timerRunningTime = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, 2, 0, 0);
            //timerRunningTime = timerRunningTime.AddDays(15);
            //DateTime timerRunningTime = DateTime.Now.AddMinutes(1);

            double tickTime = (timerRunningTime - DateTime.Now).TotalSeconds;

            _timer.Change(TimeSpan.FromSeconds(tickTime), TimeSpan.FromSeconds(tickTime));
        }

        private async Task CallTimerMethode()
        {
            _logger.Information("----START HAI PHAT LAND ACM----");
            WriteToConsole("----START HAI PHAT LAND ACM----");
            int backDate = -1;
            try
            {
                //backDate = int.Parse(configuration.GetSection("AppSettings:BackDateSchedule").Value);
                backDate = int.Parse(_options.BackDateSchedule);
            }
            catch (Exception e)
            {
                _logger.Error("Error get value BackDateSchedule: " + e);
            }

            var listNvs = GetAllNhanVienErrorUser(backDate);

            _logger.Information("----TỔNG SỐ HỒ SƠ XỬ LÝ: " + listNvs.Count);
            WriteToConsole("----TỔNG SỐ HỒ SƠ XỬ LÝ: " + listNvs.Count);

            HplServices hplServices = new HplServices(_passwordChangeProvider, _logger);
            await hplServices.CreateUserAllSys(listNvs);
        }

        private async Task CallTimerMethodeTest()
        {
            _logger.Information("----START HAI PHAT LAND ACM----");
            WriteToConsole("----START HAI PHAT LAND ACM----");
            int backDate = -1;
            try
            {
                //backDate = int.Parse(configuration.GetSection("AppSettings:BackDateSchedule").Value);
                backDate = int.Parse(_options.BackDateSchedule);
            }
            catch (Exception e)
            {
                _logger.Error("Error get value BackDateSchedule: " + e);
            }

            var listNvs = GetAllNhanVienErrorUser(backDate);
            _logger.Information("TỔNG SỐ HỒ SƠ XỬ LÝ: " + listNvs.Count);
            WriteToConsole("----TỔNG SỐ HỒ SƠ XỬ LÝ: " + listNvs.Count);
        }

        public static List<NhanVienViewModel> GetAllNhanVienErrorUser(int backDate)
        {
            var dt = DateTime.Now.AddDays(backDate);

            var listNvs = UserService.GetAllNhanVienErrorUser(dt);

            return listNvs;
        }

        private void WriteToConsole(string message)
        {
            Dispatcher?.Invoke(() => { RtbConsole.AppendText(DateTime.Now.ToString("G") + ": " + message + "\r"); });
        }

        private void BtnStartSync_Click(object sender, RoutedEventArgs e)
        {
            _logger.Information("Test DI " + _options.BackDateSchedule + " " +
                                _passwordChangeProvider.MeasureNewPasswordDistance("abc", "cbs"));
        }

        private void MainWindows_Loaded(object sender, RoutedEventArgs e)
        {
            _logger.Information("MainWindows_Loaded");
        }

        private void MainWindows_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _logger.Information("MainWindows_Closing");
        }
    }
}
