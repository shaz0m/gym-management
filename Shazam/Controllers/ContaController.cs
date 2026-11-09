using Shazam.Data;
using Microsoft.AspNetCore.Mvc;

namespace Shazam.Controllers
{
    // Controller responsável pelo login, logout e mudança de password.
    public class ContaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ContaController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Conta/Login
        public IActionResult Login()
        {
            // Se já está logado, vai direto para o painel.
            if (HttpContext.Session.GetString("UtilizadorId") != null)
            {
                return RedirecionarParaPainel(HttpContext.Session.GetString("Tipo"));
            }

            return View();
        }

        // POST: /Conta/Login
        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            var utilizador = _context.Utilizadores
                .FirstOrDefault(u => u.Username == username && u.Password == password);

            if (utilizador == null)
            {
                ViewBag.Erro = "Username ou password errados.";
                return View();
            }

            // Guarda na sessão os dados de quem fez login.
            HttpContext.Session.SetString("UtilizadorId", utilizador.Id.ToString());
            HttpContext.Session.SetString("Nome", utilizador.Nome);
            HttpContext.Session.SetString("Tipo", utilizador.Tipo);

            // No primeiro login o utilizador tem de mudar a password.
            if (utilizador.PrimeiroLogin)
            {
                return RedirectToAction("AlterarSenha");
            }

            return RedirecionarParaPainel(utilizador.Tipo);
        }

        // GET: /Conta/AlterarSenha
        public IActionResult AlterarSenha()
        {
            if (HttpContext.Session.GetString("UtilizadorId") == null)
            {
                return RedirectToAction("Login");
            }

            return View();
        }

        // POST: /Conta/AlterarSenha
        [HttpPost]
        public IActionResult AlterarSenha(string novaPassword, string confirmarPassword)
        {
            string idTexto = HttpContext.Session.GetString("UtilizadorId");
            if (idTexto == null)
            {
                return RedirectToAction("Login");
            }

            if (string.IsNullOrEmpty(novaPassword) || novaPassword.Length < 4)
            {
                ViewBag.Erro = "A password tem de ter pelo menos 4 caracteres.";
                return View();
            }

            if (novaPassword != confirmarPassword)
            {
                ViewBag.Erro = "As passwords não são iguais.";
                return View();
            }

            int id = int.Parse(idTexto);
            var utilizador = _context.Utilizadores.Find(id);

            if (utilizador == null)
            {
                return RedirectToAction("Login");
            }

            utilizador.Password = novaPassword;
            utilizador.PrimeiroLogin = false;
            _context.SaveChanges();

            TempData["Mensagem"] = "Password alterada com sucesso.";
            return RedirecionarParaPainel(utilizador.Tipo);
        }

        // GET: /Conta/Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        // Método auxiliar: decide para onde vai cada tipo de utilizador depois do login.
        private IActionResult RedirecionarParaPainel(string tipo)
        {
            if (tipo == "Admin")
            {
                return RedirectToAction("Index", "Home");
            }

            if (tipo == "Treinador")
            {
                return RedirectToAction("Treinador", "Painel");
            }

            return RedirectToAction("Cliente", "Painel");
        }
    }
}
