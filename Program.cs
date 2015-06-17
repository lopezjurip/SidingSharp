using System;
using System.Text;
using System.Net;
using System.IO;

namespace SidingTest
{
    class Program
    {
        static void Main(string[] args)
        {
            string user = "USER";
            string password = "PASSWORD";

            Siding siding = new Siding(user, password);
            string url = "https://intrawww.ing.puc.cl/siding/dirdes/ingcursos/cursos/vista.phtml";
            string html = siding.GetHTML(url);

            Console.WriteLine(html);
            Console.ReadLine();
        }
    }


    public class Siding
    {
        private const string baseUrl = "https://intrawww.ing.puc.cl/siding/dirdes/ingcursos/cursos/vista.phtml";
        private const string loginBaseUrl = "https://intrawww.ing.puc.cl/siding/index.phtml";
        private string user, password;
        private CookieCollection cookies;

        public Siding(string user, string password)
        {
            this.user = user;
            this.password = password;
        }

        public string GetHTML(string url)
        {
            bool success = this.RequestCookieIfNeeded();
            if (success)
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.CookieContainer = new CookieContainer();
                request.CookieContainer.Add(this.cookies);

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                if (response.StatusCode != HttpStatusCode.Forbidden || response.StatusCode != HttpStatusCode.NotFound)
                {
                    Stream stream = response.GetResponseStream();
                    StreamReader reader = (response.CharacterSet != null) ?
                        new StreamReader(stream) : new StreamReader(stream, Encoding.GetEncoding(response.CharacterSet));

                    string html = reader.ReadToEnd();

                    stream.Close();
                    reader.Close();

                    return html;
                }
            }
            return null;
        }

        private bool RequestCookieIfNeeded()
        {
            if (this.IsCookiesExpired())
            {
                return this.RequestCookies();
            }
            return true;
        }

        private bool IsCookiesExpired()
        {
            if (this.cookies != null && this.cookies.Count != 0)
            {
                foreach (Cookie cookie in this.cookies)
                {
                    if (cookie.Expired) return true;
                }
                return false;
            }
            else
            {
                return true;
            }
        }

        private bool RequestCookies()
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Siding.loginBaseUrl);
            request.CookieContainer = new CookieContainer();
            
            string postData = String.Format("login={0}&passwd={1}&sw=&sh=&cd=", this.user, this.password);
            var data = Encoding.ASCII.GetBytes(postData);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = data.Length;

            using (var stream = request.GetRequestStream())
            {
                stream.Write(data, 0 ,data.Length);
            }

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                this.cookies = response.Cookies;
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
