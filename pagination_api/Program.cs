using Microsoft.AspNetCore.Mvc;
using PaginationApp.Services.Parts;
using DotNetEnv;
using PaginationApp.Services.ElasticSearch;
using PaginationApp.Services.Parts.Contracts;
using PaginationApp.Core.Exceptions;
using PaginationApp.Infraestucture;  
using PaginationApp.Services.ElasticSearch.Contracts;

Env.Load();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddSingleton<ElasticConnection>(_ => 
    new ElasticConnection());

builder.Services.AddScoped<IPartMapper, PartMapper>();
builder.Services.AddScoped<IElasticSearchService, ElasticSearchService>();
builder.Services.AddScoped<IPartSearchService, ElasticPartSearchService>();
builder.Services.AddScoped<LogstashManager>();

var app = builder.Build();

//Solo ejecutarlo si se necesita cargar datos de SQL a Elastic

//await InitializeElasticSearch(app);
//await RunLogstashAsync(app); 

app.MapControllers();

await app.RunAsync();



async Task RunLogstashAsync(WebApplication app)
{
    var logstashManager = app.Services.GetRequiredService<LogstashManager>();
    await logstashManager.RunLogstashAsync();
}

