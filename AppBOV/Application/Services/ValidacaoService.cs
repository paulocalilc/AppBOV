using System.IO;

namespace AppBOV.Application.Services
{
    class ValidacaoService
    {
        public bool TemDados(string caminhoArquivo)
        {
            using (var reader = new StreamReader(caminhoArquivo))
            {
                // Lê header
                if (reader.ReadLine() == null)
                    return false;

                // Verifica segunda linha
                return reader.ReadLine() != null;
            }
        }
    }
}