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
using System.Threading;

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
        Thread serviceThread=null;

        protected override void OnStart(string[] args)
        {
            serviceThread = new Thread(new ThreadStart(() =>
                {
                    try
                    {


                        //set up logging
                        string eventLogSource = "PC2CW Service";
                        if (!EventLog.SourceExists(eventLogSource))
                            EventLog.CreateEventSource(eventLogSource, "Application");

                        //setup dependencies
                        var builder = new ContainerBuilder();
                        EventLog.WriteEntry(eventLogSource, "Setting up dependencies", EventLogEntryType.Information);

                        //todo: configuration based provider setup
                        builder.Register<IisServerWorkerProcessCpuLister>(c => new IisServerWorkerProcessCpuLister()).As<IPerformanceCounterLister>();

                        //setup manager
                        manager = new CounterManager(builder);
                        manager.WriteToLog = x =>
                        {
                            string message = String.Format("{0:yyyyMMdd-HHmmss} {1}", DateTime.Now, x);
                            EventLog.WriteEntry(eventLogSource, message, EventLogEntryType.Information);
                        };
                        manager.Start();
                    }
                    catch (Exception ex)
                    {
                        //todo: log eception stopping
                        string eventLogSource = "PC2CW Service";
                        if (!EventLog.SourceExists(eventLogSource))
                            EventLog.CreateEventSource(eventLogSource, "Application");

                        EventLog.WriteEntry(eventLogSource, "Error in main service thread: " + ex.Message + Environment.NewLine + ex.StackTrace, EventLogEntryType.Error);
                    }

                }));

                serviceThread.Start();
            }

        protected override void OnStop()
        {
            try
            {
                if (manager != null)
                    manager.Stop();
                serviceThread.Abort();
            }
            catch (Exception ex)
            {
                //todo: log eception stopping
                string eventLogSource = "PC2CW Service";
                if (!EventLog.SourceExists(eventLogSource))
                    EventLog.CreateEventSource(eventLogSource, "Application");

                EventLog.WriteEntry(eventLogSource, "Error stopping: " + ex.Message, EventLogEntryType.Error);
            }
        }
    }
}
