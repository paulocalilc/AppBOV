using System.IO.Compression;
using AppBOV.Application.Interfaces;
using AppBOV.CrossCutting.Config;
using AppBOV.Infrastructure.Logging;

public class DownloadService : IDownloadService

{
    private readonly LogService _log;
    private readonly AppConfig _config;

    public DownloadService(
        AppConfig config)
    {
        _config = config;
        _log = new LogService();
    }

    public bool ProcessarDownload(string caminhoZip, string destino)
    {
        if (!EsperarDownloadFinalizar(destino, caminhoZip))
            return false;

        ZipFile.ExtractToDirectory(caminhoZip, destino, true);

        return true;
    }

    private bool EsperarDownloadFinalizar(string dir, string zip)
    {
        var inicio = DateTime.Now;

        while ((DateTime.Now - inicio).TotalSeconds < 120)
        {
            var arquivos = Directory.GetFiles(dir);

            if (arquivos.Any(a => a.EndsWith(".crdownload")))
            {
                Thread.Sleep(1000);
                continue;
            }

            if (!File.Exists(zip))
            {
                Thread.Sleep(1000);
                continue;
            }

            if (ArquivoEstaEstavel(zip))
                return true;

            Thread.Sleep(1000);
        }

        return false;
    }

    private bool ArquivoEstaEstavel(string caminho)
    {
        var t1 = new FileInfo(caminho).Length;
        Thread.Sleep(1500);
        var t2 = new FileInfo(caminho).Length;

        return t1 == t2;
    }
}