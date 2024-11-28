using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using RestSharp;
using System.Text.Json;

namespace PokeSearchMBudzisz
{
    public class PokeSearchFunction
    {
        private readonly ILogger<PokeSearchFunction> _logger;
        private readonly RestClient _client;

        public PokeSearchFunction(ILogger<PokeSearchFunction> logger)
        {
            _logger = logger;
            _client = new RestClient("https://pokeapi.co/api/v2/");
        }

        [Function("SearchByName")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequest httpRequest)
        {
            var pokeName = httpRequest.Query["name"];
            _logger.LogInformation($"SearchByName has been called with param: {pokeName}");
            var restRequest = new RestRequest($"pokemon/{pokeName}");
            var response = await _client.ExecuteAsync(restRequest);
            return new OkObjectResult($"Welcome to Azure Functions! Here I gonna write the details of pokemon: {pokeName}");
            /* obrazki:   
             * ROOT Dokumentu:
             *  sprites
                        front_default
                        front_shiny

             */

        }
    }
}
