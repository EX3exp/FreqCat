using Avalonia.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Wave;
using System.IO;
using Avalonia;
using Avalonia.Media;
using Serilog;
using System.Numerics;

namespace FreqCat.Utils 
{

    public static class WavPlotter
    {
        /// <summary>
        /// Extracts the min-max scaled amplitudes from a wav file
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private static float[] ExtractAudioSamples(string filePath, int targetsampleAmt, double Height, int waveformHeight)
        {
            using (var audioFile = new AudioFileReader(filePath))
            {
                int sampleCount = (int)(audioFile.Length / sizeof(float));
                var samples = new float[sampleCount];
                int samplesRead = audioFile.Read(samples, 0, sampleCount);

                // if stereo, average the channels and make it mono
                if (audioFile.WaveFormat.Channels == 2)
                {
                    return samples
                        .Where((_, index) => index % 2 == 0)
                        .Select((left, index) => (left + samples[index * 2 + 1]) / 2)
                        .ToArray();
                }
                samples = MinMaxScaled(samples);
                double offset = (Height - waveformHeight) / 2 ;
                for (int i = 0; i < samples.Length; ++i)
                {
                    samples[i] = (float)(samples[i] * waveformHeight + offset);
                }

                return Downsample(samples, targetsampleAmt);
            }
        }
        /// <summary>
        /// Downsamples the audio samples to the target sample count, to reduce memory usage
        /// </summary>
        /// <param name="samples"></param>
        /// <param name="targetSampleCount"></param>
        /// <returns></returns>
        private static float[] Downsample(float[] samples, int targetSampleCount)
        {
            if (samples.Length <= targetSampleCount)
            {
                return samples;
            }
            int sampleStep = samples.Length / targetSampleCount;
            var downsampled = new float[targetSampleCount];

            for (int i = 0; i < targetSampleCount; ++i)
            {
                downsampled[i] = samples[i * sampleStep];
            }

            return downsampled;
        }

        /// <summary>
        /// Returns the min-max scaled result of the array
        /// </summary>
        /// <param name="samples"></param>
        /// <returns></returns>
        static float[] MinMaxScaled(float[] samples)
        {
            double _ymin = -1;
            double _ymax = 1;

            double maxminusmin = _ymax - _ymin;
            if (maxminusmin == 0)
            {
                maxminusmin = 0.000001; // Epsilon
            }

            for (int i = 0; i < samples.Length; ++i)
            {
                samples[i] = (float)((samples[i] - _ymin) / maxminusmin);
            }
            return samples;
        }
        /// <summary>
        /// Returns points to draw a polyline as waveform
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="dpi"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public static Points GetWavFormPoints(string filePath, double Width, double Height, int waveformHeight = 400)
        {
            float[] samples = ExtractAudioSamples(filePath, 9216, Height, waveformHeight);
            Points points = new Points();

            for (int i = 0; i < samples.Length; ++i)
            {
                
                double normalizedX = (double)i / samples.Length;
                
                
                double x = normalizedX * Width;
                double y = samples[i];

                points.Add(new Point(x, y));
            }
            
            return points;
        }
    }
}
