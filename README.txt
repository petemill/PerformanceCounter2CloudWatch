This is a windows service (and identical console app) which sends data from Windows Performance Counters to CloudWatch, an Amazon Web Services Product.

Performance Counters are found by plugins which are loaded through an IoC container. These configurable components can make decisions about which Performance Counters to expose, such as:
- CPU and memory usage for a specific application
- CPU and memory usage for each worker process within IIS
- Amount of web requests for each IIS site
- Free disk space on each drive