using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace dnSvc
{
    public abstract class DnItem
    {

        [XmlIgnore]
        public CancellationToken Token;

        public int Size { get; set; } = 0;

        public bool Busy { get; set; }

        public bool Done { get; set; }

        public DateTime TimeStarted { get; set; }

        public DateTime TimeEnded { get; set; }

        public TimeSpan TimeTaken { get; set; }

        public int Index { get; set; }

        private int _retries;
        private DateTime _retryTill;
        [XmlIgnore]
        public int Retries
        {
            get
            {
                if (this.GetType() == typeof(DnSegment))
                    return ((DnSegment)(this)).Parent.Retries;
                return _retries;
            }
            set
            {
                if (this.GetType() == typeof(DnSegment))
                    ((DnSegment) (this)).Parent.Retries = value;
                else
                {
                    if (value > 0)
                        _retryTill=DateTime.Now.AddSeconds(10);
                    _retries = value;
                }
            }
        }

        public int RedPercent
        {
            get
            {
                if (Retries == 0)
                    return 0;
                var diff = _retryTill.Subtract(DateTime.Now);
                var secs = Math.Max(0,(int)diff.TotalSeconds);
                if (secs == 0)
                    return 0;
                else return secs * 100 / 10;
            }
        }



        [XmlIgnore]
        public DnTransport Transport;

        protected DnItem(CancellationToken token, Uri fromUri)
        {
            Token = token;
            Transport = DnTransport.Make(fromUri);
        }

        protected DnItem()
        {

        }

        public void Start()
        {
            var ok = false;
            Busy = true;
            TimeStarted = DateTime.Now;
            try
            {
                if (File.Exists(FileName))
                    File.Delete(FileName);
                try
                {
                    StartAction();
                    ok = true;
                }
                catch (Exception e)
                {
                    Retries += 1;
                }
            }
            finally
            {
                if (ok)
                {
                    Retries = 0;
                    Done = true;
                }
                Busy = false;
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
