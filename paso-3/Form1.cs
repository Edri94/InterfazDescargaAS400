using InterfazDescargaAS400.Data;
using InterfazDescargaAS400.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace paso_3
{
    public partial class Form1 : Form
    {
        public string gsPswdDB;
        public string gsUserDB;
        public string gsNameDB;
        public string gsCataDB;
        public string gsDSNDB;
        public string gsSrvr;

        Encriptacion encriptacion;

        FuncionesBd bd;

        bool conectado = false;

        public Form1()
        {
            InitializeComponent();

            encriptacion = new Encriptacion();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            txtConsola.Text += "$ Iniciando operacion .... \r \n" + Environment.NewLine;
            List<String> querys = new List<String>();

            if (Paso1() >= 1)
            {
                int afectados = Paso2();
            }
            else
            {
                txtConsola.Text += "$ No hay que actualizar  \r \n" + Environment.NewLine;
            }
            if (Paso3() >= 1)
            {
                int afectados = Paso4();
            }
            else
            {
                txtConsola.Text += "$ No hay que actualizar  \r \n" + Environment.NewLine;
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


        public int Paso1()
        {
            txtConsola.Text += $"$ Paso 1. \r \n" + Environment.NewLine;
            String consulta = @"
                SELECT 
                    COUNT(*)
                from 
                    ticket..TMP_HOLDS_EQTKT HT, HOLD_RETIRO hr,
                    ticket..PRODUCTO_CONTRATADO pc, ticket..HOLD ho
                where 
                    pc.fecha_vencimiento = '2022-02-25'
                    and 
                    hr.producto_contratado = pc.producto_contratado
                    and 
                    ho.producto_contratado = pc.producto_contratado
                    and 
                    CONVERT(INT,SUBSTRING(HT.DSC_LINE1,1,7)) = ho.producto_contratado
                    and 
                    ho.producto_contratado not in (select producto_contratado from bitacora_holds_eqtkt)
                    and 
                    pc.producto = 8014
                    and pc.status_producto in (8027,27);
            ";

            if (this.ConectDB())
            {
                DataTable dt = new DataTable();
                dt.Clear();
                dt.Columns.Add("count");

                return obtenerCount(bd.ejecutarConsulta(consulta));
            }

            return 0;
        }


        public int Paso3()
        {
            txtConsola.Text += $"$ Paso 2. \r \n" + Environment.NewLine;
            String consulta = @"
                SELECT 
	                COUNT(*)
                from 
	                ticket..TMP_HOLDS_EQTKT HT,
	                ticket..PRODUCTO_CONTRATADO pc, ticket..HOLD ho
                where 
	                pc.fecha_vencimiento = '2022-02-25'
	                and 
	                ho.producto_contratado = pc.producto_contratado
	                and 
	                CONVERT(INT,SUBSTRING(HT.DSC_LINE1,1,7)) = ho.producto_contratado
	                and 
	                ho.producto_contratado not in (select producto_contratado from bitacora_holds_eqtkt)
	                and 
	                pc.producto = 8014
	                and 
	                pc.status_producto in (8027,27)
	                and 
	                ho.descripcion4 like '%TPV%'
	                and 
	                ho.hold = 0;
            ";


            if (this.ConectDB())
            {
                DataTable dt = new DataTable();
                dt.Clear();
                dt.Columns.Add("count");

                return obtenerCount(bd.ejecutarConsulta(consulta));
            }

            return 0;
        }

        public int obtenerCount(SqlDataReader dr)
        {
            int resultados = 0;

            DataTable dt = new DataTable();
            dt.Clear();
            dt.Columns.Add("count");



            if (dr != null)
            {
                while (dr.Read())
                {
                    DataRow _row = dt.NewRow();

                    _row["count"] = dr.GetInt32(0);
                    resultados = dr.GetInt32(0);
                    dt.Rows.Add(_row);

                }
                dr.Close();
            }
            txtConsola.Text += $"$ Resultados: {resultados} \r \n" + Environment.NewLine;
            return resultados;
        }


        public int Paso2()
        {
            String consulta = @"
                update 
	                hr 
                set 
	                hr.hold = HT.HOLD_NO
                from 
	                ticket..TMP_HOLDS_EQTKT HT, 
	                HOLD_RETIRO hr,
	                ticket..PRODUCTO_CONTRATADO pc, ticket..HOLD ho
                where 
	                pc.fecha_vencimiento = '2022-02-25'
	                and 
	                hr.producto_contratado = pc.producto_contratado
	                and 
	                ho.producto_contratado = pc.producto_contratado
	                and 
	                CONVERT(INT,SUBSTRING(HT.DSC_LINE1,1,7)) = ho.producto_contratado
	                and 
	                ho.producto_contratado not in (select producto_contratado from bitacora_holds_eqtkt)
	                and 
	                pc.producto = 8014
	                and 
	                pc.status_producto in (8027,27)
            ";
            try
            {
                if (this.ConectDB())
                {
                    return bd.ejecutarInsert(consulta);
                }
                return -1;
            }
            catch (Exception ex)
            {
                Log.Escribe(ex);
                return -1;
            }

        }

        public int Paso4()
        {
            String consulta = @"
            update 
	            HO 
            set 
	            ho.hold = ht.HOLD_NO
            from 
	            ticket..TMP_HOLDS_EQTKT HT,
	            ticket..PRODUCTO_CONTRATADO pc, ticket..HOLD ho
            where 
	            pc.fecha_vencimiento = '2022-02-25'
	            and 
	            ho.producto_contratado = pc.producto_contratado
	            and 
	            CONVERT(INT,SUBSTRING(HT.DSC_LINE1,1,7)) = ho.producto_contratado
	            and 
	            ho.producto_contratado not in (select producto_contratado from bitacora_holds_eqtkt)
	            and 
	            pc.producto = 8014
	            and 
	            pc.status_producto in (8027,27)
	            and 
	            ho.descripcion4 like '%TPV%'
	            and 
	            ho.hold = 0
            ";
            try
            {
                if (this.ConectDB())
                {
                    return bd.ejecutarInsert(consulta);
                }
                return -1;
            }
            catch (Exception ex)
            {
                Log.Escribe(ex);
                return -1;
            }
        }
    }
}
