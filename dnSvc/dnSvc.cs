using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace dnSvc
{
    public sealed class DownloadService
    {
        public static DnConf DnConf = new DnConf();

        public ObservableCollection<DnTask> Tasks { get; set; }

        private IEnumerable<DnItem> ToWorkAll =>
            Tasks.OfType<DnItem>()
                .Where(task => task.Done == false && task.Busy == false && task.Retries == 0)
                .OrderBy(x => Guid.NewGuid())
                .Union
                (
                    Tasks
                        .Where(task => task.Done == true && task.Busy == false && 
                                       (task.Retries==0 || task.RedPercent == 0) )

                        .SelectMany(task => task.Segments)
                        .Where(segment => segment.Done == false && segment.Busy == false)
                        .OrderBy(x => Guid.NewGuid())
                )
                .Union
                (
                    Tasks
                        .Where(task => task.Done == false && task.Busy == false && task.Retries != 0)
                        .OrderBy(x => Guid.NewGuid())
                );

        private IEnumerable<DnItem> ToWork => ToWorkAll.Take(DnConf.Parallels);

        private IEnumerable<DnTask> CompletedTasks =>
            Tasks.Where( x => x.Done && x.Segments.Count > 0 && !x.Segments.Exists(y => y.Done == false));

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

        private readonly object _locker;

        private CancellationToken _token;

        private CancellationTokenSource _tokenSource;

        private readonly ManualResetEvent _signalEvent;

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
                foreach (var t in Tasks.Where(x => x.Retries > 0))
                {
                    if (t.RedPercent == 0)
                        t.Retries = 0;
                }

                var e = ToWork.ToList();
                if (e.Any())
                {
                    if (DnConf.Parallels==1)
                        e.First().Start();
                    else
                    {
                        e = e.Take(Math.Min(DnConf.Parallels, e.Count)).ToList();
                        var tl = new List<Task>();
                        foreach (var x in e)
                        {
                            var t = Task.Factory.StartNew(x.Start);
                            tl.Add(t);
                        }
                        Task.WaitAll(tl.ToArray());
                    }
                }

                foreach (var task in CompletedTasks.Where(x => x.Delivered == false))
                {
                    task.Complete();
                    task.Delivered = true;
                }

                if (Tasks.Count() == Tasks.Count(x=> x.Delivered))
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

        #region live commands
        public void Resume()
        {
            Start();
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
                if (line.Trim().StartsWith("#"))
                    continue;
                BeginDownload(new Uri(line.Trim()));
            }
        }
        #endregion

        #region state commands

        public void Save()
        {
            XmlSerializer formatter = new XmlSerializer(GetType());
            var fs = new FileStream(DnConf.StateFile, FileMode.Create);
            formatter.Serialize(fs, this);
            fs.Close();
        }

        public static DownloadService Load()
        {
            if (File.Exists(DnConf.StateFile) == false)
                return new DownloadService();
            XmlSerializer formatter = new XmlSerializer(typeof(DownloadService));
            var fs = new FileStream(DnConf.StateFile, FileMode.Open);
            var result = (DownloadService)formatter.Deserialize(fs);
            fs.Close();
            foreach (var x in result.Tasks)
            {
                x.Transport = DnTransport.Make(x.FromUri);
                x.Name = x.FromUri.Segments.Last();
                x.Segments.ForEach(y =>
                {
                    y.Parent = x;
                    y.Transport = DnTransport.Make(x.FromUri);
                });
            }

            return result;
        }
        #endregion
    }
}