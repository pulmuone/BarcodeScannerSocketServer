using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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

        public static Socket listener;

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
            listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

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
                        sb.Append(Encoding.Unicode.GetString(bytes, 0, bytesRec));
                        content = sb.ToString();
                        Console.WriteLine(content);

                        //if (content.IndexOf("}]") > -1) //<EOF>
                        //if (content.IndexOf("\n  }\n]") > -1) //<EOF>
                        if (content.IndexOf("<EOF>") > -1)
                        {
                            break;
                        }
                    }

                    Dictionary<string, string> pObj = JsonConvert.DeserializeObject<Dictionary<string, string>>(sb.ToString());
                    foreach (var item in pObj.Keys)
                    {
                        Console.WriteLine(item.ToString()); //key
                        Console.WriteLine(pObj[item]);  //value

                        if (item.ToString().Equals("P_JSON"))
                        {
                            List<EmpModel> lst = new List<EmpModel>();
                            lst = JsonConvert.DeserializeObject<List<EmpModel>>(pObj[item]);

                            StringBuilder sb = new StringBuilder();
                            foreach (var row in lst)
                            {
                                string binaryString = string.Empty;
                                Console.WriteLine(string.Format("{0}, {1}", row.EmpId, row.EmpName));
                                sb.AppendLine(string.Format("{0}, {1}", row.EmpId, row.EmpName));
                            }

                            File.WriteAllText("test.txt", sb.ToString());
                        }
                    }

                    // Echo the data back to the client.  
                    //byte[] msg = Encoding.Unicode.GetBytes("OK");
                    byte[] msg = Encoding.Unicode.GetBytes(content);

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
