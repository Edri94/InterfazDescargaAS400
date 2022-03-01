using InterfazDescargaAS400.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterfazDescargaAS400.Data
{
    public class FuncionesBd : ConexionBD
    { 
        public ConexionBD cnnConexion;
        SqlConnection connection;
        String connectionString;

        Encriptacion encriptacion;

        public FuncionesBd(string cnn) : base(cnn)
        {
            encriptacion = new Encriptacion();
            cnnConexion = new ConexionBD(cnn);
            this.connectionString = cnn;


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

        public int ejecutarDelete(string query)
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


        public int transaccionInsert(List<String> querys)
        {
            using (SqlConnection connection = new SqlConnection(this.connectionString))
            {
                connection.Open();

                SqlCommand command = connection.CreateCommand();
                SqlTransaction transaction;

                transaction = connection.BeginTransaction("transaccionInsert");

                command.Connection = connection;
                command.Transaction = transaction;

                try
                {

                    foreach (String query in querys)
                    {
                        command.CommandText = query;Log.Escribe(query, "Query:");
                        command.ExecuteNonQuery();

                    }

                    transaction.Commit();
                    Log.Escribe("records are written to database.");
                }
                catch (Exception ex)
                {
                    Log.Escribe("Commit Exception");
                    Log.Escribe(ex);
                    try
                    {
                        transaction.Rollback();
                    }
                    catch (Exception ex2)
                    {
                        Log.Escribe("Rollback Exception");
                        Log.Escribe(ex2);
                    }
                }
            }

            return 1;
        }

        public SqlDataReader ejecutarConsulta(string query)
        {
            try
            {
                cnnConexion.ActiveConnection = true;
                cnnConexion.ParametersContains = false;
                cnnConexion.CommandType = CommandType.Text;
                cnnConexion.ActiveConnection = true;

                SqlDataReader sqlRecord = cnnConexion.ExecuteDataReader(query);

                return sqlRecord;
            }
            catch (Exception)
            {
                return null;
                throw;
            }
        }
    }


}
