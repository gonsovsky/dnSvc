using System.ComponentModel;

namespace dnSvc
{
    public class DnConf
    {
        [Category("Parallels")]
        public int Parallels { get; set; } = 1;

        [Category("Segment")]
        public int Delay { get; set; } = 100;
        [Category("Segment")]
        public int SegmentSize { get; set; } = 1024;

        [Category("Dir")]
        public string Dir  { get; set; } = @"C:\temp";

        [Category("Dir")]
        public string TempDir { get; set; } = @"C:\temp";
    }
}
