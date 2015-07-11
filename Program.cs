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
            string user = "asd.cl";
            string password = "asd";

            Siding siding = new Siding(user, password);
            string url = "https://intrawww.ing.puc.cl/siding/dirdes/ingcursos/cursos/vista.phtml?accion_curso=avisos&acc_aviso=mostrar&id_curso_ic=7023";
            string html = siding.GetHTML(url);

            //Console.WriteLine(html);
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
            var request = this.RequestWithCredentials(url);
            if (request != null)
            {
                var response = (HttpWebResponse)request.GetResponse();
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var stream = response.GetResponseStream();
                    var reader = (response.CharacterSet != null) ?
                        new StreamReader(stream) : new StreamReader(stream, Encoding.GetEncoding(response.CharacterSet));

                    var html = reader.ReadToEnd();

                    stream.Close();
                    reader.Close();

                    return html;
                }
            }
            return null;
        }

        public HttpWebRequest RequestWithCredentials(string url)
        {
            bool authorized = this.Authorize();
            Console.WriteLine(authorized);
            if (authorized)
            {
                var request = (HttpWebRequest)WebRequest.Create(url);
                request.CookieContainer = new CookieContainer();
                request.CookieContainer.Add(this.cookies);
                return request;
            }
            return null;
        }

        private bool Authorize()
        {
            return this.ExpiredCookies ? this.RequestSessionCookies() : true;
        }

        private bool ExpiredCookies
        {
            get
            {
                if (this.cookies == null) return true;
                foreach (Cookie cookie in this.cookies) { if (cookie.Expired) return true; }
                return false;
            }
        }

        private bool RequestSessionCookies()
        {
            var request = (HttpWebRequest)WebRequest.Create(Siding.loginBaseUrl);
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

            var response = (HttpWebResponse)request.GetResponse();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                this.cookies = response.Cookies;
                foreach (Cookie c in this.cookies)
                    Console.WriteLine(c);
                return true;
            }
            return false;
        }
    }
}
