using System;
using System.IO;

namespace AppBOV.CrossCutting.Config
{
    public class AppConfig
    {
        //Variaveis do banco de dados"
        public string ServidorBancoDados { get; set; }    //Endereço do banco de dados sql que armazena as informações da execução
        public string LoginBd { get; set; }               //Usuario do sql server
        public string SenhaBd { get; set; }               //Senha do sql server
        public string NomeBd { get; set; }                //Nome do banco de dados sql server
        public string NomeSchema { get; set; }              //Nome do schema caso exista
        public string TabelaArquivos { get; set; }        //Tabela que mantem as informações dos arquivos baixados diariamente
        public string TabelaInfoBov { get; set; }         //Tabela que armazena as informações iniciais da bov
        public string TabelaRelatorios { get; set; }      //Tabela que contem as informações dos relatórios que serão baixados
        public string TabelaAdmin { get; set; }           //Tabela que armazena as informações de alerta do relatório
        public string TabelaCabecalho { get; set; }       //Tabela que mantem informações sobre os cabeçalhos dos arquivos BOV
        public string TabelaAlerta { get; set; }          //Tabela que mantem informações sobre as mensagens que serão alertadas
        public string TabelaAlteracoes { get; set; }       //Tabela que mantem as diferenças entre os cabeçalhos dos arquivos

        public string DirLocal { get; set; }              //Diretorio local (Ou remoto) que armazenará os dados recebidos do JBBT e tratados
        public string DirRemoto { get; set; }             //Diretorio GPP onde será armazenado o arquivo tratado
        public string DirLog { get; set; }                //Diretorio que vai armazenar o log de execução da aplicação
        public string DirDestino { get; set; }            //Diretorio das imagens usadas na aplicação

        public string DataFormatada { get; set; }         //Armazena informação da data que se deseja obter informações
        public string DataInicioExtrairBov { get; set; }  //Define data inicio da consulta
        public string DataFimExtrairBov { get; set; }     //Define data fim da consulta
        public int QtdDiasRelatorio { get; set; }
        public int QtdMesRelatorio { get; set; }
        public int FechamentoMes { get; set; }        //Define se arquivo é de fechamento mês
        public int DataManual { get; set; }
        public string HoraExecucaoDownload { get; set; }  //Define hora que será iniciado o download do processo

        public string UsuarioRede { get; set; }           //Armazena usuario de rede

        public string ConnectionString { get; set; }
        public string ChromeDriverPath { get; set; }
        public string ChromeBinaryPath { get; set; }
        public string ProxyPacUrl { get; set; }
    }
}