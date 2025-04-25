namespace GestaoIA
{
    public partial class FormTecnico: Form
    {

        private string conexaoString = "Server=DESKTOP-6H5PBKR\\SQLVICTOR;Database=GestaoIA;Integrated Security=True;TrustServerCertificate=True;";
        public FormTecnico()
        {
            InitializeComponent();
            CarregarChamados();
        }


        private void CarregarChamados()
        {
            try
            {
                using (SqlConnection conexao = new SqlConnection(conexaoString))
                {
                    string query = "SELECT ID, Nome, Email, Descricao, Status FROM Chamados";

                    using (SqlCommand cmd = new SqlCommand(query, conexao))
                    {
                        conexao.Open();
                        SqlDataReader reader = cmd.ExecuteReader();
                        DataTable dt = new DataTable();
                        dt.Load(reader);

                        dataGridViewChamados.DataSource = dt; 
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao carregar chamados: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AlterarStatus_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(txtIdChamado.Text, out int idChamado))
            {
                MessageBox.Show("Informe um ID de chamado válido!", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
          
            if (comboBoxStatus.SelectedItem == null)
            {
                MessageBox.Show("Selecione um status!", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string novoStatus = comboBoxStatus.SelectedItem.ToString();

            AtualizarStatusChamado(idChamado, novoStatus);
        }

        private void AtualizarStatusChamado(int idChamado, string novoStatus)
        {
            try
            {
                using (SqlConnection conexao = new SqlConnection(conexaoString))
                {
                    string verificarQuery = "SELECT COUNT(*) FROM Chamados WHERE ID = @IDChamado";

                    using (SqlCommand verificarCmd = new SqlCommand(verificarQuery, conexao))
                    {
                        verificarCmd.Parameters.AddWithValue("@IDChamado", idChamado);
                        conexao.Open();
                        int existe = (int)verificarCmd.ExecuteScalar();

                        if (existe == 0)
                        {
                            MessageBox.Show("Chamado não encontrado! Verifique o ID.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                    }
                   
                    string atualizarQuery = "UPDATE Chamados SET Status = @Status WHERE ID = @IDChamado";

                    using (SqlCommand atualizarCmd = new SqlCommand(atualizarQuery, conexao))
                    {
                        atualizarCmd.Parameters.AddWithValue("@Status", novoStatus);
                        atualizarCmd.Parameters.AddWithValue("@IDChamado", idChamado);
                        atualizarCmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Status atualizado com sucesso!", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao atualizar status: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void PopularComboBoxStatus()
        {
            comboBoxStatus.Items.Clear();
            comboBoxStatus.Items.Add("Aberto");
            comboBoxStatus.Items.Add("Em Andamento");
            comboBoxStatus.Items.Add("Concluído");
            comboBoxStatus.SelectedIndex = 0;
        }

        private void SalvarRespostaChamado(int idChamado, string resposta)
        {
            try
            {
                using (SqlConnection conexao = new SqlConnection(conexaoString))
                {
                    string query = "UPDATE Chamados SET Resposta = @Resposta WHERE ID = @IDChamado";

                    using (SqlCommand cmd = new SqlCommand(query, conexao))
                    {
                        cmd.Parameters.AddWithValue("@Resposta", resposta);
                        cmd.Parameters.AddWithValue("@IDChamado", idChamado);

                        conexao.Open();
                        int linhasAfetadas = cmd.ExecuteNonQuery();

                        if (linhasAfetadas > 0)
                        {
                            MessageBox.Show("Resposta salva com sucesso!", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show("Erro ao salvar a resposta. Verifique o ID do chamado.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao conectar ao banco de dados: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void EnviarEmailResposta(string emailDestino, string resposta)
        {
            try
            {
                MailMessage mail = new MailMessage();
                mail.From = new MailAddress("victorhforworking@gmail.com");
                mail.To.Add(emailDestino);
                mail.Subject = "Resposta ao seu chamado de suporte";
                mail.Body = "Olá,\n\nA equipe de suporte respondeu ao seu chamado:\n\n" + resposta + "\n\nAtenciosamente,\nEquipe de Suporte";
                mail.IsBodyHtml = false;

                SmtpClient smtp = new SmtpClient("smtp.gmail.com");
                smtp.Port = 587;
                smtp.Credentials = new NetworkCredential("victorhforworking@gmail.com", "aapf njfk tehy qcmz");
                smtp.EnableSsl = true;
                smtp.Send(mail);

                MessageBox.Show("E-mail enviado com sucesso!", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao enviar o e-mail: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnResponder_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(txtIdChamado.Text, out int idChamado))
            {
                MessageBox.Show("Selecione um chamado válido!", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string resposta = txtResposta.Text.Trim();

            if (string.IsNullOrEmpty(resposta))
            {
                MessageBox.Show("Digite a resposta antes de enviar!", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // busca e-mail
            string emailDestino = BuscarEmailChamado(idChamado);

            if (!string.IsNullOrEmpty(emailDestino))
            {
                SalvarRespostaChamado(idChamado, resposta);
                EnviarEmailResposta(emailDestino, resposta);
                CarregarChamados(); // atualiza a lista de chamados
            }
            else
            {
                MessageBox.Show("Erro ao buscar o e-mail do solicitante.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string BuscarEmailChamado(int idChamado)
        {
            try
            {
                using (SqlConnection conexao = new SqlConnection(conexaoString))
                {
                    string query = "SELECT Email FROM Chamados WHERE ID = @IDChamado";

                    using (SqlCommand cmd = new SqlCommand(query, conexao))
                    {
                        cmd.Parameters.AddWithValue("@IDChamado", idChamado);

                        conexao.Open();
                        object result = cmd.ExecuteScalar();

                        return result != null ? result.ToString() : null;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao buscar e-mail: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        private void dataGridViewChamados_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0) // verifica se a linha clicada é válida
            {
                txtIdChamado.Text = dataGridViewChamados.Rows[e.RowIndex].Cells["ID"].Value.ToString();
            }
        }

        private void btnVoltarMenu_Click(object sender, EventArgs e)
        {
            FormLogin formLogin = Application.OpenForms.OfType<FormLogin>().FirstOrDefault(); 

            if (formLogin != null)
            {
                formLogin.Show(); 
            }

            this.Close(); 
        }
    }
}
