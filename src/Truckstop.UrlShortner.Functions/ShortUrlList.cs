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
using Functions.ShortUrlGenerator.Domain;
using Functions.ShortUrlGenerator.Domain.Models;

namespace UrlShortner.Functions
{
    public class ShortUrlList
    {
        private readonly ILogger<ShortUrlList> _logger;

        public ShortUrlList(ILogger<ShortUrlList> log)
        {
            _logger = log;
        }

        [FunctionName("ShortUrlList")]
        [OpenApiOperation(operationId: "Run", tags: new[] { "name" })]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(List<ShortUrlEntity>), Description = "The OK response")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "shortUrl/List")] HttpRequest req)
        {
            _logger.LogInformation("Function processed list ShortURL request.");

            StorageTableHelper stgHelper = new StorageTableHelper(ShortUrlSettings.StorageConnectionString);
            List<ShortUrlEntity> result;
            try
            {
                 result = await stgHelper.GetAllShortUrlEntities();
                result = result.Where(p => (p.IsActive == true)).ToList();
               // var host = string.IsNullOrEmpty(customDomain) ? req.Url.Host : _settings.CustomDomain;
                foreach (ShortUrlEntity url in result)
                {
                    url.ShortUrl = Utility.GetShortUrl(ShortUrlSettings.CustomDomain, url.RowKey);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error was encountered.");
                return new BadRequestObjectResult(ex.Message);
            }

            return new OkObjectResult(result);
        }
    }
}

