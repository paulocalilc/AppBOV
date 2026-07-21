using AppBOV.Application.Orchestrators;

namespace AppBOV.Application.Interfaces
{
    internal interface IProcessamentoService
    {
        void Processar(string caminhoZip, string numBov, string idArquivo, string tabela, string procedure);


    }
}
