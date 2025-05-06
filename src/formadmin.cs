namespace GestaoIA
{
    public partial class FormPrincipal : Form
    {
        private readonly string conexaoString = Configuracoes.StringConexao;

        public FormPrincipal()
        {
            InitializeComponent();
            CarregarUsuarios();
        }

        private void ExibirMensagem(string mensagem, string titulo = "Informação", MessageBoxIcon icone = MessageBoxIcon.Information)
        {
            MessageBox.Show(mensagem, titulo, MessageBoxButtons.OK, icone);
        }

        private bool ExecutarComando(string query, Dictionary<string, object> parametros)
        {
            try
            {
                using (SqlConnection conexao = new SqlConnection(conexaoString))
                using (SqlCommand cmd = new SqlCommand(query, conexao))
                {
                    conexao.Open();
                    foreach (var param in parametros)
                        cmd.Parameters.AddWithValue(param.Key, param.Value);

                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                ExibirMensagem("Erro: " + ex.Message, "Erro", MessageBoxIcon.Error);
                return false;
            }
        }

        private void ExcluirChamado(int idChamado)
        {
            try
            {
                using (SqlConnection conexao = new SqlConnection(conexaoString))
                {
                    conexao.Open();

                    string queryAnexos = "DELETE FROM AnexosChamado WHERE IDChamado = @IDChamado";
                    ExecutarComando(queryAnexos, new() { { "@IDChamado", idChamado } });

                    string queryChamado = "DELETE FROM Chamados WHERE ID = @ID";
                    if (ExecutarComando(queryChamado, new() { { "@ID", idChamado } }))
                        ExibirMensagem("Chamado excluído com sucesso!", "Sucesso", MessageBoxIcon.Information);
                    else
                        ExibirMensagem("Nenhum chamado encontrado com esse ID!", "Aviso", MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                ExibirMensagem("Erro ao excluir chamado: " + ex.Message, "Erro", MessageBoxIcon.Error);
            }
        }

        private void ExcluirUsuario(int idUsuario)
        {
            try
            {
                using (SqlConnection conexao = new SqlConnection(conexaoString))
                {
                    conexao.Open();

                    string queryObterChamados = "SELECT ID FROM Chamados WHERE IDUsuario = @IDUsuario";
                    List<int> chamadosIds = new();

                    using (SqlCommand cmd = new SqlCommand(queryObterChamados, conexao))
                    {
                        cmd.Parameters.AddWithValue("@IDUsuario", idUsuario);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                                chamadosIds.Add(reader.GetInt32(0));
                        }
                    }

                    foreach (int chamadoId in chamadosIds)
                    {
                        string queryExcluirAnexos = "DELETE FROM AnexosChamado WHERE IDChamado = @IDChamado";
                        ExecutarComando(queryExcluirAnexos, new() { { "@IDChamado", chamadoId } });
                    }

                    string queryExcluirChamados = "DELETE FROM Chamados WHERE IDUsuario = @IDUsuario";
                    ExecutarComando(queryExcluirChamados, new() { { "@IDUsuario", idUsuario } });

                    ExibirMensagem("Chamados e anexos do usuário excluídos com sucesso.", "Sucesso", MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                ExibirMensagem("Erro ao excluir chamados do usuário: " + ex.Message, "Erro", MessageBoxIcon.Error);
            }
        }

        private void CarregarChamados()
        {
            try
            {
                using (SqlConnection conexao = new SqlConnection(conexaoString))
                {
                    string query = "SELECT ID, Email, Descricao, Status FROM Chamados";
                    using (SqlDataAdapter adapter = new SqlDataAdapter(query, conexao))
                    {
                        DataTable dataTable = new();
                        adapter.Fill(dataTable);
                        dgvChamados.DataSource = dataTable;
                    }
                }
            }
            catch (Exception ex)
            {
                ExibirMensagem("Erro ao carregar chamados: " + ex.Message, "Erro", MessageBoxIcon.Error);
            }
        }

        private void CarregarUsuarios()
        {
            try
            {
                using (SqlConnection conexao = new SqlConnection(conexaoString))
                {
                    string query = "SELECT ID, Nome, Email, TipoUsuario FROM Usuarios";
                    using (SqlDataAdapter adapter = new SqlDataAdapter(query, conexao))
                    {
                        DataTable tabelaUsuarios = new();
                        adapter.Fill(tabelaUsuarios);
                        dgvUsuarios.DataSource = tabelaUsuarios;
                    }
                }
            }
            catch (Exception ex)
            {
                ExibirMensagem("Erro ao carregar usuários: " + ex.Message, "Erro", MessageBoxIcon.Error);
            }
        }

        private void btnCarregarChamados_Click(object sender, EventArgs e) => CarregarChamados();

        private void btnAlterarStatus_Click(object sender, EventArgs e)
        {
            if (int.TryParse(txtIdChamado.Text, out int idChamado) && comboBoxStatus.SelectedItem != null)
            {
                string novoStatus = comboBoxStatus.SelectedItem.ToString();
                string query = "UPDATE Chamados SET Status = @Status WHERE ID = @IDChamado";

                if (ExecutarComando(query, new() { { "@Status", novoStatus }, { "@IDChamado", idChamado } }))
                    ExibirMensagem("Status atualizado com sucesso!", "Sucesso", MessageBoxIcon.Information);
                else
                    ExibirMensagem("Chamado não encontrado!", "Erro", MessageBoxIcon.Warning);
            }
            else
            {
                ExibirMensagem("Informe um ID válido e selecione um status!", "Aviso", MessageBoxIcon.Warning);
            }
        }

        private void btnVoltar_Click(object sender, EventArgs e)
        {
            this.Hide();
            FormLogin formLogin = new FormLogin();
            formLogin.Show();
            formLogin.FormClosed += (s, args) => this.Close();
        }

        private void btnExcluirUsuario_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(txtIdUsuario.Text, out int idUsuario))
            {
                ExibirMensagem("Informe um ID de usuário válido!", "Erro", MessageBoxIcon.Warning);
                return;
            }
            ExcluirUsuario(idUsuario);
        }

        private void btnExcluirUser_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(txtIdUser.Text, out int idUsuario))
            {
                ExibirMensagem("ID inválido! Digite um número válido.", "Erro", MessageBoxIcon.Error);
                return;
            }

            DialogResult confirmacao = MessageBox.Show($"Tem certeza que deseja excluir o usuário com ID {idUsuario}?", "Confirmação", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirmacao == DialogResult.Yes)
            {
                if (ExecutarComando("DELETE FROM Usuarios WHERE Id = @IdUsuario", new() { { "@IdUsuario", idUsuario } }))
                {
                    ExibirMensagem("Usuário excluído com sucesso!", "Sucesso", MessageBoxIcon.Information);
                    CarregarUsuarios();
                    txtIdUser.Clear();
                }
                else
                {
                    ExibirMensagem("Erro ao excluir o usuário.", "Erro", MessageBoxIcon.Error);
                }
            }
        }
    }

    public static class Configuracoes
    {
        public static string StringConexao = "Server=DESKTOP-6H5PBKR\\SQLVICTOR;Database=GestaoIA;Integrated Security=True;TrustServerCertificate=True;";
    }
}

