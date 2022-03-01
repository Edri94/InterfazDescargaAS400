﻿using InterfazDescargaAS400.Helpers;
using InterfazDescargaAS400.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace InterfazDescargaAS400.Data
{
    public class ArchivoTexto
    {
        String path;
        String archivo_as400;
        String archivo_holds;
        List<DiccionarioDatos> limites;
        FuncionesBd bd;
        Encriptacion encriptacion;
        List<String> querys;
        List<Hold_EQTKT> holds; //[prueba];

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

        String client_access;
        String PathModelos;
        String PathTransfer;
        String PathDatos;
        String ArchivoFDF;
        String ArchivoDTF;


        public ArchivoTexto()
        {
            this.path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location); 
            this.archivo_as400 = Funcion.getValueAppConfig("Archivo", "AS400");
            this.limites = new List<DiccionarioDatos>();
            this.encriptacion = new Encriptacion();
            this.querys = new List<string>();
            this.holds = new List<Hold_EQTKT>(); //[prueba]
            Log.EscribeLog = (Funcion.getValueAppConfig("Escribe", "LOG") == "Si") ? true : false;

            equipo = encriptacion.Decrypt(Funcion.getValueAppConfig("Equipo", "AS400"));
            libreria = encriptacion.Decrypt(Funcion.getValueAppConfig("Libreria", "AS400"));
            archivo = Funcion.getValueAppConfig("Archivo", "AS400");
            usuario = encriptacion.Decrypt(Funcion.getValueAppConfig("Usuario", "AS400"));
            psw = encriptacion.Decrypt(Funcion.getValueAppConfig("PSW", "AS400"));

            client_access =Funcion.getValueAppConfig("ClientAccess", "RUTAS");
            PathModelos = Funcion.getValueAppConfig("PathModelos", "RUTAS");
            PathTransfer = Funcion.getValueAppConfig("PathTransfer", "RUTAS");
            PathDatos = Funcion.getValueAppConfig("PathDatos", "RUTAS");
            ArchivoFDF = Funcion.getValueAppConfig("ArchivoFDF", "RUTAS");
            ArchivoDTF = Funcion.getValueAppConfig("ArchivoDTF", "RUTAS");
            descargaArchivoHolds();

            this.ConectDB();
        }

        private void descargaArchivoHolds()
        {
            this.archivo_as400 = this.archivo_as400.Replace("dd", DateTime.Now.ToString("dd"));
            String nombreArchivo = this.path + "\\" + this.archivo_as400;

            String nombreArchivoDtf = this.PathModelos + this.ArchivoDTF;
            String nombreArchivoDtfDestino = this.PathTransfer + "JUPFHolds" + DateTime.Now.ToString("yyMMdd") + ".DTF";
            String archivoDtfDestino = "JUPFHolds" + DateTime.Now.ToString("yyMMdd") + ".DTF";


            String nombreArchivoFDF = this.PathModelos + this.ArchivoFDF;
            String nombreArchivoFDFDestino = this.PathTransfer + "JUPFHolds" + DateTime.Now.ToString("yyMMdd") + ".FDF";
            String archivoFDFDestino = "JUPFHolds" + DateTime.Now.ToString("yyMMdd") + ".FDF";

            File.Copy(nombreArchivoDtf, nombreArchivoDtfDestino, true);
            File.Copy(nombreArchivoFDF, nombreArchivoFDFDestino, true);

            this.archivo_holds = $"{this.PathDatos}{this.archivo_as400}.TXT";

            Funcion.SetParameterTransfer("HostFile", $"{libreria}/{this.archivo_as400}", archivoDtfDestino, this.PathTransfer);
            Funcion.SetParameterTransfer("FDFFile", nombreArchivoFDFDestino, archivoDtfDestino, this.PathTransfer);
            Funcion.SetParameterTransfer("PCFile", this.archivo_holds, archivoDtfDestino, this.PathTransfer);

          
            ejecutaTransfer(nombreArchivoDtfDestino);

        }

        private void ejecutaTransfer(string nombreArchivoDtfDestino)
        {
            try
            {
                Process p = new Process();
                p.EnableRaisingEvents = false;
                p.StartInfo.FileName = $"{client_access}cwbtf.exe";
                p.StartInfo.Arguments = nombreArchivoDtfDestino;
                p.StartInfo.CreateNoWindow = false;
                p.Start();
                p.WaitForExit();

            }
            catch (Exception ex)
            {
                Log.Escribe("Error al abrir el ejecutable ", "Error");
                Log.Escribe(ex);
            }
        }

        public string LeerArchivo()
        {
            String line;
            String lines = "";

            int c = 0;

            try
            {
                if (this.ExisteArchivo())
                {
                    this.EstablecerLimites();
                   
                    StreamReader sr = new StreamReader(this.archivo_holds);
                    line = sr.ReadLine();

                    if (this.conectado)
                    {
                        bd.ejecutarDelete("delete from TMP_HOLDS_EQTKT where 1 = 1");

                        while (line != null)
                        {
                            //Sin LINQ
                            if (line.Length < 232)
                            {
                                break;
                            }

                            if (line.Substring(4, 1) != "9" && line.Substring(4, 1) != "8" && (line.Substring(49, 3).Contains("CBP") == false && (line.Substring(49, 3).Contains("CBS") == false)))
                            {
                                querys.Add(ArmaQueryInsert(line, c));
                            }

                            ////[prueba] para hacer LIINQ
                            //holds.Add(SeparaDatos(line, c));

                            line = sr.ReadLine();
                            c++;

                        }
                        sr.Close();
                    }
                }
                if(this.ConectDB() && c > 1)
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
                                fecha_out = "20" + fecha_out.Substring(1, 2) + "-" + fecha_out.Substring(3, 2) + "-" + fecha_out.Substring(5, 2);                              
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
                            query_values += line.Substring(start_index, diccionario.Posicion);
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
                            query_values += line.Substring(start_index, diccionario.Posicion);
                        }
                        else
                        {
                            if (diccionario.Etiqueta == "AMOUNT")
                            {
                                
                                query_etiquetas += diccionario.Etiqueta + ",";
                                float valorA = Int32.Parse(line.Substring(start_index, diccionario.Posicion));
                                float valorB = valorA / 100;
                                query_values += " " + valorB.ToString() + ",";
                            }
                            else
                            {
                                query_etiquetas += diccionario.Etiqueta + ",";
                                query_values += " " + line.Substring(start_index, diccionario.Posicion) + ",";
                            } 
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


        //[prueba]
        private Hold_EQTKT SeparaDatos(string line, int contador)
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
                                fecha_out = "20" + fecha_out.Substring(1, 2) + "-" + fecha_out.Substring(3, 2) + "-" + fecha_out.Substring(5, 2);                              
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
                            query_values += line.Substring(start_index, diccionario.Posicion);
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
                            query_values += line.Substring(start_index, diccionario.Posicion);
                        }
                        else
                        {
                            if (diccionario.Etiqueta == "AMOUNT")
                            {
                                
                                query_etiquetas += diccionario.Etiqueta + ",";
                                float valorA = Int32.Parse(line.Substring(start_index, diccionario.Posicion));
                                float valorB = valorA / 100;
                                query_values += " " + valorB.ToString() + ",";
                            }
                            else
                            {
                                query_etiquetas += diccionario.Etiqueta + ",";
                                query_values += " " + line.Substring(start_index, diccionario.Posicion) + ",";
                            } 
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

            return new Hold_EQTKT { };

        }

        public bool ExisteArchivo()
        {
            bool existe;

            try
            {
                existe = File.Exists(this.archivo_holds);
            }
            catch (Exception ex)
            {
                Log.Escribe("Error al encontrar el archivo: " + this.archivo_holds);
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

