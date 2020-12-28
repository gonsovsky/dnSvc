using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace dnSvc
{
    public class DnHead
    {
        public int Size { get; set; }
    }

    public abstract class DnTransport
    {
        public Uri FromUri;

        protected DnTransport(Uri fromUri)
        {
            this.FromUri = fromUri;
        }

        public static DnTransport Make(Uri uri)
        {
            if (uri.Scheme == "http" || uri.Scheme == "https")
                return new DnTransportHttp(uri);
            if (uri.Scheme == "smb")
                return new DnTransportSmb(uri);
            throw  new ApplicationException($"Unknown uri.schema for: {uri}");
        }

        public abstract DnHead Head();

        public abstract void Body(Stream target, int begin, int end);
    }
}
