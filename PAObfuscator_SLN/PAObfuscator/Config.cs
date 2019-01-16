using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PAObfuscator
{
    internal class ConfigData
    {
        [JsonProperty("mission_path")]
        public string MissionFolder { get; set; }

        [JsonProperty("export_path")]
        public string ExportFolder { get; set; }

        [JsonProperty("makepbo_path")]
        public string MakePboFolder { get; set; }

        [JsonProperty("create_pbo")]
        public bool MakePbo { get; set; }
    }
    internal class Config
    {
        private static Config m_Instance;
        private readonly string _appPath;
        private Config()
        {
            _appPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }

        public static Config Instance
        {
            get
            {
                if (m_Instance == null)
                    m_Instance = new Config();
                return m_Instance;
            }
        }

        public bool Init()
        {
            bool result = false;
            try
            {
                Error = null;
                AppPath = _appPath;
                string cfgFile = Path.Combine(_appPath, "config.json");
                if (!File.Exists(cfgFile))
                    CreateConfig();
                string cnt = File.ReadAllText(cfgFile);
                var cfgData = JsonConvert.DeserializeObject<ConfigData>(cnt);
                if(cfgData != null)
                {
                    MissionFolder = cfgData.MissionFolder;
                    ExportFolder = cfgData.ExportFolder;
                    if (!String.IsNullOrEmpty(cfgData.MakePboFolder))
                        MakePboFolder = cfgData.MakePboFolder;
                    else
                        MakePboFolder = "";

                } else
                {
                    CreateConfig();
                }

                result = true;
            }
            catch (Exception ex)
            {
                Error = ex;
                result = false;
            }
            return result;
        }

        public void CreateConfig()
        {
            string appSavePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "PAObfuscator");
            if (String.IsNullOrEmpty(MissionFolder))
            {
                if (!Directory.Exists(appSavePath))
                    Directory.CreateDirectory(appSavePath);
                MissionFolder = Path.Combine(appSavePath, "mpmission");
                if (!Directory.Exists(MissionFolder))
                    Directory.CreateDirectory(MissionFolder);
            }
                
            if (String.IsNullOrEmpty(ExportFolder))
            {
                if (!Directory.Exists(appSavePath))
                    Directory.CreateDirectory(appSavePath);
                ExportFolder = Path.Combine(appSavePath, "Export");
                if (!Directory.Exists(ExportFolder))
                    Directory.CreateDirectory(ExportFolder);
            }

            var saveCfg = new ConfigData()
            {
                ExportFolder = this.ExportFolder,
                MissionFolder = this.MissionFolder,
                MakePboFolder = this.MakePboFolder,
                MakePbo = this.CreatePbo
            };

            string cfgData = JsonConvert.SerializeObject(saveCfg);
            File.WriteAllText(Path.Combine(AppPath, "config.json"), cfgData);
        }

        public Exception Error { get; private set; }

        public string AppPath { get; private set; }

        public string MissionFolder { get; set; }

        public string ExportFolder { get; set; }

        public string CryptFolder { get; set; }

        public string MakePboFolder { get; set; }

        public bool CreatePbo { get; set; }
    }
}
