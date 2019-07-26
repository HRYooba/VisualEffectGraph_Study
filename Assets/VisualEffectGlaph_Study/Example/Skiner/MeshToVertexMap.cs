using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshToVertexMap : MonoBehaviour
{
    [SerializeField] ComputeShader _shader;
    [SerializeField] SkinnedMeshRenderer _meshRender;
    [SerializeField] RenderTexture _vertexMap;

    Mesh _mesh;
    int _count = 0;
    int _width = 0;
    int _height = 0;

    RenderTexture _tempVertexMap;
    ComputeBuffer _vertexBuffer;


    // Start is called before the first frame update
    void Start()
    {
        _mesh = new Mesh();
        _meshRender.BakeMesh(_mesh);
        _count = _mesh.vertexCount;
        _width = Mathf.CeilToInt(Mathf.Sqrt(_count));
        _height = Mathf.CeilToInt(Mathf.Sqrt(_count));

        _tempVertexMap = new RenderTexture(_width, _height, 0, RenderTextureFormat.ARGBFloat);
        _tempVertexMap.enableRandomWrite = true;
        _tempVertexMap.Create();

        Debug.Log("Count: " + _count);
        Debug.Log("Width: " + _width);
        Debug.Log("Height: " + _height);
        Debug.Log("Width x Height: " + _width * _height);
    }

    // Update is called once per frame
    void Update()
    {
        _meshRender.BakeMesh(_mesh);
        _vertexBuffer = new ComputeBuffer(_count * 3, sizeof(float));
        _vertexBuffer.SetData(_mesh.vertices);

        int kernelID = _shader.FindKernel("CSMain");
        _shader.SetBuffer(kernelID, "VertexBuffer", _vertexBuffer);
        _shader.SetTexture(kernelID, "VertexMap", _tempVertexMap);
        _shader.SetInt("Width", _width);
        _shader.SetInt("Height", _height);
        _shader.SetInt("Count", _count);
        _shader.Dispatch(kernelID, _width / 8, _height / 8, 1);

        _vertexBuffer.Release();

        Graphics.CopyTexture(_tempVertexMap, _vertexMap);
    }
}
