using System;
using System.IO;
using System.Text;
using SharpCifs.Smb;
using SharpCifs.Util.Sharpen;

namespace sambacli
{
    class Program
    {
        //private static string srv = "92.63.110.160";
        //private static string login = "Administrator";
        //private static string parol = "Kerberos123";
        //private static string fpath = $@"smb://{login}:{parol}@{srv}/temp/";
        //private static string targetf = @"/home/1/";

        private static string srv = "185.68.21.218";
        private static string login = "demo";
        private static string parol = "demo";
        private static string fpath = $@"smb://{login}:{parol}@{srv}/secured/";
        private static string targetf = @"C:\__samba";

        ////private static string srv = "WIN";
        ////private static string login = "temp";
        ////private static string parol = "temp2";
        ////private static string fpath = $@"smb://{login}:{parol}@{srv}/temp/";
        ////private static string targetf = @"C:\temp\";

        static void Main(string[] args)
        {
            SharpCifs.Config.SetProperty("jcifs.smb.client.responseTimeout", "3000");
            SharpCifs.Config.SetProperty("jcifs.smb.client.connTimeout", "3000");
            SharpCifs.Config.SetProperty("jcifs.smb.client.soTimeout", "3000");
           
            //using System;
            //using SharpCifs.Smb;

            //Get SmbFile-Object of a folder.
       

            //UnixTime
            var epocDate = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            while (true)
            {
 
                var folder = new SmbFile(fpath);
                try
                {
                    folder.ListFiles();
                    Thread.Sleep(1000);
                }
                catch (Exception e)
                {
                    try
                    {
                        for (int i = 0; i <= 3; i++)
                        {
                            SmbTransport.ClearCachedConnections(true);
                            break;
          
                        }
                    }
                    catch (Exception exception)
                    {
                        Thread.Sleep(1000);
                    }
                    Console.WriteLine(e);
                }
                continue;
                
                //List items
                foreach (SmbFile item in folder.ListFiles())
                {
               
                    var lastModDate = epocDate.AddMilliseconds(item.LastModified())
                        .ToLocalTime();

                    var name = item.GetName();
                    var type = item.IsDirectory() ? "dir" : "file";
                    var date = lastModDate.ToString("yyyy-MM-dd HH:mm:ss");
                    var msg = $"{name} ({type}) - LastMod: {date}";


                   // GetFile(item);
                    Console.WriteLine(msg);
                }
          
            }

            Console.ReadLine();
        }

        static void GetFile(SmbFile item)
        {
            
            //Get target's SmbFile.
            var file = new SmbFile(fpath + item.GetName());

            //Get readable stream.
            var readStream = file.GetInputStream();

            //Create reading buffer.
            var memStream = new FileStream(targetf + item.GetName(),FileMode.Create);

            //Get bytes.
            ((Stream)readStream).CopyTo(memStream);

            //Dispose readable stream.
            readStream.Dispose();

          //  Console.WriteLine(Encoding.UTF8.GetString(memStream.ToArray()));
        }
    }
}
