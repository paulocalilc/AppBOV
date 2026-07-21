using AppBOV.Application.Services;
using AppBOV.CrossCutting.Config;
using AppBOV.Infrastructure.Logging;

namespace AppBOV.Application.Orchestrators
{
    class ProcessadorBov
    {
        private readonly ILogService _log;
        private readonly ValidacaoService _validacao;
        private readonly NavegadorService _navegador;

        public ProcessadorBov(
            AppConfig config,
            ILogService log,
            ValidacaoService validacao,
            NavegadorService navegador)
        {
            _log = log;
            _validacao = validacao;
            _navegador = navegador;
        }

        public void Processar()
        {
            VerificarArquivosBov();
        }

        private void VerificarArquivosBov()
        {
            Console.WriteLine("Executando VerificarArquivosBov...");

            if (_navegador.DownloadArquivosBov())
            {
                _log.Gravar("INFORMAÇÃO", "Download de arquivos BOV realizado com sucesso");
                Console.WriteLine("Download de arquivos BOV realizado com sucesso");

                //Thread.Sleep(3000); // ainda ok (controle externo depois)
            }
        }
    }
}