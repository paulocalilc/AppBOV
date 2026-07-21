using System;
using System.Globalization;
using System.IO;
using AppBOV.CrossCutting.Config;
using AppBOV.Infrastructure.Logging;

namespace AppBOV.Application.Services
{
    public class InicializacaoService
    {
        private readonly LogService _log = new LogService();

        //Constantes com os dados do banco de dados
        public const string CONSTANTE_HOST_DB = "HOST_DO_BD";
        public const string CONSTANTE_LOGIN_DB = "USUARIO_BD";
        public const string CONSTANTE_PASSWORD_DB = "SENHA_DO_BD";
        public const string CONSTANTE_NAME_DB = "NOME_DO_BD";
        public const string CONSTANTE_NAME_SCHEMA = "SCHEMA_DO_BD";
        public const string CONSTANTE_TABELA_ARQUIVOS = "TABELA_DOS_ARQUIVOS_A_PROCESSAR";
        public const string CONSTANTE_TABELA_INFO_BOV = "TABELA_DE_INFOS_DO_PORTAL";
        public const string CONSTANTE_TABELA_RELATORIOS = "TABELA_DE_INFOS_DOS_RELATORIOS";
        public const string CONSTANTE_TABELA_ADMIN = "TABELA_DOS_USUARIOS_ADMIN";
        public const string CONSTANTE_TABELA_CABECALHO = "TABELA_DOS_CABECALHOS_DOS_ARQUIVOS";
        public const string CONSTANTE_TABELA_ALERTA = "TABELA_DOS_ALERTAS_DO_PORTAL";
        public const string CONSTANTE_TABELA_ALTERACOES = "TABELA_COM_AS_ALTERACOES_DE_CABECALHOS";

        //Constantes de diretorios de rede
        public const string CONSTANTE_DIR_LOCAL = @"\\PASTA_DA_REDE\PSTA_DOS_ARQUIVOS_TEMP\";
        public const string CONSTANTE_DIR_REDE = @"\\PASTA_DA_REDE\PSTA_DOS_ARQUIVOS_TEMP\";
        public const string CONSTANTE_IMG_FILTRO_GR_UNID = "IMAGEM.png";
        public static string dirLocal = string.Empty;
        public static string dirRemoto = string.Empty;
        public static string dirDestino = string.Empty;
        public static string dirOrigem = string.Empty;

        //Parametros BD
        public static string servidorBancoDados = string.Empty;
        public static string loginBd = string.Empty;
        public static string senhaBd = string.Empty;
        public static string nomeBd = string.Empty;
        public static string nomeSchema = string.Empty;
        public static string tabelaArquivos = string.Empty;
        public static string tabelaInfoBov = string.Empty;
        public static string tabelaRelatorios = string.Empty;
        public static string tabelaAdmin = string.Empty;
        public static string tabelaCabecalho = string.Empty;
        public static string tabelaAlerta = string.Empty;
        public static string tabelaAlteracoes = string.Empty;

        //Datas
        public static string dataFormatada = string.Empty;
        public static string dataInicioExtrairBov = string.Empty;
        public static string dataFimExtrairBov = string.Empty;
        public static string horaExecucaoDownload = "00:00";

        //Regra de negocio
        public static int qtdDiasRelatorio = 1; //Define como padrão D - 1
        public static int fechamentoMes = 0;    //Define se é fechamento do mês
        public static int qtdMesRelatorio = 1;  //Define quantidade de meses a se fechado
        public static int dataManual = 0;       //Define regra para adicionar data de forma manual

