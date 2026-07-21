AppBOV

Automação para solicitação, monitoramento, download e carga de relatórios do Portal BI, com processamento automatizado de arquivos e integração com SQL Server.

Visão Geral

O projeto foi desenvolvido para automatizar o ciclo completo de obtenção de relatórios batch:

Acessa o Portal BI via Selenium.
Solicita relatórios parametrizados.
Monitora o status de processamento.
Identifica relatórios disponíveis para download.
Realiza o download dos arquivos.
Valida a integridade e conteúdo dos arquivos.
Realiza carga em tabelas temporárias.
Executa procedures de tratamento e consolidação.
Atualiza o status de processamento no banco de dados.

Arquitetura
AppBOV
│
├── Services
│   ├── NavegadorService
│   ├── DataLoadService
│   ├── LogService
│   └── Interfaces
│
├── Repositories
│   ├── BovRepository
│   └── Interfaces
│
├── Models
│
├── Config
│
└── Program.cs

Principais Componentes
NavegadorService

Responsável pela automação web utilizando Selenium.

Funcionalidades:

Login no portal
Navegação entre páginas
Solicitação de relatórios
Coleta de informações da tabela de relatórios
Captura de links para download
Identificação de status do relatório
DataLoadService

Responsável pelo processamento dos arquivos baixados.

Funcionalidades:

Validação de layout
Criação de tabelas temporárias
Bulk Insert
Validação de arquivos vazios
Execução de procedures
Atualização de status
BovRepository

Camada de acesso a dados.

Responsável por:

Criação de tabelas
Execução de comandos SQL
Bulk Copy
Procedures
Controle de layouts
Registro de alertas
Atualização de status

Fluxo de Processamento

Portal BI
    │
    ▼
Solicitação de Relatório
    │
    ▼
Status Agendado
    │
    ▼
Status Processando
    │
    ▼
Status Disponível
    │
    ▼
Download do Arquivo
    │
    ▼
Validação
    │
    ├── Arquivo sem conteúdo
    │       ▼
    │    Status específico
    │
    └── Arquivo válido
            ▼
        Bulk Insert
            ▼
        Procedure
            ▼
        Finalizado

Tecnologias Utilizadas
.NET
C#
Selenium WebDriver
HtmlAgilityPack
SQL Server
ADO.NET
Bulk Copy

Tratamento de Status

O sistema identifica automaticamente o estado do relatório através do conteúdo retornado pelo Portal BI.

Exemplos:

Agendado
Aguarde
Disponível
Arquivo sem conteúdo
Erro de download
Processado

Validação de Arquivos

Antes da carga, os arquivos são validados para evitar processamento indevido.

Validações:

Existência física do arquivo
Presença de conteúdo além do cabeçalho
Estrutura compatível com o layout esperado

Exemplo:

if (!ArquivoTemConteudo(arquivo))
{
    return false;
}

Controle de Layout

O projeto possui mecanismo de detecção automática de alteração de layout.

Quando uma mudança é identificada:

O layout anterior é desativado.
O novo layout é registrado.
Um alerta é criado.
As diferenças são armazenadas para auditoria.

Logs

O sistema registra:

Execuções
Falhas
Alterações de layout
Downloads
Processamentos
Arquivos sem conteúdo
Configurações

As configurações da aplicação são centralizadas através da classe:

AppConfig

Exemplos:

Diretórios
Conexões
Competência de processamento
Parâmetros operacionais

Objetivo

Reduzir atividades manuais relacionadas à obtenção e processamento de relatórios, aumentando a confiabilidade, rastreabilidade e velocidade do processo de carga de dados.

Melhorias Futuras
Retry automático para relatórios em processamento
Controle de duplicidade
Dashboard operacional
Monitoramento em tempo real
Notificações automáticas
Agendamento via Windows Service ou Worker Service

Autor: Paulo Calil
 Projeto: AppBOV
 Linguagem: C# / .NET
 Banco de Dados: SQL Server
