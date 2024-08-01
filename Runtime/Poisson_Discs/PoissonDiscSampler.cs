// Adapted from Sebastian Lague's tutorial [Unity] Procedural Object Placement (E01: poisson disc sampling)' https://www.youtube.com/watch?v=7WcmyxyFO7o
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Runtime.ProceduralSampling
{
    public static class PoissonDiscSampler
    {
        [Serializable]
        public class Settings
        {
            public static float MinRadius = 0.08f;
            public static int PointsLimit = 2000;

        #region Fields
            public int Seed;
            public float Radius;
            public int SampleNumBeforeRejection; // Default = 25
            public NoiseSettings RadiiInfluencingNoise;

            public bool LimitPoints;
        #endregion Fields

            public Settings (float radius, NoiseSettings radiiInfluencingNoise, int sampleNumBeforeRejection)
            {
                Seed = 0;
                Radius = radius;
                SampleNumBeforeRejection = sampleNumBeforeRejection;
                RadiiInfluencingNoise = radiiInfluencingNoise;
                LimitPoints = true;
            }

            public void OnValidate ()
            {
                Radius = Mathf.Max(MinRadius, Radius);
                SampleNumBeforeRejection = Math.Max(2, SampleNumBeforeRejection);
            }
        }

        /// <summary>
        /// Fills a 2D bounds with pseudorandom points that don't overlap
        /// </summary>
        /// <param name="generalSettings"></param>
        /// <param name="settings">Smaller 'sampleNumBeforeRejection' values means faster compute times but more chance for errors like 'empty gaps'</param>
        /// <param name="noiseSettings"></param>
        /// <param name="transform"></param>
        /// <returns></returns>
        public static List<ProceduralPoint> GeneratePoints (int seed, Settings settings, Vector3 offsetWS, ProcedualBounds bounds)
        {
            if (bounds.Size.x < 0f || bounds.Size.z < 0f) {
                return new();
            }

        #region Init
            UnityEngine.Random.InitState(seed);

            settings.Radius = Mathf.Max(0.5f, settings.Radius); // Prevent excessively small gaps
            float radius = settings.Radius;
            radius = Mathf.Max(radius, 1);
            float cellSize = radius / Mathf.Sqrt(2);

            // Create storage arrays
            int [,] grid = new int[Mathf.CeilToInt(bounds.Size.x / cellSize), Mathf.CeilToInt(bounds.Size.z / cellSize)];
            List<ProceduralPoint> points = new();
            List<Vector2> spawnPoints = new()
            {
                // creates a new array
                new Vector2(bounds.Size.x, bounds.Size.z) / 2
            };

            // Prevents crashes - Improvment: should be done using area
            int maxLength = 1500;
            if (grid.GetLength(0) > maxLength || grid.GetLength(1) > maxLength) {
                Debug.LogError("Coud not generate Poisson Disc Samples as the requested grid size was too large '" + grid.GetLength(0) + "x" + grid.GetLength(1) + "'.");
                return null;
            }
        #endregion Init

            // Stats
            int iterationsBegun = 0;
            int numAccepted = 0;
            int numDiscarded = 0;

            while (spawnPoints.Count > 0) // Iterates over each point until no spawn points are left
            {
                iterationsBegun++;
                int spawnIndex = UnityEngine.Random.Range(0, spawnPoints.Count);
                Vector2 spawnCentre = spawnPoints[spawnIndex];

            #region Candidate Testing
                bool candidateAccepted = false;
                if (RejectionTest(out Vector2 candidate)) {
                    candidateAccepted = true;

                #region Radii Noise
                    if (settings.RadiiInfluencingNoise.Algorithm != NoiseSettings.SamplingAlgorithm.None) {
                        float RadiinoiseSample = RadiiNoiseSample(candidate);
                        radius = Mathf.Abs(settings.Radius * (1 - RadiinoiseSample));
                        radius = Mathf.Max(0.5f, radius); // Prevent excessively small gaps
                    }
                #endregion

                    // Point is accepted
                    points.Add(new
                    (
                        new(candidate.x, 0, candidate.y), radius
                    ));
                    spawnPoints.Add(candidate);
                    grid[(int)(candidate.x / cellSize), (int)(candidate.y / cellSize)] = points.Count;
                }

                // Does a number of pseudorandom tests at the point's position
                bool RejectionTest (out Vector2 candidate)
                {
                    for (int i = 0; i < settings.SampleNumBeforeRejection; i++)
                    {
                        float angle = UnityEngine.Random.value * Mathf.PI * 2;
                        Vector2 direction = new(Mathf.Sin(angle), Mathf.Cos(angle));
                        candidate = spawnCentre + direction * UnityEngine.Random.Range(0.8f * radius, 2 * radius);

                        if (IsCandidateValid(candidate, bounds, radius, cellSize, ref points, grid)) {
                            return true;
                        }
                    }

                    candidate = Vector2.zero;
                    return false;
                }

                float RadiiNoiseSample (Vector3 candidate)
                {
                    switch (settings.RadiiInfluencingNoise.Algorithm)
                    {
                        case NoiseSettings.SamplingAlgorithm.None:
                            return 1;

                        default:
                            settings.RadiiInfluencingNoise.Sample2D(candidate, offsetWS, out float sample);
                            return sample; // Continue onto the next sample
                    }
                }
            #endregion Candidate Testing

                if (candidateAccepted) {
                    numAccepted++;
                } else {
                    // Point has been rejected
                    numDiscarded++;
                    spawnPoints.RemoveAt(spawnIndex);
                }

            #region Safety Limiters
                int maxPoints;
                if (settings.LimitPoints) maxPoints = Settings.PointsLimit;
                else maxPoints = 60000;
                if (points.Count > maxPoints - 1) {
                    Debug.Log("Poisson Disc points have exeeded " + maxPoints + ". Point limiter is" + (settings.LimitPoints ? " enabled" : " disabled and has reached the fixed limit") + ".");
                    return points;
                }
            #endregion
            }

            //Debug.Log("Iterations begun=" + iterationsBegun + ", accepted=" + numAccepted + ", discarded=" + numDiscarded);
            return points;
        }

        private static bool IsCandidateValid (Vector2 candidate, ProcedualBounds bounds, float radius, float cellSize, ref List<ProceduralPoint> points, int [,] grid)
        {
            // Check if the candidate is inside the sample region
            if (!bounds.IsInside(candidate)) return false;

            int cellX = (int)(candidate.x / cellSize);
            int cellY = (int)(candidate.y / cellSize);

            int searchStartX = Mathf.Max(0, cellX - 2);
            int searchEndX = Mathf.Min(cellX + 2, grid.GetLength(0) - 1);

            int searchStartY = Mathf.Max(0, cellY - 2);
            int searchEndY = Mathf.Min(cellY + 2, grid.GetLength(1) - 1);

            for (int x = searchStartX; x <= searchEndX; x++)
            {
                for (int y = searchStartY; y <= searchEndY; y++)
                {
                    int pointIndex = grid[x, y] - 1;

                    // Check if no point is in that grid cell
                    if (pointIndex != -1)
                    {
                        // Get distance from candidate to the current point
                        Vector3 point = points[pointIndex].Position;
                        Vector2 point2D = new(point.x, point.z);
                        float sqrtDistance = (candidate - point2D).sqrMagnitude;
                        if (sqrtDistance < radius * radius) {
                            return false;
                        }
                    }
                }
            }
            return true;
        }
    }
}