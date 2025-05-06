using System;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace GestaoIA
{
    public partial class FormCadastro : Form
    {
        private readonly string conexaoString = "Server=DESKTOP-6H5PBKR\\SQLVICTOR;Database=GestaoIA;Integrated Security=True;TrustServerCertificate=True;";

        public FormCadastro()
        {
            InitializeComponent();
        }

        private void btnCadastrar_Click(object sender, EventArgs e)
        {
            string nome = txtNome.Text.Trim();
            string email = txtEmail.Text.Trim();
            string senha = txtSenha.Text.Trim();

            if (!CamposValidos(nome, email, senha)) return;

            string senhaHash = GerarHashSenha(senha);

            if (CadastrarUsuario(nome, email, senhaHash))
            {
                MessageBox.Show("Cadastro realizado com sucesso!", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
            }
            else
            {
                MessageBox.Show("Erro ao cadastrar usuário!", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool CamposValidos(string nome, string email, string senha)
        {
            if (string.IsNullOrWhiteSpace(nome) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(senha))
            {
                MessageBox.Show("Preencha todos os campos!", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (!ValidarEmail(email))
            {
                MessageBox.Show("E-mail inválido!", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            return true;
        }

        private bool ValidarEmail(string email)
        {
            const string padraoEmail = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email, padraoEmail);
        }

        private string GerarHashSenha(string senha)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(senha));
                return Convert.ToHexString(hashBytes).ToLower();
            }
        }

        private bool CadastrarUsuario(string nome, string email, string senhaHash)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(conexaoString))
                {
                    conn.Open();
                    string query = @"INSERT INTO Usuarios (Nome, Email, Senha, TipoUsuario)
                                     VALUES (@Nome, @Email, @Senha, 'Usuario')";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Nome", nome);
                        cmd.Parameters.AddWithValue("@Email", email);
                        cmd.Parameters.AddWithValue("@Senha", senhaHash);

                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Erro de banco de dados: " + ex.Message, "Erro SQL", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro inesperado: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return false;
        }
    }
}

