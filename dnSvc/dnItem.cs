using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;

namespace dnSvc
{
    public abstract class DnItem
    {
        public Uri FromUri { get; set; }

        public DnTransport Transport;

        public CancellationToken Token;

        public string Name { get; set; }

        public Guid Id { get; set; }

        public int Size { get; set; } = 0;

        public bool Busy { get; set; }

        public bool Done { get; set; }

        public DateTime TimeStarted { get; set; }

        public DateTime TimeEnded { get; set; }

        public TimeSpan TimeTaken { get; set; }

        public int Index { get; set; }

        protected DnItem(CancellationToken token, Uri fromUri)
        {
            Token = token;
            FromUri = fromUri;
            Name = fromUri.Segments.Last();
            Transport = DnTransport.Make(FromUri);
        }

        public void Start()
        {
            Busy = true;
            TimeStarted = DateTime.Now;
            try
            {
                if (File.Exists(FileName))
                    File.Delete(FileName);
                StartAction();
            }
            finally
            {
                Busy = false;
                Done = true;
            }
            TimeEnded = DateTime.Now;
            TimeTaken = TimeStarted.Subtract(TimeEnded);
        }

        public void Complete()
        {
            CompleteAction();
        }

        protected abstract void StartAction();

        protected abstract void CompleteAction();

        public abstract string FileName { get; }
    }
}
