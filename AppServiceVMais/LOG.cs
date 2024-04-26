using System;
using System.IO;

namespace AppServiceVMais
{
    public class LOG
    {        
        public static void GravaLog(string logtxt)
        {
            Console.WriteLine(logtxt);

            string arquivoLog = "LogServicesVMais.txt";

            try
            {
                using (StreamWriter file = new StreamWriter(arquivoLog, append: true))
                {
                    file.WriteLine(logtxt);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erro ao Gravar Log" + ex.Message);
            }
        }

    }
}