        //Parametros imacros
        public static string usuarioRede = ObterUsuarioLocal();
        public static string dirLog = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        //Construtor com array de parametros
        public InicializacaoService(string[] argument)
        {
            string argto;
            string argtoVal;

            //Verifica se possui ao menos 1 argumento valido
            if (argument.Length > 1)
            {
                //Prepara a atribuição de valores
                try
                {
                    //Estrutura de repetição para procurar os valores
                    for (var i = 0; i < argument.Length - 1; i++)
                    {
                        argto = argument[i];
                        argtoVal = argument[i + 1];

                        //Verifica se inicio do argumento é parametro
                        if (argto.Substring(0, 1) == "-")
                        {
                            //Verifica valor a valor
                            switch (argto)
                            {
                                //Endereço do servidor de banco de dados que contém o banco de dados com os nomes dos arquivos
                                case "-hostdb":
                                    PropServidorBancoDados.ServidorBancoDados = argtoVal;
                                    break;

                                //Login do usuario para o banco de dados
                                case "-userdb":
                                    PropLoginBd.LoginBd = argtoVal;
                                    break;

                                //Senha do usuario para o banco de dados
                                case "-pwdb":
                                    PropSenhaBd.SenhaBd = argtoVal;
                                    break;

                                //Nome do banco de dados
                                case "-nomedb":
                                    PropNomeBd.NomeBd = argtoVal;
                                    break;

                                //Nome do schema no banco de dados
                                case "-nomeschema":
                                    PropSchemaBd.NomeSchema = argtoVal;
                                    break;

                                //Nome da tabela que contem os arquivos baixados diariamente
                                case "-tabelaArquivos":
                                    PropTabelaArquivos.TabelaArquivos = argtoVal;
                                    break;

                                //Nome da tabela que contem as informações da tela inicial da bov
                                case "-tabelaInfoBov":
                                    PropTabelaInfoBov.TabelaInfoBov = argtoVal;
                                    break;

                                //Nome da tabela que contem o nome de todos os relatórios que serão consultados diariamente
                                case "-tabelaRelatorios":
                                    PropTabelaRelatorios.TabelaRelatorios = argtoVal;
                                    break;

                                //Nome da tabela que contem as informações de alertas
                                case "-tabelaAdmin":
                                    PropTabelaAdmin.TabelaAdmin = argtoVal;
                                    break;

                                //Nome da tabela que contem os cabeçalhos dos arquivos textos da bov
                                case "-tabelaCabecalho":
                                    PropTabelaCabecalho.TabelaCabecalho = argtoVal;
                                    break;

                                //Nome da tabela que contem os principais alertas que serão emitidos aos usuarios
                                case "-tabelaAlerta":
                                    PropTabelaAlerta.TabelaAlerta = argtoVal;
                                    break;

                                //Nome da tabela que contem as informações de alterações nos cabeçalhos
                                case "-tabelaAlteracoes":
                                    PropTabelaAlteracoes.TabelaAlteracoes = argtoVal;
                                    break;

                                //Diretorio que guardará os arquivos tratados
                                case "-dirlocal":
                                    PropDirLocal.DirLocal = argtoVal;
                                    break;

                                //Diretorio que receberá os arquivos tratados
                                case "-dirremoto":
                                    PropDirRemoto.DirRemoto = argtoVal;
                                    break;

                                //Diretorio que receberá as informações de log
                                case "-dirlog":
                                    PropDirLog.DirLog = argtoVal;
                                    break;

                                //Quantidade de dias que o relatorio deverá gerar
                                case "-qtddias":
                                    if (!string.IsNullOrEmpty(argtoVal)) { PropQtdDiasRelatorios.QtdDiasRelatorio = int.Parse(argtoVal); ; }
                                    else { PropQtdDiasRelatorios.QtdDiasRelatorio = 0; }
                                    break;

                                //Flag para identificar se deverá ser executado o fechamento do mês
                                case "-fechames":
                                    if (!string.IsNullOrEmpty(argtoVal)) { PropFechamentoMes.FechamentoMes = int.Parse(argtoVal); }
                                    else { PropFechamentoMes.FechamentoMes = 0; }
                                    break;

                                //Quantidade de meses que o relatorio irá buscar
                                case "-qtdmeses":
                                    if (!string.IsNullOrEmpty(argtoVal)) { PropQtdMesRelatorio.QtdMesRelatorio = int.Parse(argtoVal); }
                                    else { PropQtdMesRelatorio.QtdMesRelatorio = 0; }
                                    break;

                                //Usuario de rede
                                case "-userrede":
                                    PropUsuarioRede.UsuarioRede = argtoVal;
                                    break;

                                //Define flag de data manual
                                case "-datamanual":
                                    if (!string.IsNullOrEmpty(argtoVal)) { PropDataManual.DataManual = int.Parse(argtoVal); }
                                    else { PropDataManual.DataManual = 0; }
                                    break;

                                //Insere manualmente a data inicio
                                case "-datainicio":
                                    PropDataInicioExtrairBov.DataInicioExtrairBov = argtoVal;
                                    break;

                                //Insere manualmente a data fim
                                case "-datafim":
                                    PropDataFimExtrairBov.DataFimExtrairBov = argtoVal;
                                    break;

                                case "-horadownload":
                                    PropHoraDownloadBov.HoraExecucaoDownload = argtoVal;
                                    break;

                                default:
                                    //Caso não encontre um argumento valido, retorna erro
                                    throw new ArgumentException(string.Concat("Argumento //", argto, "// inválido"));
                            }
                        }
                    }
                }
                //Caso ocorra alguma exceção, retorna erro
                catch (Exception ex)
                {
                    throw new ArgumentException(ex.Message);
                }
            }
            //Valida os valores
            ValidateInitialize();

            //Chama metodo de data
            CarregarDataInicializacao(PropQtdDiasRelatorios.QtdDiasRelatorio, PropDataManual.DataManual, PropFechamentoMes.FechamentoMes, PropDataInicioExtrairBov.DataInicioExtrairBov, PropDataFimExtrairBov.DataFimExtrairBov);
        }


