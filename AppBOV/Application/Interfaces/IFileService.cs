namespace AppBOV.Application.Interfaces
{
    public interface IFileService
    {
        ArquivoEncontradoDTO VerificarArquivoDiretorio(
            string numBov,
            string dataDeArquivo,
            string dataParaArquivo);
    }
}
