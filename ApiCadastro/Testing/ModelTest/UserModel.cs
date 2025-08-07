using ApiCadastro.Model;
using NUnit.Framework;
using System.ComponentModel.DataAnnotations;

namespace ApiCadastro.Testing.ModelTest
{
    [TestFixture]
    public class UserModel
    {
        [Test]
        public void User_Name_ShouldBeRequired()
        {
            // Arrange
            var user = new User
            {
                nome = null, // Nome vazio para simular um erro de validação
                email = "test@example.com"
            };

            // Act
            var context = new ValidationContext(user, null, null);
            var results = new List<ValidationResult>();

            bool isValid = Validator.TryValidateObject(user, context, results, true);

            // Assert
            Assert.IsFalse(isValid);
            Assert.IsTrue(results.Count > 0);
            Assert.AreEqual("o nome é obrigatório.", results[0].ErrorMessage);
        }
        [Test]
        public void User_Email_ShouldBeRequired()
        {
            // Arrange
            var user = new User
            {
                nome = "John", // Nome vazio para simular um erro de validação
                email = null,
                senhas = "63g5$ES$5",
                nascimento = new DateTime(1990, 1, 1)
            };

            // Act
            var context = new ValidationContext(user, null, null);
            var results = new List<ValidationResult>();

            bool isValid = Validator.TryValidateObject(user, context, results, true);

            // Assert
            Assert.IsFalse(isValid);
            Assert.IsTrue(results.Count > 0);
            Assert.AreEqual("o email é obrigatório.", results[0].ErrorMessage);
        }

        [Test]
        public void User_Email_Invalid()
        {
            // Arrange
            var user = new User
            {
                nome = "John", // Nome vazio para simular um erro de validação
                email = "teste#xample,com",
                senhas = "63g5$ES$5",
                nascimento = new DateTime(1990, 1, 1)
            };

            // Act
            var context = new ValidationContext(user, null, null);
            var results = new List<ValidationResult>();

            bool isValid = Validator.TryValidateObject(user, context, results, true);

            // Assert
            Assert.IsFalse(isValid);
            Assert.IsTrue(results.Count > 0);
            Assert.AreEqual("Formato de e-mail inválido.", results[0].ErrorMessage);
        }
        [Test]
        public void User_Senha_ShouldBeRequired()
        {
            // Arrange
            var user = new User
            {
                nome = "John", // Nome vazio para simular um erro de validação
                email = "test@example.com",
                senhas = null,
                nascimento = new DateTime(1990, 1, 1)
            };

            // Act
            var context = new ValidationContext(user, null, null);
            var results = new List<ValidationResult>();

            bool isValid = Validator.TryValidateObject(user, context, results, true);

            // Assert
            Assert.IsFalse(isValid);
            Assert.IsTrue(results.Count > 0);
            Assert.AreEqual("A senha é obrigatória.", results[0].ErrorMessage);
        }

        [Test]
        public void User_Senha_Invalid_WithNumbers()
        {
            // Arrange
            var user = new User
            {
                nome = "John", // Nome vazio para simular um erro de validação
                email = "test@example.com",
                senhas = "123",
                nascimento = new DateTime(1990, 1, 1)
            };

            // Act
            var context = new ValidationContext(user, null, null);
            var results = new List<ValidationResult>();

            bool isValid = Validator.TryValidateObject(user, context, results, true);

            // Assert
            Assert.IsFalse(isValid);
            Assert.IsTrue(results.Count > 0);
            Assert.AreEqual("A senha deve ter pelo menos 8 caracteres," +
                " incluindo uma letra maiúscula, uma letra minúscula," +
                " um número e um caractere especial.", results[0].ErrorMessage);
        }

        [Test]
        public void User_Senha_Invalid_WithLetters()
        {
            // Arrange
            var user = new User
            {
                nome = "John", // Nome vazio para simular um erro de validação
                email = "test@example.com",
                senhas = "ektlasfnj",
                nascimento = new DateTime(1990, 1, 1)
            };

            // Act
            var context = new ValidationContext(user, null, null);
            var results = new List<ValidationResult>();

            bool isValid = Validator.TryValidateObject(user, context, results, true);

            // Assert
            Assert.IsFalse(isValid);
            Assert.IsTrue(results.Count > 0);
            Assert.AreEqual("A senha deve ter pelo menos 8 caracteres," +
                " incluindo uma letra maiúscula, uma letra minúscula," +
                " um número e um caractere especial.", results[0].ErrorMessage);
        }

        [Test]
        public void User_Name_ShouldBeValid()
        {
            // Arrange
            var user = new User
            {
                nome = "John", // Nome válido
                email = "test@example.com",
                senhas = "63g5$ES$5",
                nascimento = new DateTime(1990, 1, 1)
            };

            // Act
            var context = new ValidationContext(user, null, null);
            var results = new List<ValidationResult>();

            bool isValid = Validator.TryValidateObject(user, context, results, true);

            // Assert
            Assert.IsTrue(isValid);
            Assert.IsEmpty(results); // Espera-se que não haja erros
        }
    }
}
