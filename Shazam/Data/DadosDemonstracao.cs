using System.Globalization;
using System.Text;
using Shazam.Models;

namespace Shazam.Data
{
    // Dados de demonstração (fictícios) para apresentar a aplicação com conteúdo.
    // É chamado no arranque (Program.cs) quando "DadosDemonstracao" está a true no appsettings.json.
    //
    // - Cria 6 treinadores e 30 clientes, só os que ainda não existem (procura pelo username).
    // - Mantém o horário semanal preenchido: aulas dos últimos 7 dias e dos próximos 14,
    //   criando só as que faltam. Assim, sempre que a aplicação arranca, há aulas para os dias seguintes.
    // - Nas aulas novas inscreve clientes ao acaso (algumas ficam lotadas).
    //
    // Todos os utilizadores de demonstração entram com a password "demo1234".
    public static class DadosDemonstracao
    {
        private const string PasswordDemo = "demo1234";
        private const string DominioEmail = "exemplo.test";

        // username, nome, especialidade
        private static readonly string[,] Treinadores =
        {
            { "rita.fonseca", "Rita Fonseca", "Musculação" },
            { "tiago.almeida", "Tiago Almeida", "Crossfit" },
            { "ines.carvalho", "Inês Carvalho", "Yoga" },
            { "bruno.teixeira", "Bruno Teixeira", "Spinning" },
            { "sofia.matos", "Sofia Matos", "Pilates" },
            { "ricardo.nunes", "Ricardo Nunes", "Boxe" },
        };

        // nome, data de nascimento
        private static readonly string[,] Clientes =
        {
            { "Mariana Lopes", "1996-03-14" }, { "João Pereira", "1989-11-02" },
            { "Ana Rodrigues", "2001-07-27" }, { "Pedro Santos", "1993-01-19" },
            { "Catarina Martins", "1998-09-08" }, { "Rui Sousa", "1985-05-30" },
            { "Beatriz Ferreira", "2003-12-11" }, { "Miguel Costa", "1991-04-23" },
            { "Sara Oliveira", "1999-06-05" }, { "Diogo Ribeiro", "1994-10-17" },
            { "Joana Gomes", "1987-02-28" }, { "Tomás Marques", "2002-08-13" },
            { "Carolina Pinto", "1997-12-01" }, { "André Moreira", "1990-03-09" },
            { "Leonor Rocha", "2000-05-21" }, { "Gonçalo Lima", "1995-09-30" },
            { "Marta Cardoso", "1988-07-14" }, { "Francisco Antunes", "1992-11-25" },
            { "Daniela Correia", "1996-01-07" }, { "Nuno Mendes", "1983-04-16" },
            { "Patrícia Vieira", "1991-08-03" }, { "Hugo Fernandes", "1986-10-29" },
            { "Cláudia Soares", "1994-02-18" }, { "Vasco Neves", "2001-11-09" },
            { "Raquel Monteiro", "1998-03-26" }, { "Filipe Batista", "1993-06-12" },
            { "Teresa Lourenço", "1984-09-04" }, { "Luís Machado", "1990-12-20" },
            { "Helena Freitas", "1997-04-02" }, { "Paulo Tavares", "1982-08-31" },
        };

