using UnityEngine;

namespace Arena
{
    public class SightCheck : MonoBehaviour
    {
        private static readonly int Result = Shader.PropertyToID("result");
        private static readonly int Texa = Shader.PropertyToID("tex_a");
        private static readonly int Texb = Shader.PropertyToID("tex_b");
        [SerializeField] private Camera cameraA;
        [SerializeField] private Camera cameraB;
        [SerializeField] private ComputeShader compareShader;

        private RenderTexture _rtA;
        private RenderTexture _rtB;
        private ComputeBuffer _resultBuffer;
        private uint[] _result = new uint[1];

        private void Start()
        {
            _rtA = CreateRT();
            _rtB = CreateRT();
            cameraA.targetTexture = _rtA;
            cameraB.targetTexture = _rtB;

            _resultBuffer = new ComputeBuffer(1, sizeof(uint), ComputeBufferType.Structured);
        }

        private RenderTexture CreateRT()
        {
            return new RenderTexture(cameraA.pixelWidth, cameraA.pixelHeight, 24)
            {
                enableRandomWrite = true,
                graphicsFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_UNorm
            };
        }

        public bool Check()
        {
            _result[0] = 0;
            _resultBuffer.SetData(_result);

            cameraA.Render();
            cameraB.Render();

            int kernel = compareShader.FindKernel("cs_main");
            compareShader.SetTexture(kernel, Texa, _rtA);
            compareShader.SetTexture(kernel, Texb, _rtB);
            compareShader.SetBuffer(kernel, Result, _resultBuffer);

            int groupsX = Mathf.CeilToInt(_rtA.width / 8f);
            int groupsY = Mathf.CeilToInt(_rtA.height / 8f);

            compareShader.Dispatch(kernel, groupsX, groupsY, 1);

            // Synchronous GPU → CPU read
            _resultBuffer.GetData(_result); // This blocks

            return _result[0] > 0;
        }

        private void OnDestroy()
        {
            _rtA.Release();
            _rtB.Release();
            _resultBuffer?.Dispose();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                bool visible = Check();
                Debug.Log("Object visible: " + visible);
            }
        }
    }
}