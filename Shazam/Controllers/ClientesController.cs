using Shazam.Data;
using Shazam.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Shazam.Controllers
{
    // CRUD de clientes. Só o Admin e os Treinadores podem gerir clientes.
    public class ClientesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ClientesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Método auxiliar: verifica se quem está logado pode gerir clientes.
        private bool NaoTemPermissao()
        {
            string tipo = HttpContext.Session.GetString("Tipo");
            return tipo != "Admin" && tipo != "Treinador";
        }

        // GET: /Clientes
        public IActionResult Index()
        {
            if (NaoTemPermissao())
            {
                return RedirectToAction("Login", "Conta");
            }

            var clientes = _context.Clientes
                .Include(c => c.Utilizador)
                .Include(c => c.Inscricoes)
                .ToList();

            return View(clientes);
        }

        // GET: /Clientes/Details/5
        public IActionResult Details(int id)
        {
            if (NaoTemPermissao())
            {
                return RedirectToAction("Login", "Conta");
            }

            var cliente = _context.Clientes
                .Include(c => c.Utilizador)
                .Include(c => c.Inscricoes)
                .ThenInclude(i => i.Aula)
                .FirstOrDefault(c => c.Id == id);

            if (cliente == null)
            {
                return NotFound();
            }

            return View(cliente);
        }

        // GET: /Clientes/Create
        public IActionResult Create()
        {
            if (NaoTemPermissao())
            {
                return RedirectToAction("Login", "Conta");
            }

            return View();
        }

        // POST: /Clientes/Create
        // Cria o utilizador (para o login) e o cliente ao mesmo tempo.
        [HttpPost]
        public IActionResult Create(string nome, string email, string telefone,
            DateTime dataNascimento, string username, string password)
        {
            if (NaoTemPermissao())
            {
                return RedirectToAction("Login", "Conta");
            }

            if (string.IsNullOrEmpty(nome) || string.IsNullOrEmpty(email) ||
                string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ViewBag.Erro = "Preencha todos os campos obrigatórios.";
                return View();
            }

            // Verifica se o username já existe.
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
            utilizador.Tipo = "Cliente";
            utilizador.PrimeiroLogin = true;

            _context.Utilizadores.Add(utilizador);
            _context.SaveChanges();

            var cliente = new Cliente();
            cliente.UtilizadorId = utilizador.Id;
            cliente.Nome = nome;
            cliente.Email = email;
            cliente.Telefone = telefone;
            cliente.DataNascimento = dataNascimento;

            _context.Clientes.Add(cliente);
            _context.SaveChanges();

            TempData["Mensagem"] = "Cliente criado com sucesso.";
            return RedirectToAction("Index");
        }

        // GET: /Clientes/Edit/5
        public IActionResult Edit(int id)
        {
            if (NaoTemPermissao())
            {
                return RedirectToAction("Login", "Conta");
            }

            var cliente = _context.Clientes.Find(id);
            if (cliente == null)
            {
                return NotFound();
            }

            return View(cliente);
        }

        // POST: /Clientes/Edit/5
        [HttpPost]
        public IActionResult Edit(int id, string nome, string email, string telefone,
            DateTime dataNascimento)
        {
            if (NaoTemPermissao())
            {
                return RedirectToAction("Login", "Conta");
            }

            var cliente = _context.Clientes.Find(id);
            if (cliente == null)
            {
                return NotFound();
            }

            if (string.IsNullOrEmpty(nome) || string.IsNullOrEmpty(email))
            {
                ViewBag.Erro = "O nome e o email são obrigatórios.";
                return View(cliente);
            }

            cliente.Nome = nome;
            cliente.Email = email;
            cliente.Telefone = telefone;
            cliente.DataNascimento = dataNascimento;

            // Atualiza também o nome no utilizador ligado a este cliente.
            var utilizador = _context.Utilizadores.Find(cliente.UtilizadorId);
            if (utilizador != null)
            {
                utilizador.Nome = nome;
            }

            _context.SaveChanges();

            TempData["Mensagem"] = "Cliente atualizado com sucesso.";
            return RedirectToAction("Index");
        }

        // GET: /Clientes/Delete/5
        public IActionResult Delete(int id)
        {
            if (NaoTemPermissao())
            {
                return RedirectToAction("Login", "Conta");
            }

            var cliente = _context.Clientes
                .Include(c => c.Utilizador)
                .FirstOrDefault(c => c.Id == id);

            if (cliente == null)
            {
                return NotFound();
            }

            return View(cliente);
        }

        // POST: /Clientes/Delete/5
        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            if (NaoTemPermissao())
            {
                return RedirectToAction("Login", "Conta");
            }

            var cliente = _context.Clientes.Find(id);
            if (cliente == null)
            {
                return RedirectToAction("Index");
            }

            // Apaga primeiro as inscrições do cliente.
            var inscricoes = _context.Inscricoes.Where(i => i.ClienteId == cliente.Id).ToList();
            _context.Inscricoes.RemoveRange(inscricoes);

            // Guarda o id do utilizador antes de apagar o cliente.
            int utilizadorId = cliente.UtilizadorId;

            _context.Clientes.Remove(cliente);

            // Apaga também o utilizador (o login) deste cliente.
            var utilizador = _context.Utilizadores.Find(utilizadorId);
            if (utilizador != null)
            {
                _context.Utilizadores.Remove(utilizador);
            }

            _context.SaveChanges();

            TempData["Mensagem"] = "Cliente apagado com sucesso.";
            return RedirectToAction("Index");
        }
    }
}
