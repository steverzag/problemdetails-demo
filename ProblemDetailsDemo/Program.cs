using Microsoft.AspNetCore.Http.Features;
using ProblemDetailsDemo;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddHttpClient();
builder.Services.AddScoped<WeatherService>();
builder.Services.AddProblemDetails(options =>
	options.CustomizeProblemDetails = context =>
	{
		context.ProblemDetails.Instance =
			$"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}";

		context.ProblemDetails.Extensions.TryAdd("requestId", context.HttpContext.TraceIdentifier);

		var activity = context.HttpContext.Features.Get<IHttpActivityFeature>()?.Activity;
		context.ProblemDetails.Extensions.TryAdd("traceId", activity?.Id);
	}
);
builder.Services.AddExceptionHandler<ProblemExceptionHandler>();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseExceptionHandler();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("/weatherforecast", async (string city, string units, WeatherService weather) =>
{
	var degreeUnits = units switch
	{
		"f" => "imperial",
		"c" => "metric",
		"k" => "standard",
		_ => "invalid"
	};

	if (degreeUnits == "invalid")
	{
		// This is the original way to return a problem response. Now we handle it using ProblemExceptionHandler.
		//return Results.Problem(
		//	type: "", 
		//	title: "Invalid Units", 
		//	detail: "Invalid units. Use 'f', 'c', or 'k'.", 
		//	statusCode: StatusCodes.Status400BadRequest
		//);

		throw new ProblemException("Invalid Units", "Invalid units. Use 'f', 'c', or 'k'");
	}

	var result = await weather.GetWeatherAsync(city, degreeUnits);

	return Results.Ok(result);
})
.WithName("GetWeatherForecast");

app.Run();

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
	public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
