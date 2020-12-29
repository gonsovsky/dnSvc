using System;
using System.IO;
using SharpCifs;
using SharpCifs.Smb;
using SharpCifs.Util.Sharpen;

namespace dnSvc
{
    public class DnTransportSmb : DnTransport
    {
        static DnTransportSmb()
        {
            SharpCifs.Config.SetProperty("jcifs.smb.client.responseTimeout", "3000");
            SharpCifs.Config.SetProperty("jcifs.smb.client.connTimeout", "3000");
            SharpCifs.Config.SetProperty("jcifs.smb.client.soTimeout", "3000");
        }

        public DnTransportSmb(Uri fromUri) : base(fromUri)
        {
        }

        public override DnHead Head()
        {
            try
            {
                var file = new SmbFile(FromUri.ToString());
                var len = (int)file.Length();
                return new DnHead() { Size = len };
            }
            catch (Exception e)
            {
                Reset();
                throw;
            }
        }

        public override void Body(Stream target, int begin, int end)
        {
            try
            {
                var file = new SmbRandomAccessFile(FromUri.ToString(), "r", 0x00000001);
                file.Seek(begin);
                var bytes = new byte[end - begin];
                file.Read(bytes, 0, bytes.Length);
                target.Write(bytes, 0, bytes.Length);
            }
            catch (Exception e)
            {
                Reset();
                throw;
            }
        }

        public void Reset()
        {
            try
            {
                for (int i = 0; i <= 3; i++)
                {
                    SmbTransport.ClearCachedConnections(true);
                    break;
                }
            }
            catch (Exception)
            {
                Thread.Sleep(1000);
            }
        }
    }
}
