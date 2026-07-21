using AppBOV.CrossCutting.Config;
using System;
using System.Data;
using System.Data.SqlClient;

public class BovRepository : IBovRepository
{
    //Definição das variaveis de conexão
    private static string connectionString;
    SqlConnection connection;
    SqlCommand command;

    private readonly ILogService _log;
    private readonly AppConfig _config;

    public BovRepository(
        AppConfig config,
        ILogService log)
    {
        _config = config;
        _log = log;
    }

    //Abrir banco de dados"
    //Responsável pela abertura da conexão
    private bool OpenConn()
    {
        //String de conexão (Pode ser mutavel de acordo com o banco de dados)
        connectionString = string.Concat("Server=", _config.ServidorBancoDados,
                            ";Database=", _config.NomeBd,
                            ";User Id=", _config.LoginBd,
                            ";Password=", _config.SenhaBd,
                            ";Trusted_Connection=False; Min Pool Size=5;Max Pool Size=300;Connect Timeout=3");

        //Atribui string de conexão a variavel de conexão
        connection = new SqlConnection(_config.ConnectionString);

        try
        {
            //Tenta realizar conexão
            connection.Open();

            //Grava LOG
            _log.Gravar("INFORMAÇÃO", "Abertura de conexão", "");

            return true;
        }
        catch (SqlException ex)
        {
            //Gera Log
            _log.Gravar("ERRO", "Houve uma falha na conexão ao banco de dados", ex.Message);

            //Retorna mensagem de erro
            throw new ArgumentException(string.Concat("Houve uma falha na conexão ao banco de dados", Environment.NewLine, "Erro: ", ex.Message));
        }
    }

    //"Fechar banco de dados"
    //Responsável por encerrar a conexão com o banco de dados
    public bool CloseConn()
    {
        //Verifica se existe alguma conexão
        if ((connection is null) is false)
        {
            //Caso exista uma instancia de conexão e esteja aberta, realiza o encerramento
            if (connection.State == ConnectionState.Open)
            {
                //Fecha conexão
                connection.Close();
                return true;
            }
            else
            {
                //Não existe conexão aberta
                return false;
            }
        }
        else
        {
            //Não existe instancia de conexão
            return false;
        }
    }

    //"Verificar status da conexão"
    //Responsável por verificar o status da conexão
    public string CheckStatusConn()
    {
        //Verifica se existe alguma conexão
        if (connection.State == ConnectionState.Open)
        {
            //Caso exista uma instancia de conexão, verifica se está aberta
            if (connection.State == ConnectionState.Open)
            {
                return "Conexão aberta!";
            }
            else
            {
                //Returna informando que está fechada
                return "Conexão fechada!";
            }
        } //Não existe instancia de conexão
        else
        {
            return "Nenhuma conexão iniciada!";
        }
    }

    //"Executar Query"
    //Responsável por executar querys diretamente do código
    public SqlDataReader ExecuteQuery(string query)
    {
        //Executa querys passadas por parametro ao método
        try
        {
            //Grava LOG
            _log.Gravar("INFORMAÇÃO", string.Concat("Executado query ", query), "");

            //Abre conexão
            OpenConn();

            //Instancia execução de novo código SQL
            command = new SqlCommand();

            //Aumento de tempo do timeout das consultas. Modificado o valor default de 30 segundos devido a tempo de execução do bulkinsert
            command.CommandTimeout = 30000;
            command.CommandType = CommandType.Text;
            command.CommandText = query;
            command.Connection = connection;

            //Atribui valor da consulta
            return command.ExecuteReader();

            //Verifica se ocorreu excessão por parte do código SQL
        }
        catch (SqlException ex)
        {
            //Grava LOG
            _log.Gravar("ERRO", "Falha ao executar consulta SQL", ex.Message);
            throw new ArgumentException(string.Concat("Falha ao executar consulta SQL", Environment.NewLine, "SqlException: ", ex.Message));

        }   //Verifica se ocorreu algum outro erro   
        catch (Exception ex)
        {
            //Gera Log
            _log.Gravar("ERRO", "Falha ao realizar operação", ex.Message);
            throw new ArgumentException(string.Concat("Falha ao realizar operação", Environment.NewLine, "Exception: ", ex.Message));
        }
    }

