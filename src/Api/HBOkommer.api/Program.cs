using HBOkommer.Api.Sms;
using HBOkommer.Shared.Sms;
using HBOkommer.Shared.Policy;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Trinn 3/Policy: samtykke + policy-evaluator (ligger i Shared, gjenbrukes av Worker senere)
builder.Services.AddSingleton<IConsentChecker, PilotConsentChecker>();
builder.Services.AddSingleton<IEventPolicyEvaluator, PilotEventPolicyEvaluator>();

// Trinn 5: registrer SMS-adapter (leverandørnøytral)
builder.Services.AddSingleton<ISmsSender, PilotSmsSender>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
