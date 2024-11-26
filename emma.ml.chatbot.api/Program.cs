using emma.ml.chatbot.api.Database;
using emma.ml.chatbot.api.Database.Seeders;
using emma.ml.chatbot.api.Database.Seeders.Features;
using emma.ml.chatbot.api.Database.Utilities;
using emma.ml.chatbot.api.Features.Chat;
using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddFastEndpoints().SwaggerDocument();

builder.Services.AddDbContext<ApplicationDbContext>(
    o => o.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);

builder.Services.AddSignalR();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", builder =>
    {
        builder.WithOrigins("http://localhost:4200", "http://127.0.0.1:5500") // Add allowed origins
               .AllowAnyHeader()
               .AllowAnyMethod()
               .AllowCredentials(); // Enable credentials support
    });
});

builder.Services.AddScoped<DatabaseSeeder>();
builder.Services.AddScoped<ISeeder, PBIKnowledgeBaseFirstTest>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();

    // Apply pending migrations
    // Check if the environment is Development
    if (builder.Environment.IsDevelopment())
    {
        // Clear the database
        await DataResetUtility.ClearDatabaseAsync(dbContext);
        //dbContext.Database.EnsureDeleted();
    }

    dbContext.Database.Migrate();

    // Seed the database
    await seeder.SeedAsync(dbContext);
}

app.UseCors("AllowSpecificOrigins");

app.UseDefaultExceptionHandler().UseFastEndpoints().UseSwaggerGen();

app.MapHub<ChatHub>("/chathub");

app.Run();