        // Horário semanal: dia, hora, aula, duração (min), capacidade, username do treinador
        private static readonly (DayOfWeek Dia, string Hora, string Nome, int Duracao, int Capacidade, string Treinador)[] Horario =
        {
            (DayOfWeek.Monday, "07:30", "HIIT 30", 30, 8, "tiago.almeida"),
            (DayOfWeek.Monday, "12:30", "Pilates", 50, 10, "sofia.matos"),
            (DayOfWeek.Monday, "18:30", "Spinning", 45, 20, "bruno.teixeira"),
            (DayOfWeek.Monday, "19:30", "Boxe", 60, 14, "ricardo.nunes"),

            (DayOfWeek.Tuesday, "08:00", "Yoga Vinyasa", 60, 12, "ines.carvalho"),
            (DayOfWeek.Tuesday, "12:30", "Musculação técnica", 50, 8, "rita.fonseca"),
            (DayOfWeek.Tuesday, "18:30", "Crossfit WOD", 60, 12, "tiago.almeida"),

            (DayOfWeek.Wednesday, "07:30", "HIIT 30", 30, 8, "tiago.almeida"),
            (DayOfWeek.Wednesday, "12:30", "Treino funcional", 45, 12, "rita.fonseca"),
            (DayOfWeek.Wednesday, "18:30", "Spinning", 45, 20, "bruno.teixeira"),
            (DayOfWeek.Wednesday, "19:30", "Pilates", 50, 10, "sofia.matos"),

            (DayOfWeek.Thursday, "08:00", "Mobilidade", 45, 15, "ines.carvalho"),
            (DayOfWeek.Thursday, "12:30", "Musculação técnica", 50, 8, "rita.fonseca"),
            (DayOfWeek.Thursday, "18:30", "Crossfit WOD", 60, 12, "tiago.almeida"),
            (DayOfWeek.Thursday, "19:30", "Boxe", 60, 14, "ricardo.nunes"),

            (DayOfWeek.Friday, "07:30", "HIIT 30", 30, 8, "tiago.almeida"),
            (DayOfWeek.Friday, "12:30", "Yoga Vinyasa", 60, 12, "ines.carvalho"),
            (DayOfWeek.Friday, "18:30", "Spinning", 45, 20, "bruno.teixeira"),

            (DayOfWeek.Saturday, "10:00", "Crossfit WOD", 60, 12, "tiago.almeida"),
            (DayOfWeek.Saturday, "11:00", "Yoga Vinyasa", 60, 12, "ines.carvalho"),

            (DayOfWeek.Sunday, "10:00", "Mobilidade", 45, 15, "ines.carvalho"),
        };

        public static void Preencher(ApplicationDbContext db)
        {
            var aleatorio = new Random();

            Dictionary<string, Treinador> treinadores = CriarTreinadores(db);
            List<Cliente> clientes = CriarClientes(db);
            int aulasNovas = CriarAulas(db, treinadores, clientes, aleatorio);

            Console.WriteLine("Dados de demonstração: " + aulasNovas + " aulas novas.");
        }

        private static Dictionary<string, Treinador> CriarTreinadores(ApplicationDbContext db)
        {
            var resultado = new Dictionary<string, Treinador>();

            for (int i = 0; i < Treinadores.GetLength(0); i++)
            {
                string username = Treinadores[i, 0];
                Treinador treinador = null;

                var utilizador = db.Utilizadores.FirstOrDefault(u => u.Username == username);
                if (utilizador != null)
                {
                    treinador = db.Treinadores.FirstOrDefault(t => t.UtilizadorId == utilizador.Id);
                }
                else
                {
                    utilizador = NovoUtilizador(username, Treinadores[i, 1], "Treinador");
                    db.Utilizadores.Add(utilizador);
                }

                if (treinador == null)
                {
                    treinador = new Treinador();
                    treinador.Utilizador = utilizador;
                    treinador.Nome = Treinadores[i, 1];
                    treinador.Especialidade = Treinadores[i, 2];
                    db.Treinadores.Add(treinador);
                }

                resultado[username] = treinador;
            }

            db.SaveChanges();
            return resultado;
        }

        private static List<Cliente> CriarClientes(ApplicationDbContext db)
        {
            var resultado = new List<Cliente>();

            for (int i = 0; i < Clientes.GetLength(0); i++)
            {
                string nome = Clientes[i, 0];
                string username = Username(nome);
                Cliente cliente = null;

                var utilizador = db.Utilizadores.FirstOrDefault(u => u.Username == username);
                if (utilizador != null)
                {
                    cliente = db.Clientes.FirstOrDefault(c => c.UtilizadorId == utilizador.Id);
                }
                else
                {
                    utilizador = NovoUtilizador(username, nome, "Cliente");
                    db.Utilizadores.Add(utilizador);
                }

                if (cliente == null)
                {
                    cliente = new Cliente();
                    cliente.Utilizador = utilizador;
                    cliente.Nome = nome;
                    cliente.Email = username + "@" + DominioEmail;
                    cliente.Telefone = "900 100 " + (201 + i);
                    cliente.DataNascimento = DateTime.Parse(Clientes[i, 1], CultureInfo.InvariantCulture);
                    db.Clientes.Add(cliente);
                }

                resultado.Add(cliente);
            }

            db.SaveChanges();
            return resultado;
        }

