using Avalonia.Threading;
using R3;
using Serilog;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VYaml.Annotations;
using VYaml.Serialization;
using FreqCat.Managers;
using FreqCat.ViewModels;
using FreqCat.Utils;


namespace FreqCat.Format
{
    
    [YamlObject]    
    public partial class Fcat
    {
        public Version Version { get; set; } = new Version(0, 2);
        public FcDataRoot FcDataRoot { get; set; }

        [YamlIgnore]
        public Version CurrentVersion = new Version(0, 2);

        [YamlConstructor]
        public Fcat()
        {
            
        }
        public Fcat(MainViewModel v)
        {
            Version = CurrentVersion;
            this.FcDataRoot = v.DirectoryLoader.Data;
        }

        

        public async Task Load(MainViewModel v)
        {
            Log.Information("Loading Project");
            
            if (Version > CurrentVersion)
            {
                var res = await v.ShowConfirmWindow("menu.files.open.upgrade");
                return;
            }
            v.project = this;

            if (Version < CurrentVersion)
            {
                Log.Information($"Upgrading project from {Version} to {CurrentVersion}.");
            }

            await Dispatcher.UIThread.InvokeAsync(async () =>
            {

                v.DirectoryLoader = new DirectoryLoader(FcDataRoot);

            }, DispatcherPriority.Send);
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                // TODO: Implement Load
            }, DispatcherPriority.Normal);
        }



        public void Save(string filePath)
        {

            try
            {
                WriteYaml(filePath);
                MainManager.Instance.cmd.ProjectSaved();
            }
            catch (Exception ex)
            {
                Log.Error("Failed to save fcat: {filePath}", $"<translate:errors.failed.save>: {filePath}", ex);

            }
        }
        void WriteYaml(string yamlPath)
        {
            var utf8Yaml = YamlSerializer.SerializeToString(this);
            File.WriteAllText(yamlPath, utf8Yaml);
        }
        
    }
}
