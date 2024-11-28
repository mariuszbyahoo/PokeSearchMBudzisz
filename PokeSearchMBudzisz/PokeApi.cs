using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace PokeSearchMBudzisz
{
    public class PokeApi
    {
        private readonly ILogger<PokeApi> _logger;

        public PokeApi(ILogger<PokeApi> logger)
        {
            _logger = logger;
        }

        [Function("SearchByName")]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequest req)
        {
            var pokeName = req.Query["name"];
            _logger.LogInformation($"SearchByName has been called with param: {pokeName}");
            return new OkObjectResult($"Welcome to Azure Functions! Here I gonna write the details of pokemon: {pokeName}");
        }
    }
}
