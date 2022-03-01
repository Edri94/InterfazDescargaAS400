using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Consumir_InterfazDescargaAS400
{
    static class Program
    {
        static Form1 form;

        /// <summary>
        /// Punto de entrada principal para la aplicación.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            form = new Form1();
            Application.Run(form);       
        }

        public static void cerrar(bool cerrar)
        {
            form.Close();
            Application.Exit();
        }
    }
}
