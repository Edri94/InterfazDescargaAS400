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
        List<DiccionarioDatos> limites;
        FuncionesBd bd;
       

        public ArchivoTexto(String path)
        {
            this.path = path;
            this.bd = new FuncionesBd();
            this.limites = new List<DiccionarioDatos>();
            Log.EscribeLog = true;
        }

        public string LeerArchivo()
        {
            String line;
            String lines = "";
            String query = "";
            int c = 0;
        

            try
            {
                if (this.ExisteArchivo())
                {
                    this.EstablecerLimites();

                    StreamReader sr = new StreamReader(path);
                    line = sr.ReadLine();
                    
                    while (line != null)
                    {                      
                        if (line.Substring(4,1) != "9" && line.Substring(4, 1) != "8")
                        {                        
                            lines = lines + line;

                            query = ArmaQueryInsert(line);

                            if(bd.ConectDB())
                            {
                                bd.ejecutarInsert(query);
                            }

                           
                        }
                        line = sr.ReadLine();
                        c++;
                    }
                    sr.Close();
                }           
            }
            catch(Exception ex)
            {
                Log.Escribe(ex);
            }
            

            return lines;
        }

        private string ArmaQueryInsert(string line)
        {
            int start_index = 0, c = 0;
            String query_completo = "", query_etiquetas = "", query_values = "";
            Hold_EQTKT hold_EQTKT = new Hold_EQTKT();

            query_completo = "INSERT INTO TMP_HOLDS_EQTKT";


            try
            {
                foreach (DiccionarioDatos diccionario in limites)
                {              
                    if (diccionario.TipoDato == "varchar")
                    {                     
                        if (c == (limites.Count - 1))
                        {
                            query_etiquetas += diccionario.Etiqueta;
                            //query_values += " '" + line.Substring(start_index, diccionario.Posicion) + "'";

                            var fecha_out = line.Substring(start_index, diccionario.Posicion);
                            fecha_out = "20" + fecha_out.Substring(1, 2) + "-" + fecha_out.Substring(3, 2) + "-" + fecha_out.Substring(5, 2);
                            query_values += " '" + fecha_out + "'";
                        }
                        else
                        {
                            query_etiquetas += diccionario.Etiqueta + ",";

                            if (diccionario.Etiqueta == "START_DATE" || diccionario.Etiqueta == "EXPIRY_DATE")
                            {
                                var fecha_out = line.Substring(start_index, diccionario.Posicion);
                                fecha_out = "20" + fecha_out.Substring(1,2) + "-" + fecha_out.Substring(3, 2) + "-" + fecha_out.Substring(5, 2);
                                query_values += " '" + fecha_out + "',";
                            }
                            else
                            {                           
                                query_values += " '" + line.Substring(start_index, diccionario.Posicion) + "',";
                            }                                                   
                        }                                          
                    }
                    else if (diccionario.TipoDato == "int")
                    {
                        if (c == (limites.Count - 1))
                        {
                            query_etiquetas += diccionario.Etiqueta;
                            query_values += " '" + line.Substring(start_index, diccionario.Posicion);
                        }
                        else
                        {
                            query_etiquetas += diccionario.Etiqueta + ",";
                            query_values += " " + line.Substring(start_index, diccionario.Posicion) + ",";
                        }
                    }
                    else if (diccionario.TipoDato == "numeric")
                    {
                        if (c == (limites.Count -1))
                        {
                            query_etiquetas += diccionario.Etiqueta;
                            query_values += " '" + line.Substring(start_index, diccionario.Posicion);
                        }
                        else
                        {
                            query_etiquetas += diccionario.Etiqueta + ",";
                            query_values += " " + line.Substring(start_index, diccionario.Posicion) + ",";
                        }
                    }
                    c++;
                    start_index += diccionario.Posicion;
                   
                }

                query_completo += "(" + query_etiquetas + ") VALUES(" + query_values + ")";
                //Log.Escribe(query_completo, "Query Insertar:");

            }
            
            catch (Exception ex)
            {
                Log.Escribe(ex);
            }

            return query_completo;

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
                limites.Add(new DiccionarioDatos { Etiqueta = valores[0], Posicion = Int32.Parse(valores[1]), TipoDato = valores[2]});
            }
        }

        


    }
}

