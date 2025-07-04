using ApiCadastro.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using ApiCadastro.Model;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace ApiCadastro.Controllers
{
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMemoryCache _Mcache;
        private readonly IDistributedCache _Rcache;
        public UserController(IDistributedCache Rcache,IMemoryCache Mcache,AppDbContext context)
        {
            _context = context;
            _Mcache = Mcache;
            _Rcache = Rcache;
        }

        [HttpPost("registrar")]
        public async Task<ActionResult> Registrar([FromBody] UserRegisterDto dto)
        {
            // Verifica se já existe usuário com o mesmo email
            if (await _context.Cadastro.AnyAsync(u => u.email == dto.email))
                return BadRequest("E-mail já cadastrado.");

            var user = new User
            {
                Id = dto.Id,
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

        // DTO para registro
        public class UserRegisterDto
        {
            public int Id { get; set; }
            public string? nome { get; set; }
            public string? email { get; set; }
            public string? profissao { get; set; }
            public string? cargo { get; set; }
            public string? password { get; set; }
        }


        [HttpGet("buscar")]
        public async Task<ActionResult> Getter(int id)
        {
            //credit system
            var QuantiaDeUso = await _Rcache.GetStringAsync("usos");
            int usos = 0;
            if(!string.IsNullOrEmpty(QuantiaDeUso))
            {
                QuantiaDeUso = QuantiaDeUso.Trim('"');
                int.TryParse(QuantiaDeUso, out usos);
            }
            if(usos >= 10)
            {
                var TempoDeInvalidacao = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
                };
                usos = 10;
                await _Rcache.SetStringAsync("usos", usos.ToString(), TempoDeInvalidacao);
                return BadRequest("lack credt for 10 minutes");
            }

            //layer 1
            if (_Mcache.TryGetValue($"User_{id}", out User vget))
            {
                usos++;
                await _Rcache.SetStringAsync("usos", usos.ToString());
                return Ok(vget);
            }

            //layer 2
            var cacheredis = await _Rcache.GetStringAsync($"User_{id}");
            if(cacheredis != null)
            {
                var CorrectCacheRedis = JsonSerializer.Deserialize<User>(cacheredis);
                var Mcacheoptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
                    SlidingExpiration = TimeSpan.FromMinutes(10)
                };
                _Mcache.Set($"User_{id}", CorrectCacheRedis, Mcacheoptions);

                usos++;
                await _Rcache.SetStringAsync("usos", usos.ToString());
                return Ok(CorrectCacheRedis);
            }

            //layer 3
            vget = await _context.Cadastro.FindAsync(id);
            if(vget == null)
            {
                return NotFound("usuario não encontrado");
            }
            var cacheoptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
                SlidingExpiration = TimeSpan.FromMinutes(10)
            };
            var RediscacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(500)
            };
            await _Rcache.SetStringAsync($"User_{id}", JsonSerializer.Serialize(vget), RediscacheOptions);
            _Mcache.Set($"User_{id}", vget, cacheoptions);

            usos++;
            await _Rcache.SetStringAsync("usos", usos.ToString());
            return Ok(vget);
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
            //limit system
            var QuantiaDeUso = await _Rcache.GetStringAsync("usos");
            int usos = 0;
            if(!string.IsNullOrEmpty(QuantiaDeUso))
            {
                QuantiaDeUso = QuantiaDeUso.Trim('"');
                int.TryParse(QuantiaDeUso, out usos);
            }
            if (usos >= 10)
            {
                var RediscacheOptionsCredit = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
                };
                usos = 10;
                await _Rcache.SetStringAsync("usos", usos.ToString(), RediscacheOptionsCredit);
                return BadRequest("lack credit for 10 minutes");
            }
            //layer 1
            if (_Mcache.TryGetValue($"valueBy({nome}|{email}|{profissao}|{cargo})", out List<User> resultado))
            {
                try
                {
                    usos++;
                    await _Rcache.SetStringAsync("usos", usos.ToString());
                    return Ok(resultado);
                }
                catch (JsonException)
                {
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
                    var McacheoptionsInredis = new MemoryCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
                        SlidingExpiration = TimeSpan.FromMinutes(10)
                    };
                    _Mcache.Set($"valueBy({nome}|{email}|{profissao}|{cargo})", cache, McacheoptionsInredis);
                    usos++;
                    await _Rcache.SetStringAsync("usos", usos.ToString());
                    return Ok(cache);
                }
                catch (JsonException)
                {
                    // Cache corrompido, ignora e segue para buscar no banco
                    // Removendo o cache corrompido
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

            var Mcacheoptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
                SlidingExpiration = TimeSpan.FromMinutes(10)
            };
            var RediscacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(500)
            };
            _Mcache.Set($"valueBy({nome}|{email}|{profissao}|{cargo})", resultado2, Mcacheoptions);
            await _Rcache.SetStringAsync($"valueBy({nome}|{email}|{profissao}|{cargo})", JsonSerializer.Serialize(resultado2), RediscacheOptions);
            usos++;
            await _Rcache.SetStringAsync("usos", usos.ToString());
            return Ok(resultado2);
            

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

        //MÉTODOS PUT E DELETE NÃO SE ENCAIXAM NA PARTE DE CADASTRO
    }
}
