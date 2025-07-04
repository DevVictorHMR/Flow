using Flow.Core.Entities;
using Flow.Shared.Dtos;
using Flow.Core.Interfaces;
using Flow.Infrastructure.Data;
using Flow.Server.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Flow.Shared.DTOs;
using Flow.Server.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Configuração do DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configuração do JWT
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!);

builder.Services.AddAuthentication(options => {
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options => {
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["ValidIssuer"],
        ValidAudience = jwtSettings["ValidAudience"],
        IssuerSigningKey = new SymmetricSecurityKey(secretKey)
    };
});

// Configuração do Swagger
builder.Services.AddSwaggerGen(c => {
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Flow API", Version = "v1" });

    // Configuração de segurança JWT no Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header usando o esquema Bearer",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement {
        {
            new OpenApiSecurityScheme {
                Reference = new OpenApiReference {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Configurações adicionais
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddRazorPages();

// Registro de serviços
builder.Services.AddScoped<IFeedbackService, FeedbackService>();
builder.Services.AddScoped<AuthService>();

// Configuração de CORS
builder.Services.AddCors(options => {
    options.AddPolicy("AllowAll", policy => {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddLogging(loggingBuilder =>
{
    loggingBuilder.AddConsole();
    loggingBuilder.AddDebug();
});

builder.Services.AddScoped<BadgeService>();
builder.Services.AddSignalR();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Badges.AddRange(
        new Badge { Name = "Colaborador", FlowbitsThreshold = 10, IconUrl = "/icons/colaborador.png" },
        new Badge { Name = "Top Contributor", FlowbitsThreshold = 50, IconUrl = "/icons/top-contributor.png" }
    );
    await db.SaveChangesAsync();

    // Pipeline de Middlewares
    app.UseBlazorFrameworkFiles();
    app.UseStaticFiles();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Flow API v1");
            c.OAuthClientId("swagger-ui");
            c.OAuthAppName("Swagger UI");
        });
        app.UseWebAssemblyDebugging();
    }
    else
    {
        app.UseExceptionHandler("/Error");
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseCors("AllowAll");
    app.UseRouting();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapRazorPages();
    app.MapControllers();
    app.MapFallbackToFile("index.html");

    app.UseExceptionHandler(appError =>
    {
        appError.Run(async context =>
        {
            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
            var exceptionHandler = context.Features.Get<IExceptionHandlerFeature>();
            var exception = exceptionHandler?.Error;

            logger.LogError(exception, "Erro não tratado");

            context.Response.StatusCode = exception switch
            {
                ArgumentException => StatusCodes.Status400BadRequest,
                UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
                _ => StatusCodes.Status500InternalServerError
            };

            context.Response.ContentType = "application/json";

            await context.Response.WriteAsync(new ErrorDetails
            {
                StatusCode = context.Response.StatusCode,
                Message = exception switch
                {
                    ArgumentException argEx => argEx.Message,
                    UnauthorizedAccessException => "Acesso não autorizado",
                    _ => "Ocorreu um erro interno. Tente novamente mais tarde."
                }
            }.ToString());
        });
    });

    app.MapHub<NotificationHub>("/notificationHub");
    app.Run();
}