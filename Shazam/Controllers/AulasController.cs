using Shazam.Data;
using Shazam.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Shazam.Controllers
{
    // CRUD de aulas. Só o Admin e os Treinadores podem gerir aulas.
    public class AulasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AulasController(ApplicationDbContext context)
        {
            _context = context;
        }

        private bool NaoTemPermissao()
        {
            string tipo = HttpContext.Session.GetString("Tipo");
            return tipo != "Admin" && tipo != "Treinador";
        }

        // GET: /Aulas
        public IActionResult Index()
        {
            if (NaoTemPermissao())
            {
                return RedirectToAction("Login", "Conta");
            }

            var aulas = _context.Aulas
                .Include(a => a.Treinador)
                .Include(a => a.Inscricoes)
                .ToList();

            return View(aulas);
        }

        // GET: /Aulas/Details/5
        public IActionResult Details(int id)
        {
            if (NaoTemPermissao())
            {
                return RedirectToAction("Login", "Conta");
            }

            var aula = _context.Aulas
                .Include(a => a.Treinador)
                .Include(a => a.Inscricoes)
                .ThenInclude(i => i.Cliente)
                .FirstOrDefault(a => a.Id == id);

            if (aula == null)
            {
                return NotFound();
            }

            return View(aula);
        }

        // GET: /Aulas/Create
        public IActionResult Create()
        {
            if (NaoTemPermissao())
            {
                return RedirectToAction("Login", "Conta");
            }

            // Não dá para criar uma aula sem existir pelo menos um treinador.
            if (_context.Treinadores.Count() == 0)
            {
                TempData["Erro"] = "Crie primeiro um treinador antes de criar aulas.";
                return RedirectToAction("Index");
            }

            ViewBag.Treinadores = _context.Treinadores.ToList();
            return View();
        }

        // POST: /Aulas/Create
        [HttpPost]
        public IActionResult Create(string nome, DateTime dataHoraInicio, int duracao,
            int capacidade, int treinadorId)
        {
            if (NaoTemPermissao())
            {
                return RedirectToAction("Login", "Conta");
            }

            if (string.IsNullOrEmpty(nome) || duracao <= 0 || capacidade <= 0)
            {
                ViewBag.Erro = "Preencha todos os campos com valores válidos.";
                ViewBag.Treinadores = _context.Treinadores.ToList();
                return View();
            }

            var aula = new Aula();
            aula.Nome = nome;
            aula.DataHoraInicio = dataHoraInicio;
            aula.Duracao = duracao;
            aula.Capacidade = capacidade;
            aula.TreinadorId = treinadorId;

            _context.Aulas.Add(aula);
            _context.SaveChanges();

            TempData["Mensagem"] = "Aula criada com sucesso.";
            return RedirectToAction("Index");
        }

        // GET: /Aulas/Edit/5
        public IActionResult Edit(int id)
        {
            if (NaoTemPermissao())
            {
                return RedirectToAction("Login", "Conta");
            }

            var aula = _context.Aulas.Find(id);
            if (aula == null)
            {
                return NotFound();
            }

            ViewBag.Treinadores = _context.Treinadores.ToList();
            return View(aula);
        }

        // POST: /Aulas/Edit/5
        [HttpPost]
        public IActionResult Edit(int id, string nome, DateTime dataHoraInicio, int duracao,
            int capacidade, int treinadorId)
        {
            if (NaoTemPermissao())
            {
                return RedirectToAction("Login", "Conta");
            }

            var aula = _context.Aulas.Find(id);
            if (aula == null)
            {
                return NotFound();
            }

            if (string.IsNullOrEmpty(nome) || duracao <= 0 || capacidade <= 0)
            {
                ViewBag.Erro = "Preencha todos os campos com valores válidos.";
                ViewBag.Treinadores = _context.Treinadores.ToList();
                return View(aula);
            }

            aula.Nome = nome;
            aula.DataHoraInicio = dataHoraInicio;
            aula.Duracao = duracao;
            aula.Capacidade = capacidade;
            aula.TreinadorId = treinadorId;

            _context.SaveChanges();

            TempData["Mensagem"] = "Aula atualizada com sucesso.";
            return RedirectToAction("Index");
        }

        // GET: /Aulas/Delete/5
        public IActionResult Delete(int id)
        {
            if (NaoTemPermissao())
            {
                return RedirectToAction("Login", "Conta");
            }

            var aula = _context.Aulas
                .Include(a => a.Treinador)
                .Include(a => a.Inscricoes)
                .FirstOrDefault(a => a.Id == id);

            if (aula == null)
            {
                return NotFound();
            }

            return View(aula);
        }

        // POST: /Aulas/Delete/5
        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            if (NaoTemPermissao())
            {
                return RedirectToAction("Login", "Conta");
            }

            var aula = _context.Aulas.Find(id);
            if (aula == null)
            {
                return RedirectToAction("Index");
            }

            // Apaga primeiro as inscrições desta aula.
            var inscricoes = _context.Inscricoes.Where(i => i.AulaId == aula.Id).ToList();
            _context.Inscricoes.RemoveRange(inscricoes);

            _context.Aulas.Remove(aula);
            _context.SaveChanges();

            TempData["Mensagem"] = "Aula apagada com sucesso.";
            return RedirectToAction("Index");
        }
    }
}
