﻿using CSRedis;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpRun
{
    public class Program
    {
        static string runUid, filepath, runargs;
        static bool IsDebug = false;
        static string debugprefix = "";
        /// <summary>
        /// ID
        /// 想要运行的文件全路径
        /// 想要运行的文件参数
        /// 日志路径
        /// 日志格式
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {

            rc = new RedisClient(ConfigurationManager.AppSettings.Get("redishost"));
           
            try
            {

                rc.Connect(1000);

                  IsDebug = Boolean.Parse(ConfigurationManager.AppSettings.Get("debug"));

                if (ConfigurationManager.AppSettings.Get("password") != "")
                {

                    rc.Auth(ConfigurationManager.AppSettings.Get("password"));

                }
                if (IsDebug)
                {
                    rc.LPush("debug", args);
                    debugprefix = "debug_";
                }

                runUid = args[0];
                filepath = args[1];
                runargs = args[2];
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                rc.HSet(debugprefix+"status_" + runUid, "startTime", DateTime.Now);
                rc.HIncrBy(debugprefix + "status_" + runUid, "errorcount", 1);
                rc.HIncrBy(debugprefix + "status_" + runUid, "runcount", 1);

                rc.HSet(debugprefix + "status_" + runUid, "updateTime", DateTime.Now);


                Process proc = new Process();
                proc.StartInfo.FileName = "cmd.exe";
                proc.StartInfo.WorkingDirectory = Path.GetDirectoryName(filepath);
                proc.StartInfo.Domain = Path.GetDirectoryName(filepath);
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.RedirectStandardError = true;
                proc.StartInfo.RedirectStandardInput = true;
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.CreateNoWindow = true;
                proc.OutputDataReceived += Proc_OutputDataReceived;
                proc.ErrorDataReceived += Proc_ErrorDataReceived;
                proc.Start();
                proc.BeginErrorReadLine();
                proc.BeginOutputReadLine();

            
                string appcmd = string.Format(GenerateCmd(filepath), filepath, runargs);
                rc.LPush("debug", appcmd);
                proc.StandardInput.WriteLine(appcmd);
                proc.StandardInput.WriteLine("exit");
                proc.WaitForExit();

                //proc.Close();
                //proc.Dispose();

                stopwatch.Stop();
      
                rc.HIncrBy(debugprefix + "status_" + runUid, "errorcount", -1);
                sethistory(stopwatch.Elapsed.TotalSeconds);
            }
            catch (Exception ex)
            {
                rc.LPush(debugprefix + "error_"+ runUid, ex.ToString());
            }
        }

        private static void Proc_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data == null | e.Data == "")
            {
                return;
            }
            var now = DateTime.Now;
            rc.HSet(debugprefix + "status_" + runUid, "updateTime", now);
            rc.LPush(debugprefix + "error_" + runUid, now + "|" + e.Data);
            rc.LPush(debugprefix + "console_" + runUid, now + "|" + e.Data);

        }

        static void sethistory(double runtime)
        {
            rc.HSetNx(debugprefix + "status_" + runUid, "history", runtime);
            rc.HSet(debugprefix + "status_" + runUid, "updateTime", DateTime.Now);
            var s = double.Parse(rc.HGet(debugprefix + "status_" + runUid, "history"));
            rc.HSet(debugprefix + "status_" + runUid, "history", (runtime + s) / 2.0);
        }
        static RedisClient rc;
        private static void Proc_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data == null || e.Data == "")
            {
                return;
            }
            string[] data = e.Data.Split('|');
            var now = DateTime.Now;
            rc.HSet(debugprefix + "status_" + runUid, "updateTime", now);
            switch (data[0])
            {

                case "status":
                    rc.HSet(debugprefix + "status_" + runUid, "status", data[1]);
                    break;
            }
          
            rc.LPush(debugprefix + "console_" + runUid, now + "|" + e.Data);

        }

        public static string GenerateCmd(string filename)
        {
            string extension = filename.Split('.').Last();
            string pattern = ConfigurationManager.AppSettings.Get(extension);
            if (pattern != null)
            {
                return pattern;
            }
            return "{0} {1}";
        }
    }
}
