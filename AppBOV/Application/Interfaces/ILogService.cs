public interface ILogService
{
    void Gravar(string tipo, string mensagem, string detalhe = "");
}
