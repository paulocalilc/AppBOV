namespace AppBOV.Application.Interfaces
{
    public interface INavegadorClient
    {
        bool IniciaNavegador();
        void FecharNavegador();
        bool DownloadArquivo(string numBov, string urlDownload);
        void AcessarTelaRelatorio(string numBov);
        bool AbrirRelatorio(string numBov);
        void PreencherDatas(string dataInicio, string dataFim);
        void DesmarcarEnvioEmail();
        void GerarRelatorio();
        void AcessarPaginaRelatorio(string numBov);
        string ObterPageSource();
    }
}
