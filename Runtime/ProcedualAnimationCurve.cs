using System;
using UnityEngine;

namespace Runtime.ProceduralSampling
{
    [Serializable]
    public sealed class ProcedualAnimationCurve : IEquatable<ProcedualAnimationCurve>
    {
    #region Fields
        [Tooltip("The keyframe resolution of the animation curve")] public float Accuracy = 1;
        public NoiseSettings Noise = new();
        public Vector2 RandomAmplitudeRange;

        [Space(5)]
        [InspectorName("Falloff")] public AnimationCurve Falloff;
        [SerializeField] private AnimationCurve _output;
    #endregion Fields

    #region Properties
        public AnimationCurve Output { get => _output; }
    #endregion

        public ProcedualAnimationCurve (float accuracy, NoiseSettings noise)
        {
            Accuracy = accuracy;
            Noise = noise;
            RandomAmplitudeRange = new(0.3f, 1);

            Falloff = new();
            _output = new();
        }

        public float Evaluate (float time24)
        {
            return _output.Evaluate(Mathf.Clamp(time24, 0, 24));
        }

        public void Generate ()
        {
            UnityEngine.Random.State origState = UnityEngine.Random.state;
            int seed = UnityEngine.Random.Range(-100000, 100000);
            float multiplier = UnityEngine.Random.Range(RandomAmplitudeRange.x, RandomAmplitudeRange.y);
            _output.ClearKeys();

            float amplitude = Noise.Amplitude * UnityEngine.Random.Range(0.4f, 1);
            float accuracy = Mathf.Clamp(Accuracy, 0.01f, 10); // Stop crashes
            float step = 1 / accuracy;
            float length = 24 * accuracy;

            for (float i = 0; i < length; i += step)
            {
                float time = i * step;
                float frequency = Noise.Scale * UnityEngine.Random.Range(0.8f, 1);
                float falloff = Falloff.Evaluate(time) * multiplier;
                float value = amplitude * Mathf.PerlinNoise1D(seed + (time * frequency));
                float offset = falloff * Noise.SampleOffset;
    
                _output.AddKey(time, offset + value * falloff);
            }

            UnityEngine.Random.state = origState;
        }

        public bool Equals (ProcedualAnimationCurve other)
        {
            return Output.Equals(other.Output);
        }
    }
}