using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Natol.PerformanceCounter2CloudWatch.Framework;
using Microsoft.Web.Administration;
using System.Diagnostics;

namespace Natol.PerformanceCounter2CloudWatch.IIS
{
    public class IisServerWorkerProcessCpuLister : IPerformanceCounterLister
    {
        public IList<CounterDescriptor> UpdateCounterItems(IList<CounterDescriptor> counterItems)
        {
            ServerManager iisManager = new ServerManager();
            var processes = iisManager.WorkerProcesses;

            //remove old
            var oldIds = new List<int>();
            foreach (var counterDescriptor in counterItems.Cast<IisWorkerProcessCpuCounterDescriptor>())
            {
                if (!processes.Any(proc => proc.ProcessId == counterDescriptor.ProcessId) || !Process.GetProcesses().Any(process => process.Id == counterDescriptor.ProcessId))
                {
                    oldIds.Add(counterDescriptor.ProcessId);
                }
                else
                {
                    //fix names
                    var correctProcessName = PerformanceCounterProcessHelper.GetPerformanceCounterProcessName(counterDescriptor.ProcessId);
                    if (!String.Equals(counterDescriptor.SystemCounter.InstanceName, correctProcessName, StringComparison.InvariantCulture))
                    {
                        counterDescriptor.SystemCounter.InstanceName = correctProcessName;
                    }
                }
            }
            counterItems = counterItems.Cast<IisWorkerProcessCpuCounterDescriptor>().Where(counter => !oldIds.Contains(counter.ProcessId)).Cast<CounterDescriptor>().ToList();

            //add new
            foreach (var w3wp in processes)
            {
                if (!counterItems.Cast<IisWorkerProcessCpuCounterDescriptor>().Any(rd => rd.ProcessId == w3wp.ProcessId) && Process.GetProcesses().Any(process => process.Id == w3wp.ProcessId))
                {
                    counterItems.Add(CreateDescriptor(w3wp));
                }
            }

            return counterItems;
        }

        private CounterDescriptor CreateDescriptor(WorkerProcess w3wp)
        {
            var pc = new PerformanceCounter("Process", "% Processor Time", PerformanceCounterProcessHelper.GetPerformanceCounterProcessName(w3wp.ProcessId), true);
            var result = new IisWorkerProcessCpuCounterDescriptor { Name = w3wp.AppPoolName, SystemCounter = pc, ProcessId = w3wp.ProcessId };
            result.Dimensions.Add("SiteName", w3wp.AppPoolName);
            result.Unit = "Percent";
            result.MetricName = "CPUUtilization";

            return result;
        }
    }
}
