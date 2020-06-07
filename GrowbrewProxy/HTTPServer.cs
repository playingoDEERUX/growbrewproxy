using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Threading;

namespace GrowbrewProxy
{
    public class HTTPServer
    {
        static string Version = "HTTP/1.0";

        private static HttpListener listener = new HttpListener();
        public static void HTTPHandler()
        {
            while (listener.IsListening)
            {
                HttpListenerContext context = listener.GetContext();
                HttpListenerRequest request = context.Request;
                HttpListenerResponse response = context.Response;
                Console.WriteLine("New request from client:\n" + request.RawUrl + " " + request.HttpMethod + " " + request.UserAgent);
                if (request.HttpMethod == "POST")
                {

                    byte[] buffer = Encoding.UTF8.GetBytes(
                        "server|127.0.0.1\n" +
                        "port|2\n" +
                        "type|1\n" +
                        "beta_server|127.0.0.1\n" +
                        "beta_port|2\n" +
                        "meta|growbrew.com\n");

                    response.ContentLength64 = buffer.Length;
                    System.IO.Stream output = response.OutputStream;
                    output.Write(buffer, 0, buffer.Length);
                    output.Close();
                }
            }
            // WOOT?
            listener.Stop();
        }


        public static void StartHTTP(string[] prefixes)
        {
            Console.WriteLine("Setting up HTTP Server...");
            if (!HttpListener.IsSupported)
            {
                Console.WriteLine("Windows XP SP2 or Server 2003 is required to use the HttpListener class.");
                return;
            }
            if (prefixes == null || prefixes.Length == 0)
                throw new ArgumentException("prefixes");
            foreach (string s in prefixes)
            {
                listener.Prefixes.Add(s);
            }
            listener.Start();
            if (listener.IsListening) Console.WriteLine("Listening!");
            else Console.WriteLine("Could not listen to port 80, an error occured!");
            Thread t = new Thread(HTTPHandler);
            t.Start();
        }

        public static void StopHTTP()
        {
            
        }
    }
}
