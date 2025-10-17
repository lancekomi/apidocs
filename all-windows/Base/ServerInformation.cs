using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartDNSProxy_VPN_Client
{
    class ServerInformation
    {
        public string DnsAddress;
        public string country;
        public string city;
        public string[] protocols;
        public string name;
        public string note;
        public string torrentP2P;
        public string smartVPN;
        public string[] ip;

        public override string ToString() {
            return name;
        }

        public string ServerName
        {
            get
            {
                return name;
            }
        }
    }
}
