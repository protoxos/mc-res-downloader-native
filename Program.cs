using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;

namespace mc_res_downloader_native
{
    internal class Program
    {
        #region Funciones
        static int line = 0;
        static void W(string message, bool jump = false, params object[] par)
        {
            if (!jump)
            {
                string space = "";
                for (int i = 0; i < Console.WindowWidth; i++)
                    space += " ";

                Console.SetCursorPosition(0, line);
                Console.Write(space);
                Console.SetCursorPosition(0, line);
            }
            else
                line++;

            Console.SetCursorPosition(0, line);
            Console.Write(message, par);
        }
        static void WL(string message, params object[] par)
        {
            Console.WriteLine(message, par);
            LAST_MESSAGE_PRINTED = "";
            line++;
        }
        static string LAST_MESSAGE_PRINTED = "";
        static void WSSSSS(string message, bool replace = true, string replaceIfContains = "", string replaceIfStartsWith = "", int? replaceIfMatchFirstNChars = null)
        {
            bool r = replaceIfMatchFirstNChars != null ? LAST_MESSAGE_PRINTED.StartsWith(message.Substring(0, replaceIfMatchFirstNChars ?? 0)) : false;
            if (replace || LAST_MESSAGE_PRINTED.StartsWith(replaceIfStartsWith) || LAST_MESSAGE_PRINTED.Contains(replaceIfContains) || r)
            {
                string space = "";
                for (int i = 0; i < Console.WindowWidth; i++)
                    space += " ";

                Console.SetCursorPosition(0, Console.CursorTop);
                Console.Write(space);
                Console.SetCursorPosition(0, Console.CursorTop);
            }
            Console.Write(message);
            LAST_MESSAGE_PRINTED = message;
        }
        static string R(string message = "")
        {
            Console.Write(message);
            LAST_MESSAGE_PRINTED = message;
            return Console.ReadLine() ?? "";
        }
        static void C()
        {
            Console.Clear();
            Console.SetCursorPosition(0, 0);
        }
        #endregion
        static void Main(string[] args)
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            WebClient webClient = new WebClient();
            Minedex minedex;
            MineIndexProf mineIndexProf = null;
            string jsonContent;

            string mcDir = R("Ruta de MC (def: " + Minedex.MC_DIR + "): ");
            Minedex.MC_DIR = string.IsNullOrEmpty(mcDir) ? Minedex.MC_DIR : mcDir;

            string ver = R("Versión: ");
            string assetIndexFile = Minedex.ensureDir("/tmp/", "idx_" + ver + ".json");

            C();
            try
            {
                W("Descargando lista de assets para la versión {0}... ", true, ver);
                webClient.DownloadFile(
                    string.Format(Minedex.indexUrlPlaceholder, ver),
                    assetIndexFile
                );

                jsonContent = File.ReadAllText(assetIndexFile);
                mineIndexProf = JsonConvert
                    .DeserializeObject<MineIndexProf>(
                    jsonContent);


                WL("Ok");

            }
            catch (Exception x)
            {
                W("");
                WL("No se pudo obtener la información. Error: " + x.Message);
                return;
            }

            if (mineIndexProf == null) return;

            try
            {
                string indexFile = Minedex.ensureDir("/assets/indexes/", mineIndexProf.assetIndex.id + ".json");
                W("Descargando indice de recursos... ");
                webClient.DownloadFile(
                    mineIndexProf.assetIndex.url,
                    indexFile
                );

                jsonContent = File.ReadAllText(indexFile);
                minedex = JsonConvert.DeserializeObject<Minedex>(jsonContent);
                WL("Ok");
            }
            catch (Exception x)
            {
                W("");
                WL("No se pudo leer el archivo de recursos. Error: " + x.Message);
                return;
            }

            WL("");
            if (minedex != null)
            {
                int total = minedex.objects.Count;
                int current = 0;
                foreach (var key in minedex.objects.Keys)
                {
                    current++;
                    W("[{1}/{2}] Obteniendo el archivo {0}", false, key, current, total);
                    var obj = minedex.objects[key];
                    using (var md5 = MD5.Create())
                    {
                        //"\\assets\\objects\\"
                        string f = Minedex.ensureDir("/assets/objects/" + obj.hash.Substring(0, 2) + "/", obj.hash);
                        if (File.Exists(f))
                        {
                            using (FileStream stream = File.OpenRead(f))
                            {
                                using (SHA1Managed sha = new SHA1Managed())
                                {
                                    byte[] checksum = sha.ComputeHash(stream);
                                    string hash = BitConverter.ToString(checksum)
                                        .Replace("-", string.Empty)
                                        .ToLower();
                                    if (hash != obj.hash) goto download;
                                    else
                                    {
                                        W("");
                                        WL("[{1}/{2}] Ya existe el archivo {0}", key, current, total);
                                        continue;
                                    }
                                }
                            }
                        }

                    download:
                        try
                        {
                            webClient.DownloadFile(obj.url ?? "", f);
                        }
                        catch (Exception x)
                        {
                            WL(x.Message);
                            return;
                        }
                    }
                }
            }
            WL("");
            R("Pulsa enter para salir...");
        }
    }
}
