using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using Serilog;
using Avalonia.Media;
using System.Drawing;
using Avalonia.Platform;
using Avalonia.Media.Imaging;

namespace FreqCat.Managers
{
    public class IconManager
    {
        //public ImageBrush BosIcon;
        public IconManager()
        {
            //LoadIcon(new Uri("avares://Mirivoice.Main/Assets/UI/inton-bos.png"), out BosIcon);

        }

        public void LoadIcon(Uri uri, out ImageBrush icon)
        {
            var assets = AssetLoader.Open(uri);

            using (var stream = assets)
            {
                var bitmap = new Avalonia.Media.Imaging.Bitmap(stream);
                icon = new ImageBrush(bitmap)
                {
                    Stretch = Stretch.UniformToFill
                };
            }
        }
    }
}
