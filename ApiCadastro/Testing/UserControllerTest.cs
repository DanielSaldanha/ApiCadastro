using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using ApiCadastro.Controllers;
using ApiCadastro.Data;
using ApiCadastro.Model;
using ApiCadastro.Credit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;

public class UserControllerTest
{
    private readonly UserController _controller;
    private readonly AppDbContext _context;

    public UserControllerTest()
    {
        // Configurar o InMemoryDatabase para o AppDbContext
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;

        // Criar uma instância do AppDbContext
        _context = new AppDbContext(options);

        // Mockar o IDistributedCache
        var cacheMock = new Mock<IDistributedCache>();
        cacheMock
            .Setup(c => c.SetStringAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Mockar o IMemoryCache
        var memoryCacheMock = new Mock<IMemoryCache>();
        memoryCacheMock
            .Setup(mc => mc.Set(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<MemoryCacheEntryOptions>()))
            .Returns((string key, object value, MemoryCacheEntryOptions options) => value);

        // Mockar o CreditService
        var creditServiceMock = new Mock<CreditService>(cacheMock.Object);

        // Instanciar o controlador
        _controller = new UserController(creditServiceMock.Object, cacheMock.Object, memoryCacheMock.Object, _context);
    }

    [Fact]
    public async Task Registrar_IdadeValida_RetornaOk()
    {
        var dto = new DTO
        {
            nome = "Teste",
            email = "teste@example.com",
            profissao = "Teste",
            cargo = "Teste",
            password = "Senha@123",
            nascimento = new DateTime(2000, 1, 1) // 23 anos
        };

        var result = await _controller.Registrar(dto);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task Registrar_IdadeInferiorA18_RetornaBadRequest()
    {
        var dto = new DTO
        {
            nome = "Teste",
            email = "teste@example.com",
            profissao = "Teste",
            cargo = "Teste",
            password = "Senha@123",
            nascimento = new DateTime(2006, 1, 1) // 17 anos
        };

        var result = await _controller.Registrar(dto);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("A idade deve estar entre 18 e 65 anos.", badRequestResult.Value);
    }

    [Fact]
    public async Task Registrar_IdadeSuperiorA65_RetornaBadRequest()
    {
        var dto = new DTO
        {
            nome = "Teste",
            email = "teste@example.com",
            profissao = "Teste",
            cargo = "Teste",
            password = "Senha@123",
            nascimento = new DateTime(1950, 1, 1) // 73 anos
        };

        var result = await _controller.Registrar(dto);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("A idade deve estar entre 18 e 65 anos.", badRequestResult.Value);
    }
}
