using ApiCadastro.Controllers;
using ApiCadastro.Data;
using ApiCadastro.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using System;
using System.Threading.Tasks;
using ApiCadastro.Tests;

namespace ApiCadastro.Tests
{
    [TestFixture]
    public class ControllerToTestingsTests
    {
        private ControllerToTestings _controller;
        private AppDbContext _context;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Garante um banco único para cada teste
                .Options;

            _context = new AppDbContext(options);
            _controller = new ControllerToTestings(_context);
        }

        [Test]
        public async Task Getter_ReturnsOkResult_WhenUserExists()
        {
            // Arrange
            int userId = 1;
            var user = new User
            {
                Id = userId,
                nome = "John Doe",
                email = "john@example.com",
                ativo = true,
                senhas = "Password123!", // Senha válida
                nascimento = new DateTime(1990, 1, 1) // Data de nascimento válida
            };

            // Adicionando usuário ao contexto
            _context.Cadastro.Add(user);
            await _context.SaveChangesAsync();//para funcionar deve ter TODOS os campo OBRIGATÒRIOS

            // Act
            var result = await _controller.Getter(userId);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            var returnedUser = okResult.Value as DTO; // Assumindo que o response é do tipo DTO
            Assert.IsNotNull(returnedUser);
            Assert.AreEqual(user.nome, returnedUser.nome);
        }

        [Test]
        public async Task Getter_ReturnsNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            int userId = 999; // ID que não existe

            // Act
            var result = await _controller.Getter(userId);

            // Assert
            var notFoundResult = result as NotFoundObjectResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual(404, notFoundResult.StatusCode);
            Assert.AreEqual("usuario não encontrado", notFoundResult.Value); // Verifique a mensagem retornada
        }

        [Test]
        public async Task Getter_ReturnsBadRequest_WhenUserIsInactive()
        {
            // Arrange
            int userId = 1;
            var user = new User
            {

                Id = userId,
                nome = "John Doe",
                email = "john@example.com",
                ativo = false,
                senhas = "Password123!", // Senha válida
                nascimento = new DateTime(1990, 1, 1) // Data de nascimento válida
            };
            _context.Cadastro.Add(user);
            await _context.SaveChangesAsync();//para funcionar deve ter TODOS os campo OBRIGATÒRIOS

            // Act
            var result = await _controller.Getter(userId);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);
            Assert.AreEqual("Usuario Inativo", badRequestResult.Value); // Verifique a mensagem retornada
        }

        [Test]
        public async Task Registrar_ReturnsOkResult_WhenUserIsSuccessfullyRegistered()
        {
            // Arrange
            var dto = new DTO
            {
                nome = "John Doe",
                email = "john@example.com",
                profissao = "Developer",
                cargo = "Software Engineer",
                password = "Password123!",
                nascimento = new DateTime(1990, 1, 1) // Data de nascimento válida
            };

            // Act
            var result = await _controller.Registrar(dto);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.AreEqual("Usuário registrado com sucesso.", okResult.Value);
        }
        [Test]
        public async Task Registrar_ReturnsBadRequest_QuandoMenorDe18()
        {
            // Arrange
            var dto = new DTO
            {
                nome = "John Doe",
                email = "john@example.com",
                profissao = "Developer",
                cargo = "Software Engineer",
                password = "Password123!",
                nascimento = new DateTime(2020, 1, 1) // Data de nascimento válida
            };

            // Act
            var result = await _controller.Registrar(dto);

            // Assert
            var BadResult = result as BadRequestObjectResult;
            Assert.IsNotNull(BadResult);
            Assert.AreEqual(400, BadResult.StatusCode);
            Assert.AreEqual("A idade deve estar entre 18 e 65 anos.", BadResult.Value);
        }
    }
}