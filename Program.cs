using API.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using API.Autentificacao;

namespace API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Adiciona EF Core + Pomelo MySQL
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseMySql(
                    builder.Configuration.GetConnectionString("DefaultConnection"),
                    new MySqlServerVersion(new Version(8, 0, 32)) // Versão MySQL
                ));
            
            //JWT
            var privateKey = RSA.Create();
            privateKey.ImportFromPem(File.ReadAllText(builder.Configuration["Jwt:PrivateKeyPath"])); //Vai buscar a chave privada

            var publicKey = RSA.Create();
            publicKey.ImportFromPem(File.ReadAllText(builder.Configuration["Jwt:PublicKeyPath"])); //Vai buscar a chave pública

            builder.Services
                .AddAuthentication("JwtBearer") //Adiciona autentificação
                .AddJwtBearer("JwtBearer", options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters //Parametros do token
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new RsaSecurityKey(publicKey),

                        ValidateIssuer = true,
                        ValidIssuer = builder.Configuration["Jwt:Issuer"],

                        ValidateAudience = true,
                        ValidAudience = builder.Configuration["Jwt:Audience"],

                        ValidateLifetime = true,
                    };
                });

            builder.Services.AddScoped<JwtToken>();

            builder.Services.AddCors(options => //Cors para deixar a API fazer conexão
            {
                options.AddPolicy("AllowAll",
                    policy => policy
                        .AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod());
            });

            var app = builder.Build();

            app.UseCors("AllowAll");

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}