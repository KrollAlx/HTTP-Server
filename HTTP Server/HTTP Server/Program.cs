using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;

namespace HTTP_Server
{
    class Program
    {
        static bool shutDown = false;
        class Headers
        {
            public string Accept = "Accept: text\r\n";
            public string AcceptCharset = "Accept-Charset: utf-8\r\n";
            public string AcceptEncoding = "Accept-Encoding: uft-8\r\n";
            public string AcceptLanguage = "Accept-Language: ru\r\n";
            public string Allow = "Allow: GET\r\n";
            public string ContentEncoding = "Content-Encoding: uft-8\r\n";
            public string ContentLanguage = "Content-Language: ru\r\n";
            public string ContentLength = "Content-Length: 0\r\n";
            public string ContentType = "Content-Type: text\r\n";
            public string Date = $"Date: {DateTime.Now.ToString()}\r\n";
            public string UserAgent = "User-Agent: PuTTY\r\n";
        }
        static void Main(string[] args)
        {
            IPHostEntry ipHost = Dns.GetHostEntry("localhost");
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, 80);
            Socket listener = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                listener.Bind(ipEndPoint);
                listener.Listen(10);
                while (!shutDown)
                {
                    Console.WriteLine($"Waiting for connection through port {ipEndPoint.Port}");
                    Socket newSocket = listener.Accept();
                    Console.WriteLine("New client connected");
                    Thread serverThread= new Thread(new ParameterizedThreadStart(Server));
                    serverThread.Start(newSocket);
                }
                listener.Shutdown(SocketShutdown.Both);
                listener.Close();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        static void Server(object inputSocket)
        {
            Socket socket = (Socket)inputSocket;
            Headers headers = new Headers();
            string request = null;
            string reply = null;
            byte[] receiveBuffer = new byte[1024];
            socket.Send(Encoding.UTF8.GetBytes("Welcome to HTTP-server\r\n"));
            while (true)
            {
                try
                {
                    int nReaded = socket.Receive(receiveBuffer);
                    request = Encoding.UTF8.GetString(receiveBuffer, 0, nReaded);
                    if (request.Contains("GET") && request.Contains("HTTP/1.0"))
                    {                        
                        if (request.Contains("info=yes"))
                        {
                            reply = "";
                            List<string> transmittedHeaders = new List<string>(request.Split(' '));
                            transmittedHeaders.Remove("GET");
                            transmittedHeaders.Remove("HTTP/1.0");
                            transmittedHeaders.Remove("info=yes");
                            foreach(string header in transmittedHeaders)
                            {
                                if (header == "Accept")
                                    reply += headers.Accept;
                                if (header == "Accept-Charset")
                                    reply += headers.AcceptCharset;
                                if (header == "Accept-Encoding")
                                    reply += headers.AcceptEncoding;
                                if (header == "Accept-Language")
                                    reply += headers.AcceptLanguage;
                                if (header == "Allow")
                                    reply += headers.Allow;
                                if (header == "Content-Encoding")
                                    reply += headers.ContentEncoding;
                                if (header == "Content-Language")
                                    reply += headers.ContentLanguage;
                                if (header == "Content-Length")
                                    reply += headers.ContentLength;
                                if (header == "Content-Type")
                                    reply += headers.ContentType;
                                if (header == "Date")
                                    reply += headers.Date;
                                if (header == "User-Agent")
                                    reply += headers.UserAgent;
                            }
                        }
                        else
                        {
                            reply = "HTTP/1.0 200 OK\r\n";
                            string[] partOfRequest = request.Split(' ');
                            string path = partOfRequest[1];
                            try
                            {
                                using (StreamReader file = new StreamReader(path))
                                {
                                    reply += file.ReadToEnd();
                                    reply += "\r\n";
                                }
                            }
                            catch
                            {
                                reply = "HTTP/1.0 404 NOT FOUND\r\n";
                            }
                        }                        
                    }
                    else if (request == "disconnect")
                    {
                        break;
                    }
                    else if (request == "shutdown")
                    {
                        shutDown = true;
                        break;
                    }
                    else
                    {
                        reply = "HTTP/1.0 501 NOT IMPLEMENTED\r\n";
                    }
                    socket.Send(Encoding.UTF8.GetBytes(reply));
                }
                catch
                {
                    break;
                }                
            }
            Console.WriteLine("Client disconnected");
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
        }
    }
}
