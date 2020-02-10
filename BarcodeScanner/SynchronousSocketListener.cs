using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;


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
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 8081);

            Socket listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            
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
                    byte[] msg = Encoding.UTF8.GetBytes("OK");

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
