﻿using CSRedis;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.SelfHost;

namespace TimeSystem
{
    public class CoreService
    {
        private static Hashtable ht = new Hashtable();
        HttpSelfHostConfiguration config;
        HttpSelfHostServer server;
        string srvName;
        string srvDesc;
        public CoreService(string srvName, string srvDesc)
        {
            config = new HttpSelfHostConfiguration("http://localhost:3333");
            config.Routes.MapHttpRoute("default", "api/{controller}/{action}/{id}", new { id = RouteParameter.Optional });
            server = new HttpSelfHostServer(config);
            this.srvName = srvName;
            this.srvDesc = srvDesc;

        }
        public void Start()
        {
            try
            {
                LogHelper.WriteLog(srvName + "将要启动了...");
                //服务启动
                server.OpenAsync().Wait();
                LoadJob();

                LogHelper.WriteLog(srvName + "启动成功!");
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(srvName + "启动失败:" + ex.ToString());
                throw ex;
            }
        }
        /// <summary>
        /// 暂停服务时执行
        /// </summary>
        public void Stop()
        {
            server.CloseAsync().Wait();
            LogHelper.WriteLog(srvName + "停止了!");
            //服务停止

        }
        /// <summary>
        /// 关闭服务时执行
        /// </summary>
        public void Shutdown()
        {
            server.CloseAsync().Wait();
            LogHelper.WriteLog(srvName + "关闭了!");
            //服务关闭

        }
        /// <summary>
        /// 继续服务时
        /// </summary>
        public void Continue()
        {
            server.OpenAsync().Wait();
            TaskHelper.Sche();
            LogHelper.WriteLog(srvName + "继续了!");
            //服务继续

        }
        /// <summary>
        /// 暂停服务
        /// </summary>
        public void Pause()
        {
            server.CloseAsync().Wait();
            LogHelper.WriteLog(srvName + "暂停了!");
            //服务暂停

        }
        private void LoadJob()
        {
            TaskHelper.Sche();
        }
    }
}
