using Microsoft.OpenApi.Models;
using ODrive.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Using SQLite as a lightweight database for demo purposes
// but should be replaced with a more robust database in production
// e.g. Azure SQL.
builder.Services.AddDbContext<ODriveContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("ODriveContext")));

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "ODrive",
            Version = "v1",
            Description = "A REST API for the ODrive video storage service",
        });
    }
);

var app = builder.Build();

app.UsePathBase(new PathString("/v1"));

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();