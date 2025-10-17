using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SmartDNSProxy_VPN_Client
{
    class AutoReconnect
    {
        public string getConnectedIPAddress()
        {
            for (var i = 0; i < 10; i++)
            {
                try
                {
                    var httpWebRequest = (HttpWebRequest)WebRequest.Create("https://api.ipify.org");

                    httpWebRequest.Timeout = 5000;
                    var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                    var encoding = ASCIIEncoding.ASCII;
                    using (var reader = new System.IO.StreamReader(httpResponse.GetResponseStream(), encoding))
                    {
                        return reader.ReadToEnd();
                    }
                }
                catch
                {
                    continue;
                }
            }

            return "broken";
        }
    }
}