        public static AppConfig CriarConfig()
        {
            return new AppConfig
            {

                ServidorBancoDados = PropServidorBancoDados.ServidorBancoDados,
                LoginBd = PropLoginBd.LoginBd,
                SenhaBd = PropSenhaBd.SenhaBd,
                NomeBd = PropNomeBd.NomeBd,
                NomeSchema = PropSchemaBd.NomeSchema,
                TabelaArquivos = PropTabelaArquivos.TabelaArquivos,
                TabelaInfoBov = PropTabelaInfoBov.TabelaInfoBov,
                TabelaRelatorios = PropTabelaRelatorios.TabelaRelatorios,
                TabelaAdmin = PropTabelaAdmin.TabelaAdmin,
                TabelaCabecalho = PropTabelaCabecalho.TabelaCabecalho,
                TabelaAlerta = PropTabelaAlerta.TabelaAlerta,
                TabelaAlteracoes = PropTabelaAlteracoes.TabelaAlteracoes,

                DirLocal = PropDirLocal.DirLocal,
                DirRemoto = PropDirRemoto.DirRemoto,
                DirLog = PropDirLog.DirLog,
                DirDestino = PropDirDestino.DirDestino,

                DataFormatada = PropDataFormatada.DataFormatada,
                DataInicioExtrairBov = PropDataInicioExtrairBov.DataInicioExtrairBov,
                DataFimExtrairBov = PropDataFimExtrairBov.DataFimExtrairBov,
                QtdDiasRelatorio = PropQtdDiasRelatorios.QtdDiasRelatorio,
                QtdMesRelatorio = PropQtdMesRelatorio.QtdMesRelatorio,
                FechamentoMes = PropFechamentoMes.FechamentoMes,
                DataManual = PropDataManual.DataManual,
                HoraExecucaoDownload = PropHoraDownloadBov.HoraExecucaoDownload,
                UsuarioRede = PropUsuarioRede.UsuarioRede,


                ChromeDriverPath = PropChrome.DriverPath,
                ChromeBinaryPath = PropChrome.BinaryPath,
                ProxyPacUrl = PropChrome.ProxyPacUrl,


                ConnectionString = $"Server={PropServidorBancoDados.ServidorBancoDados};" +
                                   $"Database={PropNomeBd.NomeBd};" +
                                   $"User Id={PropLoginBd.LoginBd};" +
                                   $"Password={PropSenhaBd.SenhaBd};"
            };
        }


        public static class PropChrome
        {
            private static readonly string BaseDir = AppDomain.CurrentDomain.BaseDirectory;
            public static string DriverPath => BaseDir;
            public static string BinaryPath => Path.Combine(BaseDir, "chrome-win64", "chrome.exe");
            public static string ProxyPacUrl => "http://proxy.company.local/proxy.pac";
        }


        public class PropServidorBancoDados
        {
            public static string ServidorBancoDados
            {
                get
                {
                    return servidorBancoDados;
                }
                set
                {
                    servidorBancoDados = value;
                }
            }
        }
        public class PropLoginBd
        {
            public static string LoginBd
            {
                get
                {
                    return loginBd;
                }
                set
                {
                    loginBd = value;
                }
            }
        }

