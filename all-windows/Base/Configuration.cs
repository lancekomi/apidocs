using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartDNSProxy_VPN_Client
{
    class Configuration
    {
        public static int autoreconnectTimeInterval = 10000;
        public static int connectionTimerInterval = 30000;
        public static int vpnTimerInterval = 10000;
        public static string socketAddress = "localhost";
        public static int socketPort = 12343;
    }
}
