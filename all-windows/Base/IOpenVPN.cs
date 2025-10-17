using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartDNSProxy_VPN_Client
{
    interface IOpenVPN
    {
        void updateOpenVPNState(string state);
    }
}
