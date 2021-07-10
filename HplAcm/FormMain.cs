using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Extensions.Options;
using Serilog;
using Unosquare.PassCore.Common;
using Unosquare.PassCore.PasswordProvider;
using Timer = System.Threading.Timer;

namespace HplAcm
{
    public partial class FormMain : Form
    {
        private static PasswordChangeOptions _options;
        private static ILogger _logger;
        private static IPasswordChangeProvider _passwordChangeProvider;

        //Declare a _timer of type System.Threading  
        private static Timer _timer;

        //Local Variable of type int  
        private static int _count = 1;

        public FormMain(ILogger logger, IOptions<PasswordChangeOptions> options, IPasswordChangeProvider passwordChangeProvider)
        {
            _logger = logger;
            _options = options.Value;
            _passwordChangeProvider = passwordChangeProvider;
            InitializeComponent();


            //Initialization of _timer   
            _timer = new Timer(x => { CallTimerMethode(); }, null, Timeout.Infinite, Timeout.Infinite);
            Setup_Timer();
            Console.ReadKey();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {

        }

        /// <summary>  
        /// This method will print timer executed time and increase the count  with 1.   
        /// </summary>  
        private static void CallTimerMethode()
        {
            Console.WriteLine($"Timer Executed {_count} times.");
            _count = _count + 1;
        }

        /// <summary>  
        /// This method will set the timer execution time and will change the tick time of timer.
        /// </summary>  
        private static void Setup_Timer()
        {
            //DateTime currentTime = DateTime.Now;
            //DateTime timerRunningTime = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, 2, 0, 0);
            //timerRunningTime = timerRunningTime.AddDays(15);
            DateTime timerRunningTime = DateTime.Now.AddMinutes(1);

            double tickTime = (double)(timerRunningTime - DateTime.Now).TotalSeconds;

            _timer.Change(TimeSpan.FromSeconds(tickTime), TimeSpan.FromSeconds(tickTime));
        }
    }
}
