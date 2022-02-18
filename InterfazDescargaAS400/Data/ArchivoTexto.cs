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
        public ArchivoTexto(String path)
        {
            this.path = path;
        }

        public String leerArchivo()
        {
            String line;

            try
            {
                StreamReader sr = new StreamReader(path);
                line = sr.ReadLine();
                while (line != null)
                {
                    Console.WriteLine(line);
                    line = sr.ReadLine();
                }
                sr.Close();
                Console.ReadLine();
            }
            catch (IOException ex)
            {

                throw;
            }

            return "";
        }

        public bool existeArchivo()
        {
            bool existe;

            try
            {
                existe = File.Exists(path);
            }
            catch (Exception ex)
            {
                existe = false;
            }

            return existe;
        }

    }
}

