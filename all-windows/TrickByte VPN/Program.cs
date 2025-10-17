using RollbarDotNet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace SmartDNSProxy_VPN_Client
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static Mutex mutex = new Mutex(true, "{61765fa5-dc02-4da5-ba3e-93ead541e349}");
        [STAThread]
        static void Main()
        {
            if (!mutex.WaitOne(TimeSpan.Zero, true))
            {
                NativeMethods.PostMessage(
                    (IntPtr)NativeMethods.HWND_BROADCAST,
                    NativeMethods.WM_SHOWME,
                    IntPtr.Zero,
                    IntPtr.Zero);
                return;
            }

            Rollbar.Init(new RollbarConfig
            {
                AccessToken = "2c64ec1bfb304fda89a0741c91b2513d",
                Environment = "production"
            });
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            NetworkManagment netMgmnt = new NetworkManagment();
            Application.ApplicationExit += new EventHandler(netMgmnt.changeDNSOnExit);

        #if !DEBUG
            Application.ThreadException += (sender, args) =>
            {
                Rollbar.Report(args.Exception);
            };
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                Rollbar.Report(args.ExceptionObject as System.Exception);
            };
        #endif            
            frmMain mainForm = new frmMain();
            if (mainForm.areSettingsSet())
            {
                Application.Run(new frmMain());
            }
            else
            {
                Application.Run(new SplashScreen());
            }            
        }
    }
}