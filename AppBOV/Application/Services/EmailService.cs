using System.Net.Mail;
using System.Text;
using AppBOV.Application.Interfaces;
using AppBOV.CrossCutting.Config;

namespace AppBOV.Aplication.Services
{
    class EmailService : IEmailService
    {

        public void Enviar(string assunto, string corpo, List<string> destinatarios)
        {
            var smtp = new SmtpClient("relayinterno.telemar")
            {
                Port = 25
            };

            var mail = new MailMessage
            {
                From = new MailAddress("PP-INTELIGENCIADENEGOCIOB2B@oi.net.br"),
                Subject = assunto,
                Body = corpo,
                IsBodyHtml = true
            };

            foreach (var dest in destinatarios)
            {
                mail.To.Add(dest);
            }

            smtp.Send(mail);
            Console.WriteLine("Email enviado.");
        }

        public string GerarCorpoRelatorio(List<AlertaDTO> dados, AppConfig config)
        {
            var sb = new StringBuilder();

            // Cabeçalho
            sb.AppendLine("<html>");
            sb.AppendLine("<head>");
            sb.AppendLine("<meta http-equiv='Content-Type' content='text/html; charset=ISO-8859-1' />");
            sb.AppendLine("<style type='text/css'>");
            sb.AppendLine("body { font-family: tahoma; font-size:13px; }");
            sb.AppendLine("table { border-collapse: collapse; }");
            sb.AppendLine("table, td { border: 1px solid black; text-align:center; padding:5px; }");
            sb.AppendLine("h1 { font-size:20px; }");
            sb.AppendLine("</style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");

            // Introdução
            sb.AppendLine("Oi,<br>");
            sb.AppendLine("Segue relatório diário de exportação da BOV.<br><br>");

            sb.AppendLine($"<b>Período consultado:</b> {config.DataInicioExtrairBov} a {config.DataFimExtrairBov}<br><br>");

            sb.AppendLine("<h1>Detalhamento BOV</h1>");

            // Tabela
            sb.AppendLine("<table>");
            sb.AppendLine("<tr style='background-color:#29b4b9'>");
            sb.AppendLine("<td><b>BOV</b></td>");
            sb.AppendLine("<td><b>Nome</b></td>");
            sb.AppendLine("<td><b>Última Data</b></td>");
            sb.AppendLine("<td><b>Status BOV</b></td>");
            sb.AppendLine("</tr>");

            foreach (var item in dados)
            {
                sb.AppendLine("<tr>");
                sb.AppendLine($"<td>{item.NumBov}</td>");
                sb.AppendLine($"<td>{item.NomeBov}</td>");
                sb.AppendLine($"<td>{item.UltimoRegistro}</td>");
                sb.AppendLine($"<td>{item.StatusUpload}</td>");
                sb.AppendLine("</tr>");
            }

            sb.AppendLine("</table>");

            // Rodapé
            sb.AppendLine("<br><hr>");
            sb.AppendLine("<b>Relacionamento com Clientes Oi Soluções</b><br>");

            sb.AppendLine("<i>Esse email foi encaminhado automaticamente pelo sistema</i>");

            sb.AppendLine("</body>");
            sb.AppendLine("</html>");

            return sb.ToString();
        }
    }
}