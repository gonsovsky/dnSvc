using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;

namespace dnSvc
{
    public class DnTask: DnItem
    {
        public FileStream DestStream;

        public List<DnSegment> Segments = new List<DnSegment>();

        public int TotalSegments =>
            Segments.Count();

        public int DoneSegments =>
            Segments.Count(x => x.Done);

        public IEnumerable<DnSegment> DoneSegmentsCol =>
            Segments.Where(x => x.Done).OrderBy(x => x.TimeEnded);

        public int DoneSize =>
            Segments.Where(x => x.Done).Sum(y => y.Size);
        
        public bool Delivered { get; set; }

        public DnTask(CancellationToken token, Uri fromUri): base(token, fromUri)
        {
       
        }

        protected override void StartAction()
        {
            var head = Transport.Head();
            Size = head.Size;

            Segments = new List<DnSegment>();

            var count = this.Size / DownloadService.DnConf.SegmentSize;

            for (var segment = 0; segment <= count - 1; segment++)
            {
                var range = new DnSegment(this, segment + 1)
                {
                    Begin = segment * (DownloadService.DnConf.SegmentSize),
                    End = ((segment + 1) * (DownloadService.DnConf.SegmentSize)) - 1
                };
                Segments.Add(range);
            }

            Segments.Add(new DnSegment(this, count+1)
            {
                Begin = Segments.Any() ? Segments.Last().End + 1 : 0,
                End = this.Size
            });
        }

        protected override void CompleteAction()
        {
            using (Stream output = new FileStream(FileName, FileMode.Append,
                FileAccess.Write, FileShare.None))
            {
                foreach (var segment in Segments.OrderBy(b => b.Index))
                {
                    using (Stream input = File.OpenRead(segment.FileName))
                    {
                        input.CopyTo(output);
                    }
                    File.Delete(segment.FileName);
                }
                output.Flush();
            }
        }

        public override string FileName =>
            Path.Combine(DownloadService.DnConf.Dir, Name);

        public string Status
        {
            get
            {
                if (Busy)
                    return "downloading...";
                return Done ? "done" : "idle";
            }
        }

        public int Percent
        {
            get
            {
                if (TotalSegments == 0)
                    return 0;
                return DoneSegments * 100 / TotalSegments;
            }
        }

        public string Speed
        {
            get
            {
                double v = 0;
                var last = DoneSegmentsCol.Skip(Math.Max(0, DoneSegmentsCol.Count() - 2)).ToArray();
                if (last.Count() >= 1)
                {
                    var s = last.Last().TimeEnded.Subtract(last.First().TimeStarted).TotalSeconds;
                    v = last.Sum(x => x.Size);  
                    if (s != 0)
                        v = (double) v / s / 1024;
                }
                return $"{Math.Round(v, 2)} kb/s";
            }
        }

        public int CurrentTime
        {
            get
            {
                if (DoneSegmentsCol.Any())
                {
                    var x = DoneSegmentsCol.Last().TimeEnded;
                    return (int)x.Subtract(TimeStarted).TotalSeconds;
                }

                return 0;
            }
        }
    }
}
