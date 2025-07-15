using ApiCadastro.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;

namespace ApiCadastro.Credit
{
    public class CreditService
    {

        private readonly IDistributedCache _Rcache;

        public CreditService(IDistributedCache Rcache)
        {

            _Rcache = Rcache;
        }
        public async Task<int> Verificador()
        {

            //credit system
            var QuantiaDeUso = await _Rcache.GetStringAsync("usos");
            int usos = 0;
            if (!string.IsNullOrEmpty(QuantiaDeUso))
            {
                QuantiaDeUso = QuantiaDeUso.Trim('"');
                int.TryParse(QuantiaDeUso, out usos);
            }
            if (usos >= 10)
            {
                var TempoDeInvalidacao = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
                };
                usos = 10;
                await _Rcache.SetStringAsync("usos", usos.ToString(), TempoDeInvalidacao);
            }
            return usos;
        }
        public async Task UploadCredito(int usos)
        {
            usos++;
            await _Rcache.SetStringAsync("usos", usos.ToString());
        }

    }
}

