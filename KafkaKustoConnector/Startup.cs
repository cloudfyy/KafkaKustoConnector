using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[assembly: FunctionsStartup(typeof(KafkaKustoConnector.Startup))]
namespace KafkaKustoConnector
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddScoped<ITelemetryProvider, TelemetryProvider>();
            builder.Services.AddApplicationInsightsTelemetry();
        }
    }
}
