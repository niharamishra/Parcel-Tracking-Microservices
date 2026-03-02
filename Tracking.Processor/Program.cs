using Microsoft.EntityFrameworkCore;
using Parcel.Repositories;
using Tracking.Processor.Interfaces;
using Tracking.Processor.Validators;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ParcelDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("ParcelDb")));

builder.Services.AddScoped<IParcelRepository, ParcelRepository>();

builder.Services.AddHostedService<KafkaConsumerService>();
builder.Services.AddSingleton<IScanStageValidator, CollectionValidator>();
builder.Services.AddSingleton<IScanStageValidator, SortingHubValidator>();
builder.Services.AddSingleton<IScanStageValidator, DeliveryCenterValidator>();
builder.Services.AddSingleton<IScanStageValidator, OutForDeliveryValidator>();
builder.Services.AddSingleton<IScanStageValidator, DeliveredValidator>();

builder.Services.AddSingleton<ScanStageValidatorRegistry>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularDev",
        builder => builder
            .WithOrigins("http://localhost:4200") // Angular dev server
            .AllowAnyMethod()
            .AllowAnyHeader()
    );
});


var app = builder.Build();
app.UseCors("AllowAngularDev");
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseAuthorization();

app.MapControllers();

app.Run();