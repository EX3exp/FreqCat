using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Serilog;
using System.Threading.Tasks;
using VYaml.Annotations;

namespace FreqCat.Utils
{

    [YamlObject]
    public partial class FrqChunk
    {
        public double Frequency { get; set; }
        public double Amplitude { get; set; }

        [YamlConstructor]
        public FrqChunk() { }
        public FrqChunk(double frequency, double amplitude)
        {
            Frequency = frequency;
            Amplitude = amplitude;
        }
    }
    [YamlObject]
    public partial class FrqDataWrapper
    {
        public string HeaderText { get; set; }
        public int SamplesPerFrq { get; set; }
        public int NumOfChunks { get; set; }
        public double AverageFrq { get; set; }
        public FrqChunk[] Chunks { get; set; }

        [YamlConstructor]
        public FrqDataWrapper() { }
    }

    [YamlObject]
    public partial class Frq
    {
        public FrqDataWrapper Data { get; set; } = new FrqDataWrapper();
        [YamlConstructor]
        public Frq() { }
        public Frq(float[] frequencies)
        {
            Data = new FrqDataWrapper();
            Data.NumOfChunks = frequencies.Length;
            Data.Chunks = new FrqChunk[frequencies.Length];
            for (int i = 0; i < frequencies.Length; i++)
            {
                Data.Chunks[i] = new FrqChunk(frequencies[i], 1);
            }
            Data.AverageFrq = frequencies.Average();
            Data.HeaderText = "FREQ0003";
        }

        public Frq(string filePath)
        {
            try
            {
                // from https://github.com/titinko/frq_reader/blob/master/frq_reader.py
                using (FileStream f = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    // Header.
                    byte[] headerBytes = new byte[8];
                    f.Read(headerBytes, 0, 8);
                    Data.HeaderText = Encoding.UTF8.GetString(headerBytes);


                    // Samples per frq value. Should always be 256.
                    byte[] samplesPerFrqBytes = new byte[4];
                    f.Read(samplesPerFrqBytes, 0, 4);
                    Data.SamplesPerFrq = BitConverter.ToInt32(samplesPerFrqBytes, 0);

                    // Average frequency.
                    byte[] avgFrqBytes = new byte[8];
                    f.Read(avgFrqBytes, 0, 8);
                    Data.AverageFrq = BitConverter.ToDouble(avgFrqBytes, 0);

                    // Empty space.
                    f.Seek(16, SeekOrigin.Current);

                    // Number of chunks.
                    byte[] numChunksBytes = new byte[4];
                    f.Read(numChunksBytes, 0, 4);
                    Data.NumOfChunks = BitConverter.ToInt32(numChunksBytes, 0);

                    List<FrqChunk> chunks = new List<FrqChunk>();
                    for (int chunk = 0; chunk < Data.NumOfChunks; chunk++)
                    {
                        byte[] frequencyBytes = new byte[8];
                        f.Read(frequencyBytes, 0, 8);
                        double frequency = BitConverter.ToDouble(frequencyBytes, 0);

                        byte[] amplitudeBytes = new byte[8];
                        f.Read(amplitudeBytes, 0, 8);
                        double amplitude = BitConverter.ToDouble(amplitudeBytes, 0);

                        chunks.Add(new FrqChunk(frequency, amplitude));
                    }
                    Data.Chunks = chunks.ToArray();
                    //Log.Information($"Frq file loaded successfully. - {filePath}");
                }
            }
            catch (Exception e)
            {
                Log.Error($"Error loading frq file. - {filePath}\nMessage: {e.Message}\nTrace: {e.StackTrace}");
            }
        }
    }
}
