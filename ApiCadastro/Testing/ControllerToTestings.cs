using ApiCadastro.Data;
using Microsoft.AspNetCore.Mvc;
using ApiCadastro.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;



namespace ApiCadastro.Controllers
{
    [Authorize]
    public class ControllerToTestings : ControllerBase
    {
        private readonly AppDbContext _context;

        public ControllerToTestings(AppDbContext context)
        {
            _context = context;

        }

        private bool ValidarIdade(DateTime dataNascimento)
        {
            var idade = DateTime.Now.Year - dataNascimento.Year;
            if (dataNascimento.Date > DateTime.Now.AddYears(-idade)) idade--;
            return idade >= 18 && idade <= 65;
        }

        [HttpPost("R")]
        [ApiExplorerSettings(IgnoreApi = true)] // Adiciona este cabeçalho para ocultar do Swagger
        public async Task<ActionResult> Registrar([FromBody] DTO dto)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!ValidarIdade(dto.nascimento))
            {
                return BadRequest("A idade deve estar entre 18 e 65 anos.");
            }

            if (await _context.Cadastro.AnyAsync(u => u.email == dto.email))
                return BadRequest("E-mail já cadastrado.");

            var user = new User
            {
                nome = dto.nome,
                email = dto.email,
                profissao = dto.profissao,
                cargo = dto.cargo,
                senhas = BCrypt.Net.BCrypt.HashPassword(dto.password)
            };
            await _context.Cadastro.AddAsync(user);
            await _context.SaveChangesAsync();

            return Ok("Usuário registrado com sucesso.");
        }


        [HttpPost("RDTO")]
        [ApiExplorerSettings(IgnoreApi = true)] // Adiciona este cabeçalho para ocultar do Swagger
        public async Task<ActionResult> RegistrarSemDTO([FromBody] User usr)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Verifica se já existe usuário com o mesmo email
            if (await _context.Cadastro.AnyAsync(u => u.email == usr.email))
                return BadRequest("E-mail já cadastrado.");

            var user = new User
            {
                senhas = BCrypt.Net.BCrypt.HashPassword(usr.senhas)
            };

            await _context.Cadastro.AddAsync(user);
            await _context.SaveChangesAsync();

            return Ok("Usuário registrado com sucesso.");
        }


        [HttpGet("B")]
        [ApiExplorerSettings(IgnoreApi = true)] // Adiciona este cabeçalho para ocultar do Swagger
        public async Task<ActionResult> Getter(int id)
        {
            //layer 3
            var vget = await _context.Cadastro.FindAsync(id);

            //tratamento de erros
            if (vget == null)
            {
                return NotFound("usuario não encontrado");
            }
            if (vget.deleteAt == true)
            {
                return BadRequest("usuario não encontrado");
            }
            if (vget.ativo == false)
            {
                return BadRequest("Usuario Inativo");
            }

            DTO dto = new DTO
            {
                nome = vget.nome,
                email = vget.email,
                profissao = vget.profissao,
                cargo = vget.cargo,
                password = "não visivel"

            };

            return Ok(dto);
        }


        [HttpGet("BF")]
        [ApiExplorerSettings(IgnoreApi = true)] // Adiciona este cabeçalho para ocultar do Swagger
        public async Task<ActionResult> BuscarPorFiltro(
        [FromQuery] string? nome,
        [FromQuery] string? email,
        [FromQuery] string? profissao,
        [FromQuery] string? cargo
        )
        {
            if (string.IsNullOrEmpty(nome) && string.IsNullOrEmpty(email)
            && string.IsNullOrEmpty(profissao) && string.IsNullOrEmpty(cargo))
            {
                return BadRequest("falta de informações");
            }
           
            //layer 3
            //apenas um Iflimpador de tela abaixo
            var query = _context.Cadastro.AsQueryable();
            if (!string.IsNullOrWhiteSpace(nome))
                query = query.Where(u => u.nome != null && u.nome.Contains(nome));

            if (!string.IsNullOrWhiteSpace(email))
                query = query.Where(u => u.email != null && u.email.Contains(email));

            if (!string.IsNullOrWhiteSpace(profissao))
                query = query.Where(u => u.profissao != null && u.profissao.Contains(profissao));

            if (!string.IsNullOrWhiteSpace(cargo))
                query = query.Where(u => u.cargo != null && u.cargo.Contains(cargo));

            var resultado2 = await query.ToListAsync();

            if (resultado2.Count == 0)
                return NotFound("Nenhum usuário encontrado com os filtros informados.");
          
            //selecionador de valores
            var res = resultado2.Select(u => new DTO
            {
                nome = u.nome,
                email = u.email,
                profissao = u.profissao,
                cargo = u.cargo,
                password = "não visivel"
            }).ToList();

            return Ok(res);
        }


       

        [HttpPut("MS")]//PRECISA DE MELHORIA. ERRO NO BCRYPT
        [ApiExplorerSettings(IgnoreApi = true)] // Adiciona este cabeçalho para ocultar do Swagger
        public async Task<IActionResult> mudarSenha(string senha, int id, [FromBody] User user)
        {
            if (id != user.Id)
            {
                return BadRequest("id não correspondente");
            }
            if (!BCrypt.Net.BCrypt.Verify(senha, user.senhas))
                return BadRequest("senha incorreta");

            User userCriptografado = new User { senhas = BCrypt.Net.BCrypt.HashPassword(senha) };
            _context.Entry(userCriptografado).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("DLT")]
        [ApiExplorerSettings(IgnoreApi = true)] // Adiciona este cabeçalho para ocultar do Swagger
        public async Task<IActionResult> delete(int id)
        {
            var dlt = await _context.Cadastro.FindAsync(id);
            if (dlt == null)
            {
                return BadRequest("operção falhada");
            }
            dlt.deleteAt = true;         
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}