using System.Text.Json.Serialization;
using Scalar.AspNetCore;
using VetClinic.Api.Middleware;
using VetClinic.Application.Services;
using VetClinic.Contracts.Services;
using VetClinic.Domain.Repositories;
using VetClinic.Domain.Seeds;
using VetClinic.Infrastructure.InMemory.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<VetClinicSeed>();

builder.Services.AddSingleton<IOwnerRepository, OwnerRepository>();
builder.Services.AddSingleton<IPetRepository, PetRepository>();
builder.Services.AddSingleton<IVetRepository, VetRepository>();
builder.Services.AddSingleton<IAppointmentRepository, AppointmentRepository>();
builder.Services.AddSingleton<IBreedRepository, BreedRepository>();
builder.Services.AddSingleton<ISpecializationRepository, SpecializationRepository>();

builder.Services.AddScoped<IOwnerService, OwnerService>();
builder.Services.AddScoped<IPetService, PetService>();
builder.Services.AddScoped<IVetService, VetService>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<IBreedService, BreedService>();
builder.Services.AddScoped<ISpecializationService, SpecializationService>();
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();

builder.Services.AddExceptionHandler<KeyNotFoundExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services
    .AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddOpenApi();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapControllers();

app.Run();
