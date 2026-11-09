namespace Shazam.Models
{
    // Tabela das aulas do ginásio (criadas por um treinador).
    public class Aula
    {
        public int Id { get; set; }

        public string Nome { get; set; }

        public DateTime DataHoraInicio { get; set; }

        // Duração da aula em minutos.
        public int Duracao { get; set; }

        // Número máximo de clientes que se podem inscrever.
        public int Capacidade { get; set; }

        public int TreinadorId { get; set; }
        public Treinador Treinador { get; set; }

        // Lista das inscrições nesta aula.
        public List<Inscricao> Inscricoes { get; set; } = new List<Inscricao>();
    }
}
