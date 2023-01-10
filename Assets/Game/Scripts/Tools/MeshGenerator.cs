using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MeshGenerator
{
    private static bool debugTrianglesBuildingProcess = false;

    public static Mesh CreateMesh(Vector3[] vertices, string name = "Mesh")
    {
        Mesh newMesh = new Mesh();

        newMesh.name = name;

        MeshVertex[] meshVertices = new MeshVertex[vertices.Length];

        for (int i = 0; i < meshVertices.Length; i++)
        {
            meshVertices[i] = new MeshVertex(vertices[i], i);
        }

        MeshTriangle[] meshTriangles = DefineMeshTriangles(meshVertices);

        List<int> triangles = new List<int>();

        for (int i = 0; i < meshTriangles.Length; i++)
        {
            triangles.AddRange(meshTriangles[i].GetTriangles());
        }

        newMesh.vertices = vertices;
        newMesh.triangles = triangles.ToArray();

        newMesh.RecalculateNormals();

        return newMesh;
    }

    /// <summary>
    /// Generates mesh of line starting on A and ending on B with specified Width
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="width"></param>
    /// <returns></returns>
    public static Mesh CreateLine(Vector3 a, Vector3 b, float width)
    {
        Vector3 sideDirection = Vector3.Cross(Vector3.up, (b - a).normalized);

        float halfWidth = width / 2f;

        Vector3[] lineVertices = new Vector3[]
        {
            a + sideDirection * halfWidth,
            a - sideDirection * halfWidth,
            b + sideDirection * halfWidth,
            b - sideDirection * halfWidth
        };

        return CreateMesh(lineVertices, "Line");
    }

    private static MeshTriangle[] DefineMeshTriangles(MeshVertex[] meshVertices)
    {
        List<MeshTriangle> meshTriangles = new List<MeshTriangle>();

        Queue<MeshVertex> meshVerticesPool = new Queue<MeshVertex>(meshVertices);
        Queue<MeshVertex> availableMeshVertices;

        List<MeshVertex> secondIterationList;

        MeshVertex currentApex;
        MeshVertex currentConnectableVertex;

        MeshVertex firstClosestVertex = new MeshVertex();
        MeshVertex secondClosestVertex = new MeshVertex();

        float currentVertexSqrDistance = 0;
        float closestVertexSqrDistance = 0;

        while (meshVerticesPool.Count > 2)
        {
            currentApex = meshVerticesPool.Dequeue();

            availableMeshVertices = new Queue<MeshVertex>(meshVerticesPool);

            if (debugTrianglesBuildingProcess)
            {
                Debug.Log($" - Apex: {currentApex.Index}");
                Debug.Log($" - Available vertices on I1: {availableMeshVertices.Count}");

                foreach (MeshVertex meshVertex in availableMeshVertices)
                {
                    Debug.Log($"   {meshVertex}");
                }
            }

            closestVertexSqrDistance = float.MaxValue;

            while (availableMeshVertices.Count > 0)
            {
                currentConnectableVertex = availableMeshVertices.Dequeue();

                currentVertexSqrDistance = (currentConnectableVertex.Position - currentApex.Position).sqrMagnitude;

                if (currentVertexSqrDistance < closestVertexSqrDistance)
                {
                    firstClosestVertex = currentConnectableVertex;

                    closestVertexSqrDistance = currentVertexSqrDistance;
                }
            }

            closestVertexSqrDistance = float.MaxValue;

            secondIterationList = new List<MeshVertex>(meshVerticesPool);
            secondIterationList.Remove(firstClosestVertex);

            availableMeshVertices = new Queue<MeshVertex>(secondIterationList);

            if (debugTrianglesBuildingProcess)
            {
                Debug.Log($"   First closest vertex: {firstClosestVertex}");
                Debug.Log($" - Available vertices on I2: {availableMeshVertices.Count}");

                foreach (MeshVertex meshVertex in availableMeshVertices)
                {
                    Debug.Log($"      {meshVertex}");
                }
            }

            while (availableMeshVertices.Count > 0)
            {
                currentConnectableVertex = availableMeshVertices.Dequeue();

                currentVertexSqrDistance = (currentConnectableVertex.Position - currentApex.Position).sqrMagnitude;

                if (currentVertexSqrDistance < closestVertexSqrDistance)
                {
                    secondClosestVertex = currentConnectableVertex;

                    closestVertexSqrDistance = currentVertexSqrDistance;
                }
            }

            meshTriangles.Add(new MeshTriangle(currentApex, firstClosestVertex, secondClosestVertex));

            if (debugTrianglesBuildingProcess)
            {
                Debug.Log($"   Second closest vertex: {secondClosestVertex}");
                Debug.Log($" - Triangle {meshTriangles[meshTriangles.Count - 1]} has been defined");
            }
        }

        return meshTriangles.ToArray();
    }

    /*
    public Mesh MergeMeshes(List<Mesh> meshes, float mergeRadius)
    {
        Debug.Log(" --- Start meshes merging ---");

        Mesh complexMesh = new Mesh();

        float sqrMergeRadius = mergeRadius * mergeRadius;

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        List<Vector3> verticesPool = new List<Vector3>();

        Debug.Log(" - Adding vertices to pool:");

        foreach (Mesh mesh in meshes)
        {
            for (int i = 0; i < mesh.vertices.Length; i++)
            {
                Debug.Log($"   {mesh.vertices[i]}");
            }

            verticesPool.AddRange(mesh.vertices);
        }

        Debug.Log(" - Merging vertices:");

        bool isVertexMergable;

        for (int i = 0; i < verticesPool.Count; i++)
        {
            isVertexMergable = false;

            for (int j = 0; i < verticesPool.Count; i++)
            {
                if (j != i)
                {
                    if ((verticesPool[i] - verticesPool[j]).sqrMagnitude < sqrMergeRadius)
                    {
                        isVertexMergable = true;

                        Debug.Log($"   Vertex merged - {verticesPool[i]}");

                        break;
                    }
                }
            }

            if (!isVertexMergable)
            {
                vertices.Add(verticesPool[i]);
            }
        }

        Debug.Log($" - Vertices after merge: {vertices.Count}");

        Debug.Log(" - Initializing MeshVertices:");

        List<MeshVertex> meshVertices = new List<MeshVertex>();

        foreach (Vector3 vertexPosition in vertices)
        {
            meshVertices.Add(new MeshVertex(vertexPosition, vertices.IndexOf(vertexPosition)));

            Debug.Log($"   {meshVertices[meshVertices.Count - 1]}");
        }

        //List<MeshTriangle> meshTriangles = new List<MeshTriangle>();

        Debug.Log(" - Building MeshTriangles:");

        MeshVertex currentApex = new MeshVertex();
        MeshVertex firstClosestVertex = new MeshVertex();
        MeshVertex secondClosestVertex = new MeshVertex();

        MeshVertex[] triangulationVerticesPool;

        float currentSqrDistanceToVertex;
        float minSqrDistanceToVertex = float.MaxValue;

        float sqrDistanceToFirstClosestPoint = 0;

        while (meshVertices.Count > 2)
        {
            currentApex = meshVertices[0];

            Debug.Log($"   Apex A: {currentApex}");

            meshVertices.RemoveAt(0);

            triangulationVerticesPool = meshVertices.ToArray();

            Debug.Log($"   Triangulation pool size: {triangulationVerticesPool.Length}");

            for (int i = 0; i < triangulationVerticesPool.Length; i++)
            {
                currentSqrDistanceToVertex = (currentApex.Position - triangulationVerticesPool[i].Position).sqrMagnitude;

                if (currentSqrDistanceToVertex < minSqrDistanceToVertex)
                {
                    minSqrDistanceToVertex = currentSqrDistanceToVertex;
                    sqrDistanceToFirstClosestPoint = currentSqrDistanceToVertex;

                    firstClosestVertex = triangulationVerticesPool[i];
                }
            }

            Debug.Log($"   Apex B: {firstClosestVertex}");

            minSqrDistanceToVertex = float.MaxValue;

            for (int i = 0; i < triangulationVerticesPool.Length; i++)
            {
                currentSqrDistanceToVertex = (currentApex.Position - triangulationVerticesPool[i].Position).sqrMagnitude;

                if (currentSqrDistanceToVertex < minSqrDistanceToVertex && currentSqrDistanceToVertex > sqrDistanceToFirstClosestPoint)
                {
                    minSqrDistanceToVertex = currentSqrDistanceToVertex;

                    secondClosestVertex = triangulationVerticesPool[i];
                }
            }

            Debug.Log($"   Apex C: {secondClosestVertex}");

            triangles.AddRange(new MeshTriangle(currentApex, firstClosestVertex, secondClosestVertex).GetTriangles());

            Debug.Log($"   Complex mesh triangles (indices): {triangles}");
            Debug.Log("   ---");
        }

        Debug.Log($" - Merging finished with result --- Vertices: {complexMesh.vertices.Length} / Triangles: {complexMesh.triangles.Length}");

        complexMesh.vertices = vertices.ToArray();
        complexMesh.triangles = triangles.ToArray();

        complexMesh.RecalculateNormals();

        return complexMesh;
    }

    // 4---5
    // |   |
    // 1---3
    // |   |
    // 0---2
    //
    // 0,1,2 , 1,3,2 , 1,4,5 , 4,5,3
    */
}

public class MeshTriangle
{
    public MeshVertex[] Vertices { get; private set; }

    public MeshTriangle(MeshVertex a, MeshVertex b, MeshVertex c)
    {
        Vertices = new MeshVertex[3];

        if (Vector3.SignedAngle(b.Position - a.Position, c.Position - a.Position, Vector3.Cross(b.Position - a.Position, c.Position - a.Position)) > 0)
        {
            Vertices[0] = a;
            Vertices[1] = b;
            Vertices[2] = c;
        }
        else
        {
            Vertices[0] = a;
            Vertices[1] = c;
            Vertices[2] = b;
        }
    }

    public int[] GetTriangles()
    {
        return new int[] { Vertices[0].Index, Vertices[1].Index, Vertices[2].Index };
    }

    public override string ToString()
    {
        return "[" + Vertices[0].Index + ", " + Vertices[1].Index + ", " + Vertices[2].Index + "]";
    }
}

public struct MeshVertex
{
    public Vector3 Position { get; private set; }
    public int Index { get; private set; }

    public MeshVertex(Vector3 position, int index)
    {
        Position = position;
        Index = index;
    }

    public override string ToString()
    {
        return Position.ToString() + $" [{Index}]";
    }
}
