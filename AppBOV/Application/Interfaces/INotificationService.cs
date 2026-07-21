using AppBOV.CrossCutting.Config;

public interface INotificationService
{
    void EnviarAlertas(AppConfig config);
}
