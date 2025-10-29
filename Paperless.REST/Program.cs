using Microsoft.EntityFrameworkCore;
using Paperless.DAL;
using Paperless.Services.Mappings;
using Paperless.Services;

var builder = WebApplication.CreateBuilder(args);

// === SERVICES ===
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<DMSDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DMSDb")));

builder.Services.AddScoped<IDMSDbContext>(provider => provider.GetRequiredService<DMSDbContext>());
builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();
builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.AddAutoMapper(typeof(DocumentProfile).Assembly);

// === CORS ===
// Erlaubt das Vue-Frontend auf Port 5173
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:5173",
                "http://localhost:5174") 
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});


var app = builder.Build();

// === MIDDLEWARE ===

// 1. Routing zuerst aktivieren
app.UseRouting();

// 2. CORS direkt nach Routing (sonst kein Header)
app.UseCors("AllowFrontend");

// 3. Swagger nur im Dev-Mode
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 4. HTTPS-Redirect deaktivieren (lokal nicht nötig)
// app.UseHttpsRedirection();

// 5. Authorization (optional)
app.UseAuthorization();

// 6. Controller-Routen aktivieren
app.MapControllers();

// === Datenbank automatisch migrieren ===
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DMSDbContext>();
    db.Database.Migrate();
}

app.Run();
