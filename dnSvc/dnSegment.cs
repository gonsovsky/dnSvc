using System;
using System.IO;
using System.Net;
using System.Xml.Serialization;

namespace dnSvc
{
    public class DnSegment: DnItem
    {
        public  DnTask Parent;

        public int Begin { get; set; }

        [XmlIgnore]
        public int End
        {
            get => Begin + Size;
            set => Size = value - Begin;
        }

        public DnSegment(DnTask task, int index) : base(task.Token)
        {
            Parent = task;
            Index = index;
        }

        protected override void StartAction()
        {
            if (!(WebRequest.Create(Parent.FromUri) is HttpWebRequest httpWebRequest))
                throw new ApplicationException($"Segment {Index} for {Parent.FromUri} error");
            httpWebRequest.Method = "GET";
            httpWebRequest.AddRange(Begin, End);
            using (var httpWebResponse = httpWebRequest.GetResponse() as HttpWebResponse)
            {
                using (var fileStream =
                    new FileStream(FileName, FileMode.Create, FileAccess.Write, FileShare.Write))
                {
                    if (httpWebResponse != null)
                    {
                        var resp = httpWebResponse.GetResponseStream();
                        resp?.CopyTo(fileStream);
                    }
                }
            }
        }

        protected override void CompleteAction()
        {
        }

        public override string FileName => 
            Path.Combine(DownloadService.DnConf.TempDir, Parent.Name + "-" + Index.ToString() + ".txt");
    }
}