    //Responsável por executar as chamadas de procedures que irão alocar as informações das tabelas temproarias nas tabelas de produção
    public bool ExecuteProcedure(string nomeProcedure, string mesRef)
    {
        //Executa querys passadas por parametro ao método
        try
        {
            //Grava LOG
            _log.Gravar("INFORMAÇÃO", string.Concat("Executado procedure ", nomeProcedure), "");

            //Abre conexão
            OpenConn();

            //Instancia execução de novo código SQL
            SqlCommand cmd = new SqlCommand(nomeProcedure, connection);
            cmd.CommandTimeout = 30000;
            cmd.CommandType = CommandType.StoredProcedure;

            //Recebe parametro que será usado na procedure
            cmd.Parameters.Add("@MES_REF", SqlDbType.VarChar).Value = mesRef;

            //Executa procedure
            cmd.ExecuteNonQuery();

            return true;

        } //Verifica se ocorreu excessão por parte do código SQL
        catch (SqlException ex)
        {
            //Grava LOG
            _log.Gravar("ERRO", "Falha ao executar consulta SQL", ex.Message);
            throw new ArgumentException(string.Concat("Falha ao executar consulta SQL", Environment.NewLine, "SqlException: ", ex.Message));

        } //Verifica se ocorreu algum outro erro   
        catch (Exception ex)
        {
            //Gera Log
            _log.Gravar("ERRO", "Falha ao realizar operação", ex.Message);
            throw new ArgumentException(string.Concat("Falha ao realizar operação", Environment.NewLine, "Exception: ", ex.Message));
        }
    }

    public List<AlertaDTO> ObterAlertas()
    {
        var lista = new List<AlertaDTO>();

        string sql = $@"
            SELECT
                PROCESSADOS.id_arquivo,
                RELATORIOS.num_bov,
                PROCESSADOS.nome_bov,
                PROCESSADOS.data_hora_registro,
                CASE WHEN PROCESSADOS.filtro_bov = 'true' THEN 'SIM' else 'NÃO' END AS FILTRO_BOV,
                PROCESSADOS.status_download_bov,
                PROCESSADOS.data_hora_registro
            FROM
                [{_config.NomeSchema}].[{_config.TabelaRelatorios}] AS RELATORIOS
                LEFT JOIN [{_config.NomeSchema}].[{_config.TabelaArquivos}] AS PROCESSADOS 
                ON RELATORIOS.num_bov = PROCESSADOS.num_bov AND
                PROCESSADOS.periodo_ate_bov = '{_config.DataFormatada}' AND
                PROCESSADOS.data_hora_registro Is Not NULL
            WHERE
                RELATORIOS.status_relatorio = 1 AND
                PROCESSADOS.reprocessamento = 0
            ORDER BY CAST(PROCESSADOS.num_bov AS INT)";

        using (var reader = ExecuteQuery(sql))
        {
            while (reader.Read())
            {
                lista.Add(new AlertaDTO
                {
                    IdArquivo = Convert.ToString(reader.GetValue(0)),
                    NumBov = Convert.ToString(reader.GetValue(1)),
                    NomeBov = Convert.ToString(reader.GetValue(2)),
                    DataRegistro = Convert.ToString(reader.GetValue(3)),
                    FiltroEmpresarial = Convert.ToString(reader.GetValue(4)),
                    StatusUpload= Convert.ToString(reader.GetValue(5)),
                    UltimoRegistro = Convert.ToString(reader.GetValue(6))

                });
            }
        }

        return lista;
    }

    public List<string> ObterDestinatarios()
    {
        var lista = new List<string>();

        string sql = $@"
        SELECT email_usuario 
        FROM [{_config.NomeSchema}].[{_config.TabelaAdmin}] 
        WHERE status_usuario = 1 
          AND status_alerta_email = 1";

        using (var reader = ExecuteQuery(sql))
        {
            while (reader.Read())
            {
                lista.Add(Convert.ToString(reader.GetValue(0)));
            }
        }

        return lista;
    }

    public void AtualizarStatusDownload(string idArquivo, string status)
    {
        var sql = $@"
        UPDATE [{_config.NomeBd}].[{_config.NomeSchema}].[{_config.TabelaArquivos}]
        SET data_hora_registro = '{DateTime.Now:yyyy-MM-dd HH:mm:ss}',
            status_download_bov = '{status}'
        WHERE num_bov = '{idArquivo}'
            AND PERIODO_ATE_BOV = {_config.DataFormatada}";

        if (status == "em branco")
            sql = $@"
        UPDATE [{_config.NomeBd}].[{_config.NomeSchema}].[{_config.TabelaArquivos}]
        SET data_hora_registro = '{DateTime.Now:yyyy-MM-dd HH:mm:ss}',
            status_download_bov = '{status}',
            status_upload_bov = 1
        WHERE num_bov = '{idArquivo}'
            AND PERIODO_ATE_BOV = {_config.DataFormatada}";

        using var dr = ExecuteQuery(sql);
    }


    public void InserirArquivoBov(string numBov, string usuario)
    {
        var sql = $@"
            INSERT INTO [{_config.NomeBd}].[{_config.NomeSchema}].[{_config.TabelaArquivos}]
                (periodo_ate_bov, data_hora_registro, status_download_bov, num_bov, reprocessamento, usuario_bov)
            VALUES
                ('{_config.DataFormatada}', '{DateTime.Now:yyyy-MM-dd HH:mm:ss}', 'solicitado', '{numBov}', '0', '{usuario}')";

        using var dr = ExecuteQuery(sql);
    }

