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
using System.Linq.Expressions;

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
        //GET
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
        public async Task Getter_ReturnsBadRequest_QuandoNulo()
        {

            // Act
            var result = await _controller.Getter(30);

            // Assert
            var nullResult = result as NotFoundObjectResult;
            Assert.IsNotNull(nullResult);//estranhamente não funciona
            Assert.AreEqual(404, nullResult.StatusCode);
            Assert.AreEqual("usuario não encontrado", nullResult.Value); // Verifique a mensagem retornada
        }
        //POST

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
                nascimento = new DateTime(2020, 1, 1) // Data de nascimento inválida
            };

            // Act
            var result = await _controller.Registrar(dto);

            // Assert
            var BadResult = result as BadRequestObjectResult;
            Assert.IsNotNull(BadResult);
            Assert.AreEqual(400, BadResult.StatusCode);
            Assert.AreEqual("A idade deve estar entre 18 e 65 anos.", BadResult.Value);
        }
        [Test]
        public async Task Registrar_ReturnsBadRequest_QuandoMaiorDe65()
        {
            // Arrange
            var dto = new DTO
            {
                nome = "John Doe",
                email = "john@example.com",
                profissao = "Developer",
                cargo = "Software Engineer",
                password = "Password123!",
                nascimento = new DateTime(1900, 1, 1) // Data de nascimento inválida
            };

            // Act
            var result = await _controller.Registrar(dto);

            // Assert
            var BadResult = result as BadRequestObjectResult;
            Assert.IsNotNull(BadResult);
            Assert.AreEqual(400, BadResult.StatusCode);
            Assert.AreEqual("A idade deve estar entre 18 e 65 anos.", BadResult.Value);
        }
        [Test]
        public async Task Registrar_ReturnsBadRequest_QuandoMaiorDe65eMenorDe18()
        {
            //CT
            // Arrange
            var dtos = new List<DTO>
            {
              new DTO
              {
               nome = "John Doe",
               email = "john@example.com",
               profissao = "Developer",
               cargo = "Software Engineer",
               password = "Password123!",
               nascimento = new DateTime(2020, 1, 1) // Menor de 18
              },
              new DTO
              {
               nome = "Jane Doe",
               email = "jane@example.com",
               profissao = "Manager",
               cargo = "Product Manager",
               password = "Password123!",
               nascimento = new DateTime(1958, 1, 1) // Maior de 65
              }
            };

            foreach (var dto in dtos)
            {
                // Act
                var result = await _controller.Registrar(dto);

                // Assert
                var BadResult = result as BadRequestObjectResult;
                Assert.IsNotNull(BadResult);
                Assert.AreEqual(400, BadResult.StatusCode);
                Assert.AreEqual("A idade deve estar entre 18 e 65 anos.", BadResult.Value);
            }
        }

        [Test]
        public async Task Registrar_ReturnsResultadoEsperado()
        {
            //BVT + EPT
            // Arrange - Classes de equivalência:
            var validDto = new DTO
            {
                nome = "Valid User",
                email = "valid@example.com",
                profissao = "Developer",
                cargo = "Software Engineer",
                password = "Password123!",
                nascimento = new DateTime(2000, 1, 1) // 23 anos, que é válido
            };

            var invalidDtos = new List<DTO>
            {
               new DTO // Menor que 18 anos
               {
              nome = "Underage User",
              email = "underage@example.com",
              profissao = "Student",
              cargo = "N/A",
              password = "Password123!",
              nascimento = new DateTime(2010, 1, 1) // 13 anos, inválido
              },
              new DTO // Maior que 65 anos
              {
              nome = "Senior User",
              email = "senior@example.com",
              profissao = "Retired",
              cargo = "N/A",
              password = "Password123!",
              nascimento = new DateTime(1950, 1, 1) // 73 anos, inválido
              }
            };

            // Act - Executa o registro para o caso válido
            var validResult = await _controller.Registrar(validDto);

            // Assert - Verifica o caso válido (parte BVT)
            //Assert.IsInstanceOf<OkResult>(validResult);
            var OkRes = validResult as OkObjectResult;
            Assert.IsNotNull(OkRes);
            Assert.AreEqual(200, OkRes.StatusCode);
            Assert.AreEqual("Usuário registrado com sucesso.", OkRes.Value);

            // Act - Executa o registro para entradas inválidas
            foreach (var dto in invalidDtos)
            {
                var result = await _controller.Registrar(dto);

                // Assert - Verifica que todos os casos inválidos retornam Bad Request (parte EPT)
                var badResult = result as BadRequestObjectResult;
                Assert.IsNotNull(badResult);
                Assert.AreEqual(400, badResult.StatusCode);
                Assert.AreEqual("A idade deve estar entre 18 e 65 anos.", badResult.Value);
            }
        }

        [Test]
        public async Task Registrar_RetornaBadRequest_CasoEmailJaRegistrado()
        {
            int userId = 1;
            var user = new User
            {

                nome = "John Doe",
                email = "john@example.com",
                profissao = "Developer",
                cargo = "Software Engineer",
                senhas = "Pa2s@sw$rd123!", // Senha válida
                nascimento = new DateTime(1990, 1, 1) // Data de nascimento válida
            };
            _context.Cadastro.Add(user);
            await _context.SaveChangesAsync();//para funcionar deve ter TODOS os campo OBRIGATÒRIOS

            var dto = new DTO
            {
                nome = "John Doe",
                email = "john@example.com",
                profissao = "Developer",
                cargo = "Software Engineer",
                password = "Pa2s@sw$rd123!",
                nascimento = new DateTime(1990, 1, 1) // Data de nascimento inválida
            };
            //act
            var res = await _controller.Registrar(dto);

            // Assert
            var BadResult = res as BadRequestObjectResult;
            Assert.IsNotNull(BadResult);
            Assert.AreEqual(400, BadResult.StatusCode);
            Assert.AreEqual("E-mail já cadastrado.", BadResult.Value);


        }

        [Test]
        public async Task Registrar_ReturnsBadRequest_QuandoEmailJaRegistrado()
        {
            // Arrange
            var mockContext = new Mock<AppDbContext>();
            var mockSet = new Mock<DbSet<User>>();

            mockSet.Setup(m => m.AnyAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
                   .ReturnsAsync(true); // Simula que o e-mail já está cadastrado (as mockagens não suportam o IsAny)

            mockContext.Setup(c => c.Cadastro).Returns(mockSet.Object);

            var controller = new ControllerToTestings(mockContext.Object);

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
            var result = await controller.Registrar(dto);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);
            Assert.AreEqual("E-mail já cadastrado.", badRequestResult.Value);
        }
        //DELETE
        [Test]
        public async Task Deletar_RetornaBadRequest_CasoNaoEncontrado()
        {
            int userId = 1;
            var user = new User
            {
                Id = userId,
                nome = "John Doe",
                email = "john@example.com",
                profissao = "Developer",
                cargo = "Software Engineer",
                senhas = "Pa2s@sw$rd123!", // Senha válida
                nascimento = new DateTime(1990, 1, 1) // Data de nascimento válida
            };
            _context.Cadastro.Add(user);
            await _context.SaveChangesAsync();//para funcionar deve ter TODOS os campo OBRIGATÒRIOS

            //act
            var res = await _controller.delete(2);

            //assert
            var BadResult = res as BadRequestObjectResult;
            Assert.IsNotNull(BadResult);
            Assert.AreEqual(400, BadResult.StatusCode);
            Assert.AreEqual("operção falhada", BadResult.Value);
        }
        [Test]
        public async Task Deletar_RetornaBadRequest_CasoDeleteOAdmin()
        {
            //act
            var res = await _controller.delete(30);

            //assert
            var BadResult = res as BadRequestObjectResult;
            Assert.IsNotNull(BadResult);
            Assert.AreEqual(400, BadResult.StatusCode);
            Assert.AreEqual("você não pode deletar um admin", BadResult.Value);
        }
        [Test]
        public async Task Deletar_RetornaNoContent_CasoDeletado()
        {
            int userId = 1;
            var user = new User
            {
                Id = userId,
                nome = "John Doe",
                email = "john@example.com",
                profissao = "Developer",
                cargo = "Software Engineer",
                senhas = "Pa2s@sw$rd123!", // Senha válida
                nascimento = new DateTime(1990, 1, 1) // Data de nascimento válida
            };
            _context.Cadastro.Add(user);
            await _context.SaveChangesAsync();//para funcionar deve ter TODOS os campo OBRIGATÒRIOS

            //act
            var res = await _controller.delete(1);

            //assert
            var nocontent = res as NoContentResult;
            Assert.IsNotNull(nocontent);
            Assert.AreEqual(204, nocontent.StatusCode);
        }
    }


}