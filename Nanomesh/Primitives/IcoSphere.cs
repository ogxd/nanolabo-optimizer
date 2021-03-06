﻿using System.Collections.Generic;

namespace Nanomesh
{
    public static partial class PrimitiveUtils
    {
        public static SharedMesh CreateIcoSphere(float radius = 1f, int recursionLevel = 3)
        {
            SharedMesh mesh = mesh = new SharedMesh();

            List<Vector3> vertList = new List<Vector3>();
            Dictionary<long, int> middlePointIndexCache = new Dictionary<long, int>();

            // create 12 vertices of a icosahedron
            float t = (1f + MathF.Sqrt(5f)) / 2f;

            vertList.Add(new Vector3(-1f, t, 0f).Normalized * radius);
            vertList.Add(new Vector3(1f, t, 0f).Normalized * radius);
            vertList.Add(new Vector3(-1f, -t, 0f).Normalized * radius);
            vertList.Add(new Vector3(1f, -t, 0f).Normalized * radius);

            vertList.Add(new Vector3(0f, -1f, t).Normalized * radius);
            vertList.Add(new Vector3(0f, 1f, t).Normalized * radius);
            vertList.Add(new Vector3(0f, -1f, -t).Normalized * radius);
            vertList.Add(new Vector3(0f, 1f, -t).Normalized * radius);

            vertList.Add(new Vector3(t, 0f, -1f).Normalized * radius);
            vertList.Add(new Vector3(t, 0f, 1f).Normalized * radius);
            vertList.Add(new Vector3(-t, 0f, -1f).Normalized * radius);
            vertList.Add(new Vector3(-t, 0f, 1f).Normalized * radius);

            // create 20 triangles of the icosahedron
            List<TriangleIndices> faces = new List<TriangleIndices>
            {

                // 5 faces around point 0
                new TriangleIndices(0, 11, 5),
                new TriangleIndices(0, 5, 1),
                new TriangleIndices(0, 1, 7),
                new TriangleIndices(0, 7, 10),
                new TriangleIndices(0, 10, 11),

                // 5 adjacent faces 
                new TriangleIndices(1, 5, 9),
                new TriangleIndices(5, 11, 4),
                new TriangleIndices(11, 10, 2),
                new TriangleIndices(10, 7, 6),
                new TriangleIndices(7, 1, 8),

                // 5 faces around point 3
                new TriangleIndices(3, 9, 4),
                new TriangleIndices(3, 4, 2),
                new TriangleIndices(3, 2, 6),
                new TriangleIndices(3, 6, 8),
                new TriangleIndices(3, 8, 9),

                // 5 adjacent faces 
                new TriangleIndices(4, 9, 5),
                new TriangleIndices(2, 4, 11),
                new TriangleIndices(6, 2, 10),
                new TriangleIndices(8, 6, 7),
                new TriangleIndices(9, 8, 1)
            };

            // refine triangles
            for (int i = 0; i < recursionLevel; i++)
            {
                List<TriangleIndices> faces2 = new List<TriangleIndices>();
                foreach (TriangleIndices tri in faces)
                {
                    // replace triangle by 4 triangles
                    int a = GetMiddlePoint(tri.v1, tri.v2, ref vertList, ref middlePointIndexCache, radius);
                    int b = GetMiddlePoint(tri.v2, tri.v3, ref vertList, ref middlePointIndexCache, radius);
                    int c = GetMiddlePoint(tri.v3, tri.v1, ref vertList, ref middlePointIndexCache, radius);

                    faces2.Add(new TriangleIndices(tri.v1, a, c));
                    faces2.Add(new TriangleIndices(tri.v2, b, a));
                    faces2.Add(new TriangleIndices(tri.v3, c, b));
                    faces2.Add(new TriangleIndices(a, b, c));
                }
                faces = faces2;
            }

            mesh.positions = vertList.ToArray();

            List<int> triList = new List<int>();
            for (int i = 0; i < faces.Count; i++)
            {
                triList.Add(faces[i].v1);
                triList.Add(faces[i].v2);
                triList.Add(faces[i].v3);
            }
            mesh.triangles = triList.ToArray();

            Vector3[] normales = new Vector3[vertList.Count];
            for (int i = 0; i < normales.Length; i++)
            {
                normales[i] = vertList[i].Normalized;
            }

            return mesh;
        }

        private struct TriangleIndices
        {
            public int v1;
            public int v2;
            public int v3;

            public TriangleIndices(int v1, int v2, int v3)
            {
                this.v1 = v1;
                this.v2 = v2;
                this.v3 = v3;
            }
        }

        // return index of point in the middle of p1 and p2
        private static int GetMiddlePoint(int p1, int p2, ref List<Vector3> vertices, ref Dictionary<long, int> cache, float radius)
        {
            // first check if we have it already
            bool firstIsSmaller = p1 < p2;
            long smallerIndex = firstIsSmaller ? p1 : p2;
            long greaterIndex = firstIsSmaller ? p2 : p1;
            long key = (smallerIndex << 32) + greaterIndex;

            if (cache.TryGetValue(key, out int ret))
            {
                return ret;
            }

            // not in cache, calculate it
            Vector3 point1 = vertices[p1];
            Vector3 point2 = vertices[p2];
            Vector3 middle = new Vector3
            (
                (point1.x + point2.x) / 2f,
                (point1.y + point2.y) / 2f,
                (point1.z + point2.z) / 2f
            );

            // add vertex makes sure point is on unit sphere
            int i = vertices.Count;
            vertices.Add(middle.Normalized * radius);

            // store it, return index
            cache.Add(key, i);

            return i;
        }
    }
}
