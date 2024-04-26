using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;

namespace AppServiceVMais
{
    class Program
    {
        static void Main(string[] args)
        {
            /// Para Cada Cadastro na Tabela de Domínios 
            /// Chama o Webservice de Conexão e depois o de serviço
            /// Autor: Aécio Espinoza Miranda
            /// Data Início: 10/10/2022

            ResultWS DadosRetorno;
            string result = "";
            string arquivoLog = "LogServicesVMais.txt";
            int tempoexec = (args.Length > 0) ? Convert.ToInt32(args[0]) : 5;

            if (File.Exists(arquivoLog)) //Deleta o arquivo de Log para Cada nova Execução 
            {
                try
                {
                    var linhaData = "";

                    using (StreamReader sr = new StreamReader(arquivoLog))
                    {
                        linhaData = sr.ReadLine();
                    }

                    linhaData = linhaData.Substring(0, 10);
                    var DataArq = Convert.ToDateTime(linhaData);

                    if (DataArq.Date < DateTime.Now.Date)
                    {
                        File.Delete(arquivoLog);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Erro ao tratar Arquivo de Log" + ex.Message);
                }
            }

            if (!File.Exists(arquivoLog)) //Se o Arquivo não existe ele será criado com a primeira linha com a Data de criação
            {
                LOG.GravaLog(DateTime.Now.Date.ToString());
            }

            LOG.GravaLog(DateTime.Now.ToString() + "| LOG | " + "**************************************** Inicializando o Sistema ****************************************" + "\n");

            //string baseDirectory = AppDomain.CurrentDomain.BaseDirectory; // Obtém o diretório de onde o programa está sendo executado
            //string relativePath = @"..\..\_lib\tmp"; // Caminho relativo para a pasta "tmp"
            //string directoryPath = Path.Combine(baseDirectory, relativePath);

            string directoryPath = DAO.GetPatchSrvVMais(2); //Retorna o caminho da pasta TMP no Servidor Vmais
            int maxAgeInSeconds = 600; // 10 minutos em segundos
            int minFileCount = 3000;

            var logtemp = CleanTempFiles(directoryPath, maxAgeInSeconds, minFileCount);
            LOG.GravaLog(logtemp);
            
            if (!CriaTarefa.CriarTarefa(tempoexec))  // Chama a Criação da Tarefa Caso não Exista
            {
                LOG.GravaLog(DateTime.Now.ToString() + "| LOG | " + "**************************************** Encerrando o Sistema ****************************************" + "\n");                
                return;
            }
            
            if (DAO.VerConnDB()) //Verifica se consegue fazer a conexão
            {
                LOG.GravaLog(DateTime.Now.ToString() + "| LOG | " + "Conexão ao Banco de Dados VMAIS_GATEWAY Efetuada com Sucesso!!!");

                var Sites = DAO.GetDominios();

                foreach (Dominios site in Sites)
                {
                    var dominio = site.dominio.Trim();
                    var siteId = site.id;

                    LOG.GravaLog(DateTime.Now.ToString() + "| LOG | " + "Inicializando Cliente: " + dominio + "\n");

                    try
                    {
                        LOG.GravaLog(DateTime.Now.ToString() + "| LOG | " + "Atualizando Conexão Domínio: " + dominio);

                        result = ConsWS(siteId, 0); /********* Executa Atualização da Conexão*********/

                        LOG.GravaLog(DateTime.Now.ToString() + "| LOG | " + "Retorno do WebService: " + result);

                        DadosRetorno = JsonConvert.DeserializeObject<ResultWS>(result);

                        if (DadosRetorno.status == 200)
                        {
                            LOG.GravaLog(DateTime.Now.ToString() + "| LOG | " + "Conexão OK Executando Serviços...");

                            result = ConsWS(siteId, 1); /********* Executa os Serviços *********/

                            LOG.GravaLog(DateTime.Now.ToString() + "| LOG | " + "Retorno do WebService: " + result);

                            var DadosServices = JsonConvert.DeserializeObject<List<Ccusto>>(result);

                            //Nave nos Resultados e imprime os resultados dos centros de custos processados
                            LOG.GravaLog(DateTime.Now.ToString() + "| LOG | " + $"{DadosServices.Count} Resultados do Domínio: " + dominio + ".vmaiserp.com");
                            foreach (Ccusto dados in DadosServices)
                            {
                                if (dados.status != 200)
                                {
                                    LOG.GravaLog(DateTime.Now.ToString() + "| LOG | " + "Problemas ao Executar os Serviços no Domínio: " + dominio + "| Mensagem: " + dados.message);
                                }
                                else
                                {
                                    LOG.GravaLog(DateTime.Now.ToString() + "| LOG | " + "Centro de Custo: " + dados.cencus);
                                    LOG.GravaLog(DateTime.Now.ToString() + "| LOG | " + "Mensagem: " + dados.message);
                                    LOG.GravaLog(DateTime.Now.ToString() + "| LOG | " + "Log: " + dados.log);
                                }
                            }
                        }
                        else
                        {
                            LOG.GravaLog(DateTime.Now.ToString() + "| LOG | " + "Problemas ao Atualizar a Conexão do Domínio: " + dominio + "| Mensagem: " + DadosRetorno.message);
                        }

                    }
                    catch (Exception erro)
                    {
                        LOG.GravaLog(DateTime.Now.ToString() + "| LOG | " + "Erro ao consumir o webservice pelo Domínio: " + dominio + "| Mensagem Erro: " + erro.Message);
                    }

                    LOG.GravaLog(DateTime.Now.ToString() + "| LOG | " + "Finalizando Cliente: " + dominio + "\n");
                    Thread.Sleep(3000);

                }

            }
            else
            {
                LOG.GravaLog(DateTime.Now.ToString() + "| LOG | " + "Erro ao conectar ao Banco de Dados VMais_Gateway!!!");
            }

            LOG.GravaLog(DateTime.Now.ToString() + "| LOG | " + "**************************************** Finalizando o Sistema ****************************************" + "\n");

        }

        public static string ConsWS(int siteId, int op)
        {
            try
            {
                var linkServer = DAO.GetServerVMais();
                var urlConexao = "";

                if (op == 0)
                {
                    urlConexao = linkServer + @"/blank_services/blank_services.php";
                }
                else
                {
                    urlConexao = linkServer + @"/blank_services_exec/blank_services_exec.php";
                }
               
                LOG.GravaLog(DateTime.Now.ToString() + "| LOG | " + "URL: " + urlConexao);

                var client = new RestClient(urlConexao);

                var request = new RestRequest(Method.POST);

                request.AddHeader("Content-Type", "application/json");
                request.AddCookie("PHPSESSID", "3bb9236cdd782d8416780c0145f69eea");
                request.AddCookie("sc_actual_lang_VMais", "pt_br");
                
                //Caso precise passar parâmetros via Body
                var objId = new { id = siteId };
                var body = JsonConvert.SerializeObject(objId);
                request.AddParameter("application/json", body, ParameterType.RequestBody);

                IRestResponse response = client.Execute(request);

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    var objErro = new { status = 404, message = "Motivo: " + response.ErrorMessage, log = "Erro" };
                    var erroJson = JsonConvert.SerializeObject(objErro);
                    return erroJson;
                }
                else
                {
                    return response.Content;
                }

            }
            catch (Exception erro)
            {
                return "Erro ao Consumir o WebService: " + erro.Message;
            }
        }

