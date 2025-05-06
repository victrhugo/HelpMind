namespace GestaoIA
{
    public partial class FormChamado : Form
    {
        private OpenAIService openAI;
        public FormChamado()
        {
            InitializeComponent();
            
        }

        string conexaoString = "Server=DESKTOP-6H5PBKR\\SQLVICTOR;Database=GestaoIA;Integrated Security=True;";
     
         private void RegistrarChamado(string nome, string email, string descricao)
        {
            using (SqlConnection conexao = new SqlConnection(conexaoString))
            {
                try
                {
                    conexao.Open();
                    string query = "INSERT INTO Chamados (Nome, Email, Descricao) VALUES (@Nome, @Email, @Descricao)";

                    using (SqlCommand cmd = new SqlCommand(query, conexao))
                    {
                        cmd.Parameters.AddWithValue("@Nome", nome);
                        cmd.Parameters.AddWithValue("@Email", email);
                        cmd.Parameters.AddWithValue("@Descricao", descricao);

                        cmd.ExecuteNonQuery();
                    }

                    MessageBox.Show("Chamado registrado com sucesso!", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Erro ao registrar chamado: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            } 
        } 

        private async void btnEnviar_Click(object sender, EventArgs e)
        {
            string descricao = txtDescricao.Text;
            string nome = txtNome.Text;
            string email = txtEmail.Text;

            // 🔹 Validação dos campos obrigatórios
            if (string.IsNullOrWhiteSpace(descricao) || string.IsNullOrWhiteSpace(nome) || string.IsNullOrWhiteSpace(email))
            {
                MessageBox.Show("Por favor, preencha todos os campos!", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 🔹 Validação do e-mail
            if (!ValidarEmail(email))
            {
                MessageBox.Show("E-mail inválido! Digite um e-mail no formato correto.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 🔹 Salva chamado no banco
            int idChamado = SalvarChamadoNoBanco(descricao, nome, email);

            // 🔹 Se houver um arquivo, salva no banco também
            if (!string.IsNullOrEmpty(caminhoArquivo) && idChamado > 0)
            {
                SalvarArquivoNoBanco(idChamado, caminhoArquivo);
            }

            // 🔹 Se o chamado foi salvo com sucesso
            if (idChamado > 0)
            {
                MessageBox.Show($"Chamado registrado com sucesso!\nID do chamado: {idChamado}", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // 🔹 Chave da API OpenAI
                string apiKey = "...";  // Insira sua chave da OpenAI aqui

                if (string.IsNullOrWhiteSpace(apiKey))
                {
                    MessageBox.Show("Erro: Chave da API não pode estar vazia!", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // 🔹 Criando a instância da IA com a chave correta
                OpenAIService openAI = new OpenAIService(apiKey);
                lblResposta.Text = "Gerando resposta da IA...";

                try
                {
                    // 🔹 Obtendo resposta da IA
                    string respostaIA = await openAI.ObterRespostaAsync(descricao);
                    lblResposta.Text = respostaIA;

                    // 🔹 Salvando a resposta da IA no banco de dados
                    SalvarRespostaIA(idChamado, respostaIA);
                }
                catch (Exception ex)
                {
                    lblResposta.Text = "Erro ao obter resposta da IA!";
                    MessageBox.Show("Erro ao chamar a IA: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private bool ValidarEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            string padraoEmail = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            return Regex.IsMatch(email, padraoEmail);
        }

        private void txtDescricao_TextChanged(object sender, EventArgs e)
        {

        }

        private string caminhoArquivo = "";

    

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

        private void btnAnexarArquivo_Click_1(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Selecionar Arquivo";
            openFileDialog.Filter = "Todos os Arquivos|*.*";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                caminhoArquivo = openFileDialog.FileName;
                lblArquivoSelecionado.Text = "Arquivo: " + System.IO.Path.GetFileName(caminhoArquivo);
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
                        {
                            idChamado = Convert.ToInt32(result);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao salvar chamado: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return idChamado; 
        } 

        private void FormChamado_Load(object sender, EventArgs e)
        {
            if (int.TryParse(txtIdChamado.Text, out int idChamado))
            {
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
                                MessageBox.Show("Chamado não encontrado!", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Erro ao carregar status: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnCarregarStatus_Click(object sender, EventArgs e)
        {
            CarregarStatusChamado();
        }

        private void CarregarStatusChamado()
        {
            if (!int.TryParse(txtIdChamado.Text, out int idChamado))
            {
                MessageBox.Show("Informe um ID de chamado válido!", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

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
                        }
                        else
                        {
                            MessageBox.Show("Chamado não encontrado!", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao carregar status: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click_1(object sender, EventArgs e)
        {

        }

        private void SalvarRespostaIA(int idChamado, string resposta)
        {
            using (SqlConnection conn = new SqlConnection("SUA_STRING_DE_CONEXAO"))
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

        private void FormChamado_Load_1(object sender, EventArgs e)
        {

        }
    }
}
