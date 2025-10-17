using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartDNSProxy_VPN_Client
{
    class Drivers
    {
        public void installTAPDrivers()
        {            
            string TAPDriverInstallerPath = AppDomain.CurrentDomain.BaseDirectory + @"\TAP-Driver\tap-windows-9.21.2.exe";
            Process TAPDriverInstallationProcess = new Process();
            TAPDriverInstallationProcess.StartInfo = new ProcessStartInfo()
            {
                FileName = TAPDriverInstallerPath,
                Arguments = @"/S",
            };
            TAPDriverInstallationProcess.Start();
            TAPDriverInstallationProcess.WaitForExit();
        }
    }
}