        public class PropSenhaBd
        {
            public static string SenhaBd
            {
                get
                {
                    return senhaBd;
                }
                set
                {
                    senhaBd = value;
                }
            }
        }

        public class PropNomeBd
        {
            public static string NomeBd
            {
                get
                {
                    return nomeBd;
                }
                set
                {
                    nomeBd = value;
                }
            }
        }

        public class PropSchemaBd
        {
            public static string NomeSchema
            {
                get
                {
                    return nomeSchema;
                }
                set
                {
                    nomeSchema = value;
                }
            }
        }

        public class PropTabelaArquivos
        {
            public static string TabelaArquivos
            {
                get
                {
                    return tabelaArquivos;
                }
                set
                {
                    tabelaArquivos = value;
                }
            }
        }

        public class PropTabelaInfoBov
        {
            public static string TabelaInfoBov
            {
                get
                {
                    return tabelaInfoBov;
                }
                set
                {
                    tabelaInfoBov = value;
                }
            }
        }

        public class PropTabelaRelatorios
        {
            public static string TabelaRelatorios
            {
                get
                {
                    return tabelaRelatorios;
                }
                set
                {
                    tabelaRelatorios = value;
                }
            }
        }

        public class PropTabelaAdmin
        {
            public static string TabelaAdmin
            {
                get
                {
                    return tabelaAdmin;
                }
                set
                {
                    tabelaAdmin = value;
                }
            }
        }

        public class PropTabelaCabecalho
        {
            public static string TabelaCabecalho
            {
                get
                {
                    return tabelaCabecalho;
                }
                set
                {
                    tabelaCabecalho = value;
                }
            }
        }

        public class PropTabelaAlerta
        {
            public static string TabelaAlerta
            {
                get
                {
                    return tabelaAlerta;
                }
                set
                {
                    tabelaAlerta = value;
                }
            }
        }

        public class PropTabelaAlteracoes
        {
            public static string TabelaAlteracoes
            {
                get
                {
                    return tabelaAlteracoes;
                }
                set
                {
                    tabelaAlteracoes = value;
                }
            }
        }

        public class PropDirLocal
        {
            public static string DirLocal
            {
                get
                {
                    return dirLocal;
                }
                set
                {
                    dirLocal = value;
                }
            }
        }

        public class PropDirRemoto
        {
            public static string DirRemoto
            {
                get
                {
                    return dirRemoto;
                }
                set
                {
                    dirRemoto = value;
                }
            }
        }

        public class PropDirLog
        {
            public static string DirLog
            {
                get
                {
                    return dirLog;
                }
                set
                {
                    dirLog = value;
                }
            }
        }

        public class PropDirDestino
        {
            public static string DirDestino
            {
                get
                {
                    return dirDestino;
                }
                set
                {
                    dirDestino = value;
                }
            }
        }

        public class PropDataFormatada
        {
            public static string DataFormatada
            {
                get
                {
                    return dataFormatada;
                }
                set
                {
                    dataFormatada = value;
                }
            }
        }

        public class PropDataInicioExtrairBov
        {
            public static string DataInicioExtrairBov
            {
                get
                {
                    return dataInicioExtrairBov;
                }
                set
                {
                    dataInicioExtrairBov = value;
                }
            }
        }

        public class PropDataFimExtrairBov
        {
            public static string DataFimExtrairBov
            {
                get
                {
                    return dataFimExtrairBov;
                }
                set
                {
                    dataFimExtrairBov = value;
                }
            }
        }

        public class PropQtdDiasRelatorios
        {

            public static int QtdDiasRelatorio
            {
                get
                {
                    return qtdDiasRelatorio;
                }
                set
                {
                    qtdDiasRelatorio = value;
                }
            }
        }

        public class PropQtdMesRelatorio
        {
            public static int QtdMesRelatorio
            {
                get
                {
                    return qtdMesRelatorio;
                }
                set
                {
                    qtdMesRelatorio = value;
                }
            }
        }

        public class PropFechamentoMes
        {
            public static int FechamentoMes
            {
                get
                {
                    return fechamentoMes;
                }
                set
                {
                    fechamentoMes = value;
                }
            }
        }

