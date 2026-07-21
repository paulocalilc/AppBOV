namespace AppBOV.Application.Interfaces
{
    public interface IDownloadService
    {
        bool ProcessarDownload(string caminhoZip, string destino);
    }
}