        public static string CleanTempFiles(string directoryPath, int maxAgeInSeconds, int minFileCount)
        {

            string logtmp = DateTime.Now.ToString() + "| LOG | LIMPEZA | Iniciando Limpeza pasta TMP Vmais...\n";
            logtmp += DateTime.Now.ToString() + "| LOG | LIMPEZA | PATCH | "+directoryPath+"| AGE | "+maxAgeInSeconds.ToString()+" | QNTARQ | "+minFileCount.ToString()+"\n";

            try
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(directoryPath);
                FileInfo[] files = directoryInfo.GetFiles();

                if (files.Length >= minFileCount)
                {
                    DateTime thresholdTime = DateTime.Now.AddSeconds(-maxAgeInSeconds);
                    int totarqs = 0;

                    foreach (FileInfo file in files)
                    {
                        if (file.CreationTime <= thresholdTime)
                        {
                            file.Delete();
                            totarqs++;
                        }
                    }
                    
                    logtmp += DateTime.Now.ToString() + "| LOG | LIMPEZA | Limpeza concluída! "+totarqs.ToString() +" Arquivos Eliminados...\n";
                }
                else
                {                    
                    logtmp += DateTime.Now.ToString() + "| LOG | LIMPEZA | Não é necessário fazer a limpeza!\n";
                }
            }
            catch (Exception ex)
            {                
                logtmp += DateTime.Now.ToString() + "| LOG | LIMPEZA | "+$"Ocorreu um erro: {ex.Message}\n";
            }

            logtmp += DateTime.Now.ToString() + "| LOG | LIMPEZA | Finalizando a Limpeza pasta TMP Vmais...\n";
            return logtmp;
        }
    }
}
