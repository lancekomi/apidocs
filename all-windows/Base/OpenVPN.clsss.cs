using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace SmartDNSProxy_VPN_Client
{
    class OpenVPN
    {
        public string ca, cert, key;
        public List<Config> config;
        public string openVPNPath = AppDomain.CurrentDomain.BaseDirectory + @"\OpenVPN";
        public string logFileName = "log.txt";
        public string caFileName = "ca.txt";
        public string certFileName = "cert.txt";
        public string keyFileName = "key.txt";
        public Process pOpenVPN;
        public string certificateFile = "";
        public string passwordFile = "";
        public bool socketShouldBeConnected = false;
        private bool isOpenVPNConnected = false;
        public IOpenVPN openVPNDelegate = null;
        private Socket socket;
        private Thread socketManagementThread;
        private Action OnVPNConnectedCallback;

        public OpenVPN()
        {
            clearConfig();
        }

        private void clearConfig()
        {
            ca = string.Empty;
            cert = string.Empty;
            key = string.Empty;

            config = new List<Config>();
        }

        public void connect(OpenVPNConnectionInfo connectionInfo, Action OnOpenVPnConnectedCallback)
        {
            this.OnVPNConnectedCallback = OnOpenVPnConnectedCallback;

            string openVPNParams = string.Empty;

            string certificateFile = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString() + ".txt";
            string passwordFile = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString() + ".txt";
            TextWriter twcf = new StreamWriter(certificateFile, true);
            twcf.WriteLine("-----BEGIN CERTIFICATE-----");
            twcf.WriteLine("MIIFOTCCBCGgAwIBAgIJALHEFe9IQlCzMA0GCSqGSIb3DQEBCwUAMIHDMQswCQYD");
            twcf.WriteLine("VQQGEwJTQzENMAsGA1UECBMETWFoZTERMA8GA1UEBxMIVmljdG9yaWExHTAbBgNV");
            twcf.WriteLine("BAoTFEdsb2JhbCBTdGVhbHRoLCBJbmMuMQwwCgYDVQQLEwNWUE4xIDAeBgNVBAMT");
            twcf.WriteLine("F0dsb2JhbCBTdGVhbHRoLCBJbmMuIENBMRswGQYDVQQpExJzZXJ2ZXJsb2NhdGlv");
            twcf.WriteLine("bi1rZXkxJjAkBgkqhkiG9w0BCQEWF2FkbWluQHNlcnZlcmxvY2F0aW9uLmNvMB4X");
            twcf.WriteLine("DTE1MDIyNTIwMDIzMFoXDTI1MDIyMjIwMDIzMFowgcMxCzAJBgNVBAYTAlNDMQ0w");
            twcf.WriteLine("CwYDVQQIEwRNYWhlMREwDwYDVQQHEwhWaWN0b3JpYTEdMBsGA1UEChMUR2xvYmFs");
            twcf.WriteLine("IFN0ZWFsdGgsIEluYy4xDDAKBgNVBAsTA1ZQTjEgMB4GA1UEAxMXR2xvYmFsIFN0");
            twcf.WriteLine("ZWFsdGgsIEluYy4gQ0ExGzAZBgNVBCkTEnNlcnZlcmxvY2F0aW9uLWtleTEmMCQG");
            twcf.WriteLine("CSqGSIb3DQEJARYXYWRtaW5Ac2VydmVybG9jYXRpb24uY28wggEiMA0GCSqGSIb3");
            twcf.WriteLine("DQEBAQUAA4IBDwAwggEKAoIBAQDA94FmLbk3VPchYZmBCTc0okUFO6AwTn8trAVX");
            twcf.WriteLine("r6GVypCDmuWyCPAzCG47qT2rBlWPJMXYbmtJEq/Vrh9gcU7LYw4NQjSnXnBQ10wX");
            twcf.WriteLine("c3B+mG4x807IBwH87N2Fl6ZbL5mChIdssUalS3QyARc5Xp6YAJrX3I/UninPXYjz");
            twcf.WriteLine("jSxvMrSTnFHwS757F1vLv5z5+Udahz22+u+sqdkN31EnAsM917/fOpkWo0fd/x0r");
            twcf.WriteLine("59d0wYSeqRzqCf9UoQff08/8b+XN+kmR82S7othHEaLXBCgdXHk/lrp5zy4n1+AF");
            twcf.WriteLine("lwEXx51UNS8u5YUHlX0orJC1lTJfWjCvTWo2u/XC5iXcrEGbAgMBAAGjggEsMIIB");
            twcf.WriteLine("KDAdBgNVHQ4EFgQU69+VyGvTYVeqitctj3s/q7vcEbcwgfgGA1UdIwSB8DCB7YAU");
            twcf.WriteLine("69+VyGvTYVeqitctj3s/q7vcEbehgcmkgcYwgcMxCzAJBgNVBAYTAlNDMQ0wCwYD");
            twcf.WriteLine("VQQIEwRNYWhlMREwDwYDVQQHEwhWaWN0b3JpYTEdMBsGA1UEChMUR2xvYmFsIFN0");
            twcf.WriteLine("ZWFsdGgsIEluYy4xDDAKBgNVBAsTA1ZQTjEgMB4GA1UEAxMXR2xvYmFsIFN0ZWFs");
            twcf.WriteLine("dGgsIEluYy4gQ0ExGzAZBgNVBCkTEnNlcnZlcmxvY2F0aW9uLWtleTEmMCQGCSqG");
            twcf.WriteLine("SIb3DQEJARYXYWRtaW5Ac2VydmVybG9jYXRpb24uY2+CCQCxxBXvSEJQszAMBgNV");
            twcf.WriteLine("HRMEBTADAQH/MA0GCSqGSIb3DQEBCwUAA4IBAQBYkrR6R3QmQ04zWc5r4C7fhR7N");
            twcf.WriteLine("+rOqljrpbMXL6QfJTQJbAX2EJeHEyhjYh6xf4I3LWiM1rpSdJi8CbMagSRZulBqQ");
            twcf.WriteLine("v9ceszpFOpaoM4kgfDKWW+Z7R4cOZxZKmym1heuvcLcqMwOEk0qN7b6fyipSci38");
            twcf.WriteLine("/LnVdMHDLqnJUndTjhtN6sHmCKrBx9I3V9Yp1CAHUnEvX8mZAYKjbdhuhKhwaMiq");
            twcf.WriteLine("wOVCxXj8f872XtjATq/y1Y21vI8yv94NsK1C0zK+FBzxWWnXXQTzYBsNfCoZpox5");
            twcf.WriteLine("7LaXKtnKPSsaucbDlB2ECLqAydp8Q0f2pj0hF3X7mi5NmHEKqKc8T5ROar4D");
            twcf.WriteLine("-----END CERTIFICATE-----");
            twcf.Close();
            TextWriter twpf = new StreamWriter(passwordFile, true);
            twpf.WriteLine(connectionInfo.username + "\r");
            twpf.WriteLine(connectionInfo.password + "\r");
            twpf.Close();

            openVPNParams = "--dev tun --proto " + connectionInfo.protocol + " --remote " + connectionInfo.host + " " + connectionInfo.port.ToString() + " --resolv-retry 15 --client --auth-user-pass " + passwordFile + " --nobind --persist-key --persist-tun --ns-cert-type server --comp-lzo --reneg-sec 0 --verb 3 --ca " + certificateFile + " --ip-win32 ipapi --log log.txt --management " + Configuration.socketAddress + " " + Configuration.socketPort;
            pOpenVPN = new Process();
            pOpenVPN.StartInfo = new ProcessStartInfo()
            {
                FileName = openVPNPath + @"\openvpn.exe",
                WorkingDirectory = openVPNPath,
                Arguments = openVPNParams,
                WindowStyle = ProcessWindowStyle.Hidden,
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };
            pOpenVPN.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            pOpenVPN.Start();
            socketShouldBeConnected = true;

            InitializeOpenVPNStatusThread();
        }

        private void connectToSocket()
        {
            if (socket != null && !socket.Connected)
            {
                try
                {
                    socket.Connect(Configuration.socketAddress, Configuration.socketPort);
                    socket.ReceiveTimeout = 1000;
                }
                catch (ThreadAbortException)
                {
                    throw;
                }
                catch (Exception exception)
                {
                    Debug.WriteLine("Cannot connect to socket: " + exception.ToString());
                }
            }
        }

        private void readFromSocket()
        {
            byte[] buffer = new byte[1] { 0x00 };
            StringBuilder builder = new StringBuilder();

            while (socketShouldBeConnected)
            {
                try
                {
                    if (socket != null && socket.Connected)
                    {
                        if (socket.Available <= 0)
                        {
                            // make sure that OpenVPN process is still running
                            if (Process.GetProcessesByName("openvpn").Length == 0)
                            {
                                isOpenVPNConnected = false;
                                return;
                            }
                            Thread.Sleep(500);
                        }
                        else
                        {
                            try
                            {
                                socket.Receive(buffer);
                            }
                            catch (SocketException)
                            {
                                // read timed out
                                continue;
                            }
                            builder.Append(Encoding.ASCII.GetString(buffer));
                            if (buffer[0] == '\n')
                            {
                                postMessage(builder.ToString());
                                builder.Clear();
                            }
                        }
                    }
                }
                catch (SocketException)
                {
                    break;
                }
                catch (ThreadAbortException)
                {
                    throw;
                }
                catch (Exception exception)
                {
                    //connectToSocket(socket);
                    Debug.WriteLine("Cannot read from socket: " + exception.ToString());
                }
            }
        }

        private void sendMessageToSocket(string message)
        {
            if (socket != null && socket.Connected)
            {
                try
                {
                    socket.Send(Encoding.ASCII.GetBytes(message + "\r\n"));
                }
                catch (Exception exception)
                {
                    Debug.WriteLine("Cannot send message to socket: " + exception.ToString());
                }
            }
        }

        private void handleMessage(string message)
        {
            if (message.Contains(">INFO:OpenVPN Management"))
            {
                sendMessageToSocket("state on");
            }
            if (message.Contains(">STATE:"))
            {
                string state = getVPNConnectionState(message);
                isOpenVPNConnected = state.ToLower() == "connected";
                if (isOpenVPNConnected)
                    Task.Run(() => OnVPNConnectedCallback?.Invoke());
                postState(state);
            }
        }

        private string getVPNConnectionState(string message)
        {
            string status = "";
            string[] states = message.Split(',');
            if (states.Length > 1)
            {
                status = states[1];
            }
            return status;
        }

        private void InitializeOpenVPNStatusThread()
        {
            socketManagementThread?.Abort();
            socketManagementThread = new Thread(() =>
            {
                using (socket = new Socket(SocketType.Stream, ProtocolType.Tcp))
                {
                    try
                    {
                        connectToSocket();
                        readFromSocket();
                        sendMessageToSocket("signal SIGINT");
                    }
                    catch (ThreadAbortException)
                    {
                        // It's Either a normal disconnect or app is closing
                    }
                    finally
                    {
                        try
                        {
                            socket.Disconnect(false);
                        }
                        catch (SocketException)
                        {
                            // socket has probably already failed or disconnected in some other way.
                        }
                    }
                }
            });
            socketManagementThread.Name = "Socket watch";
            try
            {
                socketManagementThread.Start();
            }
            catch (ThreadStateException)
            {
                Task.Run(() =>
                {
                    while ((bool)socketManagementThread?.IsAlive)
                    {
                        Thread.Sleep(10);
                    }
                    socketManagementThread.Start();
                });
            }
        }

        public void disconnect()
        {
            socketShouldBeConnected = false;
            try
            {
                if (!pOpenVPN?.HasExited ?? false)
                    pOpenVPN?.Kill();
            }
            catch (Exception e)
            {
                Debug.WriteLine("An exception has occurred when trying to kill the OpenVPN Process: " + e);
            }
        }

        private void postMessage(string message)
        {
            handleMessage(message);
            Debug.WriteLine(message);
        }

        private void postState(string state)
        {
            openVPNDelegate?.updateOpenVPNState(state);
        }

        public bool isConnected()
        {
            return isOpenVPNConnected;
        }

        public class Config
        {
            public string name { get; set; }

            public string[] parameters { get; set; }
        }
    }
}
