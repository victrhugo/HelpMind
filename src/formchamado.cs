using System;
using System.Data.SqlClient;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Configuration;

namespace GestaoIA
{
    public partial class FormChamado : Form
    {
        private readonly string conexaoString = "Server=DESKTOP-6H5PBKR\\SQLVICTOR;Database=GestaoIA;Integrated Security=True;TrustServerCertificate=True;";
        private string caminhoArquivo = "";

        public FormChamado()
        {
            InitializeComponent();
        }

        private void FormChamado_Load(object sender, EventArgs e)
        {
            CarregarStatusChamado();
        }

        private void btnEnviar_Click(object sender, EventArgs e)
        {
            string descricao = txtDescricao.Text.Trim();
            string nome = txtNome.Text.Trim();
            string email = txtEmail.Text.Trim();

            if (string.IsNullOrWhiteSpace(descricao) || string.IsNullOrWhiteSpace(nome) || string.IsNullOrWhiteSpace(email))
            {
                MessageBox.Show("Por favor, preencha todos os campos!", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!ValidarEmail(email))
            {
                MessageBox.Show("E-mail inválido!", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            int idChamado = SalvarChamadoNoBanco(descricao, nome, email);

            if (!string.IsNullOrEmpty(caminhoArquivo) && File.Exists(caminhoArquivo) && idChamado > 0)
            {
                SalvarArquivoNoBanco(idChamado, caminhoArquivo);
            }

            if (idChamado > 0)
            {
                MessageBox.Show($"Chamado registrado com sucesso!\nID do chamado: {idChamado}", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);

                string apiKey = ConfigurationManager.AppSettings["OpenAI_API_Key"];
                if (string.IsNullOrWhiteSpace(apiKey))
                {
                    MessageBox.Show("Erro: Chave da API não pode estar vazia!", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                OpenAIService openAI = new OpenAIService(apiKey);
                lblResposta.Text = "Gerando resposta da IA...";

                try
                {
                    string respostaIA = openAI.ObterRespostaAsync(descricao).Result;
                    lblResposta.Text = respostaIA;
                    SalvarRespostaIA(idChamado, respostaIA);
                }
                catch (Exception ex)
                {
                    lblResposta.Text = "Erro ao obter resposta da IA!";
                    MessageBox.Show("Erro ao chamar a IA: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private int SalvarChamadoNoBanco(string descricao, string nome, string email)
        {
            int idChamado = -1;

            try
            {
                using (SqlConnection conexao = new SqlConnection(conexaoString))
                {
                    string query = "INSERT INTO Chamados (Descricao, Nome, Email) OUTPUT INSERTED.ID VALUES (@Descricao, @Nome, @Email)";
                    using (SqlCommand cmd = new SqlCommand(query, conexao))
                    {
                        cmd.Parameters.AddWithValue("@Descricao", descricao);
                        cmd.Parameters.AddWithValue("@Nome", nome);
                        cmd.Parameters.AddWithValue("@Email", email);

                        conexao.Open();
                        object result = cmd.ExecuteScalar();
                        if (result != null)
                            idChamado = Convert.ToInt32(result);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao salvar chamado: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return idChamado;
        }

        private void SalvarArquivoNoBanco(int idChamado, string caminhoArquivo)
        {
            try
            {
                byte[] arquivoBytes = File.ReadAllBytes(caminhoArquivo);
                string nomeArquivo = Path.GetFileName(caminhoArquivo);
                string tipoArquivo = Path.GetExtension(caminhoArquivo);

                using (SqlConnection conexao = new SqlConnection(conexaoString))
                {
                    string query = "INSERT INTO AnexosChamado (IDChamado, NomeArquivo, TipoArquivo, Dados) VALUES (@IDChamado, @NomeArquivo, @TipoArquivo, @Dados)";
                    using (SqlCommand cmd = new SqlCommand(query, conexao))
                    {
                        cmd.Parameters.AddWithValue("@IDChamado", idChamado);
                        cmd.Parameters.AddWithValue("@NomeArquivo", nomeArquivo);
                        cmd.Parameters.AddWithValue("@TipoArquivo", tipoArquivo);
                        cmd.Parameters.AddWithValue("@Dados", arquivoBytes);

                        conexao.Open();
                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Arquivo anexado com sucesso!", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao anexar arquivo: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SalvarRespostaIA(int idChamado, string resposta)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(conexaoString))
                {
                    string query = "UPDATE Chamados SET RespostaIA = @Resposta WHERE Id = @IdChamado";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Resposta", resposta);
                        cmd.Parameters.AddWithValue("@IdChamado", idChamado);

                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao salvar resposta da IA: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CarregarStatusChamado()
        {
            if (!int.TryParse(txtIdChamado.Text, out int idChamado))
                return;

            try
            {
                using (SqlConnection conexao = new SqlConnection(conexaoString))
                {
                    string query = "SELECT Status FROM Chamados WHERE ID = @IDChamado";
                    using (SqlCommand cmd = new SqlCommand(query, conexao))
                    {
                        cmd.Parameters.AddWithValue("@IDChamado", idChamado);

                        conexao.Open();
                        object resultado = cmd.ExecuteScalar();

                        if (resultado != null)
                        {
                            string status = resultado.ToString();
                            comboBoxStatus.Items.Clear();
                            comboBoxStatus.Items.Add(status);
                            comboBoxStatus.SelectedIndex = 0;
                            comboBoxStatus.Enabled = false;
                        }
                        else
                        {
                            comboBoxStatus.Items.Clear();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao carregar status: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnAnexarArquivo_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "Selecionar Arquivo",
                Filter = "Todos os Arquivos|*.*"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                caminhoArquivo = openFileDialog.FileName;
                lblArquivoSelecionado.Text = "Arquivo: " + Path.GetFileName(caminhoArquivo);
            }
        }

        private bool ValidarEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            string padraoEmail = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email, padraoEmail);
        }

        private void btnCarregarStatus_Click(object sender, EventArgs e)
        {
            CarregarStatusChamado();
        }
    }
}

