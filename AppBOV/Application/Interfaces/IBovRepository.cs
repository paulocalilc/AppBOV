using System.Collections.Generic;
using System.Data.SqlClient;

public interface IBovRepository
{
    bool ExecuteProcedure(string nomeProcedure, string mesRef);

    void AtualizarStatusUpload(string idArquivo, string status);

    bool ExecutarBulkInsert(string arquivo, string tabela, string rowTerminator);

    void CriarTabelaTemporaria(
        string nomeTabela,
        List<(string Nome, int Tamanho)> campos
    );

    List<string> ObterLayoutAtivo(string numBov);

    void InserirNovoLayout(string numBov, string[] campos);

    void DesativarLayout(string numBov);

    int InserirAlertaAlteracao(string numBov, int qtd, string idArquivo);

    void InserirAlteracoes(List<InconsistenciaDTO> lista, int idAlerta);

    void LimparTabelaChecks();

    List<BovPendenteDTO> ObterBovsPendentes();

    void AtualizarStatusDownload(string idArquivo, string status);

    SqlDataReader ExecuteQuery(string query);

    bool TodosArquivosForamProcessados();

    List<AlertaDTO> ObterAlertas();

    List<string> ObterDestinatarios();

    void InserirStatusSemAcesso(string numBov);

    void InserirArquivoBov(string numBov, string usuario);
}
