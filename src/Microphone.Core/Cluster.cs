﻿using System.Threading.Tasks;
using Microphone.Core.ClusterProviders;
using Microsoft.Extensions.Logging;

namespace Microphone.Core
{
    public static class Cluster
    {
        private static IClusterProvider _clusterProvider;
        private static IFrameworkProvider _frameworkProvider;

        public static Task<ServiceInformation[]> FindServiceInstancesAsync(string name)
        {
            return _clusterProvider.FindServiceInstancesAsync(name);
        }

        public static Task<ServiceInformation> FindServiceInstanceAsync(string name)
        {
            return _clusterProvider.FindServiceInstanceAsync(name);
        }


        public static void BootstrapClient(IClusterProvider clusterProvider)
        {
            _clusterProvider = clusterProvider;
            _clusterProvider.BootstrapClientAsync().Wait();
        }

        public static void Bootstrap(IFrameworkProvider frameworkProvider, IClusterProvider clusterProvider,
            string serviceName, string version, ILogger log)
        {
            log.LogInformation("Bootstrapping microphone..");
            _frameworkProvider = frameworkProvider;
            var uri = _frameworkProvider.Start(serviceName, version);
            var serviceId = serviceName + "_" + DnsUtils.GetLocalEscapedIPAddress() + "_" + uri.Port;
            _clusterProvider = clusterProvider;
            try
            {
                _clusterProvider.RegisterServiceAsync(serviceName, serviceId, version, uri).Wait();
            }
            catch
            {
                log.LogError($"Could not register service {serviceId} using {frameworkProvider.GetType().Name}");
            }
        }

        public static Task KeyValuePutAsync(string key, object value)
        {
            return _clusterProvider.KeyValuePutAsync(key, value);
        }

        public static Task<T> KeyValueGetAsync<T>(string key)
        {
            return _clusterProvider.KeyValueGetAsync<T>(key);
        }
    }
}