namespace AppBOV.Application.Interfaces
{
    public interface IDataLoadService
    {
        bool ProcessarArquivo(
            string arquivo,
            string numBov,
            string idArquivo,
            string tabelaTemp,
            string procedure);
    }
}