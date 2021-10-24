using Api;
using Api.Framework;
using Api.Services;
using static Api.EndpointAuthenticationDeclaration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddApiServices();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

Anonymous(

    app.MapGet<GetBlogsRequest>("/blogs"),
    app.MapGet<GetBlogRequest>("/blogs/{id}"),
    // curl -i -X POST -H "Content-Type: application/json" -d "{\"title\":\"test body\"}" "http://localhost:5000/test/1?v=test"
    app.MapPost<TestRequest>("/test/{id}")
);

Admin(
    // curl -i -X POST -H "Content-Type: application/json" -d "{\"title\":\"boi\"}" http://localhost:5000/admin/blogs
    app.MapPost<CreateBlogRequest>("/admin/blogs")
);



var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
       new WeatherForecast
       (
           DateTime.Now.AddDays(index),
           Random.Shared.Next(-20, 55),
           summaries[Random.Shared.Next(summaries.Length)]
       ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.Run();
