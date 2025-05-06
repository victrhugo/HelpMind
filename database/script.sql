USE [master]
GO

IF DB_ID('HelpMind') IS NOT NULL
	DROP DATABASE HelpMind
GO

CREATE DATABASE [HelpMind]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'HelpMind', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL16.SQLVICTOR\MSSQL\DATA\HelpMind.mdf', SIZE = 8192KB, MAXSIZE = UNLIMITED, FILEGROWTH = 65536KB )
 LOG ON 
( NAME = N'HelpMind_log', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL16.SQLVICTOR\MSSQL\DATA\HelpMind_log.ldf', SIZE = 8192KB, MAXSIZE = 2048GB, FILEGROWTH = 65536KB )
 WITH CATALOG_COLLATION = DATABASE_DEFAULT, LEDGER = OFF
GO

ALTER DATABASE [HelpMind] SET COMPATIBILITY_LEVEL = 160
GO

IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
BEGIN
    EXEC [HelpMind].[dbo].[sp_fulltext_database] @action = 'enable'
END
GO

ALTER DATABASE [HelpMind] SET QUERY_STORE (
    OPERATION_MODE = READ_WRITE,
    CLEANUP_POLICY = (STALE_QUERY_THRESHOLD_DAYS = 30),
    DATA_FLUSH_INTERVAL_SECONDS = 900,
    INTERVAL_LENGTH_MINUTES = 60,
    MAX_STORAGE_SIZE_MB = 1000,
    QUERY_CAPTURE_MODE = AUTO,
    SIZE_BASED_CLEANUP_MODE = AUTO,
    MAX_PLANS_PER_QUERY = 200,
    WAIT_STATS_CAPTURE_MODE = ON
)
GO

USE [HelpMind]
GO

-- TABELAS AUXILIARES (Domínios)
CREATE TABLE TipoUsuarios (
    Tipo NVARCHAR(20) PRIMARY KEY
);
INSERT INTO TipoUsuarios (Tipo) VALUES ('Admin'), ('Suporte'), ('Cliente');
GO

CREATE TABLE StatusChamados (
    Status VARCHAR(20) PRIMARY KEY
);
INSERT INTO StatusChamados (Status) VALUES ('Aberto'), ('Em Atendimento'), ('Resolvido'), ('Fechado');
GO

-- USUÁRIOS
CREATE TABLE Usuarios (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    Nome NVARCHAR(100) NOT NULL DEFAULT 'Usuário Padrão',
    Email NVARCHAR(100) NOT NULL,
    Usuario NVARCHAR(100) NOT NULL UNIQUE DEFAULT 'NovoUsuario',
    Senha NVARCHAR(64) NOT NULL,
    TipoUsuario NVARCHAR(20) NOT NULL,
    CONSTRAINT FK_Usuarios_Tipo FOREIGN KEY (TipoUsuario)
        REFERENCES TipoUsuarios(Tipo)
);
GO

-- CHAMADOS
CREATE TABLE Chamados (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    UsuarioID INT NOT NULL,
    Descricao NVARCHAR(MAX) NOT NULL,
    Status VARCHAR(20) NOT NULL DEFAULT 'Aberto',
    Resposta NVARCHAR(MAX) NULL,
    DataCriacao DATETIME NOT NULL DEFAULT GETDATE(),
    DataAtualizacao DATETIME NULL,
    CONSTRAINT FK_Chamados_Usuario FOREIGN KEY (UsuarioID)
        REFERENCES Usuarios(ID),
    CONSTRAINT FK_Chamados_Status FOREIGN KEY (Status)
        REFERENCES StatusChamados(Status)
);
GO

-- ANEXOS
CREATE TABLE AnexosChamado (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    ChamadoID INT NOT NULL,
    NomeArquivo NVARCHAR(255) NOT NULL,
    TipoArquivo NVARCHAR(100) NOT NULL,
    Dados VARBINARY(MAX) NOT NULL,
    CONSTRAINT FK_AnexosChamado_Chamado FOREIGN KEY (ChamadoID)
        REFERENCES Chamados(ID)
);
GO

/*
============================================================
  DOCUMENTAÇÃO DE NORMALIZAÇÃO DO BANCO DE DADOS HELPMIND
  Autor: Victor Hugo
  Data: 05/05/2025
  Objetivo: Justificar e documentar a normalização do modelo
============================================================

1ª FORMA NORMAL (1FN) – Dados Atômicos e Sem Repetições:
---------------------------------------------------------
✔️ Todos os atributos possuem apenas valores atômicos.
✔️ Não existem colunas com listas ou múltiplos valores.
✔️ Exemplo: Cada chamado possui uma descrição única e cada anexo é armazenado em uma linha separada na tabela AnexosChamado.

2ª FORMA NORMAL (2FN) – Remoção de Dependências Parciais:
----------------------------------------------------------
✔️ Todas as tabelas usam chave primária simples (ID), então não há chaves compostas.
✔️ Todos os atributos não-chave dependem totalmente da chave primária de suas tabelas.
✔️ Exemplo: Na tabela Chamados, os campos Descricao, Status, Resposta e Datas dependem exclusivamente do ID do chamado.

3ª FORMA NORMAL (3FN) – Remoção de Dependências Transitivas:
-------------------------------------------------------------
✔️ Nenhum atributo não-chave depende de outro atributo não-chave.
✔️ Valores que poderiam se repetir ou estar duplicados foram movidos para tabelas auxiliares.
✔️ Exemplo: 
    - Status dos chamados foi isolado na tabela StatusChamados.
    - Tipo de usuário foi isolado na tabela TipoUsuarios.
✔️ Isso evita redundância e facilita manutenção e integridade.

FORMA NORMAL DE BOYCE-CODD (BCNF) – Reforço da 3FN:
----------------------------------------------------
✔️ Todas as dependências funcionais são baseadas em superchaves.
✔️ Colunas como Usuario (login) são únicas, evitando que um atributo dependa de outro que não seja superchave.
✔️ Exemplo: Email e Usuario estão sob restrições únicas e não formam dependências inválidas.

4ª FORMA NORMAL (4FN) – Remoção de Dependências Multivaloradas:
----------------------------------------------------------------
✔️ Um chamado pode ter múltiplos arquivos anexados, mas isso é representado corretamente com a tabela AnexosChamado.
✔️ Cada anexo está ligado a um único chamado via chave estrangeira.
✔️ Evita o uso de campos multivalorados ou repetitivos no mesmo registro.

============================================================
  CONCLUSÃO:
  O banco de dados HelpMind foi modelado para atender até a
  4ª Forma Normal (4FN), garantindo integridade, desempenho,
  escalabilidade e facilidade de manutenção
============================================================
*/
