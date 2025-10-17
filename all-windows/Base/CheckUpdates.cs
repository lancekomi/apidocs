using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SmartDNSProxy_VPN_Client
{
    class CheckUpdates
    {
        public string appname = string.Empty;
        public Version version = new Version();
        public string newdownloadlink = string.Empty;

        public CheckUpdates(string uri)
        {
            WebClient client = new WebClient();
            string content = string.Empty;
            Stream stream;

            try
            {
                stream = client.OpenRead(uri);
                StreamReader reader = new StreamReader(stream);
                content = reader.ReadToEnd();
            }
            catch (WebException)
            {
                //MessageBox.Show("We have a error to connect the update servers. Please try again later.","Update error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string[] strContent = content.Split(';');
            if (strContent.Length != 3)
            {
                MessageBox.Show("text file must be in this format \"appname;version;newurl\"");
                return;
            }

            appname = strContent[0];
            version = new Version(strContent[1]);
            newdownloadlink = strContent[2];
        }
    }
}
