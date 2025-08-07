using ApiCadastro.Data;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using ApiCadastro.Model;
using ApiCadastro.Controllers;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ApiCadastro.Testing.Controller
{
    [TestFixture]
    public class TestControllerToTestings
    {
        private ControllerToTestings _controller;
        private Mock<DbSet<User>> _mockSet;
        private Mock<AppDbContext> _mockContext;

        [SetUp]
        public void Setup()
        {
            _mockSet = new Mock<DbSet<User>>();
            _mockContext = new Mock<AppDbContext>(new DbContextOptions<AppDbContext>());
            _controller = new ControllerToTestings(_mockContext.Object);
        }

        private void SetupMockData(List<User> data)
        {
            var queryableData = data.AsQueryable();
            _mockSet.As<IQueryable<User>>().Setup(m => m.Provider).Returns(queryableData.Provider);
            _mockSet.As<IQueryable<User>>().Setup(m => m.Expression).Returns(queryableData.Expression);
            _mockSet.As<IQueryable<User>>().Setup(m => m.ElementType).Returns(queryableData.ElementType);
            _mockSet.As<IQueryable<User>>().Setup(m => m.GetEnumerator()).Returns(queryableData.GetEnumerator());

            _mockContext.Setup(c => c.Cadastro).Returns(_mockSet.Object);
            var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TesteDb")
            .Options;

            _mockContext = new Mock<AppDbContext>(options);
            
        }

        [Test]
        public async Task Delete_MalSucedido()
        {
            // Arrange
            var cardapioItems = new List<User>
            {
                new User {
                Id =1,
                nome = "Jane Doe",
                email = "john@example.com", // E-mail já cadastrado
                profissao = "Developer",
                cargo = "Software Engineer",
                senhas = "Password123!",
                nascimento = new DateTime(1990, 1, 1)
                }
            };
            SetupMockData(cardapioItems);

            //mockagem do FindAsync utilizado para a obtenção do id do dado desejado
            _mockSet.Setup(m => m.FindAsync(It.IsAny<object[]>())).ReturnsAsync((object[] ids) =>
            {
                return cardapioItems.FirstOrDefault(i => i.Id == (int)ids[0]);
            });

            // Act
            var result = await _controller.delete(2);

            // Assert
            var BadRes = result as BadRequestObjectResult;
            Assert.IsNotNull(BadRes);
            Assert.AreEqual(400, BadRes.StatusCode);
            Assert.AreEqual("operção falhada", BadRes.Value);
        }

        [Test]
        public async Task Get_BemSucediso()
        {
            // Arrange
            var cardapioItems = new List<User>
            {
                new User {
                Id =1,
                nome = "Jane Doe",
                email = "john@example.com", // E-mail já cadastrado
                profissao = "Developer",
                cargo = "Software Engineer",
                senhas = "Password123!",
                nascimento = new DateTime(1990, 1, 1)
                }
            };
            SetupMockData(cardapioItems);

            //mockagem do FindAsync utilizado para a obtenção do id do dado desejado
            _mockSet.Setup(m => m.FindAsync(It.IsAny<object[]>())).ReturnsAsync((object[] ids) =>
            {
                return cardapioItems.FirstOrDefault(i => i.Id == (int)ids[0]);
            });

            // Act
            var result = await _controller.Getter(1);

            // Assert
            var OkRes = result as OkObjectResult;
            Assert.IsNotNull(OkRes);
            Assert.AreEqual(200, OkRes.StatusCode);
           // Assert.AreEqual(cardapioItems, OkRes.Value);
        }
        [Test]
        public async Task Get_MalSucedisoNull()
        {
            // Arrange
            var cardapioItems = new List<User>
            {
                new User {
                Id =1,
                nome = "Jane Doe",
                email = "john@example.com", // E-mail já cadastrado
                profissao = "Developer",
                cargo = "Software Engineer",
                senhas = "Password123!",
                nascimento = new DateTime(1990, 1, 1)
                }
            };
            SetupMockData(cardapioItems);

            //mockagem do FindAsync utilizado para a obtenção do id do dado desejado
            _mockSet.Setup(m => m.FindAsync(It.IsAny<object[]>())).ReturnsAsync((object[] ids) =>
            {
                return cardapioItems.FirstOrDefault(i => i.Id == (int)ids[0]);
            });

            // Act
            var result = await _controller.Getter(2);

            // Assert
            var BadRes = result as NotFoundObjectResult;
            Assert.IsNotNull(BadRes);
            Assert.AreEqual(404, BadRes.StatusCode);
            Assert.AreEqual("usuario não encontrado", BadRes.Value);

        }
        [Test]
        public async Task Get_MalSucedido_Inativo()
        {
            // Arrange
            var cardapioItems = new List<User>
            {
                new User {
                Id =1,
                nome = "Jane Doe",
                email = "john@example.com", // E-mail já cadastrado
                profissao = "Developer",
                cargo = "Software Engineer",
                senhas = "Password123!",
                ativo = false,
                nascimento = new DateTime(1990, 1, 1)
                }
            };
            SetupMockData(cardapioItems);

            //mockagem do FindAsync utilizado para a obtenção do id do dado desejado
            _mockSet.Setup(m => m.FindAsync(It.IsAny<object[]>())).ReturnsAsync((object[] ids) =>
            {
                return cardapioItems.FirstOrDefault(i => i.Id == (int)ids[0]);
            });

            // Act
            var result = await _controller.Getter(1);

            // Assert
            var BadRes = result as BadRequestObjectResult;
            Assert.IsNotNull(BadRes);
            Assert.AreEqual(400, BadRes.StatusCode);
            Assert.AreEqual("Usuario Inativo", BadRes.Value);
        }


        [Test]
        public async Task RegistrarSemAnyAsync_ReturnsOk_QuandoCadastroBemSucedido()
        {
            //caso tivesse simularia que o email nao estaria cadastrado (false)

            // Arrange
            var mockSet = new Mock<DbSet<User>>();

            // Simula AddAsync (não precisa fazer nada, só garantir que não lança exceção)
            mockSet.Setup(m => m.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
                   .ReturnsAsync((User user, CancellationToken token) =>
                        new Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<User>(null));

            var mockContext = new Mock<AppDbContext>(new DbContextOptions<AppDbContext>());
            mockContext.Setup(c => c.Cadastro).Returns(mockSet.Object);

            // Simula SaveChangesAsync
            mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
                       .ReturnsAsync(1);

            var controller = new ControllerToTestings(mockContext.Object);

            var dto = new DTO
            {
                nome = "Jane Doe",
                email = "jane@example.com", // E-mail novo
                profissao = "Developer",
                cargo = "Software Engineer",
                password = "Password123!",
                nascimento = new DateTime(1990, 1, 1)
            };

            // Act
            var result = await controller.RegistrarSemAnyAsync(dto);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            // Se quiser, pode validar a mensagem de sucesso, se houver
        }

        [Test]
        public async Task RegistrarSemAnyAsync_BadRequest_QuandoMenorDeIdade()
        {
            //caso tivesse simularia que o email nao estaria cadastrado (false)

            // Arrange
            var mockSet = new Mock<DbSet<User>>();

            // Simula AddAsync (não precisa fazer nada, só garantir que não lança exceção)
            mockSet.Setup(m => m.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
                   .ReturnsAsync((User user, CancellationToken token) =>
                        new Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<User>(null));

            var mockContext = new Mock<AppDbContext>(new DbContextOptions<AppDbContext>());
            mockContext.Setup(c => c.Cadastro).Returns(mockSet.Object);

            // Simula SaveChangesAsync
            mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
                       .ReturnsAsync(1);

            var controller = new ControllerToTestings(mockContext.Object);

            var dto = new List<DTO>
            {
            new DTO
            {
                nome = "Jane Doe",
                email = "jane@example.com", // E-mail novo
                profissao = "Developer",
                cargo = "Software Engineer",
                password = "Password123!",
                nascimento = new DateTime(2019, 1, 1)// -18
            },
            new DTO
            {
                nome = "Jane Doe",
                email = "jane@example.com", // E-mail novo
                profissao = "Developer",
                cargo = "Software Engineer",
                password = "Password123!",
                nascimento = new DateTime(1900, 1, 1)// +65
            },
            };
            foreach(var dtosInvalidos in dto)
            {
                // Act
                var result = await controller.RegistrarSemAnyAsync(dtosInvalidos);

                // Assert
                var BadResult = result as BadRequestObjectResult;
                Assert.IsNotNull(BadResult);
                Assert.AreEqual(400, BadResult.StatusCode);
                Assert.AreEqual("A idade deve estar entre 18 e 65 anos.", BadResult.Value);
            }
            
        }

    }
}

//    [Test]// fracasso
//    public async Task Registrar_ReturnsBadRequest_QuandoEmailJaRegistrado()
//    {
//        // Arrange
//        var options = new DbContextOptionsBuilder<AppDbContext>()
//.UseInMemoryDatabase(databaseName: "TesteDb")
//.Options;
//        var mockContext = new Mock<AppDbContext>(options);
//        var mockSet = new Mock<DbSet<User>>();

//        //mockSet.Setup(m => m.AnyAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
//        //       .ReturnsAsync(true); // Simula que o e-mail já está cadastrado
//        // ou seja não necessita de Add ou SaveChanges
//        // para simulações

//        //_mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
//        //.ReturnsAsync(1); // 1 = número de registros afetados


//        mockContext.Setup(c => c.Cadastro).Returns(mockSet.Object);

//        var controller = new ControllerToTestings(mockContext.Object);

//        var dto = new DTO
//        {
//            nome = "Jane Doe",
//            email = "john@example.com", // E-mail já cadastrado
//            profissao = "Developer",
//            cargo = "Software Engineer",
//            password = "Password123!",
//            nascimento = new DateTime(1990, 1, 1)
//        };

//        // Act
//        var result = await controller.Registrar(dto);

//        // Assert
//        var badRequestResult = result as BadRequestObjectResult;
//        Assert.IsNotNull(badRequestResult);
//        Assert.AreEqual(400, badRequestResult.StatusCode);
//        Assert.AreEqual("E-mail já cadastrado.", badRequestResult.Value);
//    }