        // Cria as aulas do horário que ainda não existem (dos últimos 7 dias aos próximos 14)
        // e inscreve clientes nelas. Devolve quantas aulas foram criadas.
        private static int CriarAulas(ApplicationDbContext db, Dictionary<string, Treinador> treinadores,
            List<Cliente> clientes, Random aleatorio)
        {
            DateTime agora = DateTime.Now;
            DateTime primeiroDia = DateTime.Today.AddDays(-7);
            DateTime ultimoDia = DateTime.Today.AddDays(14);

            // Aulas que já existem neste intervalo (para não as repetir).
            var existentes = new HashSet<string>();
            var aulasNoIntervalo = db.Aulas
                .Where(a => a.DataHoraInicio >= primeiroDia && a.DataHoraInicio < ultimoDia)
                .Select(a => new { a.Nome, a.DataHoraInicio })
                .ToList();
            foreach (var a in aulasNoIntervalo)
            {
                existentes.Add(a.Nome + "|" + a.DataHoraInicio.ToString("s"));
            }

            int criadas = 0;

            for (DateTime dia = primeiroDia; dia < ultimoDia; dia = dia.AddDays(1))
            {
                foreach (var linha in Horario)
                {
                    if (linha.Dia != dia.DayOfWeek)
                    {
                        continue;
                    }

                    DateTime inicio = dia.Add(TimeSpan.Parse(linha.Hora, CultureInfo.InvariantCulture));
                    if (existentes.Contains(linha.Nome + "|" + inicio.ToString("s")))
                    {
                        continue;
                    }

                    var aula = new Aula();
                    aula.Nome = linha.Nome;
                    aula.DataHoraInicio = inicio;
                    aula.Duracao = linha.Duracao;
                    aula.Capacidade = linha.Capacidade;
                    aula.Treinador = treinadores[linha.Treinador];
                    db.Aulas.Add(aula);

                    Inscrever(aula, clientes, agora, aleatorio);
                    criadas++;
                }
            }

            db.SaveChanges();
            return criadas;
        }

        // Inscreve na aula um número de clientes ao acaso:
        // cerca de 1 em cada 6 aulas fica lotada; as restantes ficam entre meio e quase cheias
        // (as futuras um pouco menos, porque ainda há tempo para mais inscrições).
        private static void Inscrever(Aula aula, List<Cliente> clientes, DateTime agora, Random aleatorio)
        {
            double ocupacao;
            if (aleatorio.Next(6) == 0)
            {
                ocupacao = 1.0;
            }
            else if (aula.DataHoraInicio < agora)
            {
                ocupacao = 0.5 + aleatorio.NextDouble() * 0.45;
            }
            else
            {
                ocupacao = 0.2 + aleatorio.NextDouble() * 0.6;
            }

            int quantos = Math.Min(clientes.Count, (int)Math.Round(aula.Capacidade * ocupacao));
            var escolhidos = clientes.OrderBy(c => aleatorio.Next()).Take(quantos);

            foreach (var cliente in escolhidos)
            {
                // Inscrição feita entre 2 horas e 5 dias antes da aula (nunca no futuro).
                DateTime data = aula.DataHoraInicio.AddMinutes(-aleatorio.Next(120, 5 * 24 * 60));
                if (data > agora)
                {
                    data = agora.AddMinutes(-aleatorio.Next(5, 600));
                }

                var inscricao = new Inscricao();
                inscricao.Cliente = cliente;
                inscricao.DataInscricao = data;
                inscricao.Estado = "Confirmada";
                aula.Inscricoes.Add(inscricao);
            }
        }

        private static Utilizador NovoUtilizador(string username, string nome, string tipo)
        {
            var utilizador = new Utilizador();
            utilizador.Username = username;
            utilizador.Password = PasswordDemo;
            utilizador.Nome = nome;
            utilizador.Tipo = tipo;
            utilizador.PrimeiroLogin = false;
            return utilizador;
        }

        // "Inês Carvalho" -> "ines.carvalho"
        private static string Username(string nome)
        {
            string semAcentos = new string(nome.Normalize(NormalizationForm.FormD)
                .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                .ToArray());

            return semAcentos.ToLower().Replace(' ', '.');
        }
    }
}
