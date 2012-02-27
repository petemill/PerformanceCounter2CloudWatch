using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac;
using Natol.PerformanceCounter2CloudWatch.IIS;
using Natol.PerformanceCounter2CloudWatch.Framework;
using Natol.PerformanceCounter2CloudWatch.PerformanceCounters;
using Natol.PerformanceCounter2CloudWatch.IIS.Traffic;

namespace Natol.PerformanceCounter2CloudWatch.PC2CWConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            //setup dependencies
            var builder = new ContainerBuilder();
            Console.WriteLine("Setting up dependencies");
            builder.Register<IisServerWorkerProcessCpuLister>(c => new IisServerWorkerProcessCpuLister()).As<IPerformanceCounterLister>();
            builder.Register<IisServerSiteTrafficCountLister>(c => new IisServerSiteTrafficCountLister()).As<IPerformanceCounterLister>();

            //setup manager
            var manager = new CounterManager(builder);
            manager.WriteToLog = x => Console.WriteLine("{0:yyyyMMdd-HHmmss} {1}", DateTime.Now, x);
            manager.Start();
        }
    }
}
