using AppBOV.CrossCutting.Config;
using AppBOV.Application.Interfaces;

namespace AppBOV.Application.Orchestrators

{
    class NavegadorService
    {
        private readonly AppConfig _config;
        private readonly IProcessamentoService _processamento;
        private readonly IEmailService _emailService;
        private readonly IFileService _file;
        private readonly IBovRepository _repository;
        private readonly IDownloadService _downloadService;
        private readonly IFileProcessingService _fileService;
        private readonly IDataLoadService _dataLoadService;
        private readonly ILogService _log;
        private readonly INavegadorClient _navegador;
        private readonly IRelatorioBovService _ratorioBovService;

        public NavegadorService(
            AppConfig config,
            IEmailService emailService,
            IDownloadService downloadService,
            IFileProcessingService fileProcessingService,
            IDataLoadService dataLoadService,
            IFileService fileService,
            IProcessamentoService processamentoService,
            IBovRepository repository,
            ILogService log,
            INavegadorClient navegador,
            IRelatorioBovService ratorioBovService)
        {
            _config = config;
            _processamento = processamentoService;
            _emailService = emailService;
            _file = fileService;

            _downloadService = downloadService;
            _fileService = fileProcessingService;
            _dataLoadService = dataLoadService;
            _repository = repository;
            _log = log;
            _navegador = navegador;
            _ratorioBovService = ratorioBovService;
        }

        public bool DownloadArquivosBov()
        {
            _log.Gravar("INFORMAÇÃO", "Iniciando verificação dos arquivos");

            try
            {

                var resultado = ObterBovsPendentes();

                _log.Gravar("INFO", $"Processamento finalizado: {resultado}");

                return resultado;

            }
            catch (Exception ex)
            {
                _log.Gravar("ERRO",
                    "NavegadorService - Falha ao validar as BOVs",
                    ex.ToString());

                throw;
            }
        }

        private bool ObterBovsPendentes()
        {
            _log.Gravar("INFO", "Acessando portal BOV");

            if (DateTime.Now < DateTime.Parse(_config.HoraExecucaoDownload))
                return false;

            //var repository = new BovRepository(_config);

            if (!_navegador.IniciaNavegador())
            {
                _log.Gravar("ERRO", "Navegador não foi iniciado");
                return false;
            }

            try
            {
                _repository.LimparTabelaChecks();

                var bovs = _repository.ObterBovsPendentes();

                if (!bovs.Any())
                    return false;

                foreach (var bov in bovs)
                {
                    ProcessarBov(bov, _repository);
                }

                EnviarRelatorioSeCompleto(_repository);

                return true;
            }
            catch (Exception ex)
            {
                _log.Gravar("ERRO", "Erro ao processar BOVs", ex.ToString());
                return false;
            }
            finally
            {
                _navegador.FecharNavegador();
            }
        }

        private void ProcessarBov(BovPendenteDTO bov, IBovRepository repository)
        {
            _log.Gravar("INFORMAÇÃO", $"EXECUTANDO BOV: {bov.NumBov}");

            if (string.IsNullOrEmpty(bov.DataHoraRegistro))
            {
                TratarSolicitacaoBov(bov, repository);
            }
            else
            {
                TratarBovExistente(bov, repository);
            }
        }

        private void EnviarRelatorioSeCompleto(IBovRepository repository)
        {
            if (!repository.TodosArquivosForamProcessados())
                return;

            _log.Gravar("INFO", "Todos arquivos concluídos. Enviando email...");

            var alertas = repository.ObterAlertas();
            var destinatarios = repository.ObterDestinatarios();

            if (!alertas.Any() || !destinatarios.Any())
                return;

            var assunto = $"Relatório BOV {_config.DataFimExtrairBov}";
            var corpo = _emailService.GerarCorpoRelatorio(alertas, _config);

            _emailService.Enviar(
                assunto,
                corpo,
                destinatarios
            );
        }

        private void TratarSolicitacaoBov(BovPendenteDTO bov, IBovRepository repository)
        {
            if (int.Parse(bov.Automatico) == 1 && _config.FechamentoMes == 0)
            {
                repository.InserirArquivoBov(bov.NumBov, "AUTO");

                _log.Gravar("INFO", $"BOV {bov.NumBov} solicitada automaticamente");
            }
            else
            {
                if (ProgramarBaseBov(
                    bov.NumBov,
                    _config.DataInicioExtrairBov,
                    _config.DataFimExtrairBov))
                {
                    repository.InserirArquivoBov(bov.NumBov, _config.UsuarioRede);
                    Console.WriteLine($"A BOV {bov.NumBov} está sendo solicitada.");
                    _log.Gravar("INFO", $"BOV {bov.NumBov} solicitada manualmente");
                }
                else
                {
                    _log.Gravar("ERRO", $"Falha ao programar BOV {bov.NumBov}");
                }
            }
        }

        private void TratarBovExistente(BovPendenteDTO bov, IBovRepository repository)
        {
            _log.Gravar("INFORMAÇÃO", $"BOV {bov.NumBov} PROCURANDO LISTA");

            var lista = _ratorioBovService.PegaHTML(bov.NumBov, bov.SistemaRelatorio);

            if (lista.Length == 0)
            {
                TratarListaVazia(bov, repository);
                return;
            }

            var resultado = _ratorioBovService.ProcessarListaHtml(lista, bov);

            if (resultado == null)
            {
                TratarNaoEncontrado(bov);
                return;
            }

            TratarStatusDownload(resultado, bov, repository);
        }

