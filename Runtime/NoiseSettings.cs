using Unity.Mathematics;
using UnityEngine;
using Runtime.Shared;

namespace Runtime.ProceduralSampling
{
    [System.Serializable]
    public sealed class NoiseSettings
    {
        public enum SamplingAlgorithm : int
        {
            [Tooltip("Outputs a valid sample with 1")]
            None = 0,

            // URL: https://docs.unity3d.com/ScriptReference/Mathf.PerlinNoise.htmlhttps://docs.unity3d.com/ScriptReference/Mathf.PerlinNoise.html
            Perlin = 1,

            // URL: https://docs.unity3d.com/Packages/com.unity.mathematics@1.3/api/Unity.Mathematics.noise.snoise.html
            Simplex = 2,

            [Tooltip("2D Cellular noise (\"Worley noise\") with a 2x2 search window. If in 2D, only the y value is used.")]
            // URL: https://docs.unity3d.com/Packages/com.unity.mathematics@1.3/api/Unity.Mathematics.noise.cellular2x2.html
            Worley2x2 = 4,

            [Tooltip("2D Cellular noise (\"Worley noise\") with standard 3x3 search window for good feature point values. If in 2D, only the y value is used.")]
            // URL: https://docs.unity3d.com/Packages/com.unity.mathematics@1.3/api/Unity.Mathematics.noise.cellular.html
            Worley3x3 = 3,
        }

    #region Fields
        public SamplingAlgorithm Algorithm;
        public Space Space;

        [Tooltip("The minimium and maximium possible noise values")]
        [MinMaxSlider(0.2f, 4)] public Vector2 Range;

        [Tooltip("The size of the noise")]
        public float Scale;

        [Tooltip("An optional multiplier to the noise sample")]
        public float Amplitude;

        [Tooltip("The value added to the noise sample")]
        public float SampleOffset;

        [Tooltip("Noise values above the threshold will be discarded")]
        [Range(0, 1)] public float Threshold;
    
        [Tooltip("The chance of a discarded sample being allowed")]
        [Range(0, 1)] public float LuckThreshold;
    #endregion

        /// <summary>
        /// Initializes to default values
        /// </summary>
        public NoiseSettings ()
        {
            Algorithm = SamplingAlgorithm.Perlin;
            Space = Space.Self;
            Range = new(0, 1);
            Scale = 0.08f;
            Amplitude = 1;
            SampleOffset = 0;

            Threshold = 0;
            LuckThreshold = 0;
        }

        public NoiseSettings (Space space, SamplingAlgorithm algorithm, Vector2 range, float scale, float threshold)
        {
            Space = space;
            Algorithm = algorithm;
            Range = range;
            Scale = scale;
            Threshold = threshold;
        }

        /// <summary>
        /// Samples 2D noise using the settings
        /// </summary>
        /// <param name="position">The position of the sample</param>
        /// <param name="transform">Used for worldspace noise</param>
        /// <param name="bounds"></param>
        /// <param name="sample">The final noise sample with the threshold and range applied</param>
        /// <returns>true if the sample is valid and should not be discarded</returns>
        public bool Sample2D (Vector2 position, Vector3 offsetWS, out float sample)
        {
            if (Algorithm == SamplingAlgorithm.None) {
                sample = 1;
                return true;
            }

            // Sample
            if (Space == Space.World) position += new Vector2(offsetWS.x, offsetWS.z);
            position *= Scale;
            float noiseSample = Amplitude * SampleOffset + Algorithm switch
            {
                SamplingAlgorithm.Perlin => Mathf.Clamp01(Mathf.PerlinNoise(position.x, position.y)),
                SamplingAlgorithm.Simplex => noise.snoise(position),
                SamplingAlgorithm.Worley2x2 => noise.cellular(new float2(position.x, position.y)).y,
                SamplingAlgorithm.Worley3x3 => noise.cellular2x2(new float2(position.x, position.y)).y,
                _ => 0,
            };

            if (!TestSample(noiseSample)) {
                sample = 0;
                return false;
            }

            // Range
            sample = math.remap
            (
                0, 1,
                Range.x, Range.y,
                noiseSample
            );
            return true;
        }

        /// <summary>
        /// Checks whether a noise sample passes the required thresholds
        /// </summary>
        /// <param name="noiseSample">The noise value</param>
        /// <returns>True if the sample is valid and should't not be discarded</returns>
        public bool TestSample (float noiseSample)
        {
            if (UnityEngine.Random.value < LuckThreshold) return true;
            if (noiseSample * UnityEngine.Random.value < Threshold) return false; // Threshold Test
            return true;
        }
    }
}