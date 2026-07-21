using AppBOV.Application.Interfaces;
using AppBOV.CrossCutting.Config;
using HtmlAgilityPack;

namespace AppBOV.Application.Services
{
    internal class RelatorioBovService : IRelatorioBovService
    {
        private readonly INavegadorClient _navegador;
        private readonly ILogService _log;
        private readonly AppConfig _config;

        public RelatorioBovService(
            INavegadorClient navegador, 
            ILogService logService,
            AppConfig config)
        {
            _navegador = navegador;
            _log = logService;
            _config = config;
        }

        private string ObterHtmlTabela(string pageSource)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(pageSource);

            var node = doc.DocumentNode.SelectSingleNode("//table[@id='datatable-buttons_disponiveis']");

            return node?.InnerHtml ?? string.Empty;
        }

        public string[] PegaHTML(string numBov, string sistema)
        {
            try
            {
                _navegador.AcessarPaginaRelatorio(numBov);

                var page = _navegador.ObterPageSource();
                var htmlTabela = ObterHtmlTabela(page);

                return LimparHtmlTabela(htmlTabela);
            }
            catch (Exception ex)
            {
                _log.Gravar("ERRO", "Erro ao obter HTML", ex.ToString());
                return Array.Empty<string>();
            }
        }

        public string[] LimparHtmlTabela(string html)
        {
            if (string.IsNullOrEmpty(html))
                return Array.Empty<string>();

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var linhasTratadas = new List<string>();

            var linhas = doc.DocumentNode.SelectNodes("//tr");

            if (linhas == null)
                return Array.Empty<string>();

            foreach (var linha in linhas)
            {
                var colunas = linha.SelectNodes("td");

                if (colunas == null)
                    continue;

                if (linha.InnerText.Contains("Período De"))
                    continue;

                var valores = new List<string>();

                foreach (var coluna in colunas)
                {
                    var link = coluna.SelectSingleNode(".//a[@href]");

                    if (link != null)
                    {
                        var href = link.GetAttributeValue("href", "").Trim();
                        var texto = link.InnerText.Trim();


                        valores.Add(texto); // status
                        valores.Add(href);  // link

                    }
                    else
                    {
                        valores.Add(coluna.InnerText.Trim());
                    }
                }

                if (valores.Any())
                    linhasTratadas.Add(string.Join(";", valores));
            }

            return linhasTratadas.ToArray();
        }

        public ResultadoBovDTO ProcessarListaHtml(
            string[] listaHtml,
            BovPendenteDTO bov)
        {
            if (listaHtml == null || listaHtml.Length == 0)
                return null;

            int ultimo = listaHtml.Length - 2;

            for (int i = 0; i <= ultimo; i++)
            {
                var idBov = string.Empty;
                var colunas = listaHtml[i].Split(';');

                if (colunas == null || colunas.Length == 0)
                    continue;

                var url = colunas[6].Trim();

                var status = colunas[5].Trim();

                // 🔸 valida link
                if (status == "Download" && !url.StartsWith("http"))
                {
                    continue;
                }
                else if(status == "Download" && url.StartsWith("http"))
                {
                    idBov = url.Substring(55, 7).Replace("_", "");
                }

                // 🔸 extrai dados
                var periodoDe = FormatarData(colunas[1]);
                var periodoPara = FormatarData(colunas[2]);

                var usuario = colunas[3].Trim();

                // 🔸 regra principal
                if (DeveProcessarLinha(bov, idBov, usuario, periodoPara, status))
                {
                    return new ResultadoBovDTO
                    {
                        IdBov = idBov,
                        NomeBov = bov.NomeBov,
                        PeriodoDe = periodoDe,
                        PeriodoPara = periodoPara,
                        Usuario = usuario,
                        Status = status,
                        UrlDownload = url,
                        Filtro = DeterminarFiltro(bov)
                    };
                }
            }

            return null;
        }

        private string FormatarData(string data)
        {
            var d = data.Trim();

            return string.Concat(
                d.Substring(6, 4),
                d.Substring(3, 2),
                d.Substring(0, 2)
            );
        }
        
        private bool DeveProcessarLinha(
            BovPendenteDTO bov,
            string idBov,
            string usuario,
            string periodoPara,
            string status)
        {
            if (status == "Agendado" || status == "Aguarde" || status == "Enviando Email" || status == "Enviando E-mail")
            {
                return true;
            }
            else
            {
                if (_config.DataFormatada == periodoPara &&
                    usuario.ToUpper() == bov.UsuarioBov?.ToUpper() &&
                    string.IsNullOrEmpty(bov.IdBov))
                {
                    return true;
                }

                if (idBov == bov.IdBov)
                    return true;

                return false;
            }
        }

        private string DeterminarFiltro(BovPendenteDTO bov)
        {
            if (string.IsNullOrEmpty(bov.GrupoUnidade))
                return "N";

            if (int.Parse(bov.Automatico) == 1 && _config.FechamentoMes == 0)
                return "N";

            return "S";
        }
    }
}
