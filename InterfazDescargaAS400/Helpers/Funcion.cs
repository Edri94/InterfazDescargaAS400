using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterfazDescargaAS400.Helpers
{
    public static class Funcion
    {
        public static string getValueAppConfig(string key, string section = "")
        {
            if (section.Length >= 1)
            {
                return ConfigurationManager.AppSettings[$"{section}.{key}"];
            }
            else
            {
                return ConfigurationManager.AppSettings[$"{key}"];
            }

        }

        public static bool SetParameterAppSettings(string key, string value, string section = "")
        {
            //string nombre_appconfig = "MonitorMQTKT.exe.config";
            string nombre_appconfig = "App.config";

            bool bandera_archivo_existe = false;
            try
            {
                string appPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                string[] appPath_arr = appPath.Split('\\');

                appPath = "";
                for (int i = 0; i < (appPath_arr.Length); i++)
                {
                    appPath = (i > 0) ? appPath + "\\" + appPath_arr[i] : appPath + appPath_arr[i];
                    string busqueda = $"{appPath}\\{nombre_appconfig}";
                    bandera_archivo_existe = File.Exists(busqueda);
                    if (bandera_archivo_existe) break;
                }
                if (bandera_archivo_existe)
                {
                    appPath = appPath.Substring(1, appPath.Length - 1);
                    string configFile = System.IO.Path.Combine(appPath, nombre_appconfig);
                    ExeConfigurationFileMap configFileMap = new ExeConfigurationFileMap();
                    configFileMap.ExeConfigFilename = configFile;
                    System.Configuration.Configuration config = ConfigurationManager.OpenMappedExeConfiguration(configFileMap, ConfigurationUserLevel.None);
                    if (section.Length > 0)
                    {
                        config.AppSettings.Settings[$"{section}.{key}"].Value = value;
                    }
                    else
                    {
                        config.AppSettings.Settings[key].Value = value;
                    }
                    config.Save();
                    return true;
                }
                else
                {
                    return false;
                }

            }
            catch (Exception ex)
            {
                return false;
            }

        }

        public static bool SetParameterTransfer(string key, string value, string archivo, string ruta_archivo)
        {
            try
            {
                string appPath = ruta_archivo + archivo;

                if (File.Exists(appPath))
                {
                    string buscar = $"{key}="; 
                    string remplazar = $"{key}={value}";
                    string text = File.ReadAllText(appPath).Replace(buscar, remplazar); 
                    File.WriteAllText(appPath, text);

                    return true;
                }
                else
                {
                    return false;
                }

            }
            catch (Exception ex)
            {
                Log.Escribe(ex);
                return false;
            }

        }
    }
}
