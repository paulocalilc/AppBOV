using AppBOV.Aplication.Services;
using AppBOV.Application.Interfaces;
using AppBOV.Application.Orchestrators;
using AppBOV.Application.Services;
using AppBOV.Infrastructure.Logging;
using AppBOV.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjectionConfig
{
    public static IServiceCollection AddAppServices(this IServiceCollection services)
    {
        var config = InicializacaoService.CriarConfig();
        services.AddSingleton(config);

        services.AddScoped<ILogService, LogService>();
        services.AddScoped<ValidacaoService>();

        services.AddScoped<IBovRepository, BovRepository>();

        services.AddScoped<IDataLoadService, DataLoadService>();
        services.AddScoped<IDownloadService, DownloadService>();
        services.AddScoped<IFileProcessingService, FileProcessingService>();
        services.AddScoped<IFileService, FileService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<INavegadorClient, NavegadorClient>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IRelatorioBovService, RelatorioBovService>();

        services.AddScoped<IProcessamentoService, ProcessamentoService>();

        services.AddScoped<NavegadorService>();
        services.AddScoped<ProcessadorBov>();

        return services;
    }
}
