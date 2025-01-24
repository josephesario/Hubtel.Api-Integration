using System.Text;
using System.Runtime.Serialization;
using dbContex.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using ViewModel.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add controllers with JSON options
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new InterfaceConverterFactory());
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });

// Add Swagger and customize schema generation
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Hubtel Wallet Api",
        Version = "v1",
        Description = "API for managing Hubtel Wallet"
    });

    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Enter JWT Bearer token",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Reference = new OpenApiReference
        {
            Id = JwtBearerDefaults.AuthenticationScheme,
            Type = ReferenceType.SecurityScheme
        }
    };

    options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, securityScheme);
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { securityScheme, Array.Empty<string>() }
    });

    options.CustomSchemaIds(type => type.FullName);

    // Add custom SchemaFilter
    options.SchemaFilter<ExcludeSchemaFilter>();
});

// Add CORS
var configuration = builder.Configuration;
var allowedOrigins = configuration.GetSection("AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
var jwtKey = configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not found in configuration");
var jwtIssuer = configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("JWT Issuer not found in configuration");

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowedOrigins", policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// Add database context
builder.Services.AddDbContext<HubtelWalletDbContextExtended>(options =>
    options.UseSqlServer(configuration.GetConnectionString("HubtelWalletDbContextExtended")));

// Add JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtIssuer,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

var app = builder.Build();

// Configure middleware pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();

    // Add Swagger UI
    app.UseSwagger(options =>
    {
        options.SerializeAsV2 = false;
    });

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Hubtel Wallet Api");
        options.RoutePrefix = "swagger";
        // Additional UI settings
        options.EnableDeepLinking();
        options.DisplayRequestDuration();
    });
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();

// Custom SchemaFilter to exclude schemas
public class ExcludeSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        // Exclude interfaces or specific namespaces
        if (context.Type.IsInterface || context.Type.FullName?.Contains("dbContex.Models") == false)
        {
            schema.Properties.Clear();
            schema.Description = "Excluded from documentation";
        }
    }
}
