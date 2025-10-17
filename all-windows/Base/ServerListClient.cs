using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace SmartDNSProxy_VPN_Client
{
    class ServerListClient
    {        
        List<ServerInformation> elemArray = new List<ServerInformation>();
        public ServerInformation[] GetServerList() {
            string responseBody = "";
            string CSVRemotePath = "https://network.glbls.net/vpnnetwork/VPNServerList.csv";
            string localCSVPath = AppDomain.CurrentDomain.BaseDirectory + @"VPNServerList.csv";
            string localCSVBackup = AppDomain.CurrentDomain.BaseDirectory + @"VPNServerList-bak.csv";
            if (CheckInternetConnection() && (Properties.Settings.Default.CSVLastModified != getCSVTimestamp(CSVRemotePath) || !File.Exists(localCSVBackup)))            
            {
                WebClient downloadRequest = new WebClient();
                if (File.Exists(localCSVBackup))
                {
                    File.Delete(localCSVBackup);
                    File.Copy(localCSVPath, localCSVBackup);
                }
                else
                {
                    try
                    {
                        File.Copy(localCSVPath, localCSVBackup);
                    }
                    catch
                    {
                        Debug.WriteLine("File does not exist");
                    }
                }                
                downloadRequest.DownloadFile(CSVRemotePath, localCSVPath);
                Properties.Settings.Default.CSVLastModified = getCSVTimestamp(CSVRemotePath);
                Properties.Settings.Default.Save();
            }

            try
            {
                parseServerListCsv(localCSVPath);
            }
            catch
            {
                parseServerListCsv(localCSVBackup);
            }
            return elemArray.ToArray();

            void parseServerListCsv(string csvPath)
            {
                using (StreamReader reader = new StreamReader(csvPath))
                {
                    responseBody = reader.ReadToEnd();
                }
                string[] serverListSplit = responseBody.Split('\n');
                var ports = new List<string>();
                foreach (string elem in serverListSplit.Skip(1))
                {
                    string[] elements = elem.Split(',');
                    if (elements.Length == 1)
                    {
                        continue;
                    }
                    ServerInformation serverInformation = new ServerInformation();
                    serverInformation.country = elements[0].Trim('"').Trim('\n');
                    serverInformation.city = elements[1].Trim('"').Trim('\n');
                    serverInformation.note = elements[2].Trim('"');
                    serverInformation.protocols = elements[3].Trim('\"').Trim('"').Split('\r');
                    Regex rgx = new Regex("(([a-z0-9-]*)\\.?)*\\.[a-z]{2,3}");
                    var addressDNS = rgx.Match(elements[4]);
                    elements[4] = addressDNS.ToString();
                    serverInformation.DnsAddress = elements[4].Trim('"').Replace("\n", "").Trim();
                    serverInformation.torrentP2P = elements[5].Trim('"');
                    serverInformation.smartVPN = elements[6].Trim('"');
                    serverInformation.ip = elements[7].Trim('"').Trim(' ').Split(';');
                    try
                    {
                        var port = string.Concat(elements[8].Where(c => char.IsDigit(c) || c == ';')).Split(';');
                        foreach (var p in port)
                        {
                            if (string.IsNullOrEmpty(p) || !p.All(char.IsDigit))
                                continue;
                            if (!ports.Contains(p))
                                ports.Add(p);
                        }
                    }
                    catch (Exception)
                    {
                        // csv error, ignore
                    }
                    

                    if (isCityCorrect(serverInformation.city))
                    {
                        if (serverInformation.torrentP2P.ToLower() == "yes" && serverInformation.smartVPN.ToLower() == "yes")
                        {
                            serverInformation.name = serverInformation.city + " - SmartVPN / Torrent";
                        }
                        else
                        {
                            if (serverInformation.torrentP2P.ToLower() == "yes")
                            {
                                serverInformation.name = serverInformation.city + " - Torrent";
                            }
                            else
                            {
                                if (serverInformation.smartVPN.ToLower() == "yes")
                                {
                                    serverInformation.name = serverInformation.city + " - SmartVPN";
                                }
                                else
                                {
                                    serverInformation.name = serverInformation.city;
                                }

                            }
                        }
                    }
                    else
                    {
                        serverInformation.name = serverInformation.DnsAddress;
                    }

                    elemArray.Add(serverInformation);
                }

                var portsToSort = new List<int>();
                foreach (var port in ports)
                {
                    try
                    {
                        portsToSort.Add(Convert.ToInt32(port));
                    }
                    catch (Exception)
                    {
                        // Wrong port
                    }
                }
                portsToSort.Sort();
                ports.Clear();
                foreach (var port in portsToSort)
                {
                    ports.Add(port.ToString());
                }
                frmMain.Instance.SetPorts(ports);
            }
        }
        private static bool CheckInternetConnection()
        {
            try
            {
                using (var client = new WebClient())
                {
                    using (var stream = client.OpenRead("http://www.google.com"))
                    {
                        return true;
                    }
                }
            }
            catch
            {
                return false;
            }
        }
        private static Boolean isCityCorrect(string cityToCheck)
        {
            Regex regex = new Regex(@"^[a-zA-Z0-9\s,]*$");
            return regex.IsMatch(cityToCheck);
        }

        private DateTime getCSVTimestamp(string url)
        {
            HttpWebRequest timestampRequest = (HttpWebRequest)WebRequest.Create(url);
            HttpWebResponse timestampResponse = (HttpWebResponse)timestampRequest.GetResponse();
            WebHeaderCollection CSVHeaderCollection = timestampResponse.Headers;
            DateTime CSVTimestamp = DateTime.Parse(CSVHeaderCollection.Get("Last-Modified"));
            return CSVTimestamp;
        }
    }
}
