using Shazam.Data;
using Shazam.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Shazam.Controllers
{
    // CRUD de treinadores. Só o Admin pode gerir treinadores.
    public class TreinadoresController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TreinadoresController(ApplicationDbContext context)
        {
            _context = context;
        }

        private bool NaoTemPermissao()
        {
            string tipo = HttpContext.Session.GetString("Tipo");
            return tipo != "Admin";
        }

        // GET: /Treinadores
        public IActionResult Index()
        {
            if (NaoTemPermissao())
            {
                return RedirectToAction("Login", "Conta");
            }

            var treinadores = _context.Treinadores
                .Include(t => t.Utilizador)
                .Include(t => t.Aulas)
                .ToList();

            return View(treinadores);
        }

        // GET: /Treinadores/Create
        public IActionResult Create()
        {
            if (NaoTemPermissao())
            {
                return RedirectToAction("Login", "Conta");
            }

            return View();
        }

        // POST: /Treinadores/Create
        [HttpPost]
        public IActionResult Create(string nome, string especialidade, string username, string password)
        {
            if (NaoTemPermissao())
            {
                return RedirectToAction("Login", "Conta");
            }

            if (string.IsNullOrEmpty(nome) || string.IsNullOrEmpty(username) ||
                string.IsNullOrEmpty(password))
            {
                ViewBag.Erro = "Preencha todos os campos obrigatórios.";
                return View();
            }

            var existe = _context.Utilizadores.FirstOrDefault(u => u.Username == username);
            if (existe != null)
            {
                ViewBag.Erro = "Esse username já está a ser usado.";
                return View();
            }

            var utilizador = new Utilizador();
            utilizador.Username = username;
            utilizador.Password = password;
            utilizador.Nome = nome;
            utilizador.Tipo = "Treinador";
            utilizador.PrimeiroLogin = true;

            _context.Utilizadores.Add(utilizador);
            _context.SaveChanges();

            var treinador = new Treinador();
            treinador.UtilizadorId = utilizador.Id;
            treinador.Nome = nome;
            treinador.Especialidade = especialidade;

            _context.Treinadores.Add(treinador);
            _context.SaveChanges();

            TempData["Mensagem"] = "Treinador criado com sucesso.";
            return RedirectToAction("Index");
        }

        // GET: /Treinadores/Edit/5
        public IActionResult Edit(int id)
        {
            if (NaoTemPermissao())
            {
                return RedirectToAction("Login", "Conta");
            }

            var treinador = _context.Treinadores.Find(id);
            if (treinador == null)
            {
                return NotFound();
            }

            return View(treinador);
        }

        // POST: /Treinadores/Edit/5
        [HttpPost]
        public IActionResult Edit(int id, string nome, string especialidade)
        {
            if (NaoTemPermissao())
            {
                return RedirectToAction("Login", "Conta");
            }

            var treinador = _context.Treinadores.Find(id);
            if (treinador == null)
            {
                return NotFound();
            }

            if (string.IsNullOrEmpty(nome))
            {
                ViewBag.Erro = "O nome é obrigatório.";
                return View(treinador);
            }

            treinador.Nome = nome;
            treinador.Especialidade = especialidade;

            var utilizador = _context.Utilizadores.Find(treinador.UtilizadorId);
            if (utilizador != null)
            {
                utilizador.Nome = nome;
            }

            _context.SaveChanges();

            TempData["Mensagem"] = "Treinador atualizado com sucesso.";
            return RedirectToAction("Index");
        }

        // GET: /Treinadores/Delete/5
        public IActionResult Delete(int id)
        {
            if (NaoTemPermissao())
            {
                return RedirectToAction("Login", "Conta");
            }

            var treinador = _context.Treinadores
                .Include(t => t.Utilizador)
                .Include(t => t.Aulas)
                .FirstOrDefault(t => t.Id == id);

            if (treinador == null)
            {
                return NotFound();
            }

            return View(treinador);
        }

        // POST: /Treinadores/Delete/5
        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            if (NaoTemPermissao())
            {
                return RedirectToAction("Login", "Conta");
            }

            var treinador = _context.Treinadores.Find(id);
            if (treinador == null)
            {
                return RedirectToAction("Index");
            }

            // Apaga primeiro as inscrições das aulas deste treinador,
            // depois as aulas, depois o treinador e por fim o utilizador.
            var aulas = _context.Aulas.Where(a => a.TreinadorId == treinador.Id).ToList();

            foreach (var aula in aulas)
            {
                var inscricoes = _context.Inscricoes.Where(i => i.AulaId == aula.Id).ToList();
                _context.Inscricoes.RemoveRange(inscricoes);
            }

            _context.Aulas.RemoveRange(aulas);

            int utilizadorId = treinador.UtilizadorId;
            _context.Treinadores.Remove(treinador);

            var utilizador = _context.Utilizadores.Find(utilizadorId);
            if (utilizador != null)
            {
                _context.Utilizadores.Remove(utilizador);
            }

            _context.SaveChanges();

            TempData["Mensagem"] = "Treinador apagado com sucesso.";
            return RedirectToAction("Index");
        }
    }
}
