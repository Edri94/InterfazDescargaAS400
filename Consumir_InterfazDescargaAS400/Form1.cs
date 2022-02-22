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
        
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {         
            ArchivoTexto archivo = new ArchivoTexto();
            archivo.LeerArchivo();
        }

        private void btnBorrar_Click(object sender, EventArgs e)
        {
            textBox1.Text = String.Empty;
        }
    }
}
