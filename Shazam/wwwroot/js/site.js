// Comportamentos simples da interface (sem bibliotecas extra).

// Fechar avisos (mensagens de sucesso/erro).
document.addEventListener("click", function (evento) {
    var botao = evento.target.closest("[data-fechar-aviso]");
    if (botao) {
        botao.closest(".aviso").remove();
    }
});

// Mostrar / esconder a password.
document.addEventListener("click", function (evento) {
    var botao = evento.target.closest("[data-mostrar-password]");
    if (!botao) {
        return;
    }

    var campo = botao.parentElement.querySelector("input");
    var mostrar = campo.type === "password";

    campo.type = mostrar ? "text" : "password";
    botao.setAttribute("aria-pressed", mostrar ? "true" : "false");
    botao.setAttribute("aria-label", mostrar ? "Esconder password" : "Mostrar password");
    botao.querySelector("use").setAttribute("href", mostrar ? "#i-ver-nao" : "#i-ver");
});

// Pesquisa nas tabelas: <input data-filtro="#id-da-tabela">
document.querySelectorAll("[data-filtro]").forEach(function (campo) {
    var tabela = document.querySelector(campo.dataset.filtro);
    if (!tabela) {
        return;
    }

    var linhas = Array.from(tabela.tBodies[0].rows);
    var colunas = tabela.tHead.rows[0].cells.length;

    var semResultados = document.createElement("tr");
    semResultados.className = "linha-sem-resultados";
    semResultados.hidden = true;
    semResultados.innerHTML = '<td colspan="' + colunas + '">Nenhum resultado para esta pesquisa.</td>';
    tabela.tBodies[0].appendChild(semResultados);

    var contador = document.querySelector(campo.dataset.contador);

    campo.addEventListener("input", function () {
        var termo = normalizar(campo.value);
        var visiveis = 0;

        linhas.forEach(function (linha) {
            var corresponde = normalizar(linha.textContent).indexOf(termo) !== -1;
            linha.hidden = !corresponde;
            if (corresponde) {
                visiveis++;
            }
        });

        semResultados.hidden = visiveis > 0;
        if (contador) {
            contador.textContent = visiveis;
        }
    });
});

// Ignora maiúsculas e acentos ("Inês" encontra-se com "ines").
function normalizar(texto) {
    return texto.toLowerCase().normalize("NFD").replace(/[̀-ͯ]/g, "");
}
