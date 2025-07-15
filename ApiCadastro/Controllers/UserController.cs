using ApiCadastro.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using ApiCadastro.Model;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using ApiCadastro.Credit;
using System.Text.Json.Serialization;


namespace ApiCadastro.Controllers
{
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMemoryCache _Mcache;
        private readonly IDistributedCache _Rcache;
        private readonly CreditService _creditService;
        public UserController(CreditService creditService,IDistributedCache Rcache,IMemoryCache Mcache,AppDbContext context)
        {
            _context = context;
            _Mcache = Mcache;
            _Rcache = Rcache;
            _creditService = creditService;
        }

        private bool ValidarIdade(DateTime dataNascimento)
        {
            var idade = DateTime.Now.Year - dataNascimento.Year;
            if (dataNascimento.Date > DateTime.Now.AddYears(-idade)) idade--;
            return idade >= 18 && idade <= 65;
        }

        [HttpPost("registrar")]
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

            var cacheoptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
                SlidingExpiration = TimeSpan.FromMinutes(10)
            };
            var RediscacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(500)
            };
            await _Rcache.SetStringAsync($"User_{user.Id}", JsonSerializer.Serialize(user), RediscacheOptions);
            _Mcache.Set($"User_{user.Id}", user, cacheoptions);

            return Ok("Usuário registrado com sucesso.");
        }


        [HttpPost("registrarSemDTO")]
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

            var cacheoptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
                SlidingExpiration = TimeSpan.FromMinutes(10)
            };
            var RediscacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(500)
            };
            await _Rcache.SetStringAsync($"User_{user.Id}", JsonSerializer.Serialize(user), RediscacheOptions);
            _Mcache.Set($"User_{user.Id}", user, cacheoptions);

            return Ok("Usuário registrado com sucesso.");
        }


        [HttpGet("buscar")]
        public async Task<ActionResult> Getter(int id)
        {
            //credit system
            int usos = 0;
            usos = await _creditService.Verificador();
            if(usos >= 10)
            {
                return BadRequest("lack credit for 10 minutes");
            }

            //layer 1
            if (_Mcache.TryGetValue($"User_{id}", out User vget))
            {
                if(vget.ativo == false)
                {
                    return BadRequest("Usuario Inativo");
                }
                //down credit
                await _creditService.UploadCredito(usos);
                return Ok(vget);
            }

            //layer 2
            var cacheredis = await _Rcache.GetStringAsync($"User_{id}");
            if(cacheredis != null)
            {
                var CorrectCacheRedis = JsonSerializer.Deserialize<User>(cacheredis);
                
                if (CorrectCacheRedis.ativo == false)
                {
                    return BadRequest("usuario inativo");
                }
                //cache config
                var Mcacheoptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
                    SlidingExpiration = TimeSpan.FromMinutes(10)
                };
                //save and upload
                _Mcache.Set($"User_{id}", CorrectCacheRedis, Mcacheoptions);
                //down credit
                await _creditService.UploadCredito(usos);
                DTO dtoRedis = new DTO
                {
                    nome = CorrectCacheRedis.nome,
                    email = CorrectCacheRedis.email,
                    profissao = CorrectCacheRedis.profissao,
                    cargo = CorrectCacheRedis.cargo,
                    password = "não visivel"

                };
                return Ok(dtoRedis);
            }

            //layer 3
            vget = await _context.Cadastro.FindAsync(id); 

            //tratamento de erros
            if(vget == null)
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

            //cache config
            var cacheoptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
                SlidingExpiration = TimeSpan.FromMinutes(10)
            };
            var RediscacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(500)
            };
            //save and update
            await _Rcache.SetStringAsync($"User_{id}", JsonSerializer.Serialize(vget), RediscacheOptions);
            _Mcache.Set($"User_{id}", vget, cacheoptions);
            //down credit
            await _creditService.UploadCredito(usos);

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


        [HttpGet("buscar-filtro")]
        public async Task<ActionResult> BuscarPorFiltro(
        [FromQuery] string? nome,
        [FromQuery] string? email,
        [FromQuery] string? profissao,
        [FromQuery] string? cargo
        ){
            if (string.IsNullOrEmpty(nome) && string.IsNullOrEmpty(email)
            && string.IsNullOrEmpty(profissao) && string.IsNullOrEmpty(cargo))
            {
                return BadRequest("falta de informações");
            }
            //credit system
            int usos = 0;
            usos = await _creditService.Verificador();
            if (usos >= 10)
            {
                return BadRequest("lack credit for 10 minutes");
            }
            //layer 1
            if (_Mcache.TryGetValue($"valueBy({nome}|{email}|{profissao}|{cargo})", out List<User> resultado))
            {
                try
                {
                    //down credit
                    await _creditService.UploadCredito(usos);

                    var resMem = resultado.Select(u => new DTO
                    {
                        nome = u.nome,
                        email = u.email,
                        profissao = u.profissao,
                        cargo = u.cargo,
                        password = "não visivel"
                    }).ToList();

                    return Ok(resMem);
                }
                catch (JsonException)
                {
                    //tratamento para cache corrompido (remove e ingnora)
                    await _Rcache.RemoveAsync($"valueBy({nome}|{email}|{profissao}|{cargo})");
                }
                
            }
            //layer 2
            var Key = await _Rcache.GetStringAsync($"valueBy({nome}|{email}|{profissao}|{cargo})");
            if (Key != null)
            {
                try
                {

                    var cache = JsonSerializer.Deserialize<List<User>>(Key);
                    //cache config
                    var McacheoptionsInredis = new MemoryCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
                        SlidingExpiration = TimeSpan.FromMinutes(10)
                    };
                    //cache save and update
                    _Mcache.Set($"valueBy({nome}|{email}|{profissao}|{cargo})", cache, McacheoptionsInredis);
                    //down credit
                    await _creditService.UploadCredito(usos);

                    //selecionador de valores
                    var resRedis = cache.Select(u => new DTO
                    {
                        nome = u.nome,
                        email = u.email,
                        profissao = u.profissao,
                        cargo = u.cargo,
                        password = "não visivel"
                    }).ToList();


                    return Ok(resRedis);
                }
                catch (JsonException)
                {
                    //tratamento para cache corrompido (remove e ingnora)
                    await _Rcache.RemoveAsync($"valueBy({nome}|{email}|{profissao}|{cargo})");
                }
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
            //cache config
            var Mcacheoptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
                SlidingExpiration = TimeSpan.FromMinutes(10)
            };
            var RediscacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(500)
            };
            //cache save and update
            _Mcache.Set($"valueBy({nome}|{email}|{profissao}|{cargo})", resultado2, Mcacheoptions);
            await _Rcache.SetStringAsync($"valueBy({nome}|{email}|{profissao}|{cargo})", JsonSerializer.Serialize(resultado2), RediscacheOptions);
            //down credit
            await _creditService.UploadCredito(usos);
            //selecionador de valores
            var res = resultado2.Select(u => new DTO {
                nome = u.nome,
                email = u.email,
                profissao = u.profissao,
                cargo = u.cargo,
                password = "não visivel"
            }).ToList();
            
            return Ok(res);
            

        }


        [HttpPost("inserir")]
        public async Task<ActionResult> poster([FromBody] User vpost)
        {
            await _context.Cadastro.AddAsync(vpost);
            await _context.SaveChangesAsync();
            var cacheoptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
                SlidingExpiration = TimeSpan.FromMinutes(10)
            };
            var RediscacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(500)
            };
            await _Rcache.SetStringAsync($"User_{vpost.Id}", JsonSerializer.Serialize(vpost), RediscacheOptions);
            _Mcache.Set($"User_{vpost.Id}", vpost, cacheoptions);
            return CreatedAtAction(nameof(poster), new { id = vpost.Id }, vpost);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> delete(int id)
        {
            var dlt = await _context.Cadastro.FindAsync(id);
            if(dlt == null)
            {
                return BadRequest("operção falhada");
            }
            dlt.deleteAt = true;
            await _Rcache.RemoveAsync($"User_{id}");
            _Mcache.Remove($"User_{id}");
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}


