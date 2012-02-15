using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using Natol.PerformanceCounter2CloudWatch.Framework;
using Natol.PerformanceCounter2CloudWatch.IIS;
using Autofac;

namespace Natol.PerformanceCounter2CloudWatch.PC2CWService
{
    public partial class CounterService : ServiceBase
    {
        public CounterService()
        {
            InitializeComponent();
        }

        //service-wide reference to counter-manager
        CounterManager manager;

        protected override void OnStart(string[] args)
        {
            try
            {
                //setup dependencies
                var builder = new ContainerBuilder();
                Console.WriteLine("Setting up dependencies");

                //todo: configuration based provider setup
                builder.Register<IisServerWorkerProcessCpuLister>(c => new IisServerWorkerProcessCpuLister()).As<IPerformanceCounterLister>();


                //setup manager
                manager = new CounterManager(builder);
                //todo: lof ro dilw bt setting a function on manager.WriteToLog
                manager.Start();
            }
            catch 
            {
                //todo: log exception setting up
            }
        }

        protected override void OnStop()
        {
            try
            {
                if (manager != null)
                    manager.Stop();
            }
            catch
            {
                //todo: log eception stopping
            }
        }
    }
}
