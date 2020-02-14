using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;

namespace HTTPServer
{
    class Server
    {
        /// <summary>
        /// Socket which run server 
        /// </summary>
        Socket serverSocket;
        /// <summary>
        /// Server
        /// </summary>
        /// <param name="portNumber"></param>
        /// <param name="redirectionMatrixPath"></param>
        public Server(int portNumber, string redirectionMatrixPath)
        {
            this.LoadRedirectionRules(redirectionMatrixPath);
            //TODO: initialize this.serverSocket
            this.serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //IP which server will receve
            IPEndPoint endpoint = new IPEndPoint(IPAddress.Any, portNumber);
            //Send IPs to start Bind
            this.serverSocket.Bind(endpoint);
        }

        /// <summary>
        /// StartServer
        /// </summary>
        /// 
        //threads
        public void StartServer()
        {
            // TODO: Listen to connections, with large backlog.
            serverSocket.Listen(2000);//Max queue Length is 2000
            // TODO: Accept connections in while loop and start a thread for each connection on function "Handle Connection"
            while (true)
            {
                //TODO: accept connections and start thread for each accepted connection.
                Socket ClientSocket = serverSocket.Accept();//Accept Request and put it into socket
                Thread NewThread = new Thread(new ParameterizedThreadStart(HandleConnection));
                NewThread.Start(ClientSocket);
            }
        }

        /// <summary>
        /// HandleConnection
        /// </summary>
        /// <param name="obj"></param>
        public void HandleConnection(object obj)
        {
            Socket clientSocket = (Socket)obj;
            clientSocket.ReceiveTimeout = 0; //indicates an infinite time-out period.
            // TODO: receive requests in while true until remote client closes the socket.
            while (true)
            {
                try
                {
                    // TODO: Receive request
                    byte[] ClientData = new byte[1024 * 1024];
                    int ClientDataLength = clientSocket.Receive(ClientData);
                    // TODO: break the while loop if receivedLen==0
                    if (ClientDataLength == 0)
                        break;
                    // TODO: Create a Request object using received request string
                    Request NewRequest = new Request(Encoding.ASCII.GetString(ClientData));
                    // TODO: Call HandleRequest Method that returns the response
                    Response NewResponse = HandleRequest(NewRequest);
                    // TODO: Send Response back to client
                    clientSocket.Send(Encoding.ASCII.GetBytes(NewResponse.ResponseString));
                }
                catch (Exception ex)
                {
                    Logger.LogException(ex);
                }
            }

            clientSocket.Close();
        }

        /// <summary>
        /// HandleRequest
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Response HandleRequest(Request request)
        {
            try
            {
                //TODO: check for bad request 
                if (!request.ParseRequest())
                    return new Response(StatusCode.BadRequest, "text/html", LoadDefaultPage(Configuration.BadRequestDefaultPageName), "");
                //TODO: map the relativeURI in request to get the physical path of the resource.
                string PhysicalPath = Path.Combine(Configuration.RootPath, request.relativeURI);
                //TODO: check for redirect
                string RedirectionPagePath = GetRedirectionPagePathIFExist(request.relativeURI);
                if (RedirectionPagePath != "") return new Response(StatusCode.Redirect, "text/html", LoadDefaultPage(Configuration.RedirectionDefaultPageName), RedirectionPagePath);
                //TODO: check file exists
                if (!File.Exists(PhysicalPath))
                {
                    return new Response(StatusCode.NotFound, "text/html", LoadDefaultPage(Configuration.NotFoundDefaultPageName), "");
                }
                //TODO: read the physical file
                StreamReader reader = new StreamReader(PhysicalPath);
                string file = reader.ReadToEnd();
                reader.Close();
                // Create OK response
                return new Response(StatusCode.OK, "text/html", file, "");
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                // TODO: in case of exception, return Internal Server Error.
                return new Response(StatusCode.InternalServerError, "text/html", LoadDefaultPage(Configuration.InternalErrorDefaultPageName), "");
            }
        }


        /// <summary>
        /// Gets the redirection page path IF exist.
        /// </summary>
        /// <param name="relativePath">The relative path.</param>
        /// <returns>The redirection path, empty string otherwise</returns>
        private string GetRedirectionPagePathIFExist(string relativePath)
        {
            if (Configuration.RedirectionRules.ContainsKey(relativePath))
            {
                return Configuration.RedirectionRules[relativePath];
            }

            return string.Empty;
        }

        /// <summary>
        /// LoadDefaultPage
        /// </summary>
        /// <param name="defaultPageName"></param>
        /// <returns></returns>
        private string LoadDefaultPage(string defaultPageName)
        {
            string filePath = Path.Combine(Configuration.RootPath, defaultPageName);
            if (!File.Exists(filePath))
            {
                Logger.LogException(new Exception("Default Page " + defaultPageName + " doesn't exist"));
                return string.Empty;
            }
            StreamReader reader = new StreamReader(filePath);
            string file = reader.ReadToEnd();
            reader.Close();
            return file;

        }

        /// <summary>
        /// Loads the redirection rules from the configuration file.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        private void LoadRedirectionRules(string filePath)
        {
            try
            {
                StreamReader reader = new StreamReader(filePath);


                Configuration.RedirectionRules = new Dictionary<string, string>();

                while (!reader.EndOfStream)
                {
                    string temp = reader.ReadLine();
                    string[] result = temp.Split(',');
                    Configuration.RedirectionRules.Add(result[0], result[1]);
                }

                reader.Close();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);

                Environment.Exit(1);
            }
        }


    }
}
