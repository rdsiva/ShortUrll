using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Net;
using System.Threading.Tasks;
using Truckstop.Functions.ShortUrlGenerator.Contract;
using Truckstop.Functions.ShortUrlGenerator.Domain;
using Truckstop.Functions.ShortUrlGenerator.Domain.Models;

namespace Truckstop.UrlShortner.Functions
{
    public class ShortUrlCreate
    {
        private readonly ILogger<ShortUrlCreate> _logger;

        public ShortUrlCreate(ILogger<ShortUrlCreate> log)
        {
            _logger = log;
        }

        [FunctionName("ShortUrlCreate")]
        [OpenApiOperation(operationId: "Run", tags: new[] { "name" })]
        //[OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        [OpenApiParameter(name: "ShortUrlRequest",  Required = true, Type = typeof(ShortUrlRequest), Description = "The shortUrlRequest parameter")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(ShortUrlResponse), Description = "The OK response")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "text/plain", bodyType: typeof(ShortUrlResponse), Description = "The badrequest response")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "shortUrl/Create")] ShortUrlRequest input, ExecutionContext context)
        {
            _logger.LogInformation($"__trace creating shortURL: {input}");
            string userId = string.Empty;
            var result = new ShortUrlResponse();

    
            StorageTableHelper _storageTableHelper = new StorageTableHelper(ShortUrlSettings.StorageConnectionString);
            try
            {
                if (input == null)
                {
                    return new BadRequestObjectResult("Invalid request");
                }

                // If the Url parameter only contains whitespaces or is empty return with BadRequest.
                if (string.IsNullOrWhiteSpace(input.Url))
                {
                    return new BadRequestObjectResult("The url parameter can not be empty.");
                }

                // Validates if input.url is a valid aboslute url, aka is a complete refrence to the resource, ex: http(s)://google.com
                if (!Uri.IsWellFormedUriString(input.Url, UriKind.Absolute))
                {
                    return new BadRequestObjectResult($"{input.Url} is not a valid absolute Url. The Url parameter must start with 'http://' or 'http://'.");
                }

                ShortUrlEntity newRow = new ShortUrlEntity(input.Url.Trim(), await Utility.GetValidEndUrl(_storageTableHelper), input.Title?.Trim());
                await _storageTableHelper.SaveShortUrlEntity(newRow);

                result = new ShortUrlResponse(ShortUrlSettings.CustomDomain.ToString(), newRow.Url, newRow.RowKey, newRow.Title);

                _logger.LogInformation("Short Url created.");
            }
            catch (Exception)
            {

                throw;
            }

            return new OkObjectResult(result);
        }
    }
}

