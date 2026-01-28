using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace API.Autentificacao
{
    public class JwtToken
    {
        private readonly IConfiguration _config;

        public JwtToken(IConfiguration config)
        {
            _config = config;
        }

        public string GenerateToken(int userId, string email, string nome)
        {
            // Carregar chave privada
            var rsa = RSA.Create();
            rsa.ImportFromPem(File.ReadAllText(_config["Jwt:PrivateKeyPath"]));

            var credentials = new SigningCredentials(
                new RsaSecurityKey(rsa),
                SecurityAlgorithms.RsaSha256
            );

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim("email", email),
                new Claim("nome", nome)
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(4),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}

