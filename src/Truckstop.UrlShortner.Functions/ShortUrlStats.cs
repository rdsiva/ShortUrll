using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Functions.ShortUrlGenerator.Contract;
using Functions.ShortUrlGenerator.Domain;
using Functions.ShortUrlGenerator.Domain.Models;

namespace UrlShortner.Functions
{
    public class ShortUrlStats
    {
        private readonly ILogger<ShortUrlStats> _logger;

        public ShortUrlStats(ILogger<ShortUrlStats> log)
        {
            _logger = log;
        }

        //Create Azure Functions with OpenAPI definition for HTTP trigger Crud operations with Azure Data Table

        //Genreate Azure function with open API for HTtp trigger with CRUD operations with Azure Data Table
        //https://docs.microsoft.com/en-us/azure/azure-functions/functions-openapi-definition
        //https://docs.microsoft.com/en-us/azure/azure-functions/functions-openapi-definition#example-2
        //https://docs.microsoft.com/en-us/azure/azure-functions/functions-openapi-definition#example-3
        //https://docs.microsoft.com/en-us/azure/azure-functions/functions-openapi-definition#example-4



        [FunctionName("ShortUrlStats")]
        [OpenApiOperation(operationId: "Run", tags: new[] { "name" })]
        /// [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        [OpenApiParameter(name: "shortUrl", In = ParameterLocation.Query, Required = true, Type = typeof(Uri), Description = "The **shortUrl** parameter")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(ShortUrlClickStatsList), Description = "The OK response")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "ShortUrl/stats")] string shortUrl)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request."+ shortUrl);
            //

            var shortUrlstring =shortUrl.Replace("http://","");
            //if (string.IsNullOrEmpty(shortUrl))
            //{
            //    return new BadRequestObjectResult("Please pass a shortUrl on the query string or in the request body");
            //}
        
            StorageTableHelper stgHelper = new StorageTableHelper(ShortUrlSettings.StorageConnectionString);
            ShortUrlClickStatsList result = new ShortUrlClickStatsList();
            try
            {
               
                string endUrlString = shortUrlstring.Split('/')[1];

                var rawStats = await stgHelper.GetAllStatsByKey(endUrlString.Trim());

                result.Items = rawStats.GroupBy(s => DateTime.Parse(s.Datetime).Date)
                                            .Select(stat => new ShortUrlClickStats
                                            {
                                                DateClicked = stat.Key.ToString("yyyy-MM-dd"),
                                                Count = stat.Count()
                                            }).OrderBy(s => DateTime.Parse(s.DateClicked).Date).ToList<ShortUrlClickStats>();

                //var host = string.IsNullOrEmpty(customDomain) ? req.Url.Host : _settings.CustomDomain.ToString();
                result.Url = Utility.GetShortUrl(ShortUrlSettings.CustomDomain, endUrlString);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error was encountered.");
                return new BadRequestObjectResult("An unexpected error was encountered.");
            }
            return new OkObjectResult(result);
        }
    }
}

