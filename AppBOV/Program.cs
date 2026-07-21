using Microsoft.Extensions.DependencyInjection;
using AppBOV.Application.Services;
using AppBOV.Application.Orchestrators;

namespace AppBOV
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                // ✅ PRIMEIRO: inicializa ambiente
                new InicializacaoService(Environment.GetCommandLineArgs());

                InicializacaoService.CriarPastasAuxiliares();
                InicializacaoService.ExibirValores();

                var services = new ServiceCollection();

                // ✅ DEPOIS: registra serviços (já com config pronta)
                services.AddAppServices();

                var provider = services.BuildServiceProvider();

                // ✅ executa pipeline
                var processador = provider.GetRequiredService<ProcessadorBov>();

                processador.Processar();

                Console.WriteLine("Execução finalizada.");
            }
            catch (Exception ex)
            {
                Log("ERRO", "Erro fatal na aplicação", ex.ToString());
            }
        }

        private static void Log(string tipo, string mensagem, string detalhe = "")
        {
            try
            {
                var linha = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | {tipo} | {mensagem} | {detalhe}{Environment.NewLine}";

                File.AppendAllText(
                    @"C:\\Logs\\AppBov.log",
                    linha
                );
            }
            catch
            {
                // nunca quebrar a aplicação por log
            }
        }
    }
}