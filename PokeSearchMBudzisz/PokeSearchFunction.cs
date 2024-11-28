using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
using PokeSearchMBudzisz.Models;
using RestSharp;
using System.Net;
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
        public async Task<ActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequest httpRequest)
        {
            PokemonDetails? pokemonDetails = null;
            PokeFuncResponse pokeFuncResponse;
            var msg = "";
            var name = httpRequest.Query["name"];
            _logger.LogInformation($"SearchByName has been called with query param: {name}");

            if (string.IsNullOrWhiteSpace(name))
            {
                pokeFuncResponse = new PokeFuncResponse(HttpStatusCode.BadRequest, null, $"Missing '{nameof(name)}' query parameter, try again.");
                _logger.LogInformation($"API received faulted request, missing query param");
                return new BadRequestObjectResult(pokeFuncResponse);
            }

            var restRequest = new RestRequest($"pokemon/{name}");
            var apiRes = await _client.ExecuteAsync(restRequest);

            if (apiRes.IsSuccessStatusCode && apiRes.Content is not null)
            {
                pokemonDetails = ExtractFromJson(apiRes.Content);
                if (pokemonDetails is null) 
                {
                    msg = $"External API response code: {apiRes.StatusCode}, external API response faulted, data missing fields";
                    pokeFuncResponse = new PokeFuncResponse(HttpStatusCode.BadGateway, null, msg);
                    _logger.LogInformation(msg);
                }
                else 
                { 
                    pokeFuncResponse = new PokeFuncResponse(HttpStatusCode.OK, pokemonDetails); 
                    _logger.LogInformation($"Pokemon search succeed.");
                }
            }
            else if (apiRes.StatusCode == System.Net.HttpStatusCode.NotFound) 
            { 
                pokeFuncResponse = new PokeFuncResponse(HttpStatusCode.NotFound, null, "NotFound");
                _logger.LogInformation($"Pokemon with the name of: {name} has not been found");
            }
            else 
            {
                msg = $"External API response code: {apiRes.StatusCode}, {apiRes.StatusDescription}";
                pokeFuncResponse = new PokeFuncResponse(HttpStatusCode.InternalServerError, null, msg);
                _logger.LogInformation(msg);
            }
            var jsonResult = JsonSerializer.Serialize(pokeFuncResponse);
            _logger.LogInformation($"Returning object: {jsonResult}");
            return new OkObjectResult(jsonResult);
        }

        /// <summary>
        /// Extracts fields within my field of interest from the JSON string
        /// </summary>
        /// <param name="content">JSON string</param>
        /// <returns>Valid PokemonDetails record if all went ok, and null if one of the properties is missing from the response (External API data faulted)</returns>
        private PokemonDetails? ExtractFromJson(string content)
        {
            using JsonDocument document = JsonDocument.Parse(content);
            var root = document.RootElement;
            string? name = root.GetProperty("name").GetString();
            string? baseImageUrl = root.GetProperty("sprites").GetProperty("front_default").GetString();
            string? shinyImageUrl = root.GetProperty("sprites").GetProperty("front_shiny").GetString();
            if (name is not null && baseImageUrl is not null && shinyImageUrl is not null)
            {
                return new PokemonDetails(name, baseImageUrl, shinyImageUrl);
            }
            else
            {
                return null;
            }
        }
    }
}
