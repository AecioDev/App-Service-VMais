

namespace AppServiceVMais
{
    public class Dominios
    {
        public Dominios() { } //Construtor Padrão

        public int id { get; set; }
        public string dominio { get; set; }
        public string nome { get; set; }
        public string servidor { get; set; }
        public string banco { get; set; }
        public string login { get; set; }
        public string senha { get; set; }

        public Dominios(int _id, string _dominio, string _nome, string _server, string _banco, string _login, string _senha)
        {
            //Inicializa o objeto com os dados.
            id = _id;
            dominio = _dominio;
            nome = _nome;
            servidor = _server;
            banco = _banco;
            login = _login;
            senha = _senha;
        }
    }

    public class ResultWS
    {
        public int status { get; set; }
        public string message { get; set; }
        public string log { get; set; }
    }


    /************************************************/


    public class ResultServices
    {
        public Ccusto[] CenCusto { get; set; }
    }

    public class Ccusto
    {
        public int status { get; set; }
        public string cencus { get; set; }
        public string message { get; set; }
        public string log { get; set; }
    }


    /************************************************/


}


