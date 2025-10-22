using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MarchingCubesAPI
{
    private readonly ComputeShader shader;
    private readonly int resolution;
    public Vector3 VolumeSize { get; set; }
    public Vector3 VolumeCenter { get; set; }
    public Vector3[] Vertices { get; private set;}
    public int[] Triangles { get; private set; }
    
    public MarchingCubesAPI(ComputeShader marchingCubesComputeShader, Vector3 volumeCenter, Vector3 volumeSize, uint res)
    {
        shader = marchingCubesComputeShader;
        resolution = (int)(res < 3 ? 3 : res);
        VolumeSize = volumeSize;
        VolumeCenter = volumeCenter;
    }
    
    public void Polygonize(RenderTexture densityMap, float isoLevel = 0)
    {
        // create buffers for result vertices
        var maxVertices = Mathf.Min((int)Mathf.Pow(resolution - 1, 3) * 3, 10_000_000);
        var resultVerticesBuffer = new ComputeBuffer(maxVertices, sizeof(float) * 3);
        
        // create buffers for result triangles
        int maxTriangles = Mathf.Min((int)Mathf.Pow(resolution, 3) * 5, 10_000_000);
        var resultTrianglesBuffer = new ComputeBuffer(maxTriangles, sizeof(int) * 3, ComputeBufferType.Append);
        resultTrianglesBuffer.SetCounterValue(0);
        
        // create buffer for edge vertex indices
        int numEdges = resolution * resolution * (resolution - 1) * 3;
        var edgeBuffer = new ComputeBuffer(numEdges, sizeof(int));
        edgeBuffer.SetData(Enumerable.Repeat(-1, numEdges).ToArray());
        
        // create buffer for vertex counter
        var vertexCounterBuffer = new ComputeBuffer(1, sizeof(int));
        vertexCounterBuffer.SetData(new[] {0});
        
        // Get kernel
        var kernel = shader.FindKernel("Polygonise");
        
        // Set shader parameters
        shader.SetInt("resolution", resolution);
        shader.SetFloat("isoLevel", isoLevel);
        shader.SetTexture(kernel, "densityMap", densityMap);
        shader.SetBuffer(kernel, "resultVertices", resultVerticesBuffer);
        shader.SetBuffer(kernel, "resultTriangles", resultTrianglesBuffer);
        shader.SetBuffer(kernel, "edgeVertexIndices", edgeBuffer);
        shader.SetBuffer(kernel, "vertexCounter", vertexCounterBuffer);
        
        // Dispatch kernel
        var dispatchGroupNum = Mathf.CeilToInt(resolution / 8f);
        shader.Dispatch(kernel, dispatchGroupNum, dispatchGroupNum, dispatchGroupNum);
        
        // counter buffer for result vertices
        int[] verticesCounter = { 0 };
        vertexCounterBuffer.GetData(verticesCounter);
        
        if (verticesCounter[0] == 0)
        {
            Debug.Log("No vertices generated");
            // densityMap.Release();
            resultVerticesBuffer.Dispose();
            resultTrianglesBuffer.Dispose();
            edgeBuffer.Dispose();
            vertexCounterBuffer.Dispose();
            return;
        }
        
        // counter buffer for result triangles
        var trianglesCounterBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);
        ComputeBuffer.CopyCount(resultTrianglesBuffer, trianglesCounterBuffer, 0);
        int[] trianglesCounter = { 0 };
        trianglesCounterBuffer.GetData(trianglesCounter);
        
        Vertices = new Vector3[verticesCounter[0]];
        var triangles = new Triangle[trianglesCounter[0]];
        resultVerticesBuffer.GetData(Vertices, 0, 0, Vertices.Length);
        resultTrianglesBuffer.GetData(triangles, 0, 0, triangles.Length);
        
        // densityMap.Release();
        resultVerticesBuffer.Dispose();
        resultTrianglesBuffer.Dispose();
        edgeBuffer.Dispose();
        vertexCounterBuffer.Dispose();
        trianglesCounterBuffer.Dispose();
        
        var resultTriangle = new List<int>();
        foreach (var t in triangles)
        {
            resultTriangle.Add(t.V1);
            resultTriangle.Add(t.V2);
            resultTriangle.Add(t.V3);
        }
        Triangles = resultTriangle.ToArray();
    }

    
    public RenderTexture GetSphereDensityMap(ComputeShader sdfComputeShader, Vector3 sphereCenter, float radius)
    {
        var kernel = sdfComputeShader.FindKernel("Sphere");

        var densityMap = AssignCommonParameters(sdfComputeShader, kernel, sphereCenter);
        sdfComputeShader.SetFloat("radius", radius);
        sdfComputeShader.SetVector("center", sphereCenter);
        
        var dispatchGroupNum = Mathf.CeilToInt(resolution / 8f);
        sdfComputeShader.Dispatch(kernel, dispatchGroupNum, dispatchGroupNum, dispatchGroupNum);
        return densityMap;
    }
    
    public RenderTexture GetBoxDensityMap(ComputeShader sdfComputeShader, Vector3 cubeCenter, Vector3 halfSize)
    {
        var kernel = sdfComputeShader.FindKernel("Box");

        var densityMap = AssignCommonParameters(sdfComputeShader, kernel, cubeCenter);
        sdfComputeShader.SetVector("halfSize", halfSize);
        
        var dispatchGroupNum = Mathf.CeilToInt(resolution / 8f);
        sdfComputeShader.Dispatch(kernel, dispatchGroupNum, dispatchGroupNum, dispatchGroupNum);
        return densityMap;
    }

    private RenderTexture AssignCommonParameters(ComputeShader sdfComputeShader, int kernel, Vector3 shapeCenter)
    {
        var densityMap = Create3DRenderTexture();
        densityMap.Create();
        sdfComputeShader.SetTexture(kernel, "Result", densityMap);
        sdfComputeShader.SetVector("center", shapeCenter);
        sdfComputeShader.SetInt("resolution", resolution);
        sdfComputeShader.SetVector("volumeCenter", VolumeCenter);
        sdfComputeShader.SetVector("volumeSize", VolumeSize);
        return densityMap;
    }

    private RenderTexture Create3DRenderTexture()
    {
        return new RenderTexture(resolution, resolution, 0, RenderTextureFormat.ARGBFloat)
        {
            dimension = UnityEngine.Rendering.TextureDimension.Tex3D,
            enableRandomWrite = true,
            volumeDepth = resolution,
        };
    }
    
    private struct Triangle
    {
        public int V1, V2, V3;
    }
}
