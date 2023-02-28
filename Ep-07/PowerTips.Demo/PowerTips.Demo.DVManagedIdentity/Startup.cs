using Azure.Core;
using Azure.Identity;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[assembly: FunctionsStartup(typeof(PowerTips.Demo.DVManagedIdentity.Startup))]
namespace PowerTips.Demo.DVManagedIdentity
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddMemoryCache();
            builder.Services.AddSingleton(new DefaultAzureCredential());

            builder.Services.AddSingleton<IOrganizationService, ServiceClient>(provider =>
            {
                var managedIdentity = provider.GetRequiredService<DefaultAzureCredential>();
                var environment = Environment.GetEnvironmentVariable("PowerApps:dataverseURL");
                var cache = provider.GetService<IMemoryCache>();
                return new ServiceClient(
                        tokenProviderFunction: f => GetToken(environment, managedIdentity, cache),
                        instanceUrl: new Uri(environment),
                        useUniqueInstance: true);
            });
        }

        private async Task<string> GetToken(string environment, DefaultAzureCredential credential, IMemoryCache cache)
        {
            var accessToken = await cache.GetOrCreateAsync(environment, async (cacheEntry) => {
                cacheEntry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(50);
                var token = (await credential.GetTokenAsync(new TokenRequestContext(new[] { $"{environment}/.default" })));
                return token;
            });
            return accessToken.Token;
        }
    }
}
