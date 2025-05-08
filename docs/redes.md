# Esquema de Rede para o Projeto HelpMind

## 🎯 Objetivos

O esquema de rede do projeto HelpMind deve garantir:

* ✅ Confiabilidade e estabilidade na comunicação.
* ✅ Alta disponibilidade e escalabilidade.
* ✅ Segurança dos dados em trânsito e em repouso.
* ✅ Facilidade de manutenção e monitoramento.
* ✅ Integração eficiente entre componentes do sistema.

## 🖧 Componentes Principais

### **👥 Usuários (Clientes e Técnicos)**

* Acessam o sistema via internet usando computadores ou dispositivos móveis.

### **🖥️ Servidores de Aplicação**

* Hospedam a aplicação HelpMind (backend e frontend).
* Utilizam .NET para o backend e React ou Blazor para o frontend.

### **💾 Servidor de Banco de Dados (SQL Server)**

* Armazena dados de usuários, chamados e anexos.
* Inclui mecanismos de backup e replicação para alta disponibilidade.

### **🛡️ Firewall e Proxy Reverso**

* Protegem os servidores contra acessos não autorizados.
* Realizam inspeção de tráfego e balanceamento de carga.

### **🗂️ Serviço de Armazenamento de Anexos**

* Utiliza soluções como Azure Blob Storage ou AWS S3 para armazenamento seguro de anexos grandes.

### **🔐 Serviço de Autenticação e Autorização**

* Gerencia logins, permissões e segurança de dados sensíveis.

## 🛠️ Arquitetura Geral

* **Frontend:** React ou Blazor, consumindo APIs REST do backend.
* **Backend:** .NET Web API, implementando lógica de negócios e integração com o banco de dados.
* **Banco de Dados:** SQL Server, com tabelas `Usuarios`, `Chamados` e `AnexosChamado`.
* **Armazenamento de Anexos:** Separado para facilitar escalabilidade e segurança.
* **Rede Segura:** VPN para conexões internas e HTTPS para comunicações externas.

## 📡 Comunicação entre Componentes

* HTTPS para segurança de ponta a ponta.
* API Gateway para gerenciamento centralizado de chamadas de API.
* WebSockets para notificações em tempo real (opcional).

## 🔒 Segurança

* TLS/SSL para criptografia de dados em trânsito.
* Backup diário e políticas de retenção de dados.
* Multi-factor authentication (MFA) para acesso administrativo.

## 📈 Escalabilidade e Redundância

* Balanceamento de carga para distribuir tráfego.
* Replicação de banco de dados para alta disponibilidade.
* CDN para otimizar o tempo de resposta do frontend.

## 📊 Monitoramento e Logs

* Monitoramento centralizado com ferramentas como Grafana e Prometheus.
* Logs centralizados para auditoria e análise de problemas.

## 🔌 Cabeamento e Equipamentos

* **Cabos:** Categoria 6 (CAT6) para conexões internas, garantindo suporte para até 10 Gbps.
* **Switches:** Switches gerenciáveis com suporte a VLANs e QoS para priorização de tráfego.
* **Roteadores:** Roteadores com suporte a VPN e firewall integrado para segurança adicional.
* **Access Points (Wi-Fi):** Wi-Fi 6 (802.11ax) para conexões sem fio, com capacidade para múltiplos dispositivos.
* **Patch Panels:** Para organização dos cabos no rack.
* **No-breaks (UPS):** Para garantir disponibilidade em caso de queda de energia.

## 🚀 Próximos Passos

* Definir a topologia física e lógica da rede.
* Identificar os requisitos de largura de banda e latência.
* Testar e validar a segurança do ambiente.
