using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac;
using System.Net;
using Amazon.CloudWatch.Model;
using System.Threading;
using Amazon.CloudWatch;
using System.Configuration;

namespace Natol.PerformanceCounter2CloudWatch.Framework
{
    public class CounterManager
    {
        public CounterManager(ContainerBuilder Builder)
        {
            builder = Builder;
        }
        private ContainerBuilder builder;
        private bool _stop = false;
        public Action<string> WriteToLog { private get; set; }

        public void WriteMessage(string format, params object[] args)
        {
            if (WriteToLog!=null)
                WriteToLog(String.Format(format, args));
        }


        public void Start()
        {
            //resolve lister components which will retreive PerformanceCounter objects
            var listers = builder.Build().Resolve<IEnumerable<IPerformanceCounterLister>>().ToList();

            //setup containers for each PerformanceCounter list and it's manager
            WriteMessage("Getting counter information");
            var lists = new List<PerformanceCounterListerManagedList>();
            foreach (var lister in listers)
            {
                var items = lister.UpdateCounterItems(new List<CounterDescriptor>());
                lists.Add(new PerformanceCounterListerManagedList 
                        {
                            Lister = lister,
                            List= items
                        });
                WriteMessage("{0} Counter items retreived", items.Count);
            }

            //setup our looping manager
            //TODO: make configurable, perhaps per Lister instance
            int errorCount = 0, counterUpdateInterval = 10, counterUpdatedSince = counterUpdateInterval;

            //send machine name
            string instanceId = Environment.MachineName;
            try
            {
                //set machine name to amazon instance-id if we're in ec2
                //TODO: make configurable
                instanceId = new WebClient().DownloadString("http://169.254.169.254/latest/meta-data/instance-id");
            }
            catch { } //this will fail if running machine is not in AWS EC2


            // Once a second, capture data and send to CloudWatch
            while (!_stop)
            {
                DateTime start = DateTime.Now;
                //update counter objects if required to
                if (counterUpdatedSince <= 0)
                {
                    foreach (var list in lists)
                    {
                        list.List = list.Lister.UpdateCounterItems(list.List);
                    }

                    counterUpdatedSince = counterUpdateInterval;
                }

                Console.Clear();
                var data = new List<MetricDatum>();

                foreach (var list in lists)
                {
                    foreach (var item in list.List)
                    {
                        var countValue = item.GetCount();
                        if (countValue.HasValue)
                        {

                            //get metric value and wrap for CloudWatch
                            data.Add(new MetricDatum()
                                .WithMetricName(item.MetricName)
                                .WithDimensions(
                                    item.Dimensions.Select(dim => new Dimension { Name = dim.Key, Value = dim.Value })
                                    .Concat(new[] {
                                    new Dimension { Name = "InstanceID", Value = instanceId }
                                    })
                                )
                                .WithTimestamp(DateTime.UtcNow)
                                .WithUnit(item.Unit)
                                .WithValue(countValue.Value));
                        }
                        else { errorCount++; }
                        WriteMessage("Counter:{0}  Value:{1}", item.Name, countValue);
                    }
                }

                //Console.WriteLine();
                WriteMessage("Errors:{0}", errorCount);

                WriteMessage("Sending {0} entries to CloudWatch", data.Count);
                SendMetrics(data, ConfigurationManager.AppSettings["AWS-CloudWatch-Namespace"]);
                WriteMessage("Metrics sent.");

                counterUpdatedSince--;

                //,easure time since this iteration of the loop began and subtract this TimeSpan from the sleep time
                var length = DateTime.Now - start;

                //TODO: read sleep time from configuration
                //Default is 1 minute. Whilst we could send every second, for many metrics this could get quite expensive
                //  based on CLoudWatch pricing strategy per 1,000 PUT requests.
                Thread.Sleep((1000 * 60)-Convert.ToInt32(length.TotalMilliseconds));
            }

        }

        private void SendMetrics(IList<MetricDatum> data, string dataNamesspace)
        {
            if (data == null || data.Count == 0 || String.IsNullOrWhiteSpace(dataNamesspace))
            {
                WriteMessage("Nothing to send");
                return;
            }
                

            var appSettings = ConfigurationManager.AppSettings;

            WriteMessage(String.Join(",", data.Select(d => d.Value.ToString()).ToArray()));
            //setup cloudwatch service
            AmazonCloudWatch client = Amazon.AWSClientFactory.CreateAmazonCloudWatchClient(appSettings["AWS-CloudWatch-AccessKey"], appSettings["AWS-CloudWatch-SecretKey"], new AmazonCloudWatchConfig { ServiceURL = appSettings["AWS-CloudWatch-ServiceUrl"] });

            client.PutMetricData(new PutMetricDataRequest()
                .WithMetricData(data)
                .WithNamespace(dataNamesspace));

        }


        public void Stop()
        {
            _stop = true;
        }
    }
}
