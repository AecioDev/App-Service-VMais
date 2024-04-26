using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Runtime.Remoting.Lifetime;

namespace AppServiceVMais
{
    public class DAO
    {
        public static string folder = Environment.CurrentDirectory + @"\Dados\";
        public static string PathConexao = folder + "ConnVmais.cfg"; //Caminho completo do arquivo
        public static string PathServidor = folder + "ServerVmais.cfg"; //Caminho completo do arquivo

        public static string GetServerVMais()
        {
            string connstr = "";
            int tentativas = 3;

            try
            {
                if (!File.Exists(PathServidor))
                {
                    while (tentativas > 0)
                    {
                        tentativas--;

                        //Se não existir Pede os Dados da Conexão e Cria o Arquivo
                        Console.WriteLine("Favor Cadastar os dados do Endereço do Servidor VMais!!!\n");

                        Console.WriteLine("Informe o Domínio: Ex. localhost ou site.controller.com.br: ");
                        var dominio = Console.ReadLine();

                        Console.WriteLine("Informe a Porta: Ex. 8090");
                        var porta = Console.ReadLine();

                        Console.WriteLine("Informe a Pasta Base da Aplicação: Ex. app ou app/VMais");
                        var pastabase = Console.ReadLine();

                        Console.WriteLine("Informe o Tipo: Ex. http ou https");
                        var tipo = Console.ReadLine();

                        if (string.IsNullOrEmpty(dominio) || string.IsNullOrEmpty(porta) || string.IsNullOrEmpty(pastabase) || string.IsNullOrEmpty(tipo))
                        {
                            Console.WriteLine("Favor Informar todos os dados solicitados para realizar o Cadastro!!!\n");
                        }
                        else
                        {
                            connstr = String.Format(@"{0}://{1}:{2}/{3}", tipo, dominio, porta, pastabase);
                            connstr = connstr.ToLower();

                            CriArquivoConexao(PathServidor);
                            var dadosarq = "LINK:" + connstr;
                            GravaArquivoConexao(connstr, PathServidor);
                            break;
                        }

                    }

                }

                if (tentativas == 3) //Achou o Arquivo então Lê e Retorna a String - Se acabou de cadastrar o arquivo retorna a string gerada e não lê o arquivo
                {
                    //FileStream fs = new FileStream(PathServidor, FileMode.Open, FileAccess.Read);
                    //StreamReader sr = new StreamReader(fs);
                    //connstr = sr.ReadToEnd();
                    //sr.Close(); //grava e fecha

                    connstr = GetPatchSrvVMais(1);

                }

                return connstr.Trim();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erro ao buscar o arquivo de Conexão!!!\n\n" + ex.Message);
            }

            return connstr;
        }

        #region Leitura e Gravação do Arquivo de Conexão
        public static void CriArquivoConexao(string arquivo)
        {
            try
            {
                if (!Directory.Exists(folder))
                {
                    DirectoryInfo diretorio = Directory.CreateDirectory(folder);
                    diretorio.Attributes = FileAttributes.Hidden;
                }

                FileStream fs = new FileStream(arquivo, FileMode.Create, FileAccess.ReadWrite);

                fs.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erro ao criar o arquivo de Conexão!!!\n\n" + ex.Message);
            }
        }

        public static void GravaArquivoConexao(string conexao, string arquivo)
        {
            try
            {
                FileStream fs = new FileStream(arquivo, FileMode.Create, FileAccess.Write);
                StreamWriter sw = new StreamWriter(fs);

                sw.WriteLine(conexao);

                sw.Close(); //grava e fecha
                
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erro ao gravar a conexão no arquivo de Conexão!!!\n\n" + ex.Message);
            }
        }

