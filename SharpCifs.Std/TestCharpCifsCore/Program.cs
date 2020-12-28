﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SharpCifs.Smb;
using SharpCifs.Netbios;
using System.IO;

namespace TestCharpCifsCore
{
    public class Program
    {
        private class AuthInfo
        {
            public string ServerName { get; set; }
            public string ServerIP { get; set; }
            public string UserName { get; set; }
            public string Password { get; set; }
        }

        private static AuthInfo Info = new AuthInfo();
        private static NtlmPasswordAuthentication Auth;

        public static void Main(string[] args)
        {
            if (!Secrets.HasSecrets)
                Console.WriteLine("FUCK OFF!!!!");


            Info.ServerName = Secrets.Get("ServerName");
            Info.ServerIP = Secrets.Get("ServerIp");
            Info.UserName = Secrets.Get("UserName");
            Info.Password = Secrets.Get("Password");

            using (var file = new FileStream(@"C:\dev\log\sharpcifs.log", FileMode.OpenOrCreate, FileAccess.ReadWrite))
            using (var writer = new StreamWriter(file, System.Text.Encoding.UTF8))
            {
                writer.Write("log start");
                SharpCifs.Util.LogStream.SetInstance(writer);
                var logger = SharpCifs.Util.LogStream.GetInstance();
                logger.Level = 3;

                //Auth = new NtlmPasswordAuthentication(null, Info.UserName, Info.Password);

                ////**Change local port for NetBios.
                ////  In many cases, use of the well-known port is restricted. **
                //SharpCifs.Config.SetProperty("jcifs.smb.client.lport", "2137");

                //Program.BigFileTranferTest();


                //Program.NameResolutionTest2();

                //Program.NameResolutionTest();

                //Program.LanScanTest();

                //Program.ConnectionFailure();

                Program.DisposeTest();

            }

            var end = 1;
        }

        private static string GetUriString(string path)
        {
            return $"smb://{Info.UserName}:{Info.Password}@{Info.ServerName}/{path}";
        }

        private static void BigFileTranferTest()
        {
            var url = GetUriString("FreeArea/SharpCifsTest2/bigfile.zip");

            var startTime = DateTime.Now;
            Out($"Start");

            var smb = new SmbFile(url);
            var stream = new MemoryStream();
            using (var smbStream = smb.GetInputStream())
            {
                ((Stream)smbStream).CopyTo(stream);
            }
            Out($"End: {(DateTime.Now - startTime).TotalMilliseconds} msec");

            var end = 1;
        }


        private static void NameResolutionTest()
        {
            var namedServer = new SmbFile($"smb://{Info.ServerIP}/apps/", Auth);
            var exists = namedServer.Exists();

            var list = namedServer.ListFiles();
            foreach (var smb in list)
                Out($"{smb.GetName()}");

        }

        private static void NameResolutionTest2()
        {
            var naddr = NbtAddress.GetByName($"COCO4");
            Out($"{naddr.GetHostName()}");

            var auth = new NtlmPasswordAuthentication("", Info.UserName, Info.Password);
            var namedServer = new SmbFile($"smb://{Info.ServerIP}/", auth);
            var exists = namedServer.Exists();

            var list = namedServer.ListFiles();
            foreach (var smb in list)
                Out($"{smb.GetName()}");
        }

        private static void LanScanTest()
        {
            var lan = new SmbFile("smb://", "");
            var workgroups = lan.ListFiles();

            foreach (var workgroup in workgroups)
            {
                var wgName = workgroup.GetName();
                Out("");
                Out($"Workgroup: {wgName}");

                try
                {
                    var servers = workgroup.ListFiles();
                    foreach (var server in servers)
                    {
                        var svName = server.GetName();
                        Out($"    Server: {svName}");

                        try
                        {
                            var shares = server.ListFiles();

                            foreach (var share in shares)
                            {
                                var shName = share.GetName();
                                Out($"        Share: {shName}");
                            }
                        }
                        catch (Exception)
                        {
                            Out($"    Server: {svName} - Access Denied");
                        }
                    }
                }
                catch (Exception)
                {
                    Out($"Workgroup: {wgName} - Access Denied");
                }
            }
        }

        private static void ConnectionFailure()
        {
            //try
            //{
            //    //フォルダが存在しない。
            //    var smb1 = new SmbFile("smb://root:igaHaraW@192.168.254.11/FreeArea/NotExistFolder/");
            //    var exists = smb1.Exists();
            //}
            //catch (Exception ex)
            //{
            //    throw ex;
            //}

            //try
            //{
            //    //フォルダにも拘わらず、末尾の'/'が無い。
            //    var smb1 = new SmbFile("smb://root:igaHaraW@192.168.254.11/FreeArea/SharpCifsTest3");
            //    var exists = smb1.Exists();
            //}
            //catch (Exception ex)
            //{
            //    throw ex;
            //}

            try
            {
                //サーバが存在しない。
                var smb1 = new SmbFile("smb://root:igaHaraW@192.168.254.111/FreeArea/SharpCifsTest3/");
                var exists = smb1.Exists();
            }
            catch (Exception ex)
            {
                throw ex;
            }

            try
            {
                //共有が存在しない。
                var smb1 = new SmbFile("smb://root:igaHaraW@192.168.254.11/NotExistShare/");
                var exists = smb1.Exists();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static void DisposeTest()
        {
            var url = GetUriString("FreeArea/SharpCifsTest2/bigfile.zip");

            var startTime = DateTime.Now;
            Out($"Start");

            var smb = new SmbFile(url);
            var stream = new MemoryStream();
            var smbStream = smb.GetInputStream();
            ((Stream)smbStream).CopyTo(stream);
            smbStream.Dispose();

            Out($"End: {(DateTime.Now - startTime).TotalMilliseconds} msec");

            var end = 1;
        }



        private static void Out(string message)
        {
            Console.WriteLine($"[{DateTime.Now.ToString("HH:mm:ss.fff")}  ThID: {System.Environment.CurrentManagedThreadId.ToString().PadLeft(4)}]: {message}");
        }
    }
}
