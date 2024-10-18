using Bsp01.Data;
using Bsp01.Interfaces;
using Bsp01.Services;
using Microsoft.EntityFrameworkCore;

using Microsoft.AspNetCore.Authentication.JwtBearer; // EINTRAGEN
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;


namespace Bsp01
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            // --- Connection String für SQLite konfigurieren ---
            builder.Services.AddDbContext<MeineDbContext>(options =>
                options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
            // using Bsp01.Data; // EINTRAGEN
            // using Microsoft.EntityFrameworkCore; // EINTRAGEN


            // ---- BenutzerService registerieren ----
            builder.Services.AddScoped<IBenutzerService, BenutzerService>();
            // using Bsp01.Interfaces; // EINTRAGEN


            // ---- Konfiguration aus appsettings.json laden ---- 
            builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);



            // ---- Konfiguration für JWT-Token ---- 
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = builder.Configuration["TOKEN_BASE_URL"],
                        ValidAudience = builder.Configuration["TOKEN_BASE_URL"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["TOKEN_KEY"])),
                        RoleClaimType = "rolle"  // Hier sicherstellen, dass Schlüsselwort "rolle" verwendet wird
                        // der Standard-Claim-Typ für Rollen üblicherweise ist "role" oder "roles"
                    };
                });
            // NuGet Paket Installieren
            // Install-Package Microsoft.AspNetCore.Authentication.JwtBearer -Version 6.0.35
            // using Microsoft.AspNetCore.Authentication.JwtBearer; // EINTRAGEN


            builder.Services.AddControllers();

            // ----- Swagger konfigurieren -----
            builder.Services.AddEndpointsApiExplorer();
            //builder.Services.AddSwaggerGen(); // AUSKOMMENTRIERT!
            // EINTRAGEN - Authorize BUTTON in SwaggerUI, JWT Authentifizierung 
            builder.Services.AddSwaggerGen(c =>
            {

                c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Bitte geben Sie den JWT mit Bearer ein (Bearer <Token>)",
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
                {
                    {
                        new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                        {
                            Reference = new Microsoft.OpenApi.Models.OpenApiReference
                            {
                                Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                            Scheme = "Bearer",
                            Name = "Bearer",
                            In = ParameterLocation.Header,

                        },
                        new string[] {}
                    }
                });

            });

            /*
            // DAS WIRD SPÄTER BEHANDELT !!!
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder => builder
                        .AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());
            });
            */

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // ----- Login konfigurieren -----
            app.UseAuthentication(); // Authentifizierung verwenden
            app.UseAuthorization(); // Autorisierung verwenden

            //app.UseCors("CorsPolicy");


            app.MapControllers();

            app.Run();
        }
    }
}
