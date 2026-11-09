using Shazam.Data;
using Shazam.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Shazam.Controllers
{
    // Painéis do cliente e do treinador (o que cada um vê depois do login).
    public class PainelController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PainelController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Painel/Cliente
        // Mostra as aulas disponíveis e as inscrições do cliente logado.
        public IActionResult Cliente()
        {
            string idTexto = HttpContext.Session.GetString("UtilizadorId");
            if (idTexto == null)
            {
                return RedirectToAction("Login", "Conta");
            }

            int utilizadorId = int.Parse(idTexto);

            var cliente = _context.Clientes.FirstOrDefault(c => c.UtilizadorId == utilizadorId);
            if (cliente == null)
            {
                return RedirectToAction("Login", "Conta");
            }

            var aulas = _context.Aulas
                .Include(a => a.Treinador)
                .Include(a => a.Inscricoes)
                .ToList();

            var minhasInscricoes = _context.Inscricoes
                .Include(i => i.Aula)
                .Where(i => i.ClienteId == cliente.Id)
                .ToList();

            ViewBag.Cliente = cliente;
            ViewBag.MinhasInscricoes = minhasInscricoes;

            return View(aulas);
        }

        // POST: /Painel/Inscrever
        // O cliente logado inscreve-se numa aula.
        [HttpPost]
        public IActionResult Inscrever(int aulaId)
        {
            string idTexto = HttpContext.Session.GetString("UtilizadorId");
            if (idTexto == null)
            {
                return RedirectToAction("Login", "Conta");
            }

            int utilizadorId = int.Parse(idTexto);

            var cliente = _context.Clientes.FirstOrDefault(c => c.UtilizadorId == utilizadorId);
            if (cliente == null)
            {
                return RedirectToAction("Login", "Conta");
            }

            var aula = _context.Aulas.Find(aulaId);
            if (aula == null)
            {
                return NotFound();
            }

            // Não deixa inscrever duas vezes na mesma aula.
            var jaInscrito = _context.Inscricoes
                .FirstOrDefault(i => i.ClienteId == cliente.Id && i.AulaId == aulaId);

            if (jaInscrito != null)
            {
                TempData["Erro"] = "Já está inscrito nessa aula.";
                return RedirectToAction("Cliente");
            }

            // Não deixa inscrever se a aula estiver lotada.
            int inscritos = _context.Inscricoes.Count(i => i.AulaId == aulaId);
            if (inscritos >= aula.Capacidade)
            {
                TempData["Erro"] = "Essa aula já está lotada.";
                return RedirectToAction("Cliente");
            }

            var inscricao = new Inscricao();
            inscricao.ClienteId = cliente.Id;
            inscricao.AulaId = aulaId;
            inscricao.DataInscricao = DateTime.Now;
            inscricao.Estado = "Confirmada";

            _context.Inscricoes.Add(inscricao);
            _context.SaveChanges();

            TempData["Mensagem"] = "Inscrição feita com sucesso.";
            return RedirectToAction("Cliente");
        }

        // POST: /Painel/Cancelar
        // O cliente logado cancela uma inscrição sua.
        [HttpPost]
        public IActionResult Cancelar(int inscricaoId)
        {
            string idTexto = HttpContext.Session.GetString("UtilizadorId");
            if (idTexto == null)
            {
                return RedirectToAction("Login", "Conta");
            }

            int utilizadorId = int.Parse(idTexto);

            var cliente = _context.Clientes.FirstOrDefault(c => c.UtilizadorId == utilizadorId);
            if (cliente == null)
            {
                return RedirectToAction("Login", "Conta");
            }

            var inscricao = _context.Inscricoes.Find(inscricaoId);

            // Só deixa cancelar se a inscrição for mesmo deste cliente.
            if (inscricao != null && inscricao.ClienteId == cliente.Id)
            {
                _context.Inscricoes.Remove(inscricao);
                _context.SaveChanges();
                TempData["Mensagem"] = "Inscrição cancelada.";
            }

            return RedirectToAction("Cliente");
        }

        // GET: /Painel/Treinador
        // Mostra as aulas do treinador logado e quantos inscritos tem cada uma.
        public IActionResult Treinador()
        {
            string idTexto = HttpContext.Session.GetString("UtilizadorId");
            if (idTexto == null)
            {
                return RedirectToAction("Login", "Conta");
            }

            int utilizadorId = int.Parse(idTexto);

            var treinador = _context.Treinadores.FirstOrDefault(t => t.UtilizadorId == utilizadorId);
            if (treinador == null)
            {
                return RedirectToAction("Login", "Conta");
            }

            var minhasAulas = _context.Aulas
                .Include(a => a.Inscricoes)
                .Where(a => a.TreinadorId == treinador.Id)
                .ToList();

            ViewBag.Treinador = treinador;

            return View(minhasAulas);
        }
    }
}
