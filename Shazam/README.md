# SHAZAM — Gestão de Ginásio (Projeto Final APDZ0125_15)

Design monocromático (preto e branco). Projeto: **Shazam**.

Aplicação web ASP.NET Core MVC + Entity Framework Core (SQL Server) para gestão
de um ginásio: utilizadores com login, clientes, treinadores, aulas e inscrições.

## Como correr

1. Correr o projeto (F5 no Visual Studio ou `dotnet run`).
2. A aplicação tenta ligar ao servidor da escola ("Lagostim" no `appsettings.json`).
   Se não conseguir, usa automaticamente a base de dados local do Visual Studio
   (LocalDB, connection string "Local").
3. Na primeira execução, as tabelas são criadas automaticamente e é criado
   um administrador:
   - Username: `admin`
   - Password: `admin123`
   (no primeiro login é pedido para mudar a password)

## Ligação ao servidor do curso

A password do servidor do curso **não está no repositório**. No `appsettings.json` a ligação
`Lagostim` tem o marcador `DEFINIR_NOS_USER_SECRETS`, e a password verdadeira guarda-se nos
"user secrets" (um ficheiro privado do utilizador, fora do projeto). Em cada computador, na pasta
do projeto (a que tem o `Shazam.csproj`), executar uma vez:

```bash
dotnet user-secrets set "ConnectionStrings:Lagostim" "Server=...;Database=...;User Id=...;Password=...;TrustServerCertificate=True;Connect Timeout=5;"
```

Os segredos só são lidos quando a aplicação corre em modo de desenvolvimento (o perfil do Visual Studio
já o faz). Se a ligação não estiver definida, ou o servidor não responder, a aplicação usa a base
local (LocalDB) e escreve um aviso na janela de saída. O administrador vê no rodapé qual a base em uso.

## Tipos de utilizador

- **Admin** — gere clientes, treinadores, aulas e inscrições.
- **Treinador** — tem um painel com as suas aulas, pode criar/editar aulas,
  ver clientes e gerir inscrições.
- **Cliente** — tem um painel onde vê as aulas disponíveis, inscreve-se
  (se houver vagas) e cancela inscrições.

## Estrutura

```
Models/        Utilizador, Cliente, Treinador, Aula, Inscricao
Data/          ApplicationDbContext (ligação à base de dados)
Controllers/   Conta (login), Home (painel admin), Painel (cliente/treinador),
               Clientes, Treinadores, Aulas, Inscricoes
Views/         Páginas Razor de cada controller
```

## Regras de negócio implementadas

- Login por sessão; no primeiro login é obrigatório mudar a password.
- Um cliente não se pode inscrever duas vezes na mesma aula.
- Não é possível inscrever numa aula lotada (inscritos >= capacidade).
- Ao apagar cliente/treinador/aula, os registos dependentes são apagados primeiro.

## Nota sobre o arranque

No `Program.cs`, para além do `EnsureCreated()`, há uma verificação extra:
se a base de dados já existir mas sem as tabelas deste projeto (por exemplo,
com tabelas antigas de outra versão), as tabelas são criadas na mesma.
Todo o arranque da base de dados está num try/catch para a aplicação
nunca falhar a arrancar por causa da base de dados.

## Dados de demonstração

Com `"DadosDemonstracao": true` no `appsettings.json`, a aplicação preenche a base de dados
ao arrancar (`Data/DadosDemonstracao.cs`). Os dados são fictícios:

- 6 treinadores e 30 clientes (só são criados os que ainda não existem);
- um horário semanal (21 aulas por semana) com as aulas dos últimos 7 dias e dos próximos 14;
  em cada arranque só se criam as aulas que faltam, por isso há sempre aulas nos dias seguintes;
- inscrições ao acaso nas aulas novas (algumas ficam lotadas).

Contas de demonstração (password de todas: `demo1234`):

| Perfil | Username |
|---|---|
| Cliente | `mariana.lopes` (e os outros clientes: `nome.apelido`, sem acentos) |
| Treinador | `tiago.almeida`, `rita.fonseca`, `ines.carvalho`, `bruno.teixeira`, `sofia.matos`, `ricardo.nunes` |

O administrador (`admin`) não é alterado. Para deixar de criar dados, mudar para
`"DadosDemonstracao": false`.

## Design

Sistema visual preto e branco, definido em `wwwroot/css/site.css` por cima do Bootstrap 5.3
(variáveis no início do ficheiro: cinzentos, raios, sombras, fontes).

- **Fontes** alojadas na aplicação (`wwwroot/fonts`, licença SIL OFL incluída):
  Geist para o texto, Chakra Petch para títulos e números (cantos cortados, como o logótipo).
- **Ícones** desenhados para o projeto, num único ficheiro: `Views/Shared/_Icones.cshtml`.
  Uso: `<svg class="ico"><use href="#i-aulas"></use></svg>`.
- **Layouts:** `_Layout` (aplicação, com menu que no telemóvel abre num botão) e
  `_LayoutAcesso` (página de login, ecrã dividido).
- **Helpers/Ui.cs:** iniciais dos avatares, datas em português, estado das aulas
  (Disponível, Lotada, A decorrer, Terminada) e percentagens de ocupação.
- No telemóvel as tabelas passam a blocos com etiquetas (atributo `data-label` em cada célula).
- As listas têm pesquisa sem recarregar a página (`data-filtro` em `wwwroot/js/site.js`).
- Os estados distinguem-se pela forma e pelo preenchimento, não pela cor.

## Nota de segurança

Num projeto real a password devia ser guardada com hash (ex: BCrypt) e não em
texto simples — aqui ficou simples por ser um projeto de aprendizagem.
