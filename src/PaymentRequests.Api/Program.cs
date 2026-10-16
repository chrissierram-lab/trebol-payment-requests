using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Mvc;
using PaymentRequests.Api.Contracts;
using PaymentRequests.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = _ => new BadRequestObjectResult(
        new ApiErrorResponse("INVALID_REQUEST", "La solicitud contiene datos inválidos."));
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<FeeCalculatorOptions>(
    builder.Configuration.GetSection(FeeCalculatorOptions.SectionName));
builder.Services.AddSingleton<IPaymentRequestStore, InMemoryPaymentRequestStore>();
builder.Services.AddScoped<PaymentRequestService>();
builder.Services.AddHttpClient<IFeeCalculatorClient, JavaFeeCalculatorClient>((services, client) =>
{
    var options = services.GetRequiredService<IOptions<FeeCalculatorOptions>>().Value;
    client.BaseAddress = new Uri(options.BaseUrl);
    client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
