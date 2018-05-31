using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

//[ExecuteInEditMode, RequireComponent(typeof(Renderer))]

using uOSC;

//namespace uOSC
//{
//    [RequireComponent(typeof(uOscServer))]
public class Instancing : MonoBehaviour
{

    // ==============================
    #region // Defines

    const int ThreadBlockSize = 256;

    struct CubeData
    {
        //public Vector3 BasePosition;
        public Vector3 Position;
        public Vector3 Velocity;
        public Vector3 Rotation;
        public Vector3 Albedo;
    }
    public enum LayoutType
    {
        LayoutFlat,
        LayoutGravity,
        LayoutSimpleNoise,
        LayoutFree
    }


    #endregion // Defines

    // ==============================
    #region // Serialize Fields

    // cubeの数
    [SerializeField]
    int _instanceCountX = 100;
    [SerializeField]
    int _instanceCountY = 100;

    [SerializeField]
    ComputeShader _ComputeShader;

    [SerializeField]
    LayoutType _LayoutTypeEnum = LayoutType.LayoutFlat;

    [SerializeField]
    //LayoutType _LayoutType = LayoutType.LayoutFlat;
    int _LayoutType = 0;
    int _PrevLayoutType = 0;

    // instancingするmesh
    [SerializeField]
    Mesh _CubeMesh;

    [SerializeField]
    Material _CubeMaterial;

    [SerializeField]
    Vector3 _CubeMeshScale = new Vector3(1f, 1f, 1f);

    //[SerializeField]
    //Material _NoiseMaterial;

    // compute shaderに渡すtexture
    [SerializeField]
    RenderTexture _NoiseTexture;

    /// 表示領域の中心座標
    [SerializeField]
    Vector3 _BoundCenter = Vector3.zero;

    /// 表示領域のサイズ
    [SerializeField]
    Vector3 _BoundSize = new Vector3(300f, 300f, 300f);

    /// アニメーションの位相
    [Range(-Mathf.PI, Mathf.PI)]
    [SerializeField]
    float _Phi = Mathf.PI;

    /// アニメーションの周期
    [Range(0.01f, 100)]
    [SerializeField]
    float _Lambda = 1;

    /// アニメーションの大きさ
    [SerializeField]
    float _Amplitude = 1;

    /// 重力
    [SerializeField]
    [Range(0, 10)]
    float _Gravity = 9.8f;

    /// アニメーションの速さ
    [SerializeField]
    [Range(0, 10)]
    float _Speed = 1;


    [SerializeField]
    bool useOsc = true;

    [SerializeField]
    OscReceiver _OscReceiver;

    /// 音声入力
    [SerializeField]
    //[Range(0, 1)]
    float _InputLow = 0.0f;

    [SerializeField]
    //[Range(0, 1)]
    float _InputMid = 0.0f;

    [SerializeField]
    //[Range(0, 1)]
    float _InputHi = 0.0f;

    ///// アニメーションの周期
    //[Range(0.01f, 100)]
    //[SerializeField]
    //float _Octave = 1;

    #endregion // Serialize Fields

    // ==============================
    #region // Private Fields

    ComputeBuffer _CubeDataBuffer;
    ComputeBuffer _BaseCubeDataBuffer;
    ComputeBuffer _PrevCubeDataBuffer;

    /// GPU Instancingの為の引数
    uint[] _GPUInstancingArgs = new uint[5] { 0, 0, 0, 0, 0 };

    /// GPU Instancingの為の引数バッファ
    ComputeBuffer _GPUInstancingArgsBuffer;

    // instanceの合計数
    int _instanceCount;

    #endregion // Private Fields
    private GameObject noisePlane;

    private Texture2D m_texture;

    // --------------------------------------------------
    #region // MonoBehaviour Methods

