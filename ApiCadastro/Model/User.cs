namespace ApiCadastro.Model
{
    public class User
    {
        public int Id { get; set; }
        public string? nome { get; set; }
        public string? email { get; set; }
        public string? profissao { get; set; }
        public string? cargo { get; set; }
        public string? senhas { get; set; }
    }
}

