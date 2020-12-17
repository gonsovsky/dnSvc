using System;
using System.ComponentModel;
using System.IO;
using System.Threading;

namespace dnSvc
{
    public abstract class DnItem: INotifyPropertyChanged
    {
        public CancellationToken Token;

        public string Name { get; set; }

        public Guid Id { get; set; }

        public int Size { get; set; } = 0;

        public bool Busy { get; set; }

        public bool Done { get; set; }

        public DateTime TimeStarted { get; set; }

        public TimeSpan TimeTaken { get; set; }

        public int Index { get; set; }

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
        }

        public void Complete()
        {
            CompleteAction();
        }

        protected abstract void StartAction();

        protected abstract void CompleteAction();

        public abstract string FileName { get; }

        protected DnItem(CancellationToken token)
        {
            Token = token;  
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void RaisePropertyChangedEvent(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