    void Start()
    {
        _instanceCount = _instanceCountX * _instanceCountY;

        // バッファ生成
        _CubeDataBuffer = new ComputeBuffer(_instanceCount, Marshal.SizeOf(typeof(CubeData)));
        _BaseCubeDataBuffer = new ComputeBuffer(_instanceCount, Marshal.SizeOf(typeof(CubeData)));
        _PrevCubeDataBuffer = new ComputeBuffer(_instanceCount, Marshal.SizeOf(typeof(CubeData)));
        _GPUInstancingArgsBuffer = new ComputeBuffer(1, _GPUInstancingArgs.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        //var cubeDataArr = new CubeData[_instanceCount];

        // 初期化
        int kernelId = _ComputeShader.FindKernel("Init");
        _ComputeShader.SetInt("_Width", _instanceCountX);
        _ComputeShader.SetInt("_Height", _instanceCountY);
        _ComputeShader.SetBuffer(kernelId, "_CubeDataBuffer", _CubeDataBuffer);
        _ComputeShader.SetBuffer(kernelId, "_BaseCubeDataBuffer", _BaseCubeDataBuffer);
        _ComputeShader.SetBuffer(kernelId, "_PrevCubeDataBuffer", _PrevCubeDataBuffer);
        _ComputeShader.Dispatch(kernelId, (Mathf.CeilToInt(_instanceCount / ThreadBlockSize) + 1), 1, 1);

    }

    void Update()
    {
        // grab osc params
        if (useOsc)
        {
            _InputLow = _OscReceiver._InputLow;
            _InputMid = _OscReceiver._InputMid;
            _InputHi = _OscReceiver._InputHi;
        }

        updateLayoutType();

        int kernelId;
        switch (_LayoutTypeEnum)
        {
            case LayoutType.LayoutFlat:
                kernelId = _ComputeShader.FindKernel("UpdateFlat");
                break;
            case LayoutType.LayoutGravity:
                kernelId = _ComputeShader.FindKernel("UpdateGravity");
                break;
            case LayoutType.LayoutSimpleNoise:
                kernelId = _ComputeShader.FindKernel("UpdateSimpleNoise");
                break;
            default:
                kernelId = _ComputeShader.FindKernel("Update");
                break;
        }
        //_NoiseMaterial.GetTexture()
        // ComputeShader
        //int kernelId = _ComputeShader.FindKernel("Update");
        _ComputeShader.SetFloat("_Time", Time.time / 5.0f * _Speed);
        _ComputeShader.SetInt("_Width", _instanceCountX);
        _ComputeShader.SetInt("_Height", _instanceCountY);
        _ComputeShader.SetFloat("_Phi", _Phi);
        _ComputeShader.SetFloat("_Lambda", _Lambda);
        _ComputeShader.SetFloat("_Amplitude", _Amplitude);
        _ComputeShader.SetFloat("_Gravity", _Gravity);
        _ComputeShader.SetFloat("_StepX", _CubeMeshScale.x);
        _ComputeShader.SetFloat("_StepY", _CubeMeshScale.y);
        _ComputeShader.SetFloat("_StepZ", _CubeMeshScale.z);
        _ComputeShader.SetFloat("_InputLow", _InputLow);
        _ComputeShader.SetFloat("_InputMid", _InputMid);
        _ComputeShader.SetFloat("_InputHi", _InputHi);
        _ComputeShader.SetBuffer(kernelId, "_CubeDataBuffer", _CubeDataBuffer);
        _ComputeShader.SetBuffer(kernelId, "_BaseCubeDataBuffer", _BaseCubeDataBuffer);
        _ComputeShader.SetBuffer(kernelId, "_PrevCubeDataBuffer", _PrevCubeDataBuffer);
        _ComputeShader.SetTexture(kernelId, "_NoiseTex", _NoiseTexture);

        _ComputeShader.Dispatch(kernelId, (Mathf.CeilToInt(_instanceCount / ThreadBlockSize) + 1), 1, 1);

        // GPU Instaicing
        _GPUInstancingArgs[0] = (_CubeMesh != null) ? _CubeMesh.GetIndexCount(0) : 0;
        _GPUInstancingArgs[1] = (uint)_instanceCount;
        _GPUInstancingArgsBuffer.SetData(_GPUInstancingArgs);
        _CubeMaterial.SetBuffer("_CubeDataBuffer", _CubeDataBuffer);
        _CubeMaterial.SetVector("_CubeMeshScale", _CubeMeshScale);
        Graphics.DrawMeshInstancedIndirect(_CubeMesh, 0, _CubeMaterial, new Bounds(_BoundCenter, _BoundSize), _GPUInstancingArgsBuffer);
    }

    void OnDestroy()
    {
        if (this._CubeDataBuffer != null)
        {
            this._CubeDataBuffer.Release();
            this._CubeDataBuffer = null;
        }
        if (this._BaseCubeDataBuffer != null)
        {
            this._BaseCubeDataBuffer.Release();
            this._BaseCubeDataBuffer = null;
        }
        if (this._PrevCubeDataBuffer != null)
        {
            this._PrevCubeDataBuffer.Release();
            this._PrevCubeDataBuffer = null;
        }
        if (this._GPUInstancingArgsBuffer != null)
        {
            this._GPUInstancingArgsBuffer.Release();
            this._GPUInstancingArgsBuffer = null;
        }
    }

    void updateLayoutType()
    {
        if (_PrevLayoutType != _LayoutType)
        {
            _LayoutTypeEnum = (LayoutType)_LayoutType;
        }
        else
        {
            _LayoutType = (int)_LayoutTypeEnum;
        }

        _PrevLayoutType = _LayoutType;
    }

    #endregion // MonoBehaviour Method
}
//}