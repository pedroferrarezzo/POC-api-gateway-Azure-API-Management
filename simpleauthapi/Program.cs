using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Swashbuckle.AspNetCore.SwaggerGen;

var builder = WebApplication.CreateBuilder(args);

var serverPortBuilder = builder.Configuration["Server:Port"];
if (string.IsNullOrEmpty(serverPortBuilder))
    throw new InvalidOperationException("Server:Port não está configurado.");
var serverPort = int.Parse(serverPortBuilder);

var key = builder.Configuration["Jwt:Key"];
if (string.IsNullOrEmpty(key))
    throw new InvalidOperationException("Jwt:Key não está configurado.");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
    };
});

builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Insira o token JWT no formato: Bearer {seu token}"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(serverPort);
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

var clientes = new[]
{
    "João", "Pedro", "Marcelo", "Ana", "Maria", "José", "Lucas", "Fernanda", "Patrícia", "Roberto"
};

var produtos = new[]
{
    "Lápis", "Caneta", "Caderno", "Borracha", "Apontador", "Papel", "Pincel", "Marcador", "Tesoura", "Cola"
};

app.MapPost("/login", [AllowAnonymous] ([Microsoft.AspNetCore.Mvc.FromBody] LoginRequest request) =>
{
    if (clientes.Contains(request.Username))
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, request.Username)
        };
        var keyBytes = Encoding.UTF8.GetBytes(key);
        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: new SigningCredentials(
                new SymmetricSecurityKey(keyBytes),
                SecurityAlgorithms.HmacSha256)
        );
        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        return Results.Ok(new { token = tokenString });
    }

    return Results.Unauthorized();
});

app.MapGet("/clients", [Authorize]() =>
{
    return clientes;
});

app.MapGet("/products", [Authorize]() =>
{
    return produtos;
});

app.UseAuthentication();
app.UseAuthorization();
app.Run();

public record LoginRequest(string Username);