        private void TratarStatusDownload(
            ResultadoBovDTO resultado,
            BovPendenteDTO bov,
            IBovRepository repository)
        {
            switch (resultado.Status)
            {
                case "Agendado":
                    repository.AtualizarStatusDownload(bov.NumBov, "agendado");
                    Console.WriteLine($"A BOV {bov.NumBov} está agendada ainda.");
                    break;

                case "Erro":
                    repository.AtualizarStatusDownload(bov.NumBov, "erro");
                    Console.WriteLine($"A BOV {bov.NumBov} está gerando erro.");
                    break;

                case "Aguarde":
                    repository.AtualizarStatusDownload(bov.NumBov, "em processamento");
                    Console.WriteLine($"A BOV {bov.NumBov} está em processamento ainda.");
                    break;

                case "Download":
                    Console.WriteLine($"A BOV {bov.NumBov} está disponível para download.");
                    ProcessarDownloadBov(
                        bov.NumBov,
                        resultado.UrlDownload,
                        resultado.PeriodoDe,
                        resultado.PeriodoPara,
                        resultado.Filtro,
                        bov.IdArquivo,
                        bov.TabelaTemporaria,
                        bov.NomeProcedure,
                        repository
                    );
                    break;
            }
        }

        private void ProcessarDownloadBov(
            string bdNumBov,
            string urlDownload,
            string periodoDeBov,
            string periodoParaBov,
            string filtroBov,
            string bdIdArquivo,
            string bdTabelaBulkTemporaria,
            string bdNomeProcedure,
            IBovRepository repository)
        {
            if (!_navegador.DownloadArquivo(bdNumBov, urlDownload))
            {
                _log.Gravar("ERRO", "Erro ao baixar arquivo", $"BOV {bdNumBov}");
                throw new ArgumentException("Falha no download");
            }

            _log.Gravar("INFORMAÇÃO", $"BOV {bdNumBov} SENDO BAIXADA");

            var arquivo = _file.VerificarArquivoDiretorio(
                bdNumBov,
                periodoDeBov,
                periodoParaBov
            );

            if (arquivo == null)
            {
                AtualizarStatus(repository, bdIdArquivo, "pendente");
                return;
            }

            AtualizarStatus(repository, bdIdArquivo, "concluido");

            if (!_downloadService.ProcessarDownload(arquivo.CaminhoCompleto, _config.DirLocal))
            {
                Console.WriteLine("Erro no unzip do arquivo");
                _log.Gravar("ERRO", "Erro no unzip do arquivo", $"BOV {bdNumBov}");
                return;
            }

            _log.Gravar("INFORMAÇÃO", $"BOV {bdNumBov} DEZIPADA");

            var caminhoArquivo = _fileService.ObterArquivoProcessado(bdNumBov);

            if (string.IsNullOrEmpty(caminhoArquivo))
            {
                _log.Gravar("ERRO", "Nenhum arquivo TXT/CSV encontrado", "");
                return;
            }

            if (_dataLoadService.ProcessarArquivo(
                caminhoArquivo,
                bdNumBov,
                bdIdArquivo,
                bdTabelaBulkTemporaria,
                bdNomeProcedure))
            {
                _fileService.CopiarArquivo(caminhoArquivo, _config.DirDestino);

                LimparDiretorio();
            }
        }

        private void LimparDiretorio()
        {
            var caminho = _config.DirLocal;

            var dir = new DirectoryInfo(caminho);

            foreach (var file in dir.GetFiles())
                file.Delete();
        }

        private void AtualizarStatus(IBovRepository repository, string idArquivo, string status)
        {
            var sql = $@"
        UPDATE [{_config.NomeBd}].[{_config.NomeSchema}].[{_config.TabelaArquivos}]
        SET data_hora_registro = '{DateTime.Now:yyyy-MM-dd HH:mm:ss}',
            status_download_bov = '{status}'
        WHERE id_arquivo = '{idArquivo}'";

            using var dr = repository.ExecuteQuery(sql);
        }

        private void TratarNaoEncontrado(BovPendenteDTO bov)
        {
            _log.Gravar("ERRO",
                $"BOV: {bov.NumBov} registrada no BD mas não encontrada na lista");

            if (int.Parse(bov.Automatico) == 0 || _config.FechamentoMes == 1)
            {
                ProgramarBaseBov(
                    bov.NumBov,
                    _config.DataInicioExtrairBov,
                    _config.DataFimExtrairBov);
            }
        }

        private void TratarListaVazia(BovPendenteDTO bov, IBovRepository repository)
        {
            _log.Gravar("ERRO",
                $"BOV: {bov.NumBov} NÃO CARREGOU TABELA DE DOWNLOAD");

            repository.AtualizarStatusDownload(bov.IdArquivo, "Aguardando BOV");
        }

        private bool ProgramarBaseBov(string numBov, string dataInicio, string dataFim)
        {
            try
            {
                _navegador.AcessarTelaRelatorio(numBov);

                if (!_navegador.AbrirRelatorio(numBov))
                    return true; // já tratou "sem acesso"

                _navegador.PreencherDatas(dataInicio, dataFim);

                _navegador.DesmarcarEnvioEmail();

                _navegador.GerarRelatorio();

                return true;
            }
            catch (Exception ex)
            {
                _log.Gravar("ERRO",
                    "Erro ao programar BOV",
                    ex.ToString());

                return false;
            }
        }
    }
}