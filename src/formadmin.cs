namespace GestaoIA
{
    public partial class FormPrincipal: Form
    {

        private string conexaoString = "Server=DESKTOP-6H5PBKR\\SQLVICTOR;Database=GestaoIA;Integrated Security=True;TrustServerCertificate=True;";

        public FormPrincipal()
        {
            InitializeComponent();
            CarregarUsuarios();

        }

        private void ExcluirChamado(int idChamado)
        {
            try
            {
                using (SqlConnection conexao = new SqlConnection(conexaoString))
                {
                    conexao.Open();
                 
                    string queryAnexos = "DELETE FROM AnexosChamado WHERE IDChamado = @IDChamado";
                    using (SqlCommand cmdAnexos = new SqlCommand(queryAnexos, conexao))
                    {
                        cmdAnexos.Parameters.Add("@IDChamado", SqlDbType.Int).Value = idChamado;
                        cmdAnexos.ExecuteNonQuery();
                    }
                    
                    string queryChamado = "DELETE FROM Chamados WHERE ID = @ID";
                    using (SqlCommand cmdChamado = new SqlCommand(queryChamado, conexao))
                    {
                        cmdChamado.Parameters.Add("@ID", SqlDbType.Int).Value = idChamado;
                        int linhasAfetadas = cmdChamado.ExecuteNonQuery();

                        if (linhasAfetadas > 0)
                        {
                            MessageBox.Show("Chamado excluído com sucesso!", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show("Nenhum chamado encontrado com esse ID!", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao excluir chamado: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }



        private void ExcluirUsuario(int idUsuario)
        {
            try
            {
                using (SqlConnection conexao = new SqlConnection(conexaoString))
                {
                    conexao.Open();
                   
                    string queryObterChamados = "SELECT ID FROM Chamados WHERE ID = @ID";
                    List<int> chamadosIds = new List<int>();

                    using (SqlCommand cmdObterChamados = new SqlCommand(queryObterChamados, conexao))
                    {
                        cmdObterChamados.Parameters.AddWithValue("@ID", idUsuario);
                        using (SqlDataReader reader = cmdObterChamados.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                chamadosIds.Add(reader.GetInt32(0));
                            }
                        }
                    }

                    if (chamadosIds.Count == 0)
                    {
                        MessageBox.Show("Nenhum chamado encontrado para este usuário.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    string queryExcluirAnexos = "DELETE FROM AnexosChamado WHERE IDChamado  = @IDChamado";
                    foreach (int chamadoId in chamadosIds)
                    {
                        using (SqlCommand cmdAnexos = new SqlCommand(queryExcluirAnexos, conexao))
                        {
                            cmdAnexos.Parameters.AddWithValue("@IDChamado", idUsuario);
                            cmdAnexos.ExecuteNonQuery();
                        }

                    }
                 
                    string queryExcluirChamados = "DELETE FROM Chamados WHERE ID = @ID";
                    using (SqlCommand cmdExcluirChamados = new SqlCommand(queryExcluirChamados, conexao))
                    {
                        cmdExcluirChamados.Parameters.AddWithValue("@ID", idUsuario);
                        int rowsAffected = cmdExcluirChamados.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Todos os chamados do usuário foram excluídos com sucesso!", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show("Nenhum chamado foi excluído.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao excluir chamados do usuário: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            
        }
        }

        private void btnExcluirUsuario_Click(object sender, EventArgs e)
        {
            if (int.TryParse(txtIdUsuario.Text, out int idUsuario))
            {
                ExcluirUsuario(idUsuario);
            }
            else
            {
                MessageBox.Show("Informe um ID de usuário válido!", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                        DataTable dataTable = new DataTable();
                        adapter.Fill(dataTable);
                        dgvChamados.DataSource = dataTable; 
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao carregar chamados: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCarregarChamados_Click(object sender, EventArgs e)
        {
            CarregarChamados();

        }

        private void btnAlterarStatus_Click(object sender, EventArgs e)
        {
            if (int.TryParse(txtIdChamado.Text, out int idChamado) && comboBoxStatus.SelectedItem != null)
            {
                string novoStatus = comboBoxStatus.SelectedItem.ToString(); // status selecionado

                try
                {
                    using (SqlConnection conexao = new SqlConnection(conexaoString))
                    {
                        string query = "UPDATE Chamados SET Status = @Status WHERE ID = @IDChamado";
                        using (SqlCommand cmd = new SqlCommand(query, conexao))
                        {
                            cmd.Parameters.AddWithValue("@Status", novoStatus);
                            cmd.Parameters.AddWithValue("@IDChamado", idChamado);

                            conexao.Open();
                            int rowsAffected = cmd.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("Status atualizado com sucesso!", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                    MessageBox.Show("Erro ao atualizar status: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Informe um ID válido e selecione um status!", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnVoltar_Click(object sender, EventArgs e)
        {
            this.Hide(); // Oculta
            FormLogin formLogin = new FormLogin();
            formLogin.Show();
            formLogin.FormClosed += (s, args) => this.Close(); 
        }

        private void CarregarUsuarios()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(conexaoString))
                {
                    conn.Open();
                    string query = "SELECT ID, Nome, Email, TipoUsuario FROM Usuarios";

                    using (SqlDataAdapter adapter = new SqlDataAdapter(query, conn))
                    {
                        DataTable tabelaUsuarios = new DataTable();
                        adapter.Fill(tabelaUsuarios);
                        dgvUsuarios.DataSource = tabelaUsuarios;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao carregar usuários: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnExcluirUser_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtIdUser.Text))
            {
                MessageBox.Show("Digite o ID do usuário para excluir!", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!int.TryParse(txtIdUser.Text, out int idUsuario))
            {
                MessageBox.Show("ID inválido! Digite um número válido.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            DialogResult confirmacao = MessageBox.Show($"Tem certeza que deseja excluir o usuário com ID {idUsuario}?", "Confirmação",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirmacao == DialogResult.Yes)
            {
                if (RemoverUsuario(idUsuario))
                {
                    MessageBox.Show("Usuário excluído com sucesso!", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    CarregarUsuarios(); // Atualiza a lista após a exclusão
                    txtIdUser.Clear(); // Limpa o campo após a exclusão
                }
                else
                {
                    MessageBox.Show("Erro ao excluir o usuário.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private bool RemoverUsuario(int idUsuario)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection("Server=DESKTOP-6H5PBKR\\SQLVICTOR;Database=GestaoIA;Integrated Security=True;TrustServerCertificate=True;"))
                {
                    conn.Open();
                    string query = "DELETE FROM Usuarios WHERE Id = @IdUsuario";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);

                        int rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao excluir usuário: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
    }
}