        public class PropUsuarioRede
        {
            public static string UsuarioRede
            {
                get
                {
                    return usuarioRede;
                }
                set
                {
                    usuarioRede = value;
                }
            }
        }

        public class PropDataManual
        {
            public static int DataManual
            {
                get
                {
                    return dataManual;
                }
                set
                {
                    dataManual = value;
                }
            }
        }

        public class PropHoraDownloadBov
        {
            public static string HoraExecucaoDownload
            {
                get
                {
                    return horaExecucaoDownload;
                }
                set
                {
                    horaExecucaoDownload = value;
                }
            }
        }

        public static bool CriarPastasAuxiliares()
        {
            try
            {
                //Cria os diretorios auxiliares
                if (Directory.Exists(dirLocal))
                {
                    Directory.CreateDirectory(dirLocal);
                }

                //Cria os diretorios auxiliares
                if (Directory.Exists(dirLog))
                {
                    Directory.CreateDirectory(dirLog);
                }
                return true;
            }
            catch (Exception ex)
            {
                //Gera Log
                //_log.Gravar("ERRO", "Não foi possível criar os diretórios auxiliares", ex.Message);

                throw new ArgumentException(
                        $"Não foi possível criar os diretórios: {ex.Message}"
                    );

            }
        }

        //Exibe os valores de cada variavel interna
        public static bool ExibirValores()
        {
            //Exibição dos valores
            Console.WriteLine(string.Concat(Environment.NewLine, "------------------------------------------------------------------------------------------"));
            Console.WriteLine(string.Concat("Servidor de Banco Dados: ", PropServidorBancoDados.ServidorBancoDados));
            Console.WriteLine(string.Concat("Login do Banco de Dados: ", PropLoginBd.LoginBd));
            Console.WriteLine(string.Concat("Nome do Banco de Dados: ", PropNomeBd.NomeBd));
            Console.WriteLine(string.Concat("Schema do Banco de Dados: ", PropSchemaBd.NomeSchema));
            Console.WriteLine(string.Concat("Diretório Local: ", PropDirLocal.DirLocal));
            Console.WriteLine(string.Concat("Diretório de Rede: ", PropDirRemoto.DirRemoto));
            Console.WriteLine(string.Concat("Diretório do Arquivo de Log: ", PropDirLog.DirLog));
            Console.WriteLine(string.Concat("Hora de Execução: ", PropHoraDownloadBov.HoraExecucaoDownload));
            Console.WriteLine(string.Concat("Data Formatada: ", PropDataFormatada.DataFormatada));
            Console.WriteLine(string.Concat("Data de Início da Extração: ", PropDataInicioExtrairBov.DataInicioExtrairBov));
            Console.WriteLine(string.Concat("Data de Fim da Extração: ", PropDataFimExtrairBov.DataFimExtrairBov));
            Console.WriteLine(string.Concat("Deta Dias do relatório: ", PropQtdDiasRelatorios.QtdDiasRelatorio));
            Console.WriteLine(string.Concat("Fechamento do Mês: ", PropFechamentoMes.FechamentoMes));
            Console.WriteLine(string.Concat("Usuário de Rede: ", PropUsuarioRede.UsuarioRede));
            Console.WriteLine(string.Concat("------------------------------------------------------------------------------------------", Environment.NewLine));

            return true;
        }

