using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using InterfazDescargaAS400.Data;

namespace Consumir_InterfazDescargaAS400
{
    public partial class Form1 : Form
    {
        ArchivoTexto archivo;
        public Form1()
        {
            InitializeComponent();
            archivo = new ArchivoTexto("JUPFH.TXT");

        }

        private void button1_Click(object sender, EventArgs e)
        {
            String resultado = archivo.ExisteArchivo() ? "existe" : "no existe";

            textBox1.Text = archivo.LeerArchivo();
            archivo.EstablecerLimites();
        }

        private void btnBorrar_Click(object sender, EventArgs e)
        {
            textBox1.Text = String.Empty;
        }
    }
}
