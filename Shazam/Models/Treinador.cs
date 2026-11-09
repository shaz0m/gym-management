namespace Shazam.Models
{
    // Tabela dos treinadores.
    // Cada treinador está ligado a um utilizador (para poder fazer login).
    public class Treinador
    {
        public int Id { get; set; }

        public int UtilizadorId { get; set; }
        public Utilizador Utilizador { get; set; }

        public string Nome { get; set; }

        public string Especialidade { get; set; }

        // Lista das aulas dadas por este treinador.
        public List<Aula> Aulas { get; set; } = new List<Aula>();
    }
}
