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
using System.Collections;

namespace FreqCat.Utils 
{

    public static class FrqPlotter
    {

        /// <summary>
        /// Extracts the min-max scaled f0 from a frq file
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private static float[] ExtractFrqPoints(Frq frq, double Height, int waveformHeight)
        {
            int sampleCount = frq.Data.NumOfChunks;
            float[] samples = new float[sampleCount];
            for (int i = 0; i < frq.Data.NumOfChunks; ++i)
            {
                samples[i] = (float)frq.Data.Chunks[i].Frequency;
            }

            samples = MinMaxScaled(samples);
            double offset = (Height - waveformHeight) / 2;
            for (int i = 0; i < samples.Length; ++i)
            {
                samples[i] = (float)(samples[i] * waveformHeight + offset);
            }

            return samples;
            
        }

        /// <summary>
        /// Returns the min-max scaled result of the array
        /// </summary>
        /// <param name="samples"></param>
        /// <returns></returns>
        static float[] MinMaxScaled(float[] samples)
        {

            double _ymin = 0;
            double _ymax = 1046.5; // set max to C6

            

            double maxminusmin = _ymax - _ymin;
            

            for (int i = 0; i < samples.Length; ++i)
            {
                samples[i] = 1 - (float)((samples[i] - _ymin) / maxminusmin);
            }
            return samples;
        }
        /// <summary>
        /// Returns points to draw a polyline as waveform
        /// </summary>
        public static Points GetFrqPoints(Frq frq, double Width, double Height, int waveformHeight = 800)
        {
            float[] samples = ExtractFrqPoints(frq, Height, waveformHeight);
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

        /// <summary>
        /// Reverses the points to extract the original f0 samples before min-max scaling and waveform height adjustment.
        /// </summary>
        public static float[] ReverseFrqPoints(Frq OriginalFrq, Points points, double Width, double Height, int waveformHeight=800)
        {
            int sampleCount = points.Count;
            float[] samples = new float[sampleCount];

            // Reverse offset calculation based on original waveform height adjustment
            double offset = (Height - waveformHeight) / 2;

            for (int i = 0; i < sampleCount; ++i)
            {
                // Step 1: Remove the offset and reverse the waveform height scaling
                double adjustedY = (points[i].Y - offset) / waveformHeight;

                // Step 2: Reverse the Min-Max scaling
                samples[i] = (float)((1 - adjustedY) * 1046.5); 
            }

            return samples;

        }

    }
}
