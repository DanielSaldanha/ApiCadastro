using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ApiCadastro.Controllers;// Auth.cs
namespace ApiCadastro.TokenPaste
{
    public class ValidatorToken
    {
        public static bool IsTokenValid(string token)
        {
            // Verifique se o token é válido e não está revogado
            if (RevokedTokens.IsRevoked(token))
            {
                return false; // O token foi revogado
            }

            // Lógica para validar o token (decodificando e validando o tempo de expiração)
            // ...

            return true; // O token é válido
        }
    }
}
