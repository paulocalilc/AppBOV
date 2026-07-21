using AppBOV.Aplication.Services;
using AppBOV.CrossCutting.Config;

public class NotificationService : INotificationService
{
    private readonly IBovRepository _repository;
    private readonly EmailService _email;

    public NotificationService(
        AppConfig config,
        IBovRepository repository)
    {
        _repository = repository;
        _email = new EmailService();
    }

    public void EnviarAlertas(AppConfig config)
    {
        var alertas = _repository.ObterAlertas();
        var destinatarios = _repository.ObterDestinatarios();

        if (!alertas.Any() || !destinatarios.Any())
            return;

        var corpo = _email.GerarCorpoRelatorio(alertas, config);

        _email.Enviar(
            $"Relatório BOV {config.DataFimExtrairBov}",
            corpo,
            destinatarios);
    }
}