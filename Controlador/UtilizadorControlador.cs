using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API.Data;
using API.Modelo;
using API.Autentificacao;
using BCrypt;
using DnsClient;
using API.DTO;
using Microsoft.AspNetCore.Authorization;

namespace API.Controllers
{
    
    [Route("api/[controller]")]
    [ApiController]
    public class UtilizadoresController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly JwtToken _tokenService;

        public UtilizadoresController(AppDbContext context, JwtToken tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }



        //Validar se o domínio do email existe
        private async Task<bool> DominioTemMXAsync(string email)
        {
            try
            {
                var dominio = email.Split('@').Last();

                var lookup = new LookupClient();
                var resultado = await lookup.QueryAsync(dominio, QueryType.MX);

                return resultado.Answers.MxRecords().Any();
            }
            catch
            {
                return false;
            }
        }



        // POST login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO login)
        {
            if (login == null)
                return BadRequest("Dados inválidos.");

            var user = await _context.Utilizadores
                .FirstOrDefaultAsync(u => u.Email == login.Email);

            if (user == null)
                return Unauthorized("Email ou password incorretos.");

            // Validar password com BCrypt
            if (!BCrypt.Net.BCrypt.Verify(login.Password, user.Password_Encriptado))
                return Unauthorized("Email ou password incorretos.");

            // Gerar token
            var token = _tokenService.GenerateToken(
                user.ID_Utilizador,
                user.Email,
                user.Nome
            );

            return Ok(new
            {
                token = token,
                utilizador = new
                {
                    user.ID_Utilizador,
                    user.Nome,
                    user.Email
                }
            });
        }


        // POST registo
        [HttpPost("registo")]
        public async Task<IActionResult> Registo([FromBody] RegistoDTO novoUser)
        {
            if (novoUser == null)
                return BadRequest("Dados inválidos.");

            // Verificar se já existe na base de dados
            var existe = await _context.Utilizadores
                .AnyAsync(u => u.Email == novoUser.Email);

            if (existe)
                return BadRequest("Email já registado.");

            // Verificar se o domínio tem MX
            if (!await DominioTemMXAsync(novoUser.Email))
                return BadRequest("O domínio do email não permite receber emails.");

            // Criar utilizador
            var user = new Utilizador
            {
                Nome = novoUser.Nome,
                Email = novoUser.Email,
                Password_Encriptado = BCrypt.Net.BCrypt.HashPassword(novoUser.Password)
            };

            _context.Utilizadores.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { mensagem = "Utilizador registado com sucesso!" });
        }

        //Apagar utilizador
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUtilizador(int id)
        {
            var utilizador = await _context.Utilizadores.FindAsync(id);
            if (utilizador == null)
                return NotFound();

            _context.Utilizadores.Remove(utilizador);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
