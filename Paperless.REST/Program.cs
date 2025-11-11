using Microsoft.EntityFrameworkCore;
using Paperless.DAL;
using Paperless.Services.Mappings;
using Paperless.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<DMSDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DMSDb")));

builder.Services.AddScoped<IDMSDbContext>(provider => provider.GetRequiredService<DMSDbContext>());
builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();
builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.AddAutoMapper(typeof(DocumentProfile).Assembly);


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:8080", "http://localhost:5173", "http://localhost:5174") 
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
}

app.Run();
