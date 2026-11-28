var builder = DistributedApplication.CreateBuilder(args);

var sql = builder.AddSqlServer("sql");
if (!bool.TryParse(builder.Configuration["SqlDataVolume"], out var useDataVolume) || useDataVolume)
{
    sql.WithDataVolume();
}

var database = sql.AddDatabase("vetclinic");
var kafka = builder.AddKafka("kafka").WithKafkaUI(containerName: "kafbat");

builder.AddProject<Projects.VetClinic_Api>("api")
    .WithReference(database)
    .WithReference(kafka)
    .WaitFor(database)
    .WithHttpHealthCheck("/health", endpointName: "http")
    .WithUrlForEndpoint("https", url =>
    {
        url.Url = "/scalar/v1";
        url.DisplayText = "Scalar";
    });

var generator = builder.AddProject<Projects.VetClinic_Generator>("generator")
    .WithReference(kafka)
    .WaitFor(kafka);

if (builder.Configuration["Generation:IntervalSeconds"] is { } intervalSeconds)
{
    generator.WithEnvironment("Generation__IntervalSeconds", intervalSeconds);
}

if (builder.Configuration["Generation:BatchSize"] is { } batchSize)
{
    generator.WithEnvironment("Generation__BatchSize", batchSize);
}

builder.Build().Run();
