using ApiCadastro.Controllers;
using ApiCadastro.Data;
using ApiCadastro.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace ApiCadastro.Tests
{
    [TestFixture]
    public class ControllerToTestingsTests2
    {
        private Mock<AppDbContext> _mockContext;
        private ControllerToTestings _controller;

        [SetUp]
        public void SetUp()
        {
            // Configure o mock do DbContext
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TesteDb")
                .Options;

            _mockContext = new Mock<AppDbContext>(options);
            _controller = new ControllerToTestings(_mockContext.Object);
        }

        [Test]
        public async Task Registrar_ReturnsBadRequest_QuandoMenorDe18_UsandoMocks()
        {
            // Arrange
            var dto = new DTO
            {
                nome = "John Doe",
                email = "john@example.com",
                profissao = "Developer",
                cargo = "Software Engineer",
                password = "Password123!",
                nascimento = new DateTime(2020, 1, 1) // Idade inválida
            };

            // Act
            var result = await _controller.Registrar(dto);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);
            Assert.AreEqual("A idade deve estar entre 18 e 65 anos.", badRequestResult.Value);
        }

        [Test]
        public async Task Registrar_ReturnsBadRequest_QuandoMaiorDe65_UsandoStubs()
        {
            // Arrange
            var stubService = new StubAppDbContext();
            _controller = new ControllerToTestings(stubService); // Instantiate Controller with Stub

            var dto = new DTO
            {
                nome = "Senior User",
                email = "senior@example.com",
                profissao = "Retired",
                cargo = "N/A",
                password = "Password123!",
                nascimento = new DateTime(1950, 1, 1) // Idade inválida
            };

            // Act
            var result = await _controller.Registrar(dto);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);
            Assert.AreEqual("A idade deve estar entre 18 e 65 anos.", badRequestResult.Value);
        }


    }

    // Stub da Classe AppDbContext
    //simula o ocmportamento do DbContext e evita de fazer chamadas reais ao banco de dados
    public class StubAppDbContext : AppDbContext
    {
        public StubAppDbContext() : base(new DbContextOptions<AppDbContext>())
        {
        }

        public override DbSet<User> Cadastro => Set<User>();
    }
    //Embora o stub tenha sido criado, neste teste específico,
    //a interação com o stub é apenas para validar regras de negócio
    //sem realmente utilizar as funcionalidades do banco de dados.

}
