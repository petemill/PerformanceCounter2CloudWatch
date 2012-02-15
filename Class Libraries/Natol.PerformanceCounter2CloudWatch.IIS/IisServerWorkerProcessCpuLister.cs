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
        /// <summary>
        /// This instance requires update as the process Ids of worker processes can change often
        /// </summary>
        public bool RequiresUpdate
        {
            get { return true; }
        }

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
            //obtain reference to PerformanceCounter object
            var pc = new PerformanceCounter("Process", "% Processor Time", PerformanceCounterProcessHelper.GetPerformanceCounterProcessName(w3wp.ProcessId), true);
            //read value to start counter
            pc.NextValue();

            //setup descriptor object for central framework
            var result = new IisWorkerProcessCpuCounterDescriptor 
            {
                Name = w3wp.AppPoolName, 
                SystemCounter = pc, 
                ProcessId = w3wp.ProcessId,
                Unit = "Percent",
                MetricName = "CPUUtilization"
            };
            result.Dimensions.Add("SiteName", w3wp.AppPoolName);
            
            return result;
        }

    }
}
