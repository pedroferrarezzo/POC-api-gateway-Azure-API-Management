using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

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
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
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

app.MapGet("/clients", [Authorize]() =>
{
    return clientes;
});

app.MapGet("/products", [Authorize]() =>
{
    return produtos;
});

app.MapPost("/login", ([Microsoft.AspNetCore.Mvc.FromBody] LoginRequest request) =>
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

app.UseAuthentication();
app.UseAuthorization();
app.Run();

public record LoginRequest(string Username);