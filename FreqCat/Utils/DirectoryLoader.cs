using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VYaml.Annotations;

namespace FreqCat.Utils
{
    /// <summary>
    /// Data struct per file
    /// </summary>
    [YamlObject]
    public partial class FcDataFrq
    {
        public Frq Frq { get; set; }
        
        public string FilePath { get; set; }
        public string FileName { get; set; }

        [YamlConstructor]
        public FcDataFrq() { }
        public FcDataFrq(string filePath)
        {
            this.Frq = new Frq(filePath);
            this.FilePath = filePath;
            this.FileName = Path.GetFileNameWithoutExtension(filePath);
        }

        public string ToPrintString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"File: {FileName}");
            return sb.ToString();
        }
    }

    /// <summary>
    /// Data struct per directory
    /// </summary>
    [YamlObject]
    public partial class FcDataDir
    {
        public string DirName { get; set; }
        public string DirPath { get; set; }

        /// <summary>
        /// FcDataFrq array in the directory
        /// </summary>
        public FcDataFrq[] Datas { get; set; }

        [YamlConstructor]
        public FcDataDir() { }

        public FcDataDir(string dirPath)
        {
            this.DirPath = dirPath;
            string dirName = Path.GetFileNameWithoutExtension(dirPath);
            if (dirName == "")
            {
                dirName = dirPath;
            }
            this.DirName = dirName;
            this.Datas = Directory.GetFiles(dirPath, "*.frq").Select(x => new FcDataFrq(x)).ToArray();

        }
        /// <summary>
        /// Num of files in the directory
        /// </summary>
        [YamlIgnore]
        public int Count => Datas.Length;

        public string ToPrintString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($">> Directory: {DirName}");
            sb.AppendLine($">> Num of files: {Count}");
            foreach (var data in Datas)
            {
                sb.Append("\t");
                sb.Append(data.ToPrintString());
            }
            return sb.ToString();
        }
    }

    /// <summary>
    /// Data struct per root directory
    /// </summary>
    [YamlObject]
    public partial class FcDataRoot
    {
        public string RootName { get; set; }
        public string RootPath { get; set; }

        /// <summary>
        /// FcDataDir array in the root directory
        /// </summary>
        public FcDataDir[] Datas { get; set; }

        [YamlConstructor]
        public FcDataRoot() { }

        public FcDataRoot(string rootPath)
        {
            this.RootPath = rootPath;
            this.RootName = Path.GetFileNameWithoutExtension(rootPath);
            List<FcDataDir> data = new List<FcDataDir>();
            data = Directory.GetDirectories(rootPath).Select(x => new FcDataDir(x)).ToList();
            data.Add(new FcDataDir(rootPath));
            this.Datas = data.ToArray();
            // delete data if there's no valid data
            this.Datas = this.Datas.Where(x => x.Count > 0).ToArray();

            
        }


        /// <summary>
        /// Num of directories
        /// </summary>
        [YamlIgnore]
        public int Count => Datas.Length;


        public string ToPrintString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($">> Root: {RootName}");
            sb.AppendLine($">> Num of directories: {Count}");
            foreach (var data in Datas)
            {
                sb.Append(data.ToPrintString());
                sb.Append("\n");
            }
            return sb.ToString();
        }
    }

    public class DirectoryLoader
    {
        public FcDataRoot Data;
        public DirectoryLoader(string rootPath)
        {
            this.Data = new FcDataRoot(rootPath);
        }

        public DirectoryLoader(FcDataRoot data)
        {
            Data = data;
        }
    }
}
