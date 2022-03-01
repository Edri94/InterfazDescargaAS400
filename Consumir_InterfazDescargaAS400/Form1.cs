using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using InterfazDescargaAS400.Data;
using InterfazDescargaAS400.Helpers;

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
            this.Hide();
            ArchivoTexto archivo = new ArchivoTexto();
            archivo.LeerArchivo();
        }

        private void btnBorrar_Click(object sender, EventArgs e)
        {
            textBox1.Text = String.Empty;
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void btnEnviar_Click(object sender, EventArgs e)
        {    
            Encriptacion encriptacion = new Encriptacion();

            if(txtCadena.Text != String.Empty)
            {
                if(rbEncriptar.Checked == true)
                {
                    txtResultado.Text = encriptacion.Encrypt(txtCadena.Text);
                }
                if(rbDesencriptar.Checked == true)
                {
                    txtResultado.Text = encriptacion.Decrypt(txtCadena.Text);
                }
            }
        }

        private void rbEncriptar_CheckedChanged(object sender, EventArgs e)
        {
            if(rbEncriptar.Checked == true)
            {
                rbDesencriptar.Checked = false;
            }
            else
            {
                rbDesencriptar.Checked = true;
            }
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            txtResultado.Text = String.Empty;
            txtCadena.Text = String.Empty;
        }

        private void btnCopiar_Click(object sender, EventArgs e)
        {
            Clipboard.SetDataObject(txtResultado.Text, true);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //this.Show();
            //rbEncriptar.Checked = true;
            //ArchivoTexto archivo = new ArchivoTexto();
            //archivo.LeerArchivo();
            //Program.cerrar(true);
        }

      
    }
}
