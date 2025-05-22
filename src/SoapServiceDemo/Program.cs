using CoreWCF;
using CoreWCF.Configuration;
using CoreWCF.Description;
using SoapServiceDemo.Services;

var builder = WebApplication.CreateBuilder(args);

// CoreWCF services
builder.Services.AddServiceModelServices();
builder.Services.AddServiceModelMetadata();

// Add REST logic support (optional if not using MVC)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(); // Optional: Swagger UI

var app = builder.Build();

// CoreWCF SOAP service config
app.UseServiceModel(serviceBuilder =>
{
    serviceBuilder.AddService<CalculatorService>(serviceOptions =>
        {
            serviceOptions.DebugBehavior.IncludeExceptionDetailInFaults = true;
        });

    serviceBuilder.AddServiceEndpoint<CalculatorService, ICalculatorService>(
            new BasicHttpBinding(), "/CalculatorService.svc");

    var metadataBehavior = app.Services.GetRequiredService<ServiceMetadataBehavior>();
    metadataBehavior.HttpGetEnabled = true;
    metadataBehavior.HttpsGetEnabled = true;
    metadataBehavior.HttpGetUrl = new Uri("http://localhost:59990/CalculatorService.svc");
    metadataBehavior.HttpsGetUrl = new Uri("https://localhost:44348/CalculatorService.svc");
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