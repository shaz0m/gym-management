using Shazam.Data;
using Shazam.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Shazam.Controllers
{
    // Gestão de inscrições (Admin e Treinadores).
    public class InscricoesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public InscricoesController(ApplicationDbContext context)
        {
            _context = context;
        }

        private bool NaoTemPermissao()
        {
            string tipo = HttpContext.Session.GetString("Tipo");
            return tipo != "Admin" && tipo != "Treinador";
        }

        // GET: /Inscricoes
        public IActionResult Index()
        {
            if (NaoTemPermissao())
            {
                return RedirectToAction("Login", "Conta");
            }

            var inscricoes = _context.Inscricoes
                .Include(i => i.Cliente)
                .Include(i => i.Aula)
                .ToList();

            return View(inscricoes);
        }

        // GET: /Inscricoes/Create
        public IActionResult Create()
        {
            if (NaoTemPermissao())
            {
                return RedirectToAction("Login", "Conta");
            }

            // Não dá para criar uma inscrição sem existir pelo menos
            // um cliente e uma aula.
            if (_context.Clientes.Count() == 0 || _context.Aulas.Count() == 0)
            {
                TempData["Erro"] = "É preciso existir pelo menos um cliente e uma aula para criar inscrições.";
                return RedirectToAction("Index");
            }

            ViewBag.Clientes = _context.Clientes.ToList();
            ViewBag.Aulas = _context.Aulas.ToList();
            return View();
        }

        // POST: /Inscricoes/Create
        [HttpPost]
        public IActionResult Create(int clienteId, int aulaId)
        {
            if (NaoTemPermissao())
            {
                return RedirectToAction("Login", "Conta");
            }

            // Verifica se o cliente já está inscrito nesta aula.
            var jaInscrito = _context.Inscricoes
                .FirstOrDefault(i => i.ClienteId == clienteId && i.AulaId == aulaId);

            if (jaInscrito != null)
            {
                ViewBag.Erro = "Esse cliente já está inscrito nessa aula.";
                ViewBag.Clientes = _context.Clientes.ToList();
                ViewBag.Aulas = _context.Aulas.ToList();
                return View();
            }

            // Verifica se a aula ainda tem vagas.
            var aula = _context.Aulas.Find(aulaId);
            if (aula == null)
            {
                return NotFound();
            }

            int inscritos = _context.Inscricoes.Count(i => i.AulaId == aulaId);
            if (inscritos >= aula.Capacidade)
            {
                ViewBag.Erro = "Essa aula já está lotada.";
                ViewBag.Clientes = _context.Clientes.ToList();
                ViewBag.Aulas = _context.Aulas.ToList();
                return View();
            }

            var inscricao = new Inscricao();
            inscricao.ClienteId = clienteId;
            inscricao.AulaId = aulaId;
            inscricao.DataInscricao = DateTime.Now;
            inscricao.Estado = "Confirmada";

            _context.Inscricoes.Add(inscricao);
            _context.SaveChanges();

            TempData["Mensagem"] = "Inscrição criada com sucesso.";
            return RedirectToAction("Index");
        }

        // GET: /Inscricoes/Delete/5
        public IActionResult Delete(int id)
        {
            if (NaoTemPermissao())
            {
                return RedirectToAction("Login", "Conta");
            }

            var inscricao = _context.Inscricoes
                .Include(i => i.Cliente)
                .Include(i => i.Aula)
                .FirstOrDefault(i => i.Id == id);

            if (inscricao == null)
            {
                return NotFound();
            }

            return View(inscricao);
        }

        // POST: /Inscricoes/Delete/5
        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            if (NaoTemPermissao())
            {
                return RedirectToAction("Login", "Conta");
            }

            var inscricao = _context.Inscricoes.Find(id);
            if (inscricao != null)
            {
                _context.Inscricoes.Remove(inscricao);
                _context.SaveChanges();
            }

            TempData["Mensagem"] = "Inscrição apagada com sucesso.";
            return RedirectToAction("Index");
        }
    }
}
