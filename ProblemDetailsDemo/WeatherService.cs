using System.Net.Http;
using System.Net;
using System.Text.Json.Serialization;

namespace ProblemDetailsDemo
{
	public class WeatherService
	{

		private readonly IHttpClientFactory _httpClientFactory;
		private readonly IConfiguration _configuration;

		public WeatherService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
		{
			_httpClientFactory = httpClientFactory;
			_configuration = configuration;
		}

		public async Task<WeatherResponse?> GetWeatherAsync(string city, string degreeUnits)
		{
			var apiKey = _configuration["APIKeys:OpenWeather"];
			var url = $"https://api.openweathermap.org/data/2.5/weather?q={city}&units={degreeUnits}&appid={apiKey}";

			var client = _httpClientFactory.CreateClient();
			var response = await client.GetAsync(url);
			if (response.StatusCode is HttpStatusCode.NotFound)
			{
				return null;
			}

			return await response.Content.ReadFromJsonAsync<WeatherResponse>();
		}

	}

	public class WeatherResponse
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public IEnumerable<WeatherInfo> Weather { get; set; }
		public MainWeatherInfo Main { get; set; }
		public string Base { get; set; }
		public int Visibility { get; set; }
		public int Timezone { get; set; }
	}

	public class WeatherInfo
	{
		public int Id { get; set; }
		public string Main { get; set; }
		public string Description { get; set; }
		public string Icon { get; set; }
	}

	public class MainWeatherInfo
	{
		public double Temp { get; set; }
		[JsonPropertyName("feels_like")]
		public double FeelsLike { get; set; }
		[JsonPropertyName("temp_min")]
		public double TempMin { get; set; }
		[JsonPropertyName("temp_max")]
		public double TempMax { get; set; }

	}
}
