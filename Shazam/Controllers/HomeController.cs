using Shazam.Data;
using Shazam.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace Shazam.Controllers
{
    // Painel principal do administrador.
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // O painel de administração é só para o Admin
            // (quem tem outra sessão é enviado para o seu próprio painel).
            if (HttpContext.Session.GetString("Tipo") != "Admin")
            {
                return RedirectToAction("Login", "Conta");
            }

            // Números que aparecem nos cartões do painel.
            ViewBag.TotalClientes = _context.Clientes.Count();
            ViewBag.TotalTreinadores = _context.Treinadores.Count();
            ViewBag.TotalAulas = _context.Aulas.Count();
            ViewBag.TotalInscricoes = _context.Inscricoes.Count();

            // As próximas 5 aulas (a partir de agora), com treinador e inscritos.
            DateTime agora = DateTime.Now;
            ViewBag.ProximasAulas = _context.Aulas
                .Include(a => a.Treinador)
                .Include(a => a.Inscricoes)
                .Where(a => a.DataHoraInicio >= agora)
                .OrderBy(a => a.DataHoraInicio)
                .Take(5)
                .ToList();

            // Ocupação de todas as aulas que ainda não aconteceram.
            ViewBag.VagasFuturas = _context.Aulas
                .Where(a => a.DataHoraInicio >= agora)
                .Sum(a => a.Capacidade);
            ViewBag.InscritosFuturos = _context.Inscricoes
                .Count(i => i.Aula.DataHoraInicio >= agora);

            // As 5 inscrições mais recentes.
            ViewBag.UltimasInscricoes = _context.Inscricoes
                .Include(i => i.Cliente)
                .Include(i => i.Aula)
                .OrderByDescending(i => i.DataInscricao)
                .Take(5)
                .ToList();

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            var erro = new ErrorViewModel();
            if (Activity.Current != null)
            {
                erro.RequestId = Activity.Current.Id;
            }
            else
            {
                erro.RequestId = HttpContext.TraceIdentifier;
            }

            return View(erro);
        }
    }
}
