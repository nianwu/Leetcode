using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace ConsoleApp
{
    public class DataCreator : IHostedService
    {
        private readonly ILogger<DataCreator> _logger;

        public DataCreator(ILogger<DataCreator> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Triggered when the application host is ready to start the service.
        /// </summary>
        /// <param name="cancellationToken">Indicates that the start process has been aborted.</param>
        public Task StartAsync(CancellationToken cancellationToken)
        {
            var testData = Create(10);

            _logger.LogInformation("testData:{testData}", JsonConvert.SerializeObject(testData));

            return Task.CompletedTask;
        }

        /// <summary>
        /// Triggered when the application host is performing a graceful shutdown.
        /// </summary>
        /// <param name="cancellationToken">Indicates that the shutdown process should no longer be graceful.</param>
        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public IEnumerable<int> Create(int length = 100)
        {
            var rand = new Random();

            for (int i = 0; i < length; i++)
            {
                yield return rand.Next(1, 100);
            }
        }
    }
}
