using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace InterfazDescargaAS400.Helpers
{
    public static class Log
    {
        public static bool EscribeLog;

        public static void Escribe(string vData, string tipo = "Mensaje")
        {
            String CadenaRsultante, strPathServicio;
            strPathServicio = System.Reflection.Assembly.GetEntryAssembly().Location;
            CadenaRsultante = strPathServicio.Substring(strPathServicio.LastIndexOf("\\", strPathServicio.Length) + 1);
            String strLogPath = strPathServicio.Substring(0, strPathServicio.Length - CadenaRsultante.Length) + "Logs\\";

            StackTrace trace = new StackTrace(StackTrace.METHODS_TO_SKIP + 2);
            StackFrame frame = trace.GetFrame(0);
            MethodBase caller = frame.GetMethod();

            string clase = caller.ReflectedType.Name;
            string funcion = caller.Name;
            //clase = "";

            string seccion = "escribeArchivoLOG";
            string nombre_archivo = DateTime.Now.ToString("ddMMyyyy") + "-" + Funcion.getValueAppConfig("logFileName", seccion);
            nombre_archivo = nombre_archivo.Replace("@clase", clase);

            if (EscribeLog)
            {
                using (StreamWriter outputFile = new StreamWriter(Path.Combine(strLogPath, nombre_archivo), append: true))
                {
                    vData = $"[{DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss")}]  {tipo} desde {funcion}:  {vData}";
                    outputFile.WriteLine(vData);
                }

            }
        }

        public static void Escribe(Exception ex, string tipo = "Error")
        {
            String CadenaRsultante, strPathServicio;
            strPathServicio = System.Reflection.Assembly.GetEntryAssembly().Location;
            CadenaRsultante = strPathServicio.Substring(strPathServicio.LastIndexOf("\\", strPathServicio.Length) + 1);
            String strLogPath = strPathServicio.Substring(0, strPathServicio.Length - CadenaRsultante.Length) + "Logs\\";

            StackTrace trace = new StackTrace(StackTrace.METHODS_TO_SKIP + 2);
            StackFrame frame = trace.GetFrame(0);
            MethodBase caller = frame.GetMethod();

            string clase = caller.ReflectedType.Name;
            string funcion = caller.Name;
            //clase = "";

            string vData;
            string seccion = "escribeArchivoLOG";
            string nombre_archivo = DateTime.Now.ToString("ddMMyyyy") + "-" + Funcion.getValueAppConfig("logFileName", seccion);
            nombre_archivo = nombre_archivo.Replace("@clase", clase);

            if (EscribeLog)
            {
                using (StreamWriter outputFile = new StreamWriter(Path.Combine(strLogPath, nombre_archivo), append: true))
                {
                    vData = $"[{DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss")}] {(char)13}" +
                        $"*{tipo} desde {funcion}:  {ex.Message} {(char)13}" +
                        $"*InnerException: {ex.InnerException} {(char)13}" +
                        $"*Source: {ex.Source}  {(char)13}" +
                        $"*Data: {ex.Data}  {(char)13}" +
                        $"*HelpLink: {ex.HelpLink}  {(char)13}" +
                        $"*StackTrace: {ex.StackTrace}  {(char)13}" +
                        $"*HResult: {ex.HResult}  {(char)13}" +
                        $"*TargetSite: {ex.TargetSite}  {(char)13}";
                    outputFile.WriteLine(vData);
                }

            }
        }
    }
}
