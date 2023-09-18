using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Truckstop.Functions.ShortUrlGenerator.Domain;
using Truckstop.Functions.ShortUrlGenerator.Domain.Models;

namespace Truckstop.UrlShortner.Functions
{
    public class ShortUrlRedirect
    {
        private readonly ILogger<ShortUrlList> _logger;

        public ShortUrlRedirect(ILogger<ShortUrlList> log)
        {
            _logger = log;
        }

        [FunctionName("ShortUrlRedirect")]
        [OpenApiOperation(operationId: "Run", tags: new[] { "name" })]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        [OpenApiParameter(name: "shorturl", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "The **shorturl** parameter")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "/{shorturl}")] HttpRequest req, string shortUrl)
        {
            _logger.LogInformation("Function processed list ShortURL request.");

          
            string redirectUrl = "https://truckstop.com";


            StorageTableHelper stgHelper = new StorageTableHelper(ShortUrlSettings.StorageConnectionString);
            if (!string.IsNullOrWhiteSpace(shortUrl))
            {
                //redirectUrl = _settings.DefaultRedirectUrl ?? redirectUrl;

                var tempUrl = new ShortUrlEntity(string.Empty, shortUrl);
                var newUrl = await stgHelper.GetShortUrlEntity(tempUrl);

                if (newUrl != null)
                {
                    _logger.LogInformation($"Found it: {newUrl.Url}");
                    newUrl.Clicks++;
                    await stgHelper.SaveClickStatsEntity(new ClickStatsEntity(newUrl.RowKey));
                    await stgHelper.SaveShortUrlEntity(newUrl);
                    redirectUrl = WebUtility.UrlDecode(newUrl.Url);
                }
            }
            else
            {
                _logger.LogInformation("Bad Link, resorting to fallback.");
            }

            return new RedirectResult(redirectUrl);
        }
    }
}