    public void LimparTabelaChecks()
    {
        var sql = @"
            DECLARE @DATA_MIN DATETIME, @DATA_ATUAL DATETIME;

            SET @DATA_MIN = (SELECT MIN(DATA_CHECK) FROM TBL_PERM_CHECKS_CARGAS);
            SET @DATA_ATUAL = GETDATE();

            IF (DATEDIFF(D,@DATA_MIN,@DATA_ATUAL) >= 0.8)
                TRUNCATE TABLE TBL_PERM_CHECKS_CARGAS";

        using var dr = ExecuteQuery(sql);
    }

    public List<BovPendenteDTO> ObterBovsPendentes()
    {
        var lista = new List<BovPendenteDTO>();

        using (var conexao = new SqlConnection(_config.ConnectionString))
        {
            conexao.Open();

            var sql = $@"
        SELECT 
            relatorios.num_bov,
            relatorios.filtro_grupo_unidade,
            relatorios.filtro_regiao,
            relatorios.sistema_relatorio,
            relatorios.automatico,
            processados.id_bov,
            processados.usuario_bov,
            processados.periodo_ate_bov,
            processados.data_hora_registro,
            relatorios.nome_bov,
            processados.id_arquivo,
            relatorios.tabela_temporaria,
            relatorios.nome_procedure
        FROM [{_config.NomeBd}].[{_config.NomeSchema}].[{_config.TabelaRelatorios}] relatorios
        LEFT JOIN [{_config.NomeBd}].[{_config.NomeSchema}].[{_config.TabelaArquivos}] processados
            ON relatorios.num_bov = processados.num_bov
            AND processados.periodo_ate_bov = {_config.DataFormatada}
            AND (processados.usuario_bov = '{_config.UsuarioRede}' OR processados.usuario_bov = 'auto')
            AND CONVERT(VARCHAR(10), GETDATE(), 110) = CONVERT(VARCHAR(10), processados.data_hora_registro, 110)
            AND (processados.reprocessamento <> 1 OR processados.reprocessamento IS NULL)
        WHERE 
            ISNULL(processados.status_download_bov, '') <> 'sem acesso'
            AND processados.status_upload_bov IS NULL
            AND relatorios.status_relatorio = 1
            AND relatorios.sistema_relatorio IN ('bov','bio')";

            using (var cmd = new SqlCommand(sql, conexao))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    lista.Add(new BovPendenteDTO
                    {
                        NumBov = reader["num_bov"]?.ToString(),
                        SistemaRelatorio = reader["sistema_relatorio"]?.ToString(),
                        Automatico = reader["automatico"]?.ToString(),
                        IdBov = reader["id_bov"]?.ToString(),
                        UsuarioBov = reader["usuario_bov"]?.ToString(),
                        DataHoraRegistro = reader["data_hora_registro"]?.ToString(),
                        NomeBov = reader["nome_bov"]?.ToString(),
                        IdArquivo = reader["id_arquivo"]?.ToString(),
                        TabelaTemporaria = reader["tabela_temporaria"]?.ToString(),
                        NomeProcedure = reader["nome_procedure"]?.ToString()
                    });
                }
            }
        }

        return lista;
    }

    public int ObterQuantidadeProcessadosHoje()
    {
        var sql = $@"
        SELECT COUNT(*)
        FROM [{_config.NomeSchema}].[{_config.TabelaArquivos}]
        WHERE periodo_ate_bov = {_config.DataFormatada}
          AND status_upload_bov IS NOT NULL";

        using var reader = ExecuteQuery(sql);

        if (reader.Read())
            return Convert.ToInt32(reader.GetValue(0));

        return 0;
    }

    public bool TodosArquivosForamProcessados()
    {
        var sql = $@"
        SELECT COUNT(*) AS Pendentes
        FROM [{_config.NomeSchema}].[{_config.TabelaArquivos}]
        WHERE periodo_ate_bov = {_config.DataFormatada}
          AND (
            status_upload_bov IS NULL
            OR status_download_bov IN ('erro', 'pendente', 'em processamento')
          )";

        using var reader = ExecuteQuery(sql);

        if (reader.Read())
        {
            var pendentes = Convert.ToInt32(reader["Pendentes"]);
            return pendentes == 0;
        }

        return false;
    }

    public void InserirStatusSemAcesso(string numBov)
    {
        var sql = $@"
        INSERT INTO [{_config.NomeBd}].[{_config.NomeSchema}].[{_config.TabelaArquivos}]
        (periodo_ate_bov, data_hora_registro, status_download_bov, num_bov, reprocessamento, usuario_bov)
        VALUES
        ('{_config.DataFormatada}', '{DateTime.Now:yyyy-MM-dd HH:mm:ss}', 'sem acesso', '{numBov}', '0', '{_config.UsuarioRede}')";

        using var dr = ExecuteQuery(sql);
    }

    public void AtualizarStatusUpload(string idArquivo, string status)
    {
        var sql = $@"
        UPDATE [{_config.NomeBd}].[{_config.NomeSchema}].[{_config.TabelaArquivos}]
        SET data_hora_registro = '{DateTime.Now:yyyy-MM-dd HH:mm:ss}',
            status_upload_bov = '{status}'
        WHERE id_arquivo = '{idArquivo}'";

        using var dr = ExecuteQuery(sql);
    }

    public bool ExecutarBulkInsert(string arquivo, string tabela, string rowTerminator)
    {
        try
        {
            var sql = $@"
        SET ANSI_WARNINGS OFF

        BULK INSERT [{_config.NomeBd}].[{_config.NomeSchema}].[{tabela}]
        FROM '{arquivo}'
        WITH (
            CODEPAGE = 'ACP',
            FIELDTERMINATOR = ';',
            DATAFILETYPE = 'char',
            ROWTERMINATOR = '{rowTerminator}',
            FIRSTROW = 2,
            MAXERRORS = 10
        )";

            using var dr = ExecuteQuery(sql);

            return true;
        }
        catch
        {
            return false;
        }
    }

    public void CriarTabelaTemporaria(
    string nomeTabela,
    List<(string Nome, int Tamanho)> campos)
    {
        var colunas = string.Join(",",
            campos.Select(c => $"[{c.Nome}] VARCHAR({c.Tamanho}) NULL"));

        var sql = $@"
        IF OBJECT_ID(N'{_config.NomeBd}.{_config.NomeSchema}.{nomeTabela}', N'U') IS NOT NULL
            DROP TABLE [{_config.NomeBd}].[{_config.NomeSchema}].[{nomeTabela}];

        CREATE TABLE [{_config.NomeBd}].[{_config.NomeSchema}].[{nomeTabela}] (
            {colunas}
        );";

        using var dr = ExecuteQuery(sql);
    }

    public List<string> ObterLayoutAtivo(string numBov)
    {
        var lista = new List<string>();

        var sql = $@"
        SELECT nome_coluna
        FROM [{_config.NomeBd}].[{_config.NomeSchema}].[{_config.TabelaCabecalho}]
        WHERE num_bov = '{numBov}'
          AND status_cabecalho = 1
        ORDER BY indice_cabecalho";

        using var dr = ExecuteQuery(sql);

        while (dr.Read())
            lista.Add(dr["nome_coluna"].ToString());

        return lista;
    }

    public void InserirNovoLayout(string numBov, string[] campos)
    {
        var sql = "";

        int i = 1;

        foreach (var campo in campos)
        {
            sql += $@"
        INSERT INTO [{_config.NomeSchema}].[{_config.TabelaCabecalho}]
        (num_bov, indice_cabecalho, nome_coluna, data_layout, usuario_upload, status_cabecalho)
        VALUES ('{numBov}', {i}, '{campo}', GETDATE(), '{_config.UsuarioRede}', 1);";

            i++;
        }

        using var dr = ExecuteQuery(sql);
    }

    public void DesativarLayout(string numBov)
    {
        var sql = $@"
        UPDATE [{_config.NomeSchema}].[{_config.TabelaCabecalho}]
        SET status_cabecalho = 0
        WHERE num_bov = '{numBov}'";

        using var dr = ExecuteQuery(sql);
    }

    public int InserirAlertaAlteracao(string numBov, int qtd, string idArquivo)
    {
        var sql = $@"
        INSERT INTO [{_config.NomeSchema}].[{_config.TabelaAlerta}]
        (tipo_alerta, num_bov, mensagem_alerta, status_notificacao, data_hora_registro, id_arquivo_bov)
        VALUES ('Alteração cabeçalho', '{numBov}', 'Inconsistências: {qtd}', 0, GETDATE(), '{idArquivo}');
        
        SELECT SCOPE_IDENTITY();";

        using var dr = ExecuteQuery(sql);

        dr.Read();

        return Convert.ToInt32(dr.GetValue(0));
    }

    public void InserirAlteracoes(List<InconsistenciaDTO> lista, int idAlerta)
    {
        var sql = "";

        foreach (var item in lista)
        {
            sql += $@"
        INSERT INTO [{_config.NomeSchema}].[{_config.TabelaAlteracoes}]
        (layout_antigo, layout_novo, id_alerta)
        VALUES ('{item.ColunaBanco}', '{item.ColunaArquivo}', {idAlerta});";
        }

        using var dr = ExecuteQuery(sql);
    }

}