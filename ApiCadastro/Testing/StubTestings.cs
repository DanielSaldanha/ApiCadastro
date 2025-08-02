//using ApiCadastro.Controllers;
//using ApiCadastro.Data;
//using ApiCadastro.Model;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using NUnit.Framework;
//using System;
//using System.Threading.Tasks;

//namespace ApiCadastro.Tests
//{
//    [TestFixture]
//    public class StubTestings
//    {
//        private StubAppDbContext _stubContext;
//        private ControllerToTestings _controller;

//        [SetUp]
//        public void SetUp()
//        {
//            _stubContext = new StubAppDbContext(new DbContextOptions<AppDbContext>());
//            _controller = new ControllerToTestings(_stubContext);
//        }

//        [Test]
//        public async Task Registrar_ReturnsBadRequest_QuandoEmailJaRegistrado()
//        {
//            // Arrange
//            var existingUser = new User
//            {
//                Id = 1,
//                nome = "John Doe",
//                email = "john@example.com"
//            };

//            _stubContext.AddUser(existingUser); // Adiciona um usuário ao stub

//            var dto = new DTO
//            {
//                nome = "Jane Doe",
//                email = "john@example.com", // E-mail já cadastrado
//                profissao = "Developer",
//                cargo = "Software Engineer",
//                password = "Password123!",
//                nascimento = new DateTime(1990, 1, 1)
//            };

//            // Act
//            var result = await _controller.Registrar(dto);

//            // Assert
//            var badRequestResult = result as BadRequestObjectResult;
//            Assert.IsNotNull(badRequestResult);
//            Assert.AreEqual(400, badRequestResult.StatusCode);
//            Assert.AreEqual("E-mail já cadastrado.", badRequestResult.Value);
//        }
//    }
//}
