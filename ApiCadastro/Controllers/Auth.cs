using ApiCadastro.TokenPaste;
using Microsoft.AspNetCore.Mvc;

namespace ApiCadastro.Controllers
{
    public class Auth : ControllerBase
    {
        [HttpPost("gerartoken")]
        public ActionResult GerarToken([FromBody] PSN parametros)
        {
            Console.WriteLine($"recebendo {parametros.username} e {parametros.password}");
            if(parametros.username == "admin" && parametros.password == "123")
            {
                var token = CreateToken.generateToken(parametros.username);
                return Ok(new { token });
            }
            Console.WriteLine("Parametro invalidos");
            return Unauthorized("Usuário ou senha inválidos.");
        }
    }
    public class PSN
    {
        //o nome do parametro tem que ser igual ao do front end
        public string? username { get; set; }
        public string? password { get; set; }
    }
}
