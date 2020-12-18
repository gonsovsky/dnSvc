using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace dnSvc
{
    public sealed class DownloadService
    {
        public static DnConf DnConf = new DnConf();

        public ObservableCollection<DnTask> Tasks { get; set; }

        private IEnumerable<DnItem> UnstartedTasksAndSegments =>
            Tasks
                .Where(task => task.Done == false && task.Busy == false)
                .OfType<DnItem>()
                .Union(
                    Tasks
                        .SelectMany(task => task.Segments)
                        .Where(segment => segment.Done == false && segment.Busy == false).OrderBy(x => Guid.NewGuid())
                ).Take(DnConf.Parallels);

        private IEnumerable<DnTask> CompletedTasks =>
            Tasks.Where(x => x.Segments.Count > 0 && !x.Segments.Exists(y => y.Done == false));

        public int Percent
        {
            get
            {
                if (Tasks.Count == 0)
                    return 0;
                return Tasks.Sum(x=>x.Percent)
                     / Tasks.Count;
            }
        }

        private object _locker;

        private CancellationToken _token;

        private CancellationTokenSource _tokenSource;

        private ManualResetEvent _signalEvent;

        private Thread _thread;

        public DownloadService()
        {
            this.Tasks = new ObservableCollection<DnTask>();
            this._signalEvent = new ManualResetEvent(false);
            this._locker = new object();
        }

        private void ThreadBody(object obj)
        {
            while (!this._token.IsCancellationRequested)
            {
                foreach (var dnItem in UnstartedTasksAndSegments)
                    dnItem.Start();

                foreach (var task in CompletedTasks.Where(x => x.Delivered == false))
                {
                    task.Complete();
                    task.Delivered = true;
                }

                if (Tasks.Count() == Tasks.Count(x=> x.Delivered==true))
                {
                    return;
                }


                //this._signalEvent.Reset();

                //DnTask[] tasksSnapshot;
                //lock (this._locker)
                //{
                //    tasksSnapshot = this.Tasks.ToArray();
                //    if (tasksSnapshot.Length > 0 )
                //        tasksSnapshot[0].Begin();
                //}

                //var res = WaitHandle.WaitAny(new[] {this._token.WaitHandle, this._signalEvent});
                //switch (res)
                //{
                //    case -1:
                //        break;
                //    case 0:
                //        break;
                //    case 1:
                //        break;
                //}

                Thread.Sleep(DnConf.Delay);
            }
            Console.WriteLine("DownloadService.ThreadBody exit");
        }

        public void Start()
        {
            if (this._thread != null && _thread.IsAlive)
                throw new ApplicationException("Service is running");
            this._tokenSource = new CancellationTokenSource();
            this._token = this._tokenSource.Token;
            this._thread = new Thread(this.ThreadBody);
            this._thread.Start();
        }

        public void Abort()
        {
            if (this._thread == null || !this._thread.IsAlive)
                return;
            this._tokenSource.Cancel();
            this._thread.Join(100);
            this._thread = null;
        }

        public bool Aborted => _tokenSource != null && _tokenSource.IsCancellationRequested;

        public bool Running => this._thread != null && this._thread.IsAlive;

        public void BeginDownload(Uri fromUri)
        {
            var task = new DnTask(_token, fromUri);
            lock (this._locker)
            {
                this.Tasks.Add(task);
                task.Index = Tasks.Count;
            }
            this._signalEvent.Set();
        }

        public void BeginListOfFiles(string aFile)
        {
            Tasks.Clear();
            var lines = File.ReadAllLines(aFile);
            foreach (var line in lines)
            {
                BeginDownload(new Uri(line));
            }
        }

        static DownloadService()
        {
            ServicePointManager.Expect100Continue = false;
            ServicePointManager.DefaultConnectionLimit = 100;
            ServicePointManager.MaxServicePointIdleTime = 1000;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
        }
    }
}