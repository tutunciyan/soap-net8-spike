using CoreWCF;
using CoreWCF.Channels;
using CoreWCF.Configuration;
using CoreWCF.Description;
using idunno.Authentication.Basic;
using SoapServiceDemo.Services;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// CoreWCF services
builder.Services.AddServiceModelServices();
builder.Services.AddServiceModelMetadata();

builder.Services.AddAuthentication(BasicAuthenticationDefaults.AuthenticationScheme)
    .AddBasic(options =>
    {
        options.Realm = "CalculatorService";
        options.Events = new BasicAuthenticationEvents
        {
            OnValidateCredentials = context =>
            {
                // Configure your own credential validation logic here
                if (context.Username == "admin" && context.Password == "password")
                {
                    var claims = new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, context.Username, ClaimValueTypes.String, context.Options.ClaimsIssuer),
                    };

                    context.Principal = new ClaimsPrincipal(new ClaimsIdentity(claims, context.Scheme.Name));
                    context.Success();
                }

                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// Add REST logic support (optional if not using MVC)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(); // Optional: Swagger UI

// Build the app
var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

// CoreWCF SOAP service config
app.UseServiceModel(serviceBuilder =>
{
    serviceBuilder.AddService<CalculatorService>();

    // 👇 Define a binding that requires HTTPS and credentials
    var basicHttpBinding = new BasicHttpBinding
    {
        Security =
        {
            Mode = BasicHttpSecurityMode.Transport,
            Transport =
            {
                ClientCredentialType = HttpClientCredentialType.Basic
            }
        }
    };

    // 👆 Add the service endpoint with the binding and authentication
    serviceBuilder.AddServiceEndpoint<CalculatorService, ICalculatorService>(
        basicHttpBinding, "/CalculatorService.svc");

    var metadataBehavior = app.Services.GetRequiredService<ServiceMetadataBehavior>();
    metadataBehavior.HttpsGetEnabled = true;
});

// ✅ Minimal REST endpoint - Optional
app.MapGet("/api/add", (int a, int b) =>
    {
        var calculator = new CalculatorService();
        return Results.Ok(new { result = calculator.Add(a, b) });
    })
    .WithName("Add")
    .WithOpenApi(); // For Swagger support

// Optional: Swagger UI
app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint(url: "/swagger/v1/swagger.json", name: "SoapServiceDemo"));

app.Run();