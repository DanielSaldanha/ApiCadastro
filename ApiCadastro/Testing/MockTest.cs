using ApiCadastro.Controllers;
using ApiCadastro.Data;
using ApiCadastro.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ApiCadastro.Tests
{
    [TestFixture]
    public class ControllerToTestingsT
    {
        private Mock<AppDbContext> _mockContext;
        private Mock<DbSet<User>> _mockSet;
        private ControllerToTestings _controller;

        [SetUp]
        public void SetUp()
        {
            // Cria uma lista de usuários simulada
            var users = new List<User>
            {
                new User { Id = 1, nome = "John Doe", email = "john@example.com" }
            }.AsQueryable();

            // Mock do DbSet<User>
            _mockSet = new Mock<DbSet<User>>();
            _mockSet.As<IQueryable<User>>().Setup(m => m.Provider).Returns(users.Provider);
            _mockSet.As<IQueryable<User>>().Setup(m => m.Expression).Returns(users.Expression);
            _mockSet.As<IQueryable<User>>().Setup(m => m.ElementType).Returns(users.ElementType);
            _mockSet.As<IQueryable<User>>().Setup(m => m.GetEnumerator()).Returns(users.GetEnumerator());

            //// Mock do AnyAsync
            //_mockSet.Sutup(m => m.AnyAsync(
            //    It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(),
            //    It.IsAny<CancellationToken>()))
            //    .ReturnsAsync((System.Linq.Expressions.Expression<Func<User, bool>> predicate, CancellationToken token) =>
            //        users.Any(predicate.Compile()));

            // Mock do contexto para retornar o DbSet mockado
            var options = new DbContextOptionsBuilder<AppDbContext>()
    .UseInMemoryDatabase(databaseName: "TesteDb")
    .Options;
            _mockContext = new Mock<AppDbContext>(options);
            _mockContext.Setup(c => c.Cadastro).Returns(_mockSet.Object);

            _controller = new ControllerToTestings(_mockContext.Object);
        }

        [Test]
        public async Task Registrar_ReturnsBadRequest_QuandoEmailJaRegistrado()
        {
            // Arrange
            var dto = new DTO
            {
                nome = "Jane Doe",
                email = "john@example.com", // E-mail já cadastrado
                profissao = "Developer",
                cargo = "Software Engineer",
                password = "Password123!",
                nascimento = new DateTime(1990, 1, 1)
            };

            // Act
            var result = await _controller.Registrar(dto);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);
            Assert.AreEqual("E-mail já cadastrado.", badRequestResult.Value);
        }
    }
}