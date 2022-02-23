using InterfazDescargaAS400.Helpers;
using InterfazDescargaAS400.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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
        Encriptacion encriptacion;
        List<String> querys;

        String equipo;
        String libreria;
        String archivo;
        String usuario;
        String psw;


        public string gsPswdDB;
        public string gsUserDB;
        public string gsNameDB;
        public string gsCataDB;
        public string gsDSNDB;
        public string gsSrvr;

        bool conectado = false;

        String client_access = Funcion.getValueAppConfig("ClientAccess", "RUTAS");


        public ArchivoTexto()
        {
            this.path = Funcion.getValueAppConfig("Archivo", "AS400");
            this.limites = new List<DiccionarioDatos>();
            this.encriptacion = new Encriptacion();
            this.querys = new List<string>();
            Log.EscribeLog = (Funcion.getValueAppConfig("Escribe", "LOG") == "Si") ? true : false;

            equipo = encriptacion.Decrypt(Funcion.getValueAppConfig("Equipo", "AS400"));
            libreria = encriptacion.Decrypt(Funcion.getValueAppConfig("Libreria", "AS400"));
            archivo = Funcion.getValueAppConfig("Archivo", "AS400");
            usuario = encriptacion.Decrypt(Funcion.getValueAppConfig("Usuario", "AS400"));
            psw = encriptacion.Decrypt(Funcion.getValueAppConfig("PSW", "AS400"));

            client_access = encriptacion.Decrypt(Funcion.getValueAppConfig("ClientAccess", "RUTAS"));

            this.ConectDB();
        }

        public string LeerArchivo()
        {
            String line;
            String lines = "";
            String query = "";
            int c = 0;

            try
            {

                this.path = this.path.Replace("dd", DateTime.Now.ToString("dd"));

                if (this.ExisteArchivo())
                {
                    this.EstablecerLimites();
                   
                    StreamReader sr = new StreamReader(path);
                    line = sr.ReadLine();

                    if (this.conectado)
                    {
                        bd.ejecutarDelete("delete from TMP_HOLDS_EQTKT where 1 = 1");

                        while (line != null)
                        {
                            if(line.Length < 232)
                            {
                                break;
                            }
                            if (line.Substring(4, 1) != "9" && line.Substring(4, 1) != "8")
                            {
                                querys.Add(ArmaQueryInsert(line, c));                             
                            }
                            line = sr.ReadLine();  
                           
                            c++;

                        }
                        sr.Close();
                    }
                }
                if(this.ConectDB())
                {
                    bd.transaccionInsert(querys);
                }
                
            }
            catch(Exception ex)
            {
                Log.Escribe(ex);
                Log.Escribe("error en la linea: " + c);
            }
            

            return lines;
        }

        private string ArmaQueryInsert(string line, int contador)
        {
            int start_index = 0, c = 0;
            String query_completo = "", query_etiquetas = "", query_values = "";
            Hold_EQTKT hold_EQTKT = new Hold_EQTKT();
            String fecha_out ="";

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

                            fecha_out = line.Substring(start_index, diccionario.Posicion);
                            fecha_out = "20" + fecha_out.Substring(1, 2) + "-" + fecha_out.Substring(3, 2) + "-" + fecha_out.Substring(5, 2);
                            query_values += " '" + fecha_out + "'";
                        }
                        else
                        {
                            query_etiquetas += diccionario.Etiqueta + ",";

                            if (diccionario.Etiqueta == "START_DATE" || diccionario.Etiqueta == "EXPIRY_DATE")
                            {
                                fecha_out = line.Substring(start_index, diccionario.Posicion);
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

        public bool ConectDB()
        {
            bool ConectDB = false;
            string section = "conexion";
            try
            {
                string a = Funcion.getValueAppConfig("DBCata", section);
                this.gsCataDB = encriptacion.Decrypt(a);
                this.gsDSNDB = encriptacion.Decrypt(Funcion.getValueAppConfig("DBDSN", section));
                this.gsSrvr = encriptacion.Decrypt(Funcion.getValueAppConfig("DBSrvr", section));
                this.gsUserDB = encriptacion.Decrypt(Funcion.getValueAppConfig("DBUser", section));
                this.gsPswdDB = encriptacion.Decrypt(Funcion.getValueAppConfig("DBPswd", section));
                this.gsNameDB = encriptacion.Decrypt(Funcion.getValueAppConfig("DBName", section));

                string conn_str = $"Data source ={this.gsSrvr}; uid ={this.gsUserDB}; PWD ={this.gsPswdDB}; initial catalog = {this.gsNameDB}";
                this.bd = new FuncionesBd(conn_str);

                ConectDB = true;
                conectado = true;


            }
            catch (Exception ex)
            {
                ConectDB = false;
                conectado = false;
                Log.Escribe(ex, "Error");
            }
            return ConectDB;
        }




    }
}

