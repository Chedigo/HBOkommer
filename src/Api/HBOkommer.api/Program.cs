using HBOkommer.Api.Sms;
using HBOkommer.Shared.Sms;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// Trinn 5: registrer SMS-adapter (leverandørnøytral)
builder.Services.AddSingleton<ISmsSender, PilotSmsSender>();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