        public static string Busca_Conexao()
        {
            string connstr = "";
            int tentativas = 3;

            try
            {
                if (!File.Exists(PathConexao))
                {                    
                    while (tentativas > 0)
                    {
                        tentativas--;

                        //Se não existir Pede os Dados da Conexão e Cria o Arquivo
                        Console.WriteLine("Favor Cadastar os dados da Conexão com o Banco de Dados!!!\n");

                        Console.WriteLine("Informe o Servidor de BD: ");
                        var servidor = Console.ReadLine();

                        Console.WriteLine("Informe o Nome do Banco de Dados: ");
                        var banco = Console.ReadLine();

                        Console.WriteLine("Informe o Usuário: ");
                        var user = Console.ReadLine();

                        Console.WriteLine("Informe a Senha: ");
                        var passwd = Console.ReadLine();

                        if (string.IsNullOrEmpty(servidor) || string.IsNullOrEmpty(banco) || string.IsNullOrEmpty(user) || string.IsNullOrEmpty(passwd))
                        {
                            Console.WriteLine("Favor Informar todos os dados da Conexão com o Banco de Dados para Cadastrar!!!\n");                            
                        } 
                        else
                        {
                            connstr = String.Format(@"Data Source={0};Initial Catalog={1};Persist Security Info=True;User ID={2};Password={3}",
                   servidor, banco, user, passwd);

                            CriArquivoConexao(PathConexao);
                            GravaArquivoConexao(connstr, PathConexao);

                            break;
                        }                       

                    }

                }

                if (tentativas == 3) //Achou o Arquivo então Lê e Retorna a String
                {  
                    FileStream fs = new FileStream(PathConexao, FileMode.Open, FileAccess.Read);
                    StreamReader sr = new StreamReader(fs);
                    connstr = sr.ReadToEnd(); //Le Tudo
                    sr.Close(); //Fecha a Stream
                } 

                return connstr.Trim();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erro ao buscar o arquivo de Conexão!!!\n\n" + ex.Message);
            }

            return connstr;
        }
        #endregion


        //Verifica Conexão com o Banco De Dados
        public static bool VerConnDB()
        {
            bool conectado = false;
            string StrConn = Busca_Conexao();

            try
            {
                SqlConnection conexao = new SqlConnection(StrConn);
                SqlCommand cmd = new SqlCommand("SELECT id FROM DOMINIOS", conexao);
                conexao.Open();
                SqlDataReader rd = cmd.ExecuteReader();

                while (rd.Read())
                {
                    var versao = rd.GetValue(0).ToString();
                }

                conexao.Close();
                conectado = true;
            }
            catch (Exception erro)
            {
                Console.WriteLine("Erro ao Conectar no Banco de Dados!!!\n\n" + erro.Message);
                conectado = false;
            }

            return conectado;
        }

        public static List<Dominios> GetDominios()
        {
            List<Dominios> Sites = new List<Dominios>();

            try
            {               
                SqlConnection conexao = new SqlConnection(Busca_Conexao());
                SqlCommand cmd = new SqlCommand("Select id, dominio, nome, servidor, banco, login, senha From DOMINIOS", conexao);
                conexao.Open();

                SqlDataReader rd = cmd.ExecuteReader();

                while (rd.Read())
                {                    
                    Sites.Add(new Dominios(rd.GetInt32(0), rd.GetString(1), rd.GetString(2), rd.GetString(3), rd.GetString(4), rd.GetString(5), rd.GetString(6)));
                }
                                
                conexao.Close();
                return Sites;
            }
            catch (Exception erro)
            {
                Console.WriteLine("Erro ao Listar os Domínios dos Clientes!!!\n\n" + erro.Message);
                return Sites;
            }
        }

        public static string GetPatchSrvVMais(int op)
        {
            string pastatmp = "";

            try
            { 

                FileStream fs = new FileStream(PathServidor, FileMode.Open, FileAccess.Read);
                StreamReader sr = new StreamReader(fs);
                var achou = false;

                while (sr.Peek() >= 0 && !achou)
                {
                    pastatmp = sr.ReadLine();
                    var tipo = pastatmp.Substring(0, 5);

                    if (op == 0)
                    {
                        //Retorna o Link do Servidor
                        if (tipo == "CONN:") { achou = true; }
                    } 
                    else if (op == 1)
                    {
                        //Retorna o Patch da pasta temp 
                        if (tipo == "LINK:") { achou = true; }
                    }
                    else if (op == 2)
                    {
                        //Retorna o Patch da pasta temp 
                        if (tipo == "TEMP:") { achou = true; }
                    }

                }

                sr.Close(); //grava e fecha

                return pastatmp.Substring(5);

            }
            catch (Exception ex)
            {                
                pastatmp = "Erro ao buscar o arquivo de Conexão!!!\n\n" + ex.Message;
            }

            return pastatmp;

        }

    }


}
