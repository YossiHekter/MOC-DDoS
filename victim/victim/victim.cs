using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Victim
{
    class victim
    {
        static void Main(string[] args)
        {
            TcpListener server = null;
            try
            {
                // Set the TcpListener on random port.
                Random rnd = new Random();
                int port_tcp = rnd.Next(1024, 65535);
                string host = Dns.GetHostName();
                string myIP = Dns.GetHostByName(host).AddressList[0].ToString();
                //Console.WriteLine(myIP);
                IPAddress localAddr = IPAddress.Parse(myIP);

                // TcpListener server = new TcpListener(port);
                server = new TcpListener(localAddr, port_tcp);

                // Start listening for client requests.
                server.Start();

                // Set list of connections
                List<DateTime> connections_list = new List<DateTime>();
                
                // Buffer for reading data

                //represent the client life
                bool crash = false;

                //set password
                string password = string.Empty;
                for (int i = 0; i < 6; i++)
                    password += ((char)rnd.Next(97, 122)).ToString();

                Console.WriteLine("Server listening on port " + port_tcp + ", password is " + password);
                // Enter the listening loop.
                while (true)
                {
                    
                    TcpClient client = server.AcceptTcpClient();
                    NetworkStream s = client.GetStream();
                    sent_msg(s, "Please enter your password\r\n");
                    string password_from_client = read_msg(s, 6);
                    string msg = "";
                    if (password == password_from_client)
                    {
                        DateTime time = DateTime.Now;
                        if (connections_list.Count == 10)
                        {
                            connections_list.RemoveAt(0);
                        }
                        connections_list.Add(time);
                        sent_msg(s, "Access granted\r\n");
                        msg = read_msg(s, 44);
                    }
                    else
                    {
                        client.Close();
                    }
                    if (connections_list.Count == 10)
                    {
                        crash = check_crash(connections_list);
                        if (crash)
                        {
                            Console.WriteLine(msg.Replace("\r\n", ""));
                            client.Close();
                            connections_list.Clear();
                            Thread.Sleep(2000);
                            Console.Clear();
                            Console.WriteLine("Server listening on port " + port_tcp + ", password is " + password);
                            continue;
                        }
                        else
                        {
                            client.Close();
                        }
                    }
                }

            }
            catch (SocketException e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                // Stop listening for new clients.
                server.Stop();
            }
        }
        private static void sent_msg(NetworkStream stream, string to_send)
        {
            ASCIIEncoding asen = new ASCIIEncoding();
            byte[] ba = asen.GetBytes(to_send);
            stream.Write(ba, 0, ba.Length);
        }

        private static string read_msg(NetworkStream s, int size)
        {
            string msg = "";
            Byte[] data = new Byte[size];
            int i = s.Read(data, 0, data.Length);
            if (i != 0)
            {
                msg = System.Text.Encoding.ASCII.GetString(data, 0, i);
            }
            return msg;
        }

        private static bool check_crash(List<DateTime> connections_list)
        {
            bool ans = false;
            DateTime max = connections_list.ElementAt(9);
            DateTime min = connections_list.ElementAt(0);
            TimeSpan a = max - min;
            if (a.Seconds <= 1)
                ans = true;
            
            return ans;
        }
    }
}
