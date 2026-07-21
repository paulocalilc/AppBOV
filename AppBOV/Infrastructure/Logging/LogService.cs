using System;
using System.IO;

namespace AppBOV.Infrastructure.Logging
{
    class LogService : ILogService
    {
        private readonly string _caminhoLog = @"C:\Logs\AppBov.log";

        public void Gravar(string tipo, string mensagem, string detalhe = "")
        {
            try
            {
                var caminho = _caminhoLog;
                var pasta = Path.GetDirectoryName(caminho);

                if (!Directory.Exists(pasta))
                {
                    Directory.CreateDirectory(pasta);
                }

                var linha = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | {tipo} | {mensagem} | {detalhe}{Environment.NewLine}";

                File.AppendAllText(caminho, linha);
            }
            catch
            {
                // continua silencioso por enquanto
            }
        }
    }
}
