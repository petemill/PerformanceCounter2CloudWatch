using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Natol.PerformanceCounter2CloudWatch.IIS
{
    public static class PerformanceCounterProcessHelper
    {
        public static string GetPerformanceCounterProcessName(int pid)
        {
            return GetPerformanceCounterProcessName(pid, System.Diagnostics.Process.GetProcessById(pid).ProcessName);
        }
        public static string GetPerformanceCounterProcessName(int pid, string processName)
        {
            int nameIndex = 1;
            string value = processName;
            string counterName = processName + "#" + nameIndex;
            PerformanceCounter pc = new PerformanceCounter("Process", "ID Process", counterName, true);

            while (true)
            {
                try
                {
                    if (pid == (int)pc.NextValue())
                    {
                        value = counterName;
                        break;
                    }
                    else
                    {
                        nameIndex++;
                        counterName = processName + "#" + nameIndex;
                        pc = new PerformanceCounter("Process", "ID Process", counterName, true);
                    }
                }
                catch (SystemException ex)
                {
                    if (ex.Message == "Instance '" + counterName + "' does not exist in the specified Category.")
                    {
                        break;
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            return value;
        }
    }
}
