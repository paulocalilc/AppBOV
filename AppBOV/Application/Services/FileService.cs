using AppBOV.Application.Interfaces;
using AppBOV.CrossCutting.Config;

public class FileService : IFileService
{
    private readonly AppConfig _config;

    public FileService(AppConfig config)
    {
        _config = config;
    }

    public ArquivoEncontradoDTO VerificarArquivoDiretorio(
    string numBov,
    string dataDeArquivo,
    string dataParaArquivo)
    {
        string caminho = _config.DirLocal;
        string datas = $"{dataDeArquivo}_{dataParaArquivo}";

        EsperarDownloadsFinalizarem(caminho);

        foreach (var filePath in Directory.GetFiles(caminho))
        {
            var nome = Path.GetFileName(filePath);

            if (ArquivoValido(nome, numBov, datas))
            {
                return new ArquivoEncontradoDTO
                {
                    NomeArquivo = nome,
                    CaminhoCompleto = filePath
                };
            }

            DeletarArquivo(filePath);
        }

        return null;
    }


    private void EsperarDownloadsFinalizarem(string caminho)
    {
        for (int i = 0; i < 60; i++)
        {
            var arquivos = Directory.GetFiles(caminho);

            if (!arquivos.Any(a => a.EndsWith(".crdownload")))
                return;

            Thread.Sleep(1000);
        }
    }

    private bool ArquivoValido(string nome, string numBov, string datas)
    {
        return
            (nome.Contains($"BOV_{numBov}") && nome.Contains(datas)) ||
            (nome.Contains($"BIO_{numBov}") && nome.Contains(datas));
    }

    private void DeletarArquivo(string caminhoCompleto)
    {
        try
        {
            if (File.Exists(caminhoCompleto))
            {
                File.Delete(caminhoCompleto);
            }
        }
        catch
        {
            // mantém comportamento silencioso
        }
    }





}
