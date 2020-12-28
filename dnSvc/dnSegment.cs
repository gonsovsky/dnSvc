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

        public DnSegment(DnTask task, int index) : base(task.Token, task.FromUri)
        {
            Parent = task;
            Index = index;
        }

        protected override void StartAction()
        {
            using (var fileStream =
                new FileStream(FileName, FileMode.Create, FileAccess.Write, FileShare.Write))
            {
                Transport.Body(fileStream, Begin, End);
            }
        }

        protected override void CompleteAction()
        {
        }

        public override string FileName => 
            Path.Combine(DownloadService.DnConf.TempDir, Parent.Name + "-" + Index.ToString() + ".txt");
    }
}
