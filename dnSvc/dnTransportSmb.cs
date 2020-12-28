using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using SharpCifs.Smb;

namespace dnSvc
{
    public class DnTransportSmb : DnTransport
    {
        public DnTransportSmb(Uri fromUri) : base(fromUri)
        {
        }

        public override DnHead Head()
        {
            var file = new SmbFile(FromUri.ToString());
            var len = (int)file.Length();
            return new DnHead() {Size = len};
        }

        public override void Body(Stream target, int begin, int end)
        {
            var file = new SmbFile(FromUri.ToString());
            var stream = file.GetInputStream();
            stream.Skip(begin);
            var bytes = new byte[end - begin];
            stream.Read(bytes, 0, bytes.Length);
            target.Write(bytes,0,bytes.Length);
        }
    }
}
