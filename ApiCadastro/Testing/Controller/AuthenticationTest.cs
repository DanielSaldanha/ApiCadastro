//using ApiCadastro.Controllers;
//using ApiCadastro.Data;
//using ApiCadastro.Model;
//using Microsoft.AspNetCore.Components.Forms;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using Moq;
//using NUnit.Framework;
//using NUnit.Framework.Internal.Commands;
//using System.Linq.Expressions;

//namespace ApiCadastro.Testing
//{
//    [TestFixture]
//    public class AuthenticationTest
//    {
//        private Auth _controller;
//        private Mock<DbSet<User>> _mockSet;
//        private Mock<AppDbContext> _mockContext;

//        [SetUp]
//        public void Setup()
//        {
//            var options = new DbContextOptionsBuilder<AppDbContext>()
//                .UseInMemoryDatabase(databaseName: "TesteDb") // Usando um banco de dados em memória
//                .Options;

//            _mockContext = new AppDbContext(options); // Passando o contexto em memória
//            _controller = new Auth(_mockContext);
//        }

//        [Test]
//        public async Task RetornaBadRequest_CasoUsuarioNulo()
//        {
//            // Arrange
//            var psn = new PSN
//            {
//                username = "emilho",
//                password = "Abner333"
//            };

//            // Não precisa mais de mock para o DbSet, já que o banco em memória está em uso
//            // Com a instância em memória, não há necessidade de configuração adicional

//            // Act
//            var result = await _controller.GerarToken(psn);

//            // Assert
//            var unauthorizedResult = result as UnauthorizedObjectResult;
//            Assert.IsNotNull(unauthorizedResult);
//            Assert.AreEqual("Usuário ou senha inválidos.", unauthorizedResult.Value);
//        }

//    }
//}
