using AppBOV.Application.Interfaces;
using AppBOV.CrossCutting.Config;

public class ProcessamentoService : IProcessamentoService
{
    private readonly IDownloadService _download;
    private readonly AppConfig _config;
    private readonly IFileProcessingService _file;
    private readonly IDataLoadService _data;
    private readonly INotificationService _notify;

    public ProcessamentoService(
        AppConfig config,
        IDataLoadService data,
        IDownloadService download,
        IFileProcessingService file,
        INotificationService notify)
    {
        _config = config;
        _download = download;
        _file = file;
        _data = data;
        _notify = notify;
    }

    public void Processar(string caminhoZip, string numBov, string idArquivo, string tabela, string procedure)
    {
        if (!_download.ProcessarDownload(caminhoZip, _config.DirLocal))
            return;

        var arquivo = _file.ObterArquivoProcessado(numBov);

        if (arquivo == null)
            return;

        if (!_data.ProcessarArquivo(arquivo, numBov, idArquivo, tabela, procedure))
            return;

        _file.CopiarArquivo(arquivo, _config.DirDestino);

        _notify.EnviarAlertas(_config);
    }
}