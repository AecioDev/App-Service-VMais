using Microsoft.Win32.TaskScheduler;
using System;
using System.IO;
using System.Linq;

namespace AppServiceVMais
{
    public class CriaTarefa
    {

        //TaskSchedulerClass oAgendador; 
        //ITaskDefinition oDefinicaoTarefa; //Para tratar a definição da tarefa
        //ITimeTrigger oTrigger; //Para tratar a informação do Trigger
        //IExecAction oAcao; //Para tratar a informação da Ação
                       
        public static bool CriarTarefa(int Intervalo)
        {   
            try
            {
                // Get the service on the local machine
                using (TaskService ts = new TaskService())
                {
                    //
                    // Necessário criar a Tarefa com o Usuário SYSTEM para que o executável seja executado com privilégios de ADM
                    //Utilizar o XML para Importar o Serviço que está anexo na pasta de Instalação do aplicativo
                    //

                    //Debug pra ver as tarefas cadastradas
                    //var listaTarefas = ts.RootFolder.AllTasks.ToList();
                    //foreach (var task in listaTarefas) { 
                    //    LOG.GravaLog(DateTime.Now.ToString() + "| LOG | TAREFA | " + "Tarefa | " + task.Name);
                    //}

                    if (!ts.RootFolder.AllTasks.Any(t => t.Name == "App Service Vmais")) //Se a Tarefa Não Existir Tenta Cadastrar
                    {
                        //LOG.GravaLog(DateTime.Now.ToString() + "| LOG | TAREFA | " + "Necessário Criar a Tarefa no Agendador de Tarefas...");

                        LOG.GravaLog(DateTime.Now.ToString() + "| LOG | TAREFA | " + "Criando a Tarefa Agendada no Windows para executar os Serviços!!!");

                        // Crie uma nova definição de tarefa e atribua propriedades
                        TaskDefinition td = ts.NewTask();
                        td.RegistrationInfo.Author = "Controller Informática - VMais ERP";
                        td.RegistrationInfo.Description = "Executa Serviços do Vmais em segundo plano no servidor.\n\n- Consulta Automática de Notas na SEFAZ\n- Manifesto Automático de Notas\n- Download Automático de Notas Manifestadas";
                        td.RegistrationInfo.Date = DateTime.Now;
                        td.Principal.RunLevel = TaskRunLevel.Highest;
                        
                        td.Settings.Enabled = true;
                        td.Settings.Hidden = false;
                        td.Settings.RunOnlyIfNetworkAvailable = false;

                        // Crie um gatilho que irá disparar a tarefa nesta hora todos os dias
                        var trigger = new TimeTrigger();
                        trigger.Repetition.Interval = TimeSpan.FromMinutes(Intervalo);
                        td.Triggers.Add(trigger);

                        // Crie uma ação que iniciará o Bloco de notas sempre que o gatilho for disparado
                        //td.Actions.Add(new ExecAction( ProgramaExe, ProgramaArgs, null));
                        td.Actions.Add(new ExecAction(System.Reflection.Assembly.GetExecutingAssembly().Location, null, Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)));

                        // Registra a tarefa na pasta raiz

                        // -- Tem alguns Servidores que não reconhece a tarefa com o Usuário SERVIÇO DE REDE então deixei o Padrão q ele vai criar com o usuário Logado
                        // -- mas tem que executar como administrador de todo jeito.

                        //ts.RootFolder.RegisterTaskDefinition("App Service Vmais", td, TaskCreation.CreateOrUpdate, "SERVIÇO DE REDE", null, TaskLogonType.S4U, "");
                        ts.RootFolder.RegisterTaskDefinition("App Service Vmais", td);

                        LOG.GravaLog(DateTime.Now.ToString() + "| LOG | TAREFA | " + "Tarefa Agendada com Sucesso!!!");                        
                    }
                }
            }
            catch (Exception ex)
            {
                var txterro = ex.Message + "\n" + ex.InnerException;
                txterro = "Erro ao Agendar a Tarefa: " + txterro + "\nFavor Iniciar o Programa como Administrador!!!";
                LOG.GravaLog(DateTime.Now.ToString() + "| LOG | TAREFA | ERRO |" + txterro);
                Console.WriteLine(txterro);
                Console.ReadLine();
                return false;
            }

            return true;
        }

    }
}
