using AppBOV.CrossCutting.Config;

namespace AppBOV.Application.Interfaces
{
    public interface IEmailService
    {
        void Enviar(string assunto, string corpo, List<string> destinatarios);

        string GerarCorpoRelatorio(List<AlertaDTO> dados, AppConfig config);
    }
}
