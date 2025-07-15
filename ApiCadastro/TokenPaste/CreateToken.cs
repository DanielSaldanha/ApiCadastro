using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ApiCadastro.TokenPaste
{
    public class CreateToken
    {
        private const string secretkey = "m1nh@_Ultr@_ch@v3_s3cr3t@_&_F0rt3";
        public static string generateToken(string username)
        {
            var tokenhandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(secretkey);

            var tokendescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name,username)
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenhandler.CreateToken(tokendescriptor);
            return tokenhandler.WriteToken(token);
        }
    }
}


