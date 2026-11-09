using Shazam.Data;
using Shazam.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace Shazam
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Ativa os controllers e as views (MVC).
            builder.Services.AddControllersWithViews();

            // Primeiro tenta ligar ao servidor da escola ("Lagostim").
            // Se não conseguir (por exemplo, se estiver em casa sem acesso ao servidor),
            // usa a base de dados local do Visual Studio ("Local").
            var connectionString = builder.Configuration.GetConnectionString("Lagostim");

            bool conseguiuLigar = false;
            try
            {
                using (var teste = new SqlConnection(connectionString))
                {
                    teste.Open();
                    conseguiuLigar = true;
                }
            }
            catch
            {
                conseguiuLigar = false;
            }

            if (!conseguiuLigar)
            {
                Console.WriteLine("Aviso: não foi possível ligar ao servidor remoto. A usar a base de dados local (LocalDB).");
                connectionString = builder.Configuration.GetConnectionString("Local");
            }

            // Guarda qual a base em uso, para o administrador ver no rodapé (sem a password).
            var dadosLigacao = new SqlConnectionStringBuilder(connectionString);
            BaseDeDadosEmUso.Descricao = (conseguiuLigar ? "Servidor do curso" : "Base local (LocalDB)")
                + " · " + dadosLigacao.InitialCatalog;

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            // A sessão serve para guardar quem está logado (id, nome e tipo).
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession();

            var app = builder.Build();

            // Prepara a base de dados. Está dentro de um try/catch para a aplicação
            // arrancar sempre, mesmo que haja algum problema com a base de dados.
            using (var scope = app.Services.CreateScope())
            {
                try
                {
                    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    // Cria a base de dados e as tabelas se ainda não existirem.
                    db.Database.EnsureCreated();

                    // Caso especial: se a base de dados já existia com tabelas antigas
                    // (de outra versão do projeto), o EnsureCreated não cria as tabelas
                    // novas. Aqui testamos se a tabela Utilizadores existe e,
                    // se não existir, criamos as tabelas deste projeto.
                    bool tabelasExistem = true;
                    try
                    {
                        db.Utilizadores.Any();
                    }
                    catch
                    {
                        tabelasExistem = false;
                    }

                    if (!tabelasExistem)
                    {
                        var criador = (RelationalDatabaseCreator)db.Database.GetService<IDatabaseCreator>();
                        criador.CreateTables();
                    }

                    // Cria um administrador inicial se ainda não houver utilizadores.
                    if (!db.Utilizadores.Any())
                    {
                        var admin = new Utilizador();
                        admin.Username = "admin";
                        admin.Password = "admin123";
                        admin.Nome = "Administrador";
                        admin.Tipo = "Admin";
                        admin.PrimeiroLogin = true;

                        db.Utilizadores.Add(admin);
                        db.SaveChanges();
                    }

                    // Dados de demonstração (clientes, treinadores, aulas e inscrições fictícios).
                    // Para desligar: "DadosDemonstracao": false no appsettings.json.
                    if (builder.Configuration.GetValue<bool>("DadosDemonstracao"))
                    {
                        DadosDemonstracao.Preencher(db);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Erro ao preparar a base de dados: " + ex.Message);
                }
            }

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();
            app.UseRouting();
            app.UseSession();

            // A página inicial é a página de login.
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Conta}/{action=Login}/{id?}");

            app.Run();
        }
    }
}
