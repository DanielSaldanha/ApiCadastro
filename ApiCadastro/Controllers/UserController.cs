using ApiCadastro.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using ApiCadastro.Model;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;

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