        //Método validade para inicialização
        static public bool ValidateInitialize()
        {
            string inconsistenciasPreenchimento = string.Empty;
            int contErros = 0;
            DateTime dataT;
            string formato = "dd/MM/yyyy";

            //Valida PropServidorBancoDados
            if (PropServidorBancoDados.ServidorBancoDados.Length == 0)
            {
                PropServidorBancoDados.ServidorBancoDados = CONSTANTE_HOST_DB;
            }

            //Valida PropLoginBd
            if (string.IsNullOrEmpty(PropLoginBd.LoginBd))
            {
                PropLoginBd.LoginBd = CONSTANTE_LOGIN_DB;
            }

            //Valida PropSenhaBd
            if (string.IsNullOrEmpty(PropSenhaBd.SenhaBd))
            {
                PropSenhaBd.SenhaBd = CONSTANTE_PASSWORD_DB;
            }

            //Valida PropNomeBd
            if (string.IsNullOrEmpty(PropNomeBd.NomeBd))
            {
                PropNomeBd.NomeBd = CONSTANTE_NAME_DB;
            }

            //Valida nomeSchema
            if (string.IsNullOrEmpty(PropSchemaBd.NomeSchema))
            {
                PropSchemaBd.NomeSchema = CONSTANTE_NAME_SCHEMA;
            }

            //Valida tabela arquivos
            if (string.IsNullOrEmpty(PropTabelaArquivos.TabelaArquivos))
            {
                PropTabelaArquivos.TabelaArquivos = CONSTANTE_TABELA_ARQUIVOS;
            }

            //Valida tabela info bov
            if (string.IsNullOrEmpty(PropTabelaInfoBov.TabelaInfoBov))
            {
                PropTabelaInfoBov.TabelaInfoBov = CONSTANTE_TABELA_INFO_BOV;
            }

            //Valida tabela relatórios
            if (string.IsNullOrEmpty(PropTabelaRelatorios.TabelaRelatorios))
            {
                PropTabelaRelatorios.TabelaRelatorios = CONSTANTE_TABELA_RELATORIOS;
            }

            //Valida tabela admin
            if (string.IsNullOrEmpty(PropTabelaAdmin.TabelaAdmin))
            {
                PropTabelaAdmin.TabelaAdmin = CONSTANTE_TABELA_ADMIN;
            }

            //Valida tabela cabeçalho
            if (string.IsNullOrEmpty(PropTabelaCabecalho.TabelaCabecalho))
            {
                PropTabelaCabecalho.TabelaCabecalho = CONSTANTE_TABELA_CABECALHO;
            }

            //Valida tabela alerta
            if (string.IsNullOrEmpty(PropTabelaAlerta.TabelaAlerta))
            {
                PropTabelaAlerta.TabelaAlerta = CONSTANTE_TABELA_ALERTA;
            }

            //Valida tabela alterações
            if (string.IsNullOrEmpty(PropTabelaAlteracoes.TabelaAlteracoes))
            {
                PropTabelaAlteracoes.TabelaAlteracoes = CONSTANTE_TABELA_ALTERACOES;
            }

            //Valida PropDirLocal
            if (string.IsNullOrEmpty(PropDirLocal.DirLocal))
            {
                PropDirLocal.DirLocal = CONSTANTE_DIR_LOCAL;
            }

            //Valida PropDirRemoto
            if (string.IsNullOrEmpty(PropDirRemoto.DirRemoto))
            {
                PropDirRemoto.DirRemoto = CONSTANTE_DIR_REDE;
            }

            //Valida PropDirLog
            if (string.IsNullOrEmpty(PropDirLog.DirLog))
            {
                PropDirLog.DirLog = dirLog;
            }

            //Valida PropDirDestino
            if (string.IsNullOrEmpty(PropDirDestino.DirDestino))
            {
                PropDirDestino.DirDestino = string.Concat("C:\\Users\\", usuarioRede, "\\Documents\\iMacros\\DataSources\\");
            }

            //Verifica se valor QtdDiasRelatorios é numerico
            if (!(PropQtdDiasRelatorios.QtdDiasRelatorio > 0))
            {
                inconsistenciasPreenchimento += string.Concat("* Valor de dias do relatório não númerico", Environment.NewLine);
                contErros += 1;
            }

            //Verifica se valor qtdDiasRelatorio é numerico
            if (!(PropQtdMesRelatorio.QtdMesRelatorio > 0))
            {
                inconsistenciasPreenchimento += string.Concat("* Valor de meses do relatório não númerico", Environment.NewLine);
                contErros += 1;
            }

            //Verifica se data é manual
            if (PropDataManual.DataManual == 1)
            {

                //Verifica se valor data inicio é do tipo data
                if (!DateTime.TryParseExact(PropDataInicioExtrairBov.DataInicioExtrairBov, formato, new CultureInfo("pt-BR"), DateTimeStyles.None, out dataT))
                {
                    inconsistenciasPreenchimento += string.Concat("* Valor de dataInicio não é data ou data invalida", Environment.NewLine);
                    contErros += 1;
                }

                //Verifica se valor data fim é do tipo data
                if (!DateTime.TryParseExact(PropDataFimExtrairBov.DataFimExtrairBov, formato, new CultureInfo("pt-BR"), DateTimeStyles.None, out dataT))
                {
                    inconsistenciasPreenchimento += string.Concat("* Valor de dataFim não é data ou data invalida", Environment.NewLine);
                    contErros += 1;
                }

                //Verifica se ambos valores são datas
                if (!(PropDataInicioExtrairBov.DataInicioExtrairBov == null) && !(PropDataFimExtrairBov.DataFimExtrairBov == null))
                {

                    //Verifica se o valor inicial é menor que valor final
                    if (DateTime.Parse(PropDataInicioExtrairBov.DataInicioExtrairBov) > DateTime.Parse(PropDataFimExtrairBov.DataFimExtrairBov))
                    {
                        inconsistenciasPreenchimento += string.Concat("* A data inicial deve ser menor ou igual a data final", Environment.NewLine);
                        contErros += 1;
                    }
                }
            }

            //Verifica se encontrou algum erro
            if (contErros > 0)
            {
                //Gera Log
                //_log.Gravar("ERRO", string.Concat("Foram detectados ", contErros, " erros"), inconsistenciasPreenchimento);
                throw new ArgumentException(string.Concat("Foram detectados ", contErros, " erros.", Environment.NewLine, Environment.NewLine, inconsistenciasPreenchimento));
            }
            else
            {
                //Retorna verdadeiro
                return true;
            }
        }

