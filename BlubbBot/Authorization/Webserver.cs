using System;
using System.Diagnostics;
using System.Net;
using System.Text;

namespace BlubbBot.Autorization
{
    internal class Webserver : IDisposable
    {
        private HttpListener _httpListener;

        public Webserver(string listenOn)
        {
            this._httpListener = new HttpListener();
            this._httpListener.Prefixes.Add(listenOn);
            this._httpListener.Start();
        }

        public void Dispose()
        {
            this._httpListener?.Stop();
            this._httpListener = null;
        }

        public Uri WaitListen()
        {
            try
            {
                var context = this._httpListener.GetContext();
                var response = context.Response;
                response.AddHeader("Content-Type", "text/html");
                var content = Encoding.UTF8.GetBytes(
                    "<html>" +
                    "<head>" +
                        "<title>BlubbBot</title>" +
                    "</head>" +
                    "<body>" +
                        "<p> You can now close this window </p>" +
                    "</body>" +
                    "</html>");
                response.OutputStream.Write(content, 0, content.Length);
                response.OutputStream.Close();

                return context.Request.Url;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return null;
            }
        }
    }
}
