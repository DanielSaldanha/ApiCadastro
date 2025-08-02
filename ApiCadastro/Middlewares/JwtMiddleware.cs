using ApiCadastro.TokenPaste;

public class JwtMiddleware
{
    private readonly RequestDelegate _next;

    public JwtMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        //pegar token do cabeçalho
        var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split("").Last();

        if (token != null && !CreateToken.IsTokenValid(token))//seu método
        {
            context.Response.StatusCode = 401; // Unauthorized
            await context.Response.WriteAsync("Unauthorized access");
            return;
        }

        await _next(context);
    }
}