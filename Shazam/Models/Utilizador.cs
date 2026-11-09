namespace Shazam.Models
{
    // Tabela dos utilizadores (dados de login).
    // O campo Tipo pode ser: "Admin", "Treinador" ou "Cliente".
    public class Utilizador
    {
        public int Id { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public string Nome { get; set; }

        public string Tipo { get; set; }

        // Quando é true, o utilizador é obrigado a mudar a password no primeiro login.
        public bool PrimeiroLogin { get; set; }
    }
}
