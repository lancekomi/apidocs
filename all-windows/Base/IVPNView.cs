using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartDNSProxy_VPN_Client
{
    interface IVPNView
    {
        bool isInvokeRequired();
        object Invoke(Delegate method);
        void updateState(string state);
        void addToLog(string log, string message);
        void UpdateConnectionStatistics(long bytesReceived, long bytesTransmitted);
    }
}
