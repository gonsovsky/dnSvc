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
        public int SegmentSize { get; set; } = (int)(0.2 * 1048576);

        [Category("Dir")]
        public string Dir  { get; set; } = @"C:\temp";

        [Category("Dir")]
        public string TempDir { get; set; } = @"C:\temp";

        [Category("Behavior")] public int RedFlag { get; set; } = 10;

        [Browsable(false)]
        [Category("Dir")]
        public string StateFile { get; set; } = @"state.xml";
    }
}
