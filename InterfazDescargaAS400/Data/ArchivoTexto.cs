using InterfazDescargaAS400.Helpers;
using InterfazDescargaAS400.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;


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

        /// <summary>
        /// Copia archivos de una carpeta a otra
        /// </summary>
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

        /// <summary>
        /// Ejecuta programa desde la ocnsola epserando que tenga exito
        /// </summary>
        /// <param name="nombreArchivoDtfDestino"></param>
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

        /// <summary>
        /// lee el archivo linea por linea, esta funcion es el nucleo de todo
        /// </summary>
        /// <returns></returns>
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
                            //[prueba] para hacer LIINQ
                            holds.Add(SeparaDatos(line, c));

                            line = sr.ReadLine();
                            c++;
                        }

                        List<Hold_EQTKT> lista_holds = new List<Hold_EQTKT>();
                        lista_holds = rellenarLista(holds);

                        List<Hold_EQTKT> resultados = (
                            from
                                h in lista_holds
                            where
                                (!h.RESP_CODE.Contains("CBP") && !h.RESP_CODE.Contains("CBS"))
                                &&
                                (!h.ACCOUNT.StartsWith("9") && !h.ACCOUNT.StartsWith("8"))
                            select
                                h
                        ).ToList();

                        if (this.ConectDB() && c > 1)
                        {
                            bd.transaccionInsert(ArmaQuerys(resultados));
                        }

                        sr.Close();
                    }
                }
    
            }
            catch(Exception ex)
            {
                Log.Escribe(ex);
                Log.Escribe("error en la linea: " + c);
            }
            

            return lines;
        }

        /// <summary>
        /// Crea los query insert a partir de una lista de objetos Hold_EQTKT
        /// </summary>
        /// <param name="lista_holds"></param>
        /// <returns></returns>
        private List<string> ArmaQuerys(List<Hold_EQTKT> lista_holds)
        {
            List<string> inserts = new List<string>();
            String query_completo = "";

            foreach (Hold_EQTKT h in lista_holds)
            {
                query_completo = $"" +
                    $"INSERT INTO TMP_HOLDS_EQTKT" +
                    $"(CD_BRANCH,ACCOUNT,SUFIX,HOLD_NO,START_DATE,EXPIRY_DATE,AMOUNT,RESP_CODE,REASON_CODE,DSC_LINE1,DSC_LINE2,DSC_LINE3,DSC_LINE4,INPUT_DATE)" +
                    $"VALUES" +
                    $"( '{h.CD_BRANCH}', '{h.ACCOUNT}', '{h.SUFIX}', {h.HOLD_NO}, '{h.START_DATE}', '{h.EXPIRY_DATE}', {h.AMOUNT}, '{h.RESP_CODE}', '{h.REASON_CODE}', '{h.DSC_LINE1}', '{h.DSC_LINE2}', '{h.DSC_LINE3}', '{h.DSC_LINE4}', '{h.INPUT_DATE}')";

                inserts.Add(query_completo);
            }

            return inserts;
        }


        /// <summary>
        /// Separa una cadena de texto en un objeto de tipo Hold_EQTKT
        /// </summary>
        /// <param name="line">cadena con datos tabulados</param>
        /// <param name="contador">numero de linea, no sirve de nada pero ahi lo dejo</param>
        /// <returns></returns>
        private Hold_EQTKT SeparaDatos(string line, int contador)
        {
            int start_index = 0, c = 0;
            Hold_EQTKT hold_EQTKT = new Hold_EQTKT();
            string fecha_out = "";


            try
            {
                foreach (DiccionarioDatos diccionario in limites)
                {               
                    switch(diccionario.Etiqueta)
                    {
                        case "CD_BRANCH": hold_EQTKT.CD_BRANCH = line.Substring(start_index, diccionario.Posicion); break;
                        case "ACCOUNT":                            
                            hold_EQTKT.ACCOUNT = line.Substring(start_index, diccionario.Posicion).Trim();
                            hold_EQTKT.ACCOUNT = (hold_EQTKT.ACCOUNT == "") ? "XXX" : hold_EQTKT.ACCOUNT;
                            break;                                             
                        case "SUFIX":hold_EQTKT.SUFIX = line.Substring(start_index, diccionario.Posicion); break;
                        case "HOLD_NO": hold_EQTKT.HOLD_NO = Int32.Parse(line.Substring(start_index, diccionario.Posicion)); break;
                        case "START_DATE":
                            fecha_out = line.Substring(start_index, diccionario.Posicion);
                            fecha_out = "20" + fecha_out.Substring(1, 2) + "-" + fecha_out.Substring(3, 2) + "-" + fecha_out.Substring(5, 2);
                            hold_EQTKT.START_DATE = fecha_out; 
                            break;
                        case "EXPIRY_DATE":
                            fecha_out = line.Substring(start_index, diccionario.Posicion);
                            fecha_out = "20" + fecha_out.Substring(1, 2) + "-" + fecha_out.Substring(3, 2) + "-" + fecha_out.Substring(5, 2);
                            hold_EQTKT.EXPIRY_DATE = fecha_out;
                            break;
                        case "AMOUNT":
                            float valorA = Int32.Parse(line.Substring(start_index, diccionario.Posicion));
                            float valorB = valorA / 100;
                            hold_EQTKT.AMOUNT = valorB;
                            break;
                        case "RESP_CODE":
                            hold_EQTKT.RESP_CODE = line.Substring(start_index, diccionario.Posicion).Trim();
                            hold_EQTKT.RESP_CODE = (hold_EQTKT.RESP_CODE == "") ? "XXX" : hold_EQTKT.RESP_CODE;
                            break;
                        case "REASON_CODE": hold_EQTKT.REASON_CODE = line.Substring(start_index, diccionario.Posicion); break;
                        case "DSC_LINE1": hold_EQTKT.DSC_LINE1 = line.Substring(start_index, diccionario.Posicion); break;
                        case "DSC_LINE2": hold_EQTKT.DSC_LINE2 = line.Substring(start_index, diccionario.Posicion); break;
                        case "DSC_LINE3": hold_EQTKT.DSC_LINE3 = line.Substring(start_index, diccionario.Posicion); break;
                        case "DSC_LINE4": hold_EQTKT.DSC_LINE4 = line.Substring(start_index, diccionario.Posicion); break;
                        case "INPUT_DATE":
                            fecha_out = line.Substring(start_index, diccionario.Posicion);
                            fecha_out = "20" + fecha_out.Substring(1, 2) + "-" + fecha_out.Substring(3, 2) + "-" + fecha_out.Substring(5, 2);
                            hold_EQTKT.INPUT_DATE = fecha_out;
                            break;
                    }                                  
                    c++;
                    start_index += diccionario.Posicion;             
                }

            }
            
            catch (Exception ex)
            {
                Log.Escribe(ex);
            }

            return hold_EQTKT;

        }

       /// <summary>
       /// Metodo parad devolver una lista sin nulos
       /// </summary>
       /// <param name="lista"> lista con valores nulos</param>
       /// <returns></returns>
        public List<Hold_EQTKT> rellenarLista(List<Hold_EQTKT> lista)
        {
            List<Hold_EQTKT> lista_hold = new List<Hold_EQTKT>();

            foreach(Hold_EQTKT h in lista)
            {
                if(h.CD_BRANCH != null)
                {
                    lista_hold.Add(h);
                }
            }

            return lista_hold;
        }

        /// <summary>
        /// Verifica si existe un archivo 
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Obtiene los limites establecidos en ell app.config
        /// </summary>
        public void EstablecerLimites()
        {
            int num_limites = Int32.Parse(Funcion.getValueAppConfig("limites", "parametro"));

            for (int i = 1; i <= num_limites; i++)
            {
                String[] valores = Funcion.getValueAppConfig("limite" + i).Split(',');
                limites.Add(new DiccionarioDatos { Etiqueta = valores[0], Posicion = Int32.Parse(valores[1]), TipoDato = valores[2]});
            }
        }

        /// <summary>
        /// Conectarse a la base de datosf
        /// </summary>
        /// <returns></returns>
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

