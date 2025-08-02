using ApiCadastro.Data;
using ApiCadastro.TokenPaste;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiCadastro.Controllers
{
    public class Auth : ControllerBase
    {
        private readonly AppDbContext _context;
        public Auth(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("gerartoken")]
        public async Task<ActionResult> GerarToken([FromBody] PSN parametros)
        {
            var user = await _context.Cadastro.FirstOrDefaultAsync(u => u.nome == parametros.username);

            if (user == null || string.IsNullOrEmpty(user.senhas) ||
                !BCrypt.Net.BCrypt.Verify(parametros.password, user.senhas))//valor do password == chave para desautenticar
            {
                return Unauthorized("Usuário ou senha inválidos.");
            }

            var token = CreateToken.generateToken(user.nome ?? "");
            return Ok(new { token });
        }
        [HttpPost("logout")]
        public IActionResult Logout([FromBody] string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest("Token inválido");
            }

            // Adiciona o token à lista de revogação
            RevokedTokens.Revoke(token);

            return NoContent(); // 204 No Content
        }
    }

    public class PSN
    {
        public string? username { get; set; }
        public string? password { get; set; }
    }
    public static class RevokedTokens
    {
        private static readonly HashSet<string> revokedTokens = new HashSet<string>();

        public static void Revoke(string token)
        {
            revokedTokens.Add(token);
        }

        public static bool IsRevoked(string token)
        {
            return revokedTokens.Contains(token);
        }
    }
}

