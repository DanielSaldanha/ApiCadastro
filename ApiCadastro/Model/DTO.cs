﻿using System.ComponentModel.DataAnnotations;

namespace ApiCadastro.Model
{
    public class DTO
    {
        // Outros campos
        public string? nome { get; set; }

        [Required(ErrorMessage = "O e-mail é obrigatório.")]
        [EmailAddress(ErrorMessage = "Formato de e-mail inválido.")]
        public string? email { get; set; }

        public string? profissao { get; set; }
        public string? cargo { get; set; }

        [Required(ErrorMessage = "A senha é obrigatória.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
            ErrorMessage = "A senha deve ter pelo menos 8 caracteres, incluindo uma letra maiúscula, " +
            "uma letra minúscula, um número e um caractere especial.")]
        public string? password { get; set; }

        [Required(ErrorMessage = "A data de nascimento é obrigatória.")]
        [Range(typeof(DateTime), "1900-01-01", "2005-01-01", ErrorMessage = "A idade deve estar entre 18 e 90 anos.")]
        public DateTime nascimento { get; set; }
    }
}
