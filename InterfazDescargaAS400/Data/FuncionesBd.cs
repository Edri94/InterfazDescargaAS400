using InterfazDescargaAS400.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterfazDescargaAS400.Data
{
    public class FuncionesBd
    {

        public string gsPswdDB;
        public string gsUserDB;
        public string gsNameDB;
        public string gsCataDB;
        public string gsDSNDB;
        public string gsSrvr;

        public ConexionBD cnnConexion;

        Encriptacion encriptacion;

        public FuncionesBd()
        {
            encriptacion = new Encriptacion();
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
                this.cnnConexion = new ConexionBD(conn_str);

                ConectDB = true;

                return ConectDB;

            }
            catch (Exception ex)
            {
                Log.Escribe(ex, "Error");
            }
            return ConectDB;
        }

        public int ejecutarInsert(string query)
        {
            try
            {
                cnnConexion.ActiveConnection = true;
                cnnConexion.ParametersContains = false;
                cnnConexion.CommandType = CommandType.Text;
                cnnConexion.ActiveConnection = true;

                int afectados = cnnConexion.ExecuteNonQuery(query);

                return afectados;
            }
            catch (Exception ex)
            {
                Log.Escribe(ex);
                return -1;
            }
        }
    }
}
