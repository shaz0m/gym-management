namespace Shazam.Models
{
    // Tabela das inscrições: liga um cliente a uma aula.
    public class Inscricao
    {
        public int Id { get; set; }

        public int ClienteId { get; set; }
        public Cliente Cliente { get; set; }

        public int AulaId { get; set; }
        public Aula Aula { get; set; }

        public DateTime DataInscricao { get; set; }

        // Estado da inscrição, por exemplo "Confirmada" ou "Cancelada".
        public string Estado { get; set; }
    }
}