        //Metodo para definir data global
        static private bool CarregarDataInicializacao(int diasAnteriores, int dataManual, int fechamentoMes, string dataInicial, string dataFinal)
        {
            string dataRelatorios;
            string dia;
            string mes;
            string ano;

            //Verifica se data é manual
            if (dataManual == 1)
            {
                //Realiza separação do dia, mes e ano
                dia = DateTime.Parse(dataFinal).ToString("dd");
                mes = DateTime.Parse(dataFinal).ToString("MM");
                ano = DateTime.Parse(dataFinal).ToString("yyyy");

                //Incrementa as variaveis da classe
                dataFormatada = string.Concat(ano, mes, dia);
                dataInicioExtrairBov = dataInicial;
                dataFimExtrairBov = dataFinal;
            }
            //Verifica a data para obter relatorio
            else if (fechamentoMes == 1)
            {
                //Obtem data do relatorio
                dataRelatorios = DateTime.Now.AddMonths(-PropQtdMesRelatorio.QtdMesRelatorio).ToString("dd/MM/yyyy");

                //Trata informação para identificar o primeiro e ultimo dia do mes anterior
                string primeiroDia = string.Concat("01/", dataRelatorios.Substring(3, 7));
                string ultimoDia = DateTime.Parse(primeiroDia).AddMonths(1).AddDays(-1).ToString("dd/MM/yyyy");

                //Realiza separação do dia, mes e ano
                dia = DateTime.Parse(ultimoDia).ToString("dd");
                mes = DateTime.Parse(ultimoDia).ToString("MM");
                ano = DateTime.Parse(ultimoDia).ToString("yyyy");

                //Incrementa as variaveis da classe
                dataFormatada = string.Concat(ano, mes, dia);
                dataInicioExtrairBov = string.Concat("01/", mes, "/", ano);
                dataFimExtrairBov = string.Concat(dia, "/", mes, "/", ano);
            }
            else
            {
                //Obtem data do relatorio
                dataRelatorios = DateTime.Now.AddDays(-diasAnteriores).ToString("dd/MM/yyyy");

                //Realiza separação do dia, mes e ano
                dia = DateTime.Parse(dataRelatorios).ToString("dd");
                mes = DateTime.Parse(dataRelatorios).ToString("MM");
                ano = DateTime.Parse(dataRelatorios).ToString("yyyy");

                //Incrementa as variaveis da classe
                dataFormatada = string.Concat(ano, mes, dia);
                dataInicioExtrairBov = string.Concat("01/", mes, "/", ano);
                dataFimExtrairBov = string.Concat(dia, "/", mes, "/", ano);
            }
            return true;
        }

        //Função responsável por obter o login de rede do usuario que está logado no momento da execução do script (Necessário já que a BOV utiliza o login de rede do usuario)
        public static string ObterUsuarioLocal()
        {
            string usuario = Environment.UserName;
            // Retorna o nome do usuario
            return usuario;
        }
    }
}
