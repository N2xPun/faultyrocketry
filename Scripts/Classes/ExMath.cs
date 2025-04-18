using System;
using UnityEngine;
using System.Linq;

namespace ExtraMath
{
    public class ExMath
    {
        const float Tau = 2 * Mathf.PI;

        public static float Logistic(float x)
        {
            return 1 / (1 + Mathf.Pow(2.7182818f, -x));
        }

        public static Vector3 RodriguesVectorRotation(Vector3 kar, Vector3 v, float angle)
        {
            return (v * Mathf.Cos(angle)) + (Vector3.Cross(kar, v) * Mathf.Sin(angle)) + ((1 - Mathf.Cos(angle)) * Vector3.Dot(kar, v) * kar);
        }

        public static float TaxicabDistance(Vector3 a, Vector3 b)
        {
            Vector3 ab = b - a;
            return Mathf.Abs(ab.x) + Mathf.Abs(ab.y) + Mathf.Abs(ab.z);
        }

        public static bool WithinCuboid(Vector3 pos, Vector3 center, Vector3 dims)
        {
            Vector3 away = pos - center;
            if (Mathf.Abs(away.x) <= dims.x/2 && Mathf.Abs(away.y) <= dims.y/2 && Mathf.Abs(away.z) <= dims.z / 2)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static int Modp(int x, int m)
        {
            int r = x % m;
            return r < 0 ? r + m : r;
        }

        public static int FloorDivision(int x, int y)
        {
            return x < 0 ? ((x / y) + ((x % y) == 0 ? 0 : -1)) : x / y;
        }

        public static Vector2Int DVMult(Vector2Int v, Vector2Int u)
        {
            return new Vector2Int(v.x * u.x, v.y * u.y);
        }

        public static Vector2Int VSign(Vector2Int v)
        {
            return new Vector2Int(Math.Sign(v.x), Math.Sign(v.y));
        }

        public static float[,] PerlinNoise(int size, int gridSize, int seed, Vector2Int offset)
        {
            int encoverSize = size + gridSize;
            float[,] iDot = new float[encoverSize, encoverSize];
            int gridScale = (size / gridSize) + 2;
            Vector2[,] randomVectors = new Vector2[gridScale, gridScale];

            for (int i = 0; i < gridScale; i++)
            {
                int x = i + FloorDivision(offset.x, gridSize);
                for (int j = 0; j < gridScale; j++)
                {
                    int y = j + FloorDivision(offset.y, gridSize);
                    System.Random rnd = new(seed + (x * x * 0x4c1906) + (x * 0x5ac0db) + (y * y * 0x4307a7) + (y * 0x5f24f) ^ 0x3ad8025f); //stolen from minecraft's slime chunk generation.
                    float randomAngle = (float)rnd.NextDouble() * Tau;
                    randomVectors[i, j] = new Vector2(Mathf.Sin(randomAngle), Mathf.Cos(randomAngle));
                }
            }

            for (int i = 0; i < encoverSize; i++)
            {
                int gridX = Mathf.Clamp(i / gridSize, 0, gridScale - 1);
                float inGridX = i % gridSize / (float)gridSize;

                for (int j = 0; j < encoverSize; j++)
                {
                    int gridY = Mathf.Clamp(j / gridSize, 0, gridScale - 1);
                    float inGridY = j % gridSize / (float)gridSize;

                    float
                    dotBL = Vector2.Dot(
                        randomVectors[gridX, gridY],
                        new Vector2(inGridX, inGridY)),
                    dotBR = Vector2.Dot(
                        randomVectors[gridX + 1, gridY],
                        new Vector2(inGridX - 1, inGridY)),
                    dotTL = Vector2.Dot(
                        randomVectors[gridX, gridY + 1],
                        new Vector2(inGridX, inGridY - 1)),
                    dotTR = Vector2.Dot(
                        randomVectors[gridX + 1, gridY + 1],
                        new Vector2(inGridX - 1, inGridY - 1));

                    float x1 = Mathf.SmoothStep(dotBL, dotBR, inGridX),
                        x2 = Mathf.SmoothStep(dotTL, dotTR, inGridX);

                    iDot[i,j] = (Mathf.SmoothStep(x1, x2, inGridY) + 1) / 2;
                }
            }

            float[,] res = new float[size, size];
            for (int i = size - 1; i >= 0; i--)
            {
                int x = i + Modp(offset.x, gridSize);
                for (int j = size - 1; j >= 0; j--)
                {
                    int y = j + Modp(offset.y, gridSize);
                    res[i, j] = iDot[x, y];
                }
            }

            return iDot;
        }
    }
}
