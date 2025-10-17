using System.Collections.Generic;
using System.Net;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using HtmlAgilityPack;

namespace SmartDNSProxy_VPN_Client
{
    class VPNGate
    {
        public const string vpnGateURL = "http://www.vpngate.net/en/";
        public VPNGate()
        {
            
        }

        // Get L2TP VPN List
        public List<VPNGateData> getVPNDataList()
        {
            string plainHTML = httpGet(vpnGateURL);
            string __VIEWSTATE = string.Empty, __VIEWSTATEGENERATOR = string.Empty, __EVENTVALIDATION = string.Empty;

            HtmlDocument htmlDoc = new HtmlDocument();

            htmlDoc.OptionFixNestedTags = true;

            htmlDoc.LoadHtml(plainHTML);

            if (htmlDoc.DocumentNode != null)
            {
                HtmlNode bodyNode = htmlDoc.DocumentNode.SelectSingleNode("//body");

                if (bodyNode != null)
                {
                    foreach (HtmlNode input in bodyNode.SelectNodes("//input"))
                    {
                        switch(input.Id)
                        {
                            case "__VIEWSTATE":
                                {
                                    __VIEWSTATE = input.Attributes["value"].Value;
                                    break;
                                }
                            case "__VIEWSTATEGENERATOR":
                                {
                                    __VIEWSTATEGENERATOR = input.Attributes["value"].Value;
                                    break;
                                }
                            case "__EVENTVALIDATION":
                                {
                                    __EVENTVALIDATION = input.Attributes["value"].Value;
                                    break;
                                }
                        }
                    }

                }
            }

            var postValue = new NameValueCollection();
            postValue["__VIEWSTATE"] = __VIEWSTATE;
            postValue["__VIEWSTATEGENERATOR"] = __VIEWSTATEGENERATOR;
            postValue["__EVENTVALIDATION"] = __EVENTVALIDATION;
            postValue["C_L2TP"] = "on";
            postValue["Button3"] = "Refresh Servers List";

            // L2TP Page HTML
            string l2tpPlainHTML = httpPost(vpnGateURL, postValue);

            htmlDoc.LoadHtml(l2tpPlainHTML);

            bool startFetch = false;
            int fetchCount = 0;

            List<VPNGateData> vpnGateDataList = new List<VPNGateData>();
            VPNGateData vpnGateData = new VPNGateData();

            foreach (HtmlNode table in htmlDoc.DocumentNode.SelectNodes("//table"))
            {
                if (table.Id == "vpngate_main_table")
                {
                    foreach (HtmlNode row in table.SelectNodes("tr"))
                    {
                        //System.Diagnostics.Debug.WriteLine("row");
                        foreach (HtmlNode cell in row.SelectNodes("//td[contains(@class,'vg_table_row')]"))
                        {
                            if (cell.InnerHtml.Contains("<br>") && !startFetch) startFetch = true;

                            if (startFetch)
                            {
                                if (cell.InnerHtml == string.Empty)
                                    continue;

                                if (cell.InnerHtml.Contains("/images/flags/") && fetchCount > 0)
                                {
                                    fetchCount = 0;
                                    vpnGateDataList.Add(vpnGateData);
                                    vpnGateData = new VPNGateData();
                                }

                                //System.Diagnostics.Debug.WriteLine(fetchCount);

                                switch (fetchCount)
                                {
                                    case 0:
                                        {
                                            // Country
                                            vpnGateData.flagImagePath = cell.ChildNodes[0].GetAttributeValue("src", null);
                                            vpnGateData.country = cell.ChildNodes[2].InnerText;
                                            break;
                                        }
                                    case 1:
                                        {
                                            // IP & Hostname & ISP
                                            vpnGateData.hostname = cell.ChildNodes[0].InnerText;
                                            vpnGateData.ip = cell.ChildNodes[2].InnerText;
                                            if(cell.ChildNodes.ElementAtOrDefault(4) != null) vpnGateData.ispHostName = cell.ChildNodes[4].InnerText;
                                            break;
                                        }
                                    case 2:
                                        {
                                            // Sessions & Uptime & Cumulative Users
                                            vpnGateData.sessions = cell.ChildNodes[0].InnerText;
                                            vpnGateData.uptime = cell.ChildNodes[2].InnerText;
                                            vpnGateData.cumulativeUsers = cell.ChildNodes[4].InnerText;
                                            break;
                                        }
                                    case 3:
                                        {
                                            // Line quality & Throughput and Ping & Cumulative transfers & Logging policy
                                            vpnGateData.line_quality = cell.ChildNodes[0].InnerText;
                                            vpnGateData.ping = cell.ChildNodes[3].InnerText;
                                            vpnGateData.cumulative_transfers = cell.ChildNodes[6].InnerText;
                                            vpnGateData.loggingPolicy = cell.ChildNodes[10].InnerText;
                                            break;
                                        }
                                    case 7:
                                        {
                                            // By
                                            vpnGateData.by = cell.ChildNodes[0].InnerText;
                                            break;
                                        }
                                    case 8:
                                        {
                                            // Score
                                            vpnGateData.score = cell.ChildNodes[0].InnerText;
                                            break;
                                        }
                                    default:
                                        {
                                            //System.Diagnostics.Debug.WriteLine("cell: " + cell.InnerHtml);
                                            break;
                                        }
                                }
                                fetchCount++;

                            }
                        }
                    }
                }
            }
            
            return vpnGateDataList;
        }

        public class VPNGateData
        {
            public string flagImagePath { get; set; }

            public string country { get; set; }

            public string ip { get; set; }

            public string hostname { get; set; }

            public string ispHostName { get; set; }

            public string ping { get; set; }

            public string cumulative_transfers { get; set; }

            public string loggingPolicy { get; set; }

            public string sessions { get; set; }

            public string uptime { get; set; }

            public string cumulativeUsers { get; set; }

            public string line_quality { get; set; }

            public string by { get; set; }

            public string score { get; set; }
        }

        private string httpPost(string url, NameValueCollection postValue)
        {
            using (var client = new WebClient())
            {
                var response = client.UploadValues(url, postValue);
                return Encoding.Default.GetString(response);
            }
        }

        private string httpGet(string url)
        {
            using (var client = new WebClient())
            {
                return client.DownloadString(url);
            }
        }
    }
}
