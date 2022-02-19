using InterfazDescargaAS400.Helpers;
using InterfazDescargaAS400.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterfazDescargaAS400.Data
{
    public class ArchivoTexto
    {
        String path;
        Dictionary<String, String> limites = new Dictionary<String, String>();
        public ArchivoTexto(String path)
        {
            this.path = path;
           
        }

        public string LeerArchivo()
        {
            String line;
            String lines = "";
            int c = 0;
        

            try
            {
                if(this.ExisteArchivo())
                {
                    this.EstablecerLimites();

                    StreamReader sr = new StreamReader(path);
                    line = sr.ReadLine();

                    while (line != null)
                    {
                        line = sr.ReadLine();
                        lines = lines + line;

                        String prueba = ObtenerQuery(line);

                        c++;
                    }
                    sr.Close();
                }           
            }
            catch (IOException ex)
            {
                Log.Escribe(ex);
            }

            return lines;
        }

        private string ObtenerQuery(string line)
        {
            int start_index = 0;
            int lenght_usado = 0;
            String query = "";
            Hold_EQTKT hold_EQTKT = new Hold_EQTKT();

            query = "INSERT INTO TMP_HOLDS_EQTKT(CD_BRANCH,ACCOUNT,SUFIX,HOLD_NO,START_DATE,EXPIRY_DATE,AMOUNT,RESP_CODE,REASON_CODE,DSC_LINE1,DSC_LINE2,DSC_LINE3,DSC_LINE4,INPUT_DATE) VALUES(";


            try
            {
                for(int i = 0; i <= limites.Count; i++)
                {
                    hold_EQTKT.CD_BRANCH = line.Substring(0, 4);
                }
            }
            catch (Exception ex)
            {
                Log.Escribe(ex);
            }

            return "";

        }

        public bool ExisteArchivo()
        {
            bool existe;

            try
            {
                existe = File.Exists(path);
            }
            catch (Exception ex)
            {
                Log.Escribe(ex);
                existe = false;
            }

            return existe;
        }

        public void EstablecerLimites()
        {
            int num_limites = Int32.Parse(Funcion.getValueAppConfig("limites", "parametro"));

            for (int i = 1; i <= num_limites; i++)
            {
                String[] valores = Funcion.getValueAppConfig("limite" + i).Split(',');
                limites.Add(valores[0], valores[1]);
            }
        }

    }
}

