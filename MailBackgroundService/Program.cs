using MailBackgroundService.Services;
using MailBackgroundService.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<I3PLUserCredentialsService, _3PLUserCredentialsService>();
builder.Services.AddSingleton<I3PLGlobalUserCredentialsService, _3PLGlobalUserCredentialsService>();
builder.Services.AddSingleton<IRatesService, RatesService>();
builder.Services.AddSingleton<IGoogleUserCredentialsService, GoogleUserCredentialsService>();
builder.Services.AddHostedService<MailBackgroundService.Services.MailBackgroundService>();
var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
