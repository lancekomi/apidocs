using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace SmartDNSProxy_VPN_Client
{
    public partial class frmUpdater : MetroFramework.Forms.MetroForm
    {
        private byte[] downloadedData;
        public string downloadedPath;
        //private string savePath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        private string savePath = System.IO.Path.GetTempPath();
        public frmUpdater()
        {
            InitializeComponent();
        }
        private void frmUpdater_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Hide();
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
            }
        }

        //Connects to a URL and attempts to download the file
        private void downloadData(string url)
        {
            url = url.TrimEnd(Environment.NewLine.ToCharArray());
            progressBar1.Value = 0;

            downloadedData = new byte[0];
            try
            {
                string installPath = savePath + "VPNClient-update";
                System.IO.DirectoryInfo di = new DirectoryInfo(installPath);
                if (di.Exists)
                {
                    File.Delete(installPath + ".zip");
                    foreach (FileInfo file in di.GetFiles())
                    {
                        file.Delete();
                    }
                    System.IO.Directory.Delete(installPath, true);

                }
                //Optional
                statusText.Text = "Connecting...";
                Application.DoEvents();

                //Get a data stream from the url
                WebRequest req = WebRequest.Create(url);
                WebResponse response = req.GetResponse();
                Stream stream = response.GetResponseStream();

                //Download in chuncks
                byte[] buffer = new byte[1024];
                statusText.Text = "Download Data through HTTP";
                //Get Total Size
                int dataLength = (int)response.ContentLength;

                //With the total data we can set up our progress indicators
                progressBar1.Maximum = dataLength;

                statusText.Text = "Downloading...";
                Application.DoEvents();

                //Download to memory
                //Note: adjust the streams here to download directly to the hard drive
                MemoryStream memStream = new MemoryStream();
                while (true)
                {
                    //Try to read the data
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);

                    if (bytesRead == 0)
                    {
                        //Finished downloading
                        progressBar1.Value = progressBar1.Maximum;

                        Application.DoEvents();
                        break;
                    }
                    else
                    {
                        //Write the downloaded data
                        memStream.Write(buffer, 0, bytesRead);

                        //Update the progress bar
                        if (progressBar1.Value + bytesRead <= progressBar1.Maximum)
                        {
                            progressBar1.Value += bytesRead;

                            progressBar1.Refresh();
                            Application.DoEvents();
                        }
                    }
                }

                //Convert the downloaded stream to a byte array
                downloadedData = memStream.ToArray();

                //Clean up
                stream.Close();
                memStream.Close();
                statusText.Text = "File downloaded.";

                System.Threading.Thread.Sleep(5000);

                statusText.Text = "Saving Data...";
                Application.DoEvents();
                //Write the bytes to a file and save file in temp folder
                
                FileStream newFile = new FileStream(savePath + "\\VPNClient-update.zip", FileMode.Create);
                newFile.Write(downloadedData, 0, downloadedData.Length);
                newFile.Close();
                statusText.Text = "Download Data";
                this.TopMost = false;
                installUpdate();
            }
            catch (Exception)
            {
                //May not be connected to the internet
                //Or the URL might not exist
                statusText.Text = "There was an error accessing the URL.";
            }
            
        }
        private void installUpdate() {
            statusText.Text = "Extract files";
            string extractedFile = savePath + "VPNClient-update.zip";
            ZipFile.ExtractToDirectory(extractedFile, savePath + "VPNClient-update");
            statusText.Text = "Delete archive";
            File.Delete(extractedFile);
            this.Close();
            Process.Start(savePath + "\\VPNClient-update\\VPNClientSetup.msi");
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            File.Delete(Path.Combine(desktopPath, Application.ProductName + ".lnk"));
            Invoke(new Action(Application.Exit));
        }
        public void runUpdate()
        {
            downloadData(downloadedPath);
            downloadedPath = downloadedPath.TrimEnd(Environment.NewLine.ToCharArray());
            //Get the last part of the url, ie the file name
            if (downloadedData != null && downloadedData.Length != 0)
            {
                string urlName = downloadedPath;
                if (urlName.EndsWith("/"))
                    urlName = urlName.Substring(0, urlName.Length - 1); //Chop off the last '/'
                urlName = urlName.Substring(urlName.LastIndexOf('/') + 1);
            }
        }
    }
}
