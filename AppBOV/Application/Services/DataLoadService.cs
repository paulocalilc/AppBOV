using AppBOV.Application.Interfaces;
using AppBOV.CrossCutting.Config;

public class DataLoadService : IDataLoadService
{
    private readonly IBovRepository _repository;
    private readonly ILogService _log;
    private readonly AppConfig _config;

    public DataLoadService(
        AppConfig config,
        IBovRepository repository,
        ILogService log)
    {
        _config = config;
        _repository = repository;
        _log = log;
    }

    public bool ProcessarArquivo(
        string arquivo,
        string numBov,
        string idArquivo,
        string tabelaTemp,
        string procedure)
    {
        var cabecalho = File.ReadLines(arquivo).FirstOrDefault();

        if (string.IsNullOrEmpty(cabecalho))
            return false;

        if (!ValidarLayout(numBov, cabecalho, idArquivo))
            return false;

        CriarTabela(cabecalho, tabelaTemp);

        if (!ExecutarBulk(arquivo, tabelaTemp, numBov))
            return false;

        Console.WriteLine("Bulk excutado.");

        if (!_repository.ExecuteProcedure(procedure, _config.DataFormatada.Substring(0, 6)))
            return false;

        Console.WriteLine("Procedure excutada.");

        _repository.AtualizarStatusUpload(idArquivo, "1");

        return true;
    }

    private bool ValidarLayout(string numBov, string cabecalho, string idArquivo)
    {
        var camposArquivo = cabecalho.Split(';');

        var layoutBanco = _repository.ObterLayoutAtivo(numBov);

        // ✅ primeiro cadastro
        if (!layoutBanco.Any())
        {
            _repository.InserirNovoLayout(numBov, camposArquivo);
            return true;
        }

        var inconsistencias = CompararLayouts(camposArquivo, layoutBanco);

        if (!inconsistencias.Any())
            return true;

        // ✅ atualização de layout
        _repository.DesativarLayout(numBov);
        _repository.InserirNovoLayout(numBov, camposArquivo);

        var idAlerta = _repository.InserirAlertaAlteracao(
            numBov,
            inconsistencias.Count,
            idArquivo);

        _repository.InserirAlteracoes(inconsistencias, idAlerta);

        return true;
    }

    private List<InconsistenciaDTO> CompararLayouts(
    string[] arquivo,
    List<string> banco)
    {
        var inconsistencias = new List<InconsistenciaDTO>();

        var max = Math.Max(arquivo.Length, banco.Count);

        for (int i = 0; i < max; i++)
        {
            var colArquivo = i < arquivo.Length ? arquivo[i] : "NULL";
            var colBanco = i < banco.Count ? banco[i] : "NULL";

            if (!string.Equals(colArquivo?.Trim(), colBanco?.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                inconsistencias.Add(new InconsistenciaDTO
                {
                    ColunaBanco = colBanco,
                    ColunaArquivo = colArquivo
                });
            }
        }

        return inconsistencias;
    }

    private void CriarTabela(string cabecalho, string nomeTabela)
    {
        var campos = ProcessarCabecalho(cabecalho);

        _repository.CriarTabelaTemporaria(nomeTabela, campos);

        _log.Gravar("INFORMAÇÃO",
            $"Tabela temporária criada: {nomeTabela}",
            "");
    }

    private bool ExecutarBulk(string arquivo, string tabelaTemp, string numBov)
    {
        try
        {


            // ✅ valida conteúdo (evita só cabeçalho)
            if (!ArquivoTemConteudo(arquivo))
            {
                _log.Gravar("INFORMAÇÃO",
                    $"Arquivo {arquivo} está vazio (somente cabeçalho)",
                    "");

                _repository.AtualizarStatusDownload(numBov, "em branco");
                Console.WriteLine($"Arquivo da BOV {numBov} está em branco.");

                return false;
            }

            // ✅ tenta com LF
            if (_repository.ExecutarBulkInsert(arquivo, tabelaTemp, "0x0a"))
                return true;

            // ✅ fallback CR
            if (_repository.ExecutarBulkInsert(arquivo, tabelaTemp, "0x0d"))
                return true;

            _log.Gravar("ERRO",
                "Falha no bulk insert com ambos ROWTERMINATOR",
                arquivo);

            return false;
        }
        catch (Exception ex)
        {
            _log.Gravar("ERRO",
                "Erro ao executar bulk insert",
                ex.ToString());

            return false;
        }
    }

    private bool ArquivoTemConteudo(string caminhoArquivo)
    {
        try
        {
            return File.ReadLines(caminhoArquivo).Skip(1).Any();
        }
        catch
        {
            return false;
        }
    }

    private List<(string Nome, int Tamanho)> ProcessarCabecalho(string cabecalho)
    {
        var campos = cabecalho.Split(';');

        var lista = new List<(string Nome, int Tamanho)>();

        foreach (var campo in campos)
        {
            var nomeLimpo = SanitizarNomeCampo(campo);

            var tamanho = DefinirTamanhoCampo(nomeLimpo);

            lista.Add((nomeLimpo, tamanho));
        }

        return lista;
    }

    private string SanitizarNomeCampo(string campo)
    {
        return campo
            .Trim()
            .Replace(" ", "_")
            .Replace("-", "_")
            .Replace("/", "_");
    }

    private int DefinirTamanhoCampo(string nomeCampo)
    {
        var camposGrandes = new HashSet<string>
        {
            "BAIRRO",
            "OBSERVACAO_ATENDENTE"
        };

        return camposGrandes.Contains(nomeCampo.ToUpper())
            ? 5000
            : 500;
    }
}