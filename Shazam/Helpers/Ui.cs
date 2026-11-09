using System.Globalization;
using Shazam.Models;

namespace Shazam.Helpers
{
    // Pequenas funções de apresentação usadas nas views
    // (iniciais para os avatares, datas em português, percentagens).
    public static class Ui
    {
        public static readonly CultureInfo Pt = new CultureInfo("pt-PT");

        // "Mariana Lopes" -> "ML"
        public static string Iniciais(string nome)
        {
            if (string.IsNullOrWhiteSpace(nome))
            {
                return "?";
            }

            string[] partes = nome.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            string iniciais = partes[0].Substring(0, 1);

            if (partes.Length > 1)
            {
                iniciais += partes[partes.Length - 1].Substring(0, 1);
            }

            return iniciais.ToUpper();
        }

        // "Mariana Lopes" -> "Mariana"
        public static string PrimeiroNome(string nome)
        {
            if (string.IsNullOrWhiteSpace(nome))
            {
                return "";
            }

            return nome.Split(' ', StringSplitOptions.RemoveEmptyEntries)[0];
        }

        // Nome do tipo de utilizador para mostrar no ecrã.
        public static string NomeTipo(string tipo)
        {
            if (tipo == "Admin")
            {
                return "Administrador";
            }

            return tipo;
        }

        // "qui", "sex", ... (sem o ponto da abreviatura)
        public static string DiaSemana(DateTime data)
        {
            return data.ToString("ddd", Pt).TrimEnd('.');
        }

        // "nov", "dez", ...
        public static string Mes(DateTime data)
        {
            return data.ToString("MMM", Pt).TrimEnd('.');
        }

        // Título de um dia no horário: "Hoje", "Amanhã" ou "Domingo".
        public static string NomeDia(DateTime data)
        {
            if (data.Date == DateTime.Today)
            {
                return "Hoje";
            }

            if (data.Date == DateTime.Today.AddDays(1))
            {
                return "Amanhã";
            }

            string nome = data.ToString("dddd", Pt);
            return char.ToUpper(nome[0]) + nome.Substring(1);
        }

        // "quinta-feira, 5 de novembro de 2026"
        public static string DataLonga(DateTime data)
        {
            return data.ToString("dddd, d 'de' MMMM 'de' yyyy", Pt);
        }

        // Estado de uma aula para mostrar no ecrã:
        // "Terminada", "A decorrer", "Lotada" ou "Disponível".
        public static string EstadoAula(Aula aula)
        {
            DateTime agora = DateTime.Now;
            DateTime fim = aula.DataHoraInicio.AddMinutes(aula.Duracao);

            if (fim < agora)
            {
                return "Terminada";
            }

            if (aula.DataHoraInicio <= agora)
            {
                return "A decorrer";
            }

            if (aula.Inscricoes.Count >= aula.Capacidade)
            {
                return "Lotada";
            }

            return "Disponível";
        }

        // Classe CSS da etiqueta de estado (aulas e inscrições).
        public static string ClasseEstado(string estado)
        {
            if (estado == "Lotada")
            {
                return "estado--lotada";
            }

            if (estado == "Terminada" || estado == "Cancelada")
            {
                return "estado--terminada";
            }

            return "";
        }

        // Ordena as aulas: primeiro as que ainda vão acontecer (da mais próxima
        // para a mais distante), depois as que já passaram.
        public static List<Aula> OrdenarAulas(IEnumerable<Aula> aulas)
        {
            DateTime agora = DateTime.Now;

            return aulas
                .OrderBy(a => a.DataHoraInicio < agora)
                .ThenBy(a => a.DataHoraInicio)
                .ToList();
        }

        // Percentagem inteira (0 a 100) para as barras de ocupação.
        public static int Percentagem(int parte, int total)
        {
            if (total <= 0)
            {
                return 0;
            }

            int valor = (int)Math.Round(parte * 100.0 / total);
            return Math.Min(100, valor);
        }
    }
}
