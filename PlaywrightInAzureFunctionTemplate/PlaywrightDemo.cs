using System.Collections.Generic;
using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;

namespace PlaywrightInAzureFunctionTemplate
{
    public class PlaywrightDemo
    {
        private readonly ILogger _logger;

        public PlaywrightDemo(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<PlaywrightDemo>();
        }

        [Function(nameof(PlaywrightDemo))]
        public async Task<HttpResponseData> RunPlaywrightDemoAsync([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

            response.WriteString("Welcome to Azure Functions!");

            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });
            var page = await browser.NewPageAsync();
            await page.GotoAsync("https://playwright.dev/dotnet");

            return response;
        }
    }
}
