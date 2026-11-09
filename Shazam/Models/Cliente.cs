namespace Shazam.Models
{
    // Tabela dos clientes do ginásio.
    // Cada cliente está ligado a um utilizador (para poder fazer login).
    public class Cliente
    {
        public int Id { get; set; }

        public int UtilizadorId { get; set; }
        public Utilizador Utilizador { get; set; }

        public string Nome { get; set; }

        public string Email { get; set; }

        public string Telefone { get; set; }

        public DateTime DataNascimento { get; set; }

        // Lista das inscrições deste cliente em aulas.
        public List<Inscricao> Inscricoes { get; set; } = new List<Inscricao>();
    }
}
