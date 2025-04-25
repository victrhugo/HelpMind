namespace GestaoIA
{
    public partial class FormLogin : Form
    {
        private string conexaoString = "Server=DESKTOP-6H5PBKR\\SQLVICTOR;Database=GestaoIA;Integrated Security=True;TrustServerCertificate=True;";

        public FormLogin()
        {
            InitializeComponent();
        }

private void btnLogin_Click(object sender, EventArgs e)
        {
            string email = txtUsuario.Text.Trim();
            string senha = txtSenha.Text.Trim();

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(senha))
            {
                MessageBox.Show("Preencha todos os campos!", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Gera o hash da senha digitada para comparar com a salva no banco
            string senhaHash = GerarHashSenha(senha);
            string tipoUsuario = ObterTipoUsuario(email, senhaHash);

            if (tipoUsuario == "Admin")
            {
                MessageBox.Show("Login de Administrador realizado com sucesso!", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Hide();
                FormPrincipal formPrincipal = new FormPrincipal();
                formPrincipal.Show();
                formPrincipal.FormClosed += (s, args) => this.Show();
            }
            else if (tipoUsuario == "Tecnico")
            {
                MessageBox.Show("Login de Técnico realizado com sucesso!", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Hide();
                FormTecnico formTecnico = new FormTecnico();
                formTecnico.Show();
                formTecnico.FormClosed += (s, args) => this.Show();
            }
            else if (!string.IsNullOrEmpty(tipoUsuario)) // Qualquer outro tipo de usuário
            {
                MessageBox.Show("Login realizado com sucesso!", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Hide();
                FormChamado formChamado = new FormChamado();
                formChamado.Show();
                formChamado.FormClosed += (s, args) => this.Show();
            }
            else
            {
                MessageBox.Show("Usuário, e-mail ou senha inválidos!", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        /// <summary>
        /// Obtém o tipo de usuário no banco de dados com base no e-mail e senha.
        /// </summary>
        private string ObterTipoUsuario(string email, string senhaHash)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(conexaoString))
                {
                    conn.Open();

                    // 🔹 Buscar senha e tipo de usuário no banco
                    string query = "SELECT TipoUsuario, Senha FROM Usuarios WHERE Email = @Email";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Email", email);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read()) // 🔹 Se encontrou o usuário
                            {
                                string senhaNoBanco = reader["Senha"].ToString().Trim(); // Remove espaços extras
                                string tipoUsuario = reader["TipoUsuario"].ToString();

                                // 🔹 Exibir valores para depuração
                                MessageBox.Show($"Hash gerado: {senhaHash}\nHash no banco: {senhaNoBanco}");

                                if (senhaNoBanco == senhaHash) // 🔹 Comparação de hashes
                                {
                                    return tipoUsuario;
                                }
                                else
                                {
                                    MessageBox.Show("Senhas diferentes!", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                            }
                            else
                            {
                                MessageBox.Show("Usuário não encontrado!", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao acessar o banco de dados: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return null;

        }
        
        /// <summary>
        /// Gera um hash SHA-256 para a senha.
        /// </summary>
        private string GerarHashSenha(string senha)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(senha));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in hashedBytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }

        private void btnCriarConta_Click(object sender, EventArgs e)
        {
            FormCadastro formCadastro = new FormCadastro();
            formCadastro.ShowDialog();
        }

        private void btnTesteHash_Click(object sender, EventArgs e)
        {
            string senhaTeste = "victor12"; // Digite a senha exata que cadastrou
            string hashTeste = GerarHashSenha(senhaTeste); // Gera o hash com a mesma função usada no login

            MessageBox.Show($"Hash gerado para 'victor12': {hashTeste}");
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }
    }
}
