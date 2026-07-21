using AppBOV.Application.Interfaces;
using AppBOV.CrossCutting.Config;
using AppBOV.Infrastructure.Logging;
public class FileProcessingService : IFileProcessingService
{
    private readonly LogService _log = new();
    private readonly AppConfig _config;

    public FileProcessingService(AppConfig config)
    {
        _config = config;
    }

    public string ObterArquivoProcessado(string numBov)
    {
        var arquivos = Directory.GetFiles(_config.DirLocal)
            .Where(a =>
                (a.EndsWith(".txt", StringComparison.OrdinalIgnoreCase) ||
                 a.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
                 && a.Contains(numBov))
            .OrderByDescending(File.GetLastWriteTime)
            .ToList();

        return arquivos.FirstOrDefault();
    }

    public string ObterCabecalho(string arquivo)
    {
        return File.ReadLines(arquivo).FirstOrDefault() ?? "";
    }

    public bool ArquivoTemConteudo(string arquivo)
    {
        return File.ReadLines(arquivo).Skip(1).Any();
    }

    public bool CopiarArquivo(string origem, string destino)
    {
        if (!File.Exists(origem))
            return false;

        Directory.CreateDirectory(destino);

        var nome = Path.GetFileName(origem);

        File.Copy(origem, Path.Combine(destino, nome), true);

        return true;
    }
}