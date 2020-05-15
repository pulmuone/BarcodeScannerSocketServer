using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Linq;
using System.IO;

namespace BarcodeScanner
{
    public class StateObject
    {
        // Client  socket.  
        public Socket workSocket = null;
        // Size of receive buffer.  
        public const int BufferSize = 2048;
        // Receive buffer.  
        public byte[] buffer = new byte[BufferSize];
        // Received data string.  
        public StringBuilder sb = new StringBuilder();

        public List<byte[]> listBuffer = new List<byte[]>();
    }

    public class AsynchronousSocketListener
    {
        // Thread signal.  
        public static ManualResetEvent allDone = new ManualResetEvent(false);

        public AsynchronousSocketListener()
        {

        }

        public static void StartListening()
        {
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, 7000);
            Socket listener = new Socket(SocketType.Stream, ProtocolType.Tcp);
            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(100);

                while (true)
                {
                    allDone.Reset();

                    Console.WriteLine("Waiting for a connection...");
                    listener.BeginAccept(new AsyncCallback(AcceptCallback),listener);

                    allDone.WaitOne();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public static void AcceptCallback(IAsyncResult ar)
        {
            allDone.Set();

            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);

            StateObject state = new StateObject();
            state.workSocket = handler;
            handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
        }

        public static void ReadCallback(IAsyncResult ar)
        {
            String content = String.Empty;

            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;

            int bytesRead = handler.EndReceive(ar);

            if (bytesRead > 0)
            {
                state.sb.Append(Encoding.UTF8.GetString(state.buffer, 0, bytesRead));
                content = state.sb.ToString();

                //if (content.IndexOf("\n  }\n]") > -1) //<EOF>
                if (content.IndexOf("}]") > -1) //<EOF>
                {
                    List<EmpModel> lst = new List<EmpModel>();

                    lst = JsonConvert.DeserializeObject<List<EmpModel>>(content);
                    
                    StringBuilder sb = new StringBuilder();
                    foreach (var item in lst)
                    {
                        string binaryString = string.Empty;

                        //Console.WriteLine(string.Format("{0}, {1}",item.EmpId, ByteToString(item.EmpName)));
                        Console.WriteLine(string.Format("{0}, {1}",item.EmpId, item.EmpName));
                        sb.AppendLine(string.Format("{0}, {1}", item.EmpId, item.EmpName));
                    }
                    
                    File.WriteAllText("test.txt", sb.ToString());

                    Send(handler, content);
                    //Send(handler, "OK");
                }
                else
                {
                    // Not all data received. Get more.  
                    handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
                }
            }
        }

        //private static string ByteToString(byte[] response)
        //{
        //    string binaryString = string.Empty;
        //    foreach (byte b in response)
        //    {
        //        // byte를 2진수 문자열로 변경
        //        string s = Convert.ToString(b, 2);
        //        binaryString += s.PadLeft(8, '0');
        //    }

        //    int nbytes = binaryString.Length / 8;
        //    byte[] outBytes = new byte[nbytes];

        //    for (int i = 0; i < nbytes; i++)
        //    {
        //        // 8자리 숫자 즉 1바이트 문자열 얻기
        //        string binStr = binaryString.Substring(i * 8, 8);
        //        // 2진수 문자열을 숫자로 변경
        //        outBytes[i] = (byte)Convert.ToInt32(binStr, 2);
        //    }
            
        //    return Encoding.UTF8.GetString(outBytes);
        //}


        private static void Send(Socket handler, String data)
        {
            byte[] byteData = Encoding.UTF8.GetBytes(data);

            handler.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), handler);
        }

        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                Socket handler = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.  
                int bytesSent = handler.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to client.", bytesSent);

                handler.Shutdown(SocketShutdown.Both);
                handler.Close();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
       
    }
}
