namespace AppBOV.Application.Interfaces
{
    internal interface IRelatorioBovService
    {
        string[] LimparHtmlTabela(string html);
        string[] PegaHTML(string numBov, string sistema);
        ResultadoBovDTO ProcessarListaHtml(string[] listaHtml,BovPendenteDTO bov);
    }
}
