using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium;
using AppBOV.CrossCutting.Config;
using AppBOV.Application.Interfaces;

namespace AppBOV.Infrastructure.Repositories
{
    public class NavegadorClient : INavegadorClient
    {
        private ChromeDriverService service;
        private ChromeOptions opcao;
        private IWebDriver objNVT;
        private WebDriverWait wait;
        private Actions acao;

        private readonly ILogService _log;
        private readonly AppConfig _config;

        public NavegadorClient(
            AppConfig config,
            ILogService log)
        {
            _config = config;
            _log = log;
        }

        public bool IniciaNavegador()
        {
            try
            {
                objNVT = CriarDriver();

                wait = new WebDriverWait(objNVT, TimeSpan.FromSeconds(10));
                acao = new Actions(objNVT);

                objNVT.Navigate().GoToUrl("http://portalbi.telemar/");

                _log.Gravar("INFORMAÇÃO", "Navegador iniciado com sucesso");

                return true;
            }
            catch (Exception ex)
            {
                _log.Gravar("ERRO", "Falha ao iniciar navegador", ex.ToString());
                return false;
            }
        }

        private IWebDriver CriarDriver()
        {
            var service = ChromeDriverService.CreateDefaultService(_config.ChromeDriverPath);

            service.SuppressInitialDiagnosticInformation = true;
            service.DisableBuildCheck = true;
            service.HideCommandPromptWindow = true;

            var options = new ChromeOptions();

            options.BinaryLocation = _config.ChromeBinaryPath;

            options.AddArgument("start-maximized");
            options.AddArgument($"proxy-pac-url={_config.ProxyPacUrl}");
            options.AddUserProfilePreference("download.default_directory", _config.DirRemoto);

            return new ChromeDriver(service, options);
        }

        public void FecharNavegador()
        {
            try
            {
                objNVT?.Quit();
                objNVT?.Dispose();
            }
            catch { }
        }

        public bool DownloadArquivo(string numBov, string urlDownload)
        {
            try
            {
                string caminho = _config.DirLocal;

                DirectoryInfo di = new DirectoryInfo(caminho);

                foreach (FileInfo file in di.GetFiles())
                {
                    file.Delete();
                }

                objNVT.Navigate().GoToUrl(urlDownload);
                Thread.Sleep(5000);

                return true;
            }
            catch (Exception ex)
            {
                _log.Gravar("ERRO", "Não foi possivel baixar as BOVs", ex.Message);
                return false;
            }
        }

        public void AcessarTelaRelatorio(string numBov)
        {
            var wait = new WebDriverWait(objNVT, TimeSpan.FromSeconds(60));

            objNVT.Navigate().GoToUrl("http://portalbi.telemar/ListaRelatorios.aspx");

            var campo = wait.Until(
                SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.Id("txtNumero2"))
            );

            campo.SendKeys(numBov);

            var btnConsultar = wait.Until(
                SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(By.Id("btnConsultar"))
            );

            btnConsultar.Click();
        }

        public bool AbrirRelatorio(string numBov)
        {
            var wait = new WebDriverWait(objNVT, TimeSpan.FromSeconds(60));

            var btnAbrirDisponivel = wait.Until(
                SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(By.Id("menuBtnRelatorioAbrir"))
            );

            if (!btnAbrirDisponivel.Enabled)
            {
                var btnAcesso = objNVT.FindElement(By.Id("menuBtnAcesso"));

                if (btnAcesso.Enabled)
                {
                    _log.Gravar("ERRO",
                        $"Sem acesso ao relatório {numBov}",
                        "");

                    Console.WriteLine($"Sem acesso a BOV {numBov}.");
                    return false;
                }

                return false;
            }

            objNVT.FindElement(By.Id("menuBtnRelatorioAbrir")).Click();

            Thread.Sleep(1000);

            return true;
        }

        public void PreencherDatas(string dataInicio, string dataFim)
        {
            var wait = new WebDriverWait(objNVT, TimeSpan.FromSeconds(30));

            var txtDataDe = wait.Until(
                SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.Id("txtDataDe"))
            );

            txtDataDe.Clear();
            txtDataDe.SendKeys(dataInicio);

            var txtDataAte = objNVT.FindElement(By.Id("txtDataAte"));
            txtDataAte.Clear();
            txtDataAte.SendKeys(dataFim);
        }

        public void DesmarcarEnvioEmail()
        {
            var wait = new WebDriverWait(objNVT, TimeSpan.FromSeconds(30));

            try
            {
                var checkboxWrapper = wait.Until(
                    SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(
                        By.XPath("//input[@id='Ckbemail']/parent::div")
                    )
                );

                checkboxWrapper.Click();

                _log.Gravar("INFO", "Checkbox desmarcado via wrapper iCheck");
            }
            catch (Exception ex)
            {
                _log.Gravar("ERRO", "Erro ao clicar no wrapper do checkbox", ex.ToString());
            }
        }

        public void GerarRelatorio()
        {
            var wait = new WebDriverWait(objNVT, TimeSpan.FromSeconds(30));

            var btnGerar = wait.Until(
                SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(By.Id("btnGerarRelatorio"))
            );

            btnGerar.Click();
        }

        public void AcessarPaginaRelatorio(string numBov)
        {
            var wait = new WebDriverWait(objNVT, TimeSpan.FromSeconds(60));

            objNVT.Navigate().GoToUrl("http://portalbi.telemar/ListaRelatorios.aspx");

            var campo = wait.Until(
                SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.Id("txtNumero2"))
            );

            campo.SendKeys(numBov);

            var btnConsultar = wait.Until(
                SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(By.Id("btnConsultar"))
            );

            btnConsultar.Click();

            var btnAbrir = wait.Until(
                SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(By.Id("menuBtnRelatorioAbrir"))
            );

            btnAbrir.Click();

            Thread.Sleep(2000);
        }

        public string ObterPageSource()
        {
            return objNVT.PageSource;
        }

    }
}
