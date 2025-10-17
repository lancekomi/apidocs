using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using IWshRuntimeLibrary;
using Microsoft.CSharp;

namespace SmartDNSProxy_VPN_Client
{
    class AutostartManager
    {
        [DllImport("shell32.dll")]
        static extern bool SHGetSpecialFolderPath(IntPtr hwndOwner, [Out] StringBuilder lpszPath, int nFolder, bool fCreate);

        public static void saveFileInAutostart()
        {
            CreateShortcut(Application.ProductName, Environment.GetFolderPath(Environment.SpecialFolder.Startup), Application.ExecutablePath);
        }

        public static void removeFileInAutostart()
        {
            RemoveShortcutFromAllUsersStartupFolder();
        }

        public static string getAllUsersStartupFolder()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.Startup);
        }


        public static void CreateShortcut(string shortcutName, string shortcutPath, string targetFileLocation)
        {
            string shortcutLocation = System.IO.Path.Combine(shortcutPath, shortcutName + ".lnk");
            WshShell shell = new WshShell();
            IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutLocation);
            shortcut.IconLocation = AppDomain.CurrentDomain.BaseDirectory+"Network-Vpn-icon.ico";           // The icon of the shortcut
            shortcut.TargetPath = targetFileLocation;                 // The path of the file that will launch when the shortcut is run
            // TODO: Remove whole class if unnecessary
            //shortcut.Save();                                    // Save the shortcut
        }

        public static bool RemoveShortcutFromAllUsersStartupFolder()
        {
            bool retVal = false;
            string path = Path.Combine(getAllUsersStartupFolder(), Application.ProductName) + ".lnk";
            if (System.IO.File.Exists(path))
            {
                try
                {
                    System.IO.File.Delete(path);
                    retVal = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format("Unable to remove this application from the Startup list.  Administrative privledges are required to perform this operation.\n\nDetails: SecurityException: {0}", ex.Message), "Update Startup Mode", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            return retVal;
        }
        public static void AddApplicationToStartup()
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
            {
                key.SetValue(Application.ProductName.ToString(), "\"" + Application.ExecutablePath + "\"");
            }
        }
        public static void RemoveApplicationFromStartup()
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
            {
                key.DeleteValue(Application.ProductName.ToString(), false);
            }
        }
    }
}
