namespace HelpMind
{
    public partial class FormTecnico : Form
    {
        private readonly string _connectionString = "Server=DESKTOP-6H5PBKR\\SQLVICTOR;Database=GestaoIA;Integrated Security=True;TrustServerCertificate=True;";

        public FormTecnico()
        {
            InitializeComponent();
            PopularStatusComboBox();
            CarregarChamados();
        }

        private void CarregarChamados()
        {
            try
            {
                using (SqlConnection conexao = new SqlConnection(_connectionString))
                {
                    string query = "SELECT ID, Nome, Email, Descricao, Status FROM Chamados";
                    SqlDataAdapter adapter = new SqlDataAdapter(query, conexao);
                    DataTable dtChamados = new DataTable();
                    adapter.Fill(dtChamados);
                    dataGridViewChamados.DataSource = dtChamados;
                }
            }
            catch (Exception ex)
            {
                ExibirErro("Erro ao carregar chamados", ex);
            }
        }

        private void btnAlterarStatus_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(txtIdChamado.Text, out int idChamado))
            {
                ExibirAviso("Informe um ID de chamado válido!");
                return;
            }

            if (comboBoxStatus.SelectedItem == null)
            {
                ExibirAviso("Selecione um status!");
                return;
            }

            string novoStatus = comboBoxStatus.SelectedItem.ToString();
            AtualizarStatusChamado(idChamado, novoStatus);
        }

        private void AtualizarStatusChamado(int idChamado, string novoStatus)
        {
            try
            {
                using (SqlConnection conexao = new SqlConnection(_connectionString))
                {
                    conexao.Open();

                    string verificarQuery = "SELECT COUNT(*) FROM Chamados WHERE ID = @ID";
                    using (SqlCommand cmdVerificar = new SqlCommand(verificarQuery, conexao))
                    {
                        cmdVerificar.Parameters.AddWithValue("@ID", idChamado);
                        int existe = (int)cmdVerificar.ExecuteScalar();

                        if (existe == 0)
                        {
                            ExibirAviso("Chamado não encontrado. Verifique o ID.");
                            return;
                        }
                    }

                    string atualizarQuery = "UPDATE Chamados SET Status = @Status WHERE ID = @ID";
                    using (SqlCommand cmdAtualizar = new SqlCommand(atualizarQuery, conexao))
                    {
                        cmdAtualizar.Parameters.AddWithValue("@Status", novoStatus);
                        cmdAtualizar.Parameters.AddWithValue("@ID", idChamado);
                        cmdAtualizar.ExecuteNonQuery();
                    }
                }

                ExibirMensagem("Status atualizado com sucesso!");
                CarregarChamados();
            }
            catch (Exception ex)
            {
                ExibirErro("Erro ao atualizar status", ex);
            }
        }

        private void btnResponder_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(txtIdChamado.Text, out int idChamado))
            {
                ExibirAviso("Selecione um chamado válido!");
                return;
            }

            string resposta = txtResposta.Text.Trim();
            if (string.IsNullOrEmpty(resposta))
            {
                ExibirAviso("Digite a resposta antes de enviar!");
                return;
            }

            string emailDestino = BuscarEmailPorIdChamado(idChamado);
            if (!string.IsNullOrEmpty(emailDestino))
            {
                SalvarResposta(idChamado, resposta);
                EnviarEmail(emailDestino, resposta);
                CarregarChamados();
            }
            else
            {
                ExibirErro("Erro ao buscar o e-mail do solicitante.");
            }
        }

        private void SalvarResposta(int idChamado, string resposta)
        {
            try
            {
                using (SqlConnection conexao = new SqlConnection(_connectionString))
                {
                    string query = "UPDATE Chamados SET Resposta = @Resposta WHERE ID = @ID";
                    using (SqlCommand cmd = new SqlCommand(query, conexao))
                    {
                        cmd.Parameters.AddWithValue("@Resposta", resposta);
                        cmd.Parameters.AddWithValue("@ID", idChamado);
                        conexao.Open();

                        int linhasAfetadas = cmd.ExecuteNonQuery();
                        if (linhasAfetadas > 0)
                            ExibirMensagem("Resposta salva com sucesso!");
                        else
                            ExibirErro("Erro ao salvar a resposta. Verifique o ID do chamado.");
                    }
                }
            }
            catch (Exception ex)
            {
                ExibirErro("Erro ao salvar resposta", ex);
            }
        }

        private string BuscarEmailPorIdChamado(int idChamado)
        {
            try
            {
                using (SqlConnection conexao = new SqlConnection(_connectionString))
                {
                    string query = "SELECT Email FROM Chamados WHERE ID = @ID";
                    using (SqlCommand cmd = new SqlCommand(query, conexao))
                    {
                        cmd.Parameters.AddWithValue("@ID", idChamado);
                        conexao.Open();
                        object resultado = cmd.ExecuteScalar();
                        return resultado?.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                ExibirErro("Erro ao buscar e-mail", ex);
                return null;
            }
        }

        private void EnviarEmail(string destino, string corpo)
        {
            try
            {
                MailMessage mail = new MailMessage("victorhforworking@gmail.com", destino)
                {
                    Subject = "Resposta ao seu chamado de suporte",
                    Body = $"Olá,\n\nA equipe de suporte respondeu ao seu chamado:\n\n{corpo}\n\nAtenciosamente,\nEquipe de Suporte",
                    IsBodyHtml = false
                };

                using (SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587))
                {
                    smtp.Credentials = new NetworkCredential("victorhforworking@gmail.com", "aapf njfk tehy qcmz");
                    smtp.EnableSsl = true;
                    smtp.Send(mail);
                }

                ExibirMensagem("E-mail enviado com sucesso!");
            }
            catch (Exception ex)
            {
                ExibirErro("Erro ao enviar e-mail", ex);
            }
        }

        private void PopularStatusComboBox()
        {
            comboBoxStatus.Items.Clear();
            comboBoxStatus.Items.AddRange(new[] { "Aberto", "Em Andamento", "Concluído" });
            comboBoxStatus.SelectedIndex = 0;
        }

        private void dataGridViewChamados_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                txtIdChamado.Text = dataGridViewChamados.Rows[e.RowIndex].Cells["ID"].Value.ToString();
            }
        }

        private void btnVoltarMenu_Click(object sender, EventArgs e)
        {
            FormLogin formLogin = Application.OpenForms.OfType<FormLogin>().FirstOrDefault();
            formLogin?.Show();
            Close();
        }

        // Utilitários
        private void ExibirMensagem(string mensagem) => MessageBox.Show(mensagem, "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
        private void ExibirAviso(string mensagem) => MessageBox.Show(mensagem, "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        private void ExibirErro(string mensagem, Exception ex = null) => MessageBox.Show($"{mensagem}: {ex?.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}
