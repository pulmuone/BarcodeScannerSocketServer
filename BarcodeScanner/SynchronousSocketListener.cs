using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace BarcodeScanner
{
    public class SynchronousSocketListener
    {

        // Incoming data from the client.  
        public static string data = string.Empty;
        public static StringBuilder sb = new StringBuilder();

        public static void StartListening()
        {
            byte[] bytes = new Byte[1024];
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());

            foreach (var item in ipHostInfo.AddressList)
            {

                if (item.AddressFamily == AddressFamily.InterNetwork)
                {
                    Debug.WriteLine(item);
                }
            }

            //IPAddress ipAddress = ipHostInfo.AddressList[0];

            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, 7000);
            
            Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            
            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(10);

                while (true)
                {
                    Console.WriteLine("Waiting for a connection...");
                    Socket handler = listener.Accept();
                    //data = string.Empty;
                    sb.Clear();
                    String content = String.Empty;

                    while (true)
                    {
                        int bytesRec = handler.Receive(bytes);
                        //data += Encoding.UTF8.GetString(bytes, 0, bytesRec);
                        sb.Append(Encoding.UTF8.GetString(bytes, 0, bytesRec));
                        content = sb.ToString();

                        //if (data.IndexOf("}]") > -1)
                        if (content.IndexOf("}]") > -1)
                        {
                            break;
                        }
                    }

                    // Show the data on the console.  
                    //Console.WriteLine("Text received : {0}", data);
                    List<EmpModel> lst = new List<EmpModel>();
                    lst = JsonConvert.DeserializeObject<List<EmpModel>>(content);

                    foreach (var item in lst)
                    {
                        Console.WriteLine(string.Format("{0}, {1}", item.EmpId, item.EmpName));
                    }

                    // Echo the data back to the client.  
                    //byte[] msg = Encoding.UTF8.GetBytes("OK");
                    byte[] msg = Encoding.UTF8.GetBytes(content);

                    Thread.Sleep(5000);
                    handler.Send(msg);
                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
