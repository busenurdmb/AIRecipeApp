
using AIRecipeApp.Api.Context;
using AIRecipeApp.Api.Interfaces;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 📌 MongoDB ve Servis Bağlantıları
builder.Services.Configure<MongoDbContext>(builder.Configuration);
builder.Services.AddSingleton<MongoDbContext>();
builder.Services.AddScoped<IRecipeService, RecipeService>();
builder.Services.AddScoped<IOpenAiService, OpenAiService>();


// 📌 JWT Authentication Ekle
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Issuer"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

builder.Services.AddAuthorization(); // 📌 Authorization Middleware

// 📌 OpenAPI (Swagger) desteğini ekle ve JWT desteğini dahil et
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "AI Recipe API",
        Version = "v1",
        Description = "Malzemelerden tarif oluşturabilen yapay zeka destekli API"
    });

    // 📌 Swagger UI'ye JWT Token Desteği Ekleyelim
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "JWT Token girin. Örnek: Bearer {token}",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] { }
        }
    });
});

// 📌 Controller Desteği
builder.Services.AddControllers();

var app = builder.Build();

// 📌 Swagger'ı Etkinleştir
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "AI Recipe API v1");
        c.RoutePrefix = "swagger"; // "/swagger" yolunu kullan
    });
}

// 📌 Middleware Sırası ÖNEMLİ
app.UseHttpsRedirection();

app.UseAuthentication(); // 📌 JWT Authentication Middleware
app.UseAuthorization();  // 📌 Authorization Middleware

app.MapControllers();

app.Run();
