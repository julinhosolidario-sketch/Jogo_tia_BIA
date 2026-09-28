using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;

namespace JogoTiaBiaVisual
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new JogoForm());
        }
    }

    public class JogoForm : Form
    {
        // ============================================================
        // VARIÁVEIS DO JOGO
        // ============================================================
        private int faseAtual = 0;
        private int pontos = 0;
        private int dinheiro = 2000;
        private bool jogoTerminado = false;

        private const string CaminhoSave = "save_jogo.json";

        // ============================================================
        // ELEMENTOS DA INTERFACE (UI)
        // ============================================================
        private Label lblTitulo;
        private Label lblHistoria;
        private Button btnOpcao1;
        private Button btnOpcao2;
        private Button btnOpcao3;

        private Label lblAvatarTia;
        private Label lblAvatarJoao;

        private Panel panelMenu;
        private Button btnMenuIniciar;
        private Button btnMenuContinuar;
        private Button btnMenuInstrucoes;
        private Button btnMenuSair;

        // ============================================================
        // ESTRUTURA DE FASES
        // ============================================================
        private struct Fase
        {
            public string Titulo;
            public string Pergunta;
            public string[] Opcoes;
            public string[] Respostas;
            public int[] Pontos;
            public int[] Gastos;
        }

        private Fase[] fases;

        public JogoForm()
        {
            ConfigurarJanela();
            InicializarFases();
            MostrarMenuPrincipal();
        }

        private void ConfigurarJanela()
        {
            this.Text = "O Jogo do João e da Tia Bia";
            this.Size = new Size(800, 550);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(44, 62, 80); // Cor de fundo moderna

            // Título da Fase / Jogo
            lblTitulo = new Label
            {
                Location = new Point(30, 20),
                Size = new Size(720, 35),
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(241, 196, 15)
            };
            this.Controls.Add(lblTitulo);

            // Avatar Tia Bia (Esquerda)
            lblAvatarTia = new Label
            {
                Text = "👵🏽\nTia Bia",
                Location = new Point(30, 80),
                Size = new Size(90, 120),
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter
            };
            this.Controls.Add(lblAvatarTia);

            // Caixa de Texto / Diálogo (História)
            lblHistoria = new Label
            {
                Location = new Point(140, 80),
                Size = new Size(480, 140),
                Font = new Font("Segoe UI", 11, FontStyle.Regular),
                ForeColor = Color.FromArgb(44, 62, 80),
                BackColor = Color.FromArgb(236, 240, 241),
                Padding = new Padding(10)
            };
            this.Controls.Add(lblHistoria);

            // Avatar João (Direita)
            lblAvatarJoao = new Label
            {
                Text = "👦🏽\nJoão",
                Location = new Point(640, 80),
                Size = new Size(90, 120),
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter
            };
            this.Controls.Add(lblAvatarJoao);

            // Botões de Opções
            btnOpcao1 = CriarBotaoOpcao(new Point(30, 250), 0);
            btnOpcao2 = CriarBotaoOpcao(new Point(30, 320), 1);
            btnOpcao3 = CriarBotaoOpcao(new Point(30, 390), 2);

            // Painel do Menu Principal
            panelMenu = new Panel
            {
                Location = new Point(200, 80),
                Size = new Size(400, 360),
                BackColor = Color.FromArgb(52, 73, 94)
            };

            Label lblMenuTitulo = new Label
            {
                Text = "O JOGO DA TIA BIA",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(241, 196, 15),
                Location = new Point(50, 30),
                Size = new Size(300, 30),
                TextAlign = ContentAlignment.MiddleCenter
            };
            panelMenu.Controls.Add(lblMenuTitulo);

            btnMenuIniciar = CriarBotaoMenu("Iniciar o Jogo", 80, OnIniciarClicked);
            btnMenuContinuar = CriarBotaoMenu("Continuar Jogo", 140, OnContinuarClicked);
            btnMenuInstrucoes = CriarBotaoMenu("Instruções", 200, OnInstrucoesClicked);
            btnMenuSair = CriarBotaoMenu("Sair", 260, (s, e) => Application.Exit());

            panelMenu.Controls.Add(btnMenuIniciar);
            panelMenu.Controls.Add(btnMenuContinuar);
            panelMenu.Controls.Add(btnMenuInstrucoes);
            panelMenu.Controls.Add(btnMenuSair);

            this.Controls.Add(panelMenu);
        }

        private Button CriarBotaoOpcao(Point local, int indice)
        {
            Button btn = new Button
            {
                Location = local,
                Size = new Size(720, 50),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Color.FromArgb(236, 240, 241),
                ForeColor = Color.FromArgb(44, 62, 80),
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleLeft,
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.Click += (s, e) => EscolherOpcao(indice);
            this.Controls.Add(btn);
            return btn;
        }

        private Button CriarBotaoMenu(string texto, int posY, EventHandler acao)
        {
            Button btn = new Button
            {
                Text = texto,
                Location = new Point(50, posY),
                Size = new Size(300, 45),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.Click += acao;
            return btn;
        }

        private void InicializarFases()
        {
            fases = new Fase[]
            {
                new Fase {
                    Titulo = "FASE 1 — CASAMENTO",
                    Pergunta = "Tia Bia pergunta:\n\n'Meu filho, por que adiaste o casamento?'",
                    Opcoes = new string[] { "1 — Não tenho dinheiro suficiente.", "2 — Preciso organizar melhor os gastos.", "3 — Estou a pensar em desistir." },
                    Respostas = new string[] { "O dinheiro é importante, mas também precisas de um plano.", "Muito bem! Reconhecer o problema e procurar uma solução demonstra inteligência emocional.", "Não desistas. As dificuldades podem ser enfrentadas com calma, coragem e organização." },
                    Pontos = new int[] { 5, 10, -5 },
                    Gastos = new int[] { 0, 0, 0 }
                },
                new Fase {
                    Titulo = "FASE 2 — A VARANDA",
                    Pergunta = "Tia Bia também sonhava construir uma varanda. Planeou e trabalhou até conseguir.\n\nO que João deve aprender?",
                    Opcoes = new string[] { "1 — Desistir quando algo é difícil.", "2 — Definir um objetivo e planear.", "3 — Gastar dinheiro sem pensar." },
                    Respostas = new string[] { "Desistir não ajuda a realizar o objetivo.", "Excelente! Ter paciência, definir objetivos e planear são decisões inteligentes.", "Antes de gastar, faz uma pausa, controla o impulso e pensa nas consequências." },
                    Pontos = new int[] { -5, 15, -10 },
                    Gastos = new int[] { 0, 0, 200 }
                },
                new Fase {
                    Titulo = "FASE 3 — CASA DE BANHO",
                    Pergunta = "Tia Bia quer comprar uma sanita e um autoclismo.\n\nQual seria a melhor decisão?",
                    Opcoes = new string[] { "1 — Pesquisar e comparar preços.", "2 — Comprar sem verificar o preço.", "3 — Desistir da ideia." },
                    Respostas = new string[] { "Boa decisão! Pesquisar ajuda a criar o orçamento.", "Não decidas por impulso. Respira, compara os preços e escolhe com consciência.", "Desistir não resolve o problema. Podes procurar outras soluções." },
                    Pontos = new int[] { 15, -5, -5 },
                    Gastos = new int[] { 0, 300, 0 }
                },
                new Fase {
                    Titulo = "FASE 4 — ORÇAMENTO",
                    Pergunta = "Escolhe o tipo de casamento:",
                    Opcoes = new string[] { "1 — Casamento simples: 800 euros.", "2 — Casamento médio: 1500 euros.", "3 — Casamento luxuoso: 3000 euros." },
                    Respostas = new string[] { "Escolheste um casamento simples e adequado.", "Escolheste um casamento médio. Mantém o equilíbrio entre os teus desejos e a tua realidade.", "O casamento ultrapassou o orçamento. Não precisas de impressionar os outros para seres feliz." },
                    Pontos = new int[] { 20, 10, -15 },
                    Gastos = new int[] { 800, 1500, 3000 }
                },
                new Fase {
                    Titulo = "FASE 5 — LIÇÃO FINAL",
                    Pergunta = "Qual é a principal aprendizagem da história?",
                    Opcoes = new string[] { "1 — Basta sonhar.", "2 — Só o dinheiro importa.", "3 — Sonhar, planear e agir." },
                    Respostas = new string[] { "Sonhar é importante, mas também precisamos de agir.", "O dinheiro é importante, mas o equilíbrio emocional também orienta as nossas decisões.", "Muito bem! Sonhar, planear e agir caminham juntos. Isso é inteligência emocional." },
                    Pontos = new int[] { -5, -5, 20 },
                    Gastos = new int[] { 0, 0, 0 }
                }
            };
        }

        private void MostrarMenuPrincipal()
        {
            panelMenu.Visible = true;
            lblTitulo.Visible = false;
            lblHistoria.Visible = false;
            lblAvatarTia.Visible = false;
            lblAvatarJoao.Visible = false;
            btnOpcao1.Visible = false;
            btnOpcao2.Visible = false;
            btnOpcao3.Visible = false;

            btnMenuContinuar.Enabled = File.Exists(CaminhoSave);
            btnMenuContinuar.BackColor = btnMenuContinuar.Enabled ? Color.FromArgb(46, 204, 113) : Color.Gray;
        }

        private void IniciarInterfaceJogo()
        {
            panelMenu.Visible = false;
            lblTitulo.Visible = true;
            lblHistoria.Visible = true;
            lblAvatarTia.Visible = true;
            lblAvatarJoao.Visible = true;
            btnOpcao1.Visible = true;
            btnOpcao2.Visible = true;
            btnOpcao3.Visible = true;

            MostrarFaseAtual();
        }

        private void OnIniciarClicked(object sender, EventArgs e)
        {
            if (File.Exists(CaminhoSave)) File.Delete(CaminhoSave);
            faseAtual = 0;
            pontos = 0;
            dinheiro = 2000;
            jogoTerminado = false;
            IniciarInterfaceJogo();
        }

        private void OnContinuarClicked(object sender, EventArgs e)
        {
            if (CarregarJogo())
            {
                IniciarInterfaceJogo();
            }
        }

        private void OnInstrucoesClicked(object sender, EventArgs e)
        {
            MessageBox.Show("INSTRUÇÕES DO JOGO:\n\n- Acompanha o João e a Tia Bia.\n- Toma decisões inteligentes baseadas em planeamento e inteligência emocional.\n- Gere o teu dinheiro e pontuação para alcançar o sucesso financeiro!", "Instruções do Jogo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void MostrarFaseAtual(string mensagemAnterior = "")
        {
            if (faseAtual >= fases.Length)
            {
                MostrarResultadoFinal();
                return;
            }

            Fase fase = fases[faseAtual];
            lblTitulo.Text = $"{fase.Titulo}   |   Pontos: {pontos}   |   Dinheiro: {dinheiro} €";
            lblHistoria.Text = (mensagemAnterior != "" ? mensagemAnterior + "\n\n" : "") + fase.Pergunta;

            btnOpcao1.Text = fase.Opcoes[0];
            btnOpcao2.Text = fase.Opcoes[1];
            btnOpcao3.Text = fase.Opcoes[2];
            btnOpcao3.Visible = true;
        }

        private void EscolherOpcao(int indice)
        {
            if (jogoTerminado) return;

            Fase fase = fases[faseAtual];
            string resposta = fase.Respostas[indice];

            pontos += fase.Pontos[indice];
            dinheiro -= fase.Gastos[indice];
            faseAtual++;

            GuardarJogo();

            if (faseAtual < fases.Length)
            {
                MostrarFaseAtual(resposta);
            }
            else
            {
                MostrarResultadoFinal(resposta);
            }
        }

        private void MostrarResultadoFinal(string ultimaResposta = "")
        {
            jogoTerminado = true;
            string resultado = pontos >= 70 && dinheiro >= 0 ? "EXCELENTE PLANIFICADOR!" : (pontos >= 40 && dinheiro >= 0 ? "BOM PLANIFICADOR!" : "PRECISAS DE MELHORAR A PLANIFICAÇÃO.");

            lblTitulo.Text = "RESULTADO FINAL";
            lblHistoria.Text = $"{ultimaResposta}\n\n{resultado}\n\nPontuação Final: {pontos} pontos | Dinheiro: {dinheiro} €\n\nO jogo terminou!";

            btnOpcao1.Text = "JOGAR NOVAMENTE";
            btnOpcao2.Text = "VOLTAR AO MENU PRINCIPAL";
            btnOpcao3.Visible = false;

            btnOpcao1.Click -= null;
            btnOpcao1.Click = (s, e) => OnIniciarClicked(s, e);

            btnOpcao2.Click -= null;
            btnOpcao2.Click = (s, e) => {
                jogoTerminado = false;
                MostrarMenuPrincipal();
            };

            if (File.Exists(CaminhoSave)) File.Delete(CaminhoSave);
        }

        private void GuardarJogo()
        {
            var dados = new Dictionary<string, int> { { "fase_atual", faseAtual }, { "pontos", pontos }, { "dinheiro", dinheiro } };
            File.WriteAllText(CaminhoSave, JsonSerializer.Serialize(dados));
        }

        private bool CarregarJogo()
        {
            if (!File.Exists(CaminhoSave)) return false;
            try
            {
                var dados = JsonSerializer.Deserialize<Dictionary<string, int>>(File.ReadAllText(CaminhoSave));
                if (dados != null)
                {
                    faseAtual = dados["fase_atual"];
                    pontos = dados["pontos"];
                    dinheiro = dados["dinheiro"];
                    jogoTerminado = false;
                    return true;
                }
            }
            catch { return false; }
            return false;
        }
    }
}