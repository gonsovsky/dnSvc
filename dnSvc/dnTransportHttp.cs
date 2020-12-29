using System;
using System.IO;
using System.Net;

namespace dnSvc
{
    public class DnTransportHttp: DnTransport
    {
        static DnTransportHttp()
        {
            ServicePointManager.Expect100Continue = false;
            ServicePointManager.DefaultConnectionLimit = 100;
            ServicePointManager.MaxServicePointIdleTime = 1000;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
        }

        public DnTransportHttp(Uri fromUri) : base(fromUri)
        {
        }

        public override DnHead Head()
        {
            WebRequest webRequest = WebRequest.Create(FromUri);
            webRequest.Method = "HEAD";
            using (var webResponse = webRequest.GetResponse())
            {
                var responseLength = long.Parse(webResponse.Headers.Get("Content-Length"));
                return new DnHead(){Size =  (int)responseLength};
            }
        }

        public override void Body(Stream target, int begin, int end)
        {
            if (!(WebRequest.Create(FromUri) is HttpWebRequest httpWebRequest))
                throw new ApplicationException("unknown error");
            httpWebRequest.Method = "GET";
            httpWebRequest.AddRange(begin, end);
            var httpWebResponse = httpWebRequest.GetResponse() as HttpWebResponse;
            if (httpWebResponse == null) return;
            var stream = httpWebResponse.GetResponseStream();
            stream?.CopyTo(target);
        }
    }
}
