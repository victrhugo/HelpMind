#  Gestão de Chamados com IA - BACKLOG

##  Funcionais
### Autenticação e Cadastro de Usuários
- Cadastro de usuários com validação de e-mail.
- Login seguro com autenticação.
- Recuperação de senha via e-mail.
- Gestão de permissões (colaborador, técnico, administrador).

### Abertura e Gerenciamento de Chamados
- Abertura de chamados com descrição, categoria e nível de urgência.
- Upload de anexos (.png, .jpg, .pdf).
- Acompanhamento do status dos chamados.
- Atualização e comentários pelos técnicos.

### Atendimento com IA e Encaminhamento Inteligente
- Sugestão automática de soluções baseadas em chamados similares.
- Encaminhamento inteligente para o técnico mais adequado.

### Notificações e Comunicação
- Notificações automáticas sobre atualizações nos chamados.
- Mensagens entre técnicos e colaboradores dentro do chamado.

### Relatórios e Métricas
- Tempo médio de resposta dos chamados.
- Número de chamados resolvidos.
- Eficiência da IA na resolução automática.
- Relatórios sobre a taxa de sucesso das sugestões da IA.

## Tecnologias Utilizadas
- **Linguagem:** C#
- **Banco de Dados:** SQL Server
- **Frameworks:** .NET
- **Ferramentas:** Astah (modelagem de diagramas), BrModelo (modelagem ER)

## Requisitos Não Funcionais
- **Segurança:** Autenticação segura e controle de acesso baseado em permissões.
- **Desempenho:** Respostas rápidas (< 3s para consultas comuns, < 5s para IA).
- **Disponibilidade:** 99,9% (máx. 2h/mês de inatividade).
- **Usabilidade:** Interface intuitiva e acessível.
- **Escalabilidade:** Suporte a aumento de carga sem perda de desempenho.



