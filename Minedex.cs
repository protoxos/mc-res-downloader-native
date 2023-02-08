using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace mc_res_downloader_native
{
    public class Minedex
    {
        public static readonly string indexUrlPlaceholder = "https://raw.githubusercontent.com/MultiMC/meta-multimc/master/net.minecraft/{0}.json";
        static string mc_dir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\.minecraft";
        public static string MC_DIR
        {
            get => mc_dir;
            set => mc_dir = mc_dir.TrimEnd(Path.PathSeparator);
        }
        /// <summary>
        /// Verifica que exista la ruta relativa a MC_DIR, si no existe, la crea y retorna la ruta completa
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string ensureDir(string path, string file = null)
        {
            path = path.Replace("/", "\\");
            path = path.StartsWith("\\") ? path.Substring(1) : path;
            path = mc_dir + "\\" + path;

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            return path + (string.IsNullOrEmpty(file) ? "" : file);
        }

        public Dictionary<string, MineObject> objects { get; set; }

        public Minedex()
        {
            objects = new Dictionary<string, MineObject>();
        }
    }

    public class MineObject
    {
        public string hash { get; set; }
        public int size { get; set; }
        public string url
        {
            get
            {
                return "https://resources.download.minecraft.net/" + hash.Substring(0, 2) + "/" + hash;
            }
        }

        public MineObject()
        {
            hash = string.Empty;
            size = 0;
        }
    }
    public class MineIndexProf
    {
        public MineAssetIndex assetIndex { get; set; }
        public MineIndexProf()
        {
            assetIndex = new MineAssetIndex();
        }
    }
    public class MineAssetIndex
    {
        public string id { get; set; }
        public string sha1 { get; set; }
        public int size { get; set; }
        public int totalSize { get; set; }
        public string url { get; set; }

        public MineAssetIndex()
        {
            id = "";
            sha1 = "";
            size = 0;
            totalSize = 0;
            url = "";
        }

    }
}
