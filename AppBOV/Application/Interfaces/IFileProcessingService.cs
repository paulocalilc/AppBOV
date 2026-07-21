namespace AppBOV.Application.Interfaces
{
    public interface IFileProcessingService
    {
        string ObterArquivoProcessado(string numBov);
        bool CopiarArquivo(string origem, string destino);
    }
}