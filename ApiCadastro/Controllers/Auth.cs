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
            var hash = BCrypt.Net.BCrypt.HashPassword("minhaSenhaSegura");
            Console.WriteLine(hash);
            var user = await _context.Cadastro.FirstOrDefaultAsync(u => u.nome == parametros.username);

            if (user == null || string.IsNullOrEmpty(user.senhas) ||
                !BCrypt.Net.BCrypt.Verify(parametros.password, user.senhas))
            {
                return Unauthorized("Usuário ou senha inválidos.");
            }

            var token = CreateToken.generateToken(user.nome ?? "");
            return Ok(new { token });
        }
    }

    public class PSN
    {
        public string? username { get; set; }
        public string? password { get; set; }
    }
}
