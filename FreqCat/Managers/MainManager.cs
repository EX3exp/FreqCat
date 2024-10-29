using FreqCat.Managers;

using Serilog;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using VYaml.Serialization;
using FreqCat.Utils;
using FreqCat.Format;


namespace FreqCat
{
    // 1 singleton only
    public class MainManager : SingletonBase<MainManager>
    {
        public Task? InitializationTask = null;

        public PathManager PathM = new PathManager();

        public CommandManager cmd = new CommandManager();

        public IconManager IconM;// will be initialized in MainViewModel

        public int DefaultVoicerIndex = 0;
        public int DefaultMetaIndex = 0;

        public UserSetting Setting = new UserSetting();
        public RecentFiles Recent = new RecentFiles();


        public void Initialize()
        {

            CheckDirs();
            LoadSetting();
            
            InitializationTask = Task.Run(() => {
                Log.Information("MainManager Initialize Start");
            });
            
        }

        public void LoadSetting()
        {
            if (File.Exists(MainManager.Instance.PathM.SettingsPath))
            {
                var yamlUtf8Bytes = System.Text.Encoding.UTF8.GetBytes(ReadTxtFile(MainManager.Instance.PathM.SettingsPath));
                MainManager.Instance.Setting = YamlSerializer.Deserialize<UserSetting>(yamlUtf8Bytes);
            }
            else
            {
                MainManager.Instance.Setting.Save();
            }
        }
        private static void DeleteExtractedZip(string zipFilePath)
        {
            // deletes zip file and split files
            string baseFileName = Path.GetFileNameWithoutExtension(zipFilePath);
            string directory = Path.GetDirectoryName(zipFilePath);

            // delete zip file
            if (File.Exists(zipFilePath))
            {
                File.Delete(zipFilePath);
                Console.WriteLine($"Deleted: {zipFilePath}");
            }

            // delete split files
            int index = 1;
            while (true)
            {
                string splitFilePath = Path.Combine(directory, $"{baseFileName}.z{index:D2}");
                if (File.Exists(splitFilePath))
                {
                    File.Delete(splitFilePath);
                    index++;
                }
                else
                {
                    break;
                }
            }
        }


        public string ReadTxtFile(string txtPath)
        {
            using (StreamReader sr = new StreamReader(txtPath))
            {
                return (sr.ReadToEnd());
            }
        }

        public static void CheckDirs()
        {

            if (!System.IO.Directory.Exists(MainManager.Instance.PathM.CachePath))
            {
                System.IO.Directory.CreateDirectory(MainManager.Instance.PathM.CachePath);
            }

            if (!File.Exists(MainManager.Instance.PathM.SettingsPath))
            {
                MainManager.Instance.Setting.Save();
            }
            if (!File.Exists(MainManager.Instance.PathM.RecentFilesPath))
            {
                MainManager.Instance.Setting.Save();
            }
        }

        

    }

    

}
