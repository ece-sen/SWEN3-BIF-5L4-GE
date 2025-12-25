using Microsoft.EntityFrameworkCore;
using Minio;
using Minio.DataModel.Args;
using Paperless.DAL;
using Paperless.Services;
using Paperless.Services.Elasticsearch;
using Paperless.Services.Mappings;
using Paperless.Services.RabbitMq;
using Serilog;
using Serilog.AspNetCore;
using Elastic.Clients.Elasticsearch;
using Paperless.Services.Elasticsearch;


Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("Logs/paperless.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    Log.Information("Starting Paperless REST API...");

    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog();

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    builder.Services.AddDbContext<DMSDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DMSDb")));

    builder.Services.AddScoped<IDMSDbContext>(provider => provider.GetRequiredService<DMSDbContext>());
    builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();
    builder.Services.AddScoped<IDocumentService, DocumentService>();
    builder.Services.AddAutoMapper(typeof(DocumentProfile).Assembly);
    builder.Services.Configure<RabbitMqSettings>(
        builder.Configuration.GetSection("RabbitMQ"));

    builder.Services.AddSingleton<IRabbitMqProducer, RabbitMqProducer>();

    builder.Services.AddSingleton<IMinioClient>(sp =>
    {
        var cfg = builder.Configuration.GetSection("Minio");
        return new MinioClient()
            .WithEndpoint(cfg["Endpoint"])
            .WithCredentials(cfg["AccessKey"], cfg["SecretKey"])
            .WithSSL(Convert.ToBoolean(cfg["UseSSL"]))
            .Build();
    });

    // Elasticsearch Client
    builder.Services.AddSingleton(_ =>
    {
        var settings = new ElasticsearchClientSettings(
            new Uri(builder.Configuration["ELASTIC_URL"] ?? "http://elasticsearch:9200")
        ).DefaultIndex("documents");

        return new ElasticsearchClient(settings);
    });

    // Elasticsearch READ wrapper
    builder.Services.AddScoped<IElasticSearchClientWrapper, ElasticSearchClientWrapper>();

    // Search Service
    builder.Services.AddScoped<IElasticsearchSearchService, ElasticsearchSearchService>();



    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowFrontend", policy =>
        {
            policy
                .WithOrigins("http://localhost:8080", "http://localhost:5173", "http://localhost:5174")
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
    });

    var app = builder.Build();

    app.UseRouting();
    app.UseCors("AllowFrontend");

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseAuthorization();
    app.MapControllers();

    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<DMSDbContext>();
        db.Database.Migrate();
        Log.Information("Database migration applied successfully");
    }

    using (var scope = app.Services.CreateScope())
    {
        var minio = scope.ServiceProvider.GetRequiredService<IMinioClient>();
        var bucket = builder.Configuration["Minio:BucketName"];

        bool exists = await minio.BucketExistsAsync(new BucketExistsArgs().WithBucket(bucket));
        if (!exists)
        {
            await minio.MakeBucketAsync(new MakeBucketArgs().WithBucket(bucket));
        }
    }


    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application startup failed");
}
finally
{
    Log.Information("Shutting down logger...");
    Log.CloseAndFlush();
}
