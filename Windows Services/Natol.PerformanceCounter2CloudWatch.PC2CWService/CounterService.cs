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
                            //do not log every single status report, as this means we're storing all the stats in the event log!
                            //  this should only be used in debug
#if DEBUG
                            string message = String.Format("{0:yyyyMMdd-HHmmss} {1}", DateTime.Now, x);
                            EventLog.WriteEntry(eventLogSource, message, EventLogEntryType.Information);
#endif
                        };
                        EventLog.WriteEntry(eventLogSource, "Starting CounterManager...", EventLogEntryType.Information);
                        manager.Start();
                        EventLog.WriteEntry(eventLogSource, "CounterManager started", EventLogEntryType.Information);
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
                {
                    EventLog.WriteEntry(eventLogSource, "Stopping CounterManager...", EventLogEntryType.Information);
                    manager.Stop();
                    EventLog.WriteEntry(eventLogSource, "CounterManager stopped.", EventLogEntryType.Information);
                }
                EventLog.WriteEntry(eventLogSource, "Aborting service thread...", EventLogEntryType.Information);
                serviceThread.Abort();
                EventLog.WriteEntry(eventLogSource, "Service thread aborted (last action in OnStop() )", EventLogEntryType.Information);
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
