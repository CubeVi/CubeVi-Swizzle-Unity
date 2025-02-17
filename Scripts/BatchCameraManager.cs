using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace CubeVi_Swizzle
{
    public class BatchCameraManager : MonoBehaviour
    {
        [Header("Batch Camera Settings")]
        public Transform Root;
        public Camera BatchCameraPrefab;
        public bool useCameraPrefab = false;

        [Header("Batch Camera Foces Settings")]
        public Transform TargetTransform;
        [Range(0.1f, 500.0f)]
        public float FocalPlane = 10f;
        public bool useTargetFocal = true;

        [Header("Focus and Frustum")]
        public bool showFocalPlane = false;
        public bool showFrustumFrame = false;

        Shader swizzle;
        // Parameters
        private DeviceData _device;
        private float fl;
        // Grid cameras
        private Camera[] _batchcameras;
        private RenderTexture GridTexture;
        // Target object
        private Transform Target;
        // Device screen
        private int mytargetScreenIndex = 0;
        private bool foundDisplay = false;
        // Display
        private GameObject quadObject;
        private Camera displayCamera;
        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;
        private Material _quadMaterial;
        // Frustum
        private GameObject frustumFrame;
        private LineRenderer nearFrameRenderer;
        private LineRenderer farFrameRenderer;
        private LineRenderer connectLineRendererTopLeft;
        private LineRenderer connectLineRendererTopRight;
        private LineRenderer connectLineRendererBottomLeft;
        private LineRenderer connectLineRendererBottomRight;
        // FocalPlane
        private GameObject focalPlaneObject;
        private MeshFilter focalPlaneMeshFilter;
        private MeshRenderer focalPlaneMeshRenderer;

        private void Awake()
        {
            DontDestroyOnLoad(this);
            InitDeviceData();

#if UNITY_EDITOR
            SwizzleLog.LogInfo("Editor environment, Swizzle output defaults to Display2");
            mytargetScreenIndex = 1;
            if (Display.displays.Length > 1)
                Display.displays[1].Activate();
            if (Display.displays.Length > 2)
                Display.displays[2].Activate();
#else
            SwizzleLog.LogInfo("Runtime environment");
            mytargetScreenIndex = 0;
            bool foundDisplay = false;
            for (int i = 0; i < Display.displays.Length; i++)
            {
                Display display = Display.displays[i];
                // Identify display features based on resolution
                if (display.renderingWidth == 1440 && display.renderingHeight == 2560)
                {
                    mytargetScreenIndex = i;
                    foundDisplay = true;
                    break;
                }
            }
            if (!foundDisplay)
            {
                this.enabled = false;
                return;
            }
            if (Display.displays.Length > mytargetScreenIndex)
            {
                Display.displays[mytargetScreenIndex].Activate();
            }
#endif

            InitRenderTexture();

            LoadSwzzleConfig();

            LoadSwizzleShader();
        }


        private void Start()
        {
            if (Root == null || TargetTransform == null)
            {
                SwizzleLog.LogError("Root or TargetTransform is not assigned.");
                this.enabled = false;
                return;
            }

            InitTarget();

            InitCamera();

            InitDisplayCamera();

            InitQuad();

            InitFrustumFrame();

            InitFocalPlane();
        }

        private void Update()
        {
            UpdateTarget();

            UpdateCameraPositions();

            UpdateFrustumFrame();

            UpdateFocalPlane();
        }

        private void InitDeviceData()
        {
            _device = new DeviceData()
            {
                // Default parameters
                name = "5.7",
                imgs_count_x = 8,
                imgs_count_y = 5,
                viewnum = 40,
                theta = 40f,
                output_size_X = 1440f,
                output_size_Y = 2560f,
                subimg_width = 540,
                subimg_height = 960,
                f_cam = 3806f,
                tan_alpha_2 = 0.071f,
                x0 = 3.59f,
                interval = 19.6169f,
                slope = 0.1021f,
                nearrate = 0.96f,
                farrate = 1.08f
            };
        }

        private void InitRenderTexture()
        {
            // Initialize RenderTexture for the grid
            GridTexture = new RenderTexture(_device.subimg_width * _device.imgs_count_x, _device.subimg_height * _device.imgs_count_y, 2);
            GridTexture.enableRandomWrite = true;
            GridTexture.Create();
        }


        private void LoadSwzzleConfig()
        {
            SwizzleConfig config = new SwizzleConfig();
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string OAinstallPath = Path.Combine(appDataPath, "OpenstageAI", "deviceConfig.json");

            if (File.Exists(OAinstallPath))
            {
                string jsonContent = File.ReadAllText(OAinstallPath);
                var json = JsonUtility.FromJson<JsonWrapper>(jsonContent);

                string decryptedConfig = DecryptAes(json.config);
                var decryptedData = JsonUtility.FromJson<DeviceConfig>(decryptedConfig);

                _device.slope = decryptedData.config.obliquity;
                _device.interval = decryptedData.config.lineNumber;
                _device.x0 = decryptedData.config.deviation;
            }
            else
            {
                SwizzleLog.LogError("Please check if OpenstageAI platform is correctly installed or if Companion 01 device is properly connected");
            }
        }

        public static void DeriveKeyAndIv(byte[] passphrase, byte[] salt, int iterations, out byte[] key, out byte[] iv)
        {
            var hashList = new List<byte>();

            var preHashLength = passphrase.Length + (salt?.Length ?? 0);
            var preHash = new byte[preHashLength];

            Buffer.BlockCopy(passphrase, 0, preHash, 0, passphrase.Length);
            if (salt != null)
                Buffer.BlockCopy(salt, 0, preHash, passphrase.Length, salt.Length);

            var hash = MD5.Create();
            var currentHash = hash.ComputeHash(preHash);

            for (var i = 1; i < iterations; i++)
            {
                currentHash = hash.ComputeHash(currentHash);
            }

            hashList.AddRange(currentHash);

            while (hashList.Count < 48) // for 32-byte key and 16-byte iv
            {
                preHashLength = currentHash.Length + passphrase.Length + (salt?.Length ?? 0);
                preHash = new byte[preHashLength];

                Buffer.BlockCopy(currentHash, 0, preHash, 0, currentHash.Length);
                Buffer.BlockCopy(passphrase, 0, preHash, currentHash.Length, passphrase.Length);
                if (salt != null)
                    Buffer.BlockCopy(salt, 0, preHash, currentHash.Length + passphrase.Length, salt.Length);

                currentHash = hash.ComputeHash(preHash);

                for (var i = 1; i < iterations; i++)
                {
                    currentHash = hash.ComputeHash(currentHash);
                }

                hashList.AddRange(currentHash);
            }

            hash.Clear();
            key = new byte[32];
            iv = new byte[16];
            hashList.CopyTo(0, key, 0, 32);
            hashList.CopyTo(32, iv, 0, 16);
        }

        public static string DecryptAes(string encryptedString)
        {
            var passphrase = "3f5e1a2b4c6d7e8f9a0b1c2d3e4f5a6b";
            // encryptedString is a base64-encoded string starting with "Salted__" followed by a 8-byte salt and the
            // actual ciphertext. Split them here to get the salted and the ciphertext
            var base64Bytes = Convert.FromBase64String(encryptedString);
            var saltBytes = base64Bytes[8..16];
            var cipherTextBytes = base64Bytes[16..];

            // get the byte array of the passphrase
            var passphraseBytes = Encoding.UTF8.GetBytes(passphrase);

            // derive the key and the iv from the passphrase and the salt, using 1 iteration
            // (cryptojs uses 1 iteration by default)
            DeriveKeyAndIv(passphraseBytes, saltBytes, 1, out var keyBytes, out var ivBytes);

            // create the AES decryptor
            using var aes = Aes.Create();
            aes.Key = keyBytes;
            aes.IV = ivBytes;
            // here are the config that cryptojs uses by default
            // https://cryptojs.gitbook.io/docs/#ciphers
            aes.KeySize = 256;
            aes.Padding = PaddingMode.PKCS7;
            aes.Mode = CipherMode.CBC;
            var decryptor = aes.CreateDecryptor(keyBytes, ivBytes);

            // example code on MSDN https://docs.microsoft.com/en-us/dotnet/api/system.security.cryptography.aes?view=net-5.0
            using var msDecrypt = new MemoryStream(cipherTextBytes);
            using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
            using var srDecrypt = new StreamReader(csDecrypt);

            // read the decrypted bytes from the decrypting stream and place them in a string.
            return srDecrypt.ReadToEnd();
        }

        private void LoadSwizzleShader()
        {
            swizzle = Shader.Find("CustomRenderTexture/MultiView");
            if (swizzle != null)
            {
                _quadMaterial = new Material(swizzle);
                _quadMaterial.mainTexture = GridTexture;

                _quadMaterial.SetFloat("_Slope", _device.slope);
                _quadMaterial.SetFloat("_Interval", _device.interval);
                _quadMaterial.SetFloat("_X0", _device.x0);
                _quadMaterial.SetFloat("_ImgsCountX", _device.imgs_count_x);
                _quadMaterial.SetFloat("_ImgsCountY", _device.imgs_count_y);
                _quadMaterial.SetFloat("_ImgsCountAll", _device.viewnum);
                _quadMaterial.SetFloat("_Gamma", 1.0f);
                _quadMaterial.SetFloat("_OutputSizeX", _device.output_size_X);
                _quadMaterial.SetFloat("_OutputSizeY", _device.output_size_Y);
                SwizzleLog.LogImportant("Swizzle rendering has loaded parameters");
            }
        }

        private void InitTarget()
        {
            // Initialize target
            Target = new GameObject("Target").transform;
            DontDestroyOnLoad(Target);
        }


        private void InitCamera()
        {
            _batchcameras = new Camera[_device.viewnum];
            for (int i = 0; i < _device.viewnum; i++)
            {
                // Use prefab or create new camera
                GameObject cameraObj;
                if (useCameraPrefab && BatchCameraPrefab != null)
                {
                    cameraObj = Instantiate(BatchCameraPrefab.gameObject);
                    cameraObj.name = $"_BatchCameraPrefab[{i}]";
                    _batchcameras[i] = cameraObj.GetComponent<Camera>();
                }
                else
                {
                    cameraObj = new GameObject($"_BatchCamera[{i}]");
                    _batchcameras[i] = cameraObj.AddComponent<Camera>();
                }
                DontDestroyOnLoad(cameraObj);

                // Enable physical camera properties
                _batchcameras[i].usePhysicalProperties = true;
                _batchcameras[i].enabled = true;

                _batchcameras[i].focalLength = _device.f_cam;
                _batchcameras[i].sensorSize = new Vector2(_device.subimg_width, _device.subimg_height);

                // Camera image distribution calculation
                _batchcameras[i].targetTexture = GridTexture;
                int n_i = i;
                int m = _device.imgs_count_y - Mathf.FloorToInt(n_i / _device.imgs_count_x) - 1;
                int n = n_i % _device.imgs_count_x;
                _batchcameras[i].rect = new Rect(
                    0f + (n * 1f) / _device.imgs_count_x,
                    (m * 1f) / _device.imgs_count_y,
                    1f / _device.imgs_count_x,
                    1f / _device.imgs_count_y
                );
            }
            fl = 1f / (2f * _device.tan_alpha_2);
        }


        private void InitDisplayCamera()
        {
            // Create a new camera for displaying images
            GameObject displayCameraObj = new GameObject("_DisplayCamera");
            DontDestroyOnLoad(displayCameraObj);

            displayCamera = displayCameraObj.AddComponent<Camera>();
            displayCamera.nearClipPlane = 0.5f;
            displayCamera.farClipPlane = 100.0f;
            displayCamera.targetDisplay = mytargetScreenIndex;
            displayCamera.enabled = true;
            displayCamera.orthographic = true;
            displayCamera.orthographicSize = 0.5f;

            displayCamera.backgroundColor = Color.black;
            displayCamera.gameObject.transform.position = new Vector3(0f, 1000f, 0f);
        }

        private void InitQuad()
        {
            // Create a new Quad
            quadObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
            DontDestroyOnLoad(quadObject);

            // Set Quad material
            meshFilter = quadObject.GetComponent<MeshFilter>();
            meshRenderer = quadObject.GetComponent<MeshRenderer>();

            if (_quadMaterial != null)
            {
                meshRenderer.material = _quadMaterial;
            }
            else
            {
                SwizzleLog.LogWarning("Unable to find the interleaved shader, output the grid diagram.");
                meshRenderer.material.mainTexture = GridTexture;
            }

            // Set Quad position and rotation
            quadObject.transform.position = new Vector3(displayCamera.transform.position.x, displayCamera.transform.position.y, displayCamera.transform.position.z + 1f);
            quadObject.transform.rotation = displayCamera.transform.rotation;
            // Adjust Quad size to fill DisplayCamera view
            float quadHeight = displayCamera.orthographicSize * 2;
            float quadWidth = quadHeight * (_device.output_size_X / _device.output_size_Y);
            quadObject.transform.localScale = new Vector3(quadWidth, quadHeight, 1);
        }

        private void UpdateCameraPositions()
        {
            float x_fov = FocalPlane * Mathf.Tan(_device.theta / 2f * Mathf.Deg2Rad);

            Vector3 UpDir = Root.transform.up;
            Vector3 curCamDir = Vector3.Normalize(Target.position - Root.position);

            Vector3 x_positive_dir = Vector3.Normalize(Vector3.Cross(curCamDir, UpDir));
            Vector3 x_negative_dir = Vector3.Cross(UpDir, curCamDir);

            for (int i = 0; i < _device.viewnum; i++)
            {
                int n_i = (i + _device.viewnum * 10) % _device.viewnum;
                float x_i = -(-x_fov + (n_i * 2 * x_fov) / (_device.viewnum - 1));
                float a_i = ((x_i * fl) / FocalPlane);

                _batchcameras[i].transform.position = Root.position + x_positive_dir * x_i;
                _batchcameras[i].lensShift = new Vector2(a_i, 0);
                _batchcameras[i].transform.rotation = Root.rotation;
            }
        }

        private void UpdateTarget()
        {
            if (useTargetFocal)
            {
                Target.position = TargetTransform.position;
                FocalPlane = Vector3.Distance(Root.position, Target.position);
            }
            else
            {
                Target.position = Root.position + Root.forward * FocalPlane;
            }
        }

        private void InitFrustumFrame()
        {
            // Create independent FrustumFrame
            frustumFrame = new GameObject("FrustumFrame");
            DontDestroyOnLoad(frustumFrame);

            GameObject nearFrame = new GameObject("NearFrame");
            nearFrame.transform.parent = frustumFrame.transform;
            nearFrameRenderer = nearFrame.AddComponent<LineRenderer>();
            SetupLineRenderer(nearFrameRenderer, Color.yellow);

            GameObject farFrame = new GameObject("FarFrame");
            farFrame.transform.parent = frustumFrame.transform;
            farFrameRenderer = farFrame.AddComponent<LineRenderer>();
            SetupLineRenderer(farFrameRenderer, Color.yellow);

            GameObject connectLineTopLeft = new GameObject("ConnectLineTopLeft");
            connectLineTopLeft.transform.parent = frustumFrame.transform;
            connectLineRendererTopLeft = connectLineTopLeft.AddComponent<LineRenderer>();
            SetupLineRenderer(connectLineRendererTopLeft, Color.yellow);

            GameObject connectLineTopRight = new GameObject("ConnectLineTopRight");
            connectLineTopRight.transform.parent = frustumFrame.transform;
            connectLineRendererTopRight = connectLineTopRight.AddComponent<LineRenderer>();
            SetupLineRenderer(connectLineRendererTopRight, Color.yellow);

            GameObject connectLineBottomLeft = new GameObject("ConnectLineBottomLeft");
            connectLineBottomLeft.transform.parent = frustumFrame.transform;
            connectLineRendererBottomLeft = connectLineBottomLeft.AddComponent<LineRenderer>();
            SetupLineRenderer(connectLineRendererBottomLeft, Color.yellow);

            GameObject connectLineBottomRight = new GameObject("ConnectLineBottomRight");
            connectLineBottomRight.transform.parent = frustumFrame.transform;
            connectLineRendererBottomRight = connectLineBottomRight.AddComponent<LineRenderer>();
            SetupLineRenderer(connectLineRendererBottomRight, Color.yellow);
        }

        private void SetupLineRenderer(LineRenderer renderer, Color color)
        {
            renderer.material = new Material(Shader.Find("Sprites/Default"));
            renderer.startColor = color;
            renderer.endColor = color;
            renderer.startWidth = 0.05f;
            renderer.endWidth = 0.05f;
            renderer.useWorldSpace = true;
        }

        private void InitFocalPlane()
        {
            focalPlaneObject = new GameObject("FocalPlane");
            focalPlaneMeshFilter = focalPlaneObject.AddComponent<MeshFilter>();
            focalPlaneMeshRenderer = focalPlaneObject.AddComponent<MeshRenderer>();
            DontDestroyOnLoad(focalPlaneObject);

            // Create a square mesh
            Mesh focalPlaneMesh = new Mesh();
            focalPlaneMesh.vertices = new Vector3[]
            {
                new Vector3(-0.5f, -0.5f, 0f),
                new Vector3(0.5f, -0.5f, 0f),
                new Vector3(0.5f, 0.5f, 0f),
                new Vector3(-0.5f, 0.5f, 0f)
            };
            focalPlaneMesh.triangles = new int[]
            {
                0, 2, 1,
                2, 0, 3
            };
            focalPlaneMesh.RecalculateNormals();
            focalPlaneMeshFilter.mesh = focalPlaneMesh;

            // Set material
            Material focalMaterial = new Material(Shader.Find("Sprites/Default"));
            focalMaterial.color = new Color(1, 0, 0, 0.3f);
            focalPlaneMeshRenderer.material = focalMaterial;
        }

        private void UpdateFrustumFrame()
        {
            if (!showFrustumFrame)
            {
                frustumFrame.SetActive(showFrustumFrame);
                return;
            }

            // Get average camera parameters
            var (pos, forward, right, up, aspect) = GetAverageCameraParams();

            float nearDistance = FocalPlane * _device.nearrate;
            float farDistance = FocalPlane * _device.farrate;

            float nearHeight = nearDistance * Mathf.Tan(_device.theta / 2f * Mathf.Deg2Rad) * 2;
            float nearWidth = nearHeight * aspect;
            float farHeight = farDistance * Mathf.Tan(_device.theta / 2f * Mathf.Deg2Rad) * 2;
            float farWidth = farHeight * aspect;

            // Calculate vertices using average position and direction
            Vector3 nearTopLeft = pos + forward * nearDistance - right * nearWidth / 2 + up * nearHeight / 2;
            Vector3 nearTopRight = pos + forward * nearDistance + right * nearWidth / 2 + up * nearHeight / 2;
            Vector3 nearBottomLeft = pos + forward * nearDistance - right * nearWidth / 2 - up * nearHeight / 2;
            Vector3 nearBottomRight = pos + forward * nearDistance + right * nearWidth / 2 - up * nearHeight / 2;

            Vector3 farTopLeft = pos + forward * farDistance - right * farWidth / 2 + up * farHeight / 2;
            Vector3 farTopRight = pos + forward * farDistance + right * farWidth / 2 + up * farHeight / 2;
            Vector3 farBottomLeft = pos + forward * farDistance - right * farWidth / 2 - up * farHeight / 2;
            Vector3 farBottomRight = pos + forward * farDistance + right * farWidth / 2 - up * farHeight / 2;

            nearFrameRenderer.positionCount = 8;
            Vector3[] nearPositions = new Vector3[] {
                nearTopLeft, nearTopRight,
                nearTopRight, nearBottomRight,
                nearBottomRight, nearBottomLeft,
                nearBottomLeft, nearTopLeft
            };
            nearFrameRenderer.SetPositions(nearPositions);

            farFrameRenderer.positionCount = 8;
            Vector3[] farPositions = new Vector3[] {
                farTopLeft, farTopRight,
                farTopRight, farBottomRight,
                farBottomRight, farBottomLeft,
                farBottomLeft, farTopLeft
            };
            farFrameRenderer.SetPositions(farPositions);

            connectLineRendererTopLeft.positionCount = 2;
            Vector3[] connectPositionsTopLeft = new Vector3[] {
                nearTopLeft, farTopLeft
            };
            connectLineRendererTopLeft.SetPositions(connectPositionsTopLeft);

            connectLineRendererTopRight.positionCount = 2;
            Vector3[] connectPositionsTopRight = new Vector3[] {
                nearTopRight, farTopRight
            };
            connectLineRendererTopRight.SetPositions(connectPositionsTopRight);

            connectLineRendererBottomLeft.positionCount = 2;
            Vector3[] connectPositionsBottomLeft = new Vector3[] {
                nearBottomLeft, farBottomLeft
            };
            connectLineRendererBottomLeft.SetPositions(connectPositionsBottomLeft);

            connectLineRendererBottomRight.positionCount = 2;
            Vector3[] connectPositionsBottomRight = new Vector3[] {
                nearBottomRight, farBottomRight
            };
            connectLineRendererBottomRight.SetPositions(connectPositionsBottomRight);
        }

        private (Vector3 pos, Vector3 forward, Vector3 right, Vector3 up, float aspect) GetAverageCameraParams()
        {
            // Calculate average parameters of two middle cameras
            int midIndexFloor = (_device.viewnum / 2) - 1;
            int midIndexCeil = _device.viewnum / 2;

            Camera camFloor = _batchcameras[midIndexFloor];
            Camera camCeil = _batchcameras[midIndexCeil];

            // Calculate average position and direction
            Vector3 pos = (camFloor.transform.position + camCeil.transform.position) / 2f;
            Vector3 forward = (camFloor.transform.forward + camCeil.transform.forward).normalized;
            Vector3 right = (camFloor.transform.right + camCeil.transform.right).normalized;
            Vector3 up = (camFloor.transform.up + camCeil.transform.up).normalized;
            float aspect = (camFloor.aspect + camCeil.aspect) / 2f;

            return (pos, forward, right, up, aspect);
        }

        private void UpdateFocalPlane()
        {
            if (!showFocalPlane)
            {
                focalPlaneObject.SetActive(showFocalPlane);
                return;
            }

            float focalHeight = FocalPlane * Mathf.Tan(_device.theta / 2f * Mathf.Deg2Rad) * 2;
            float focalWidth = focalHeight * (_device.output_size_X / _device.output_size_Y);
            Vector3 focalPlanePos = Root.position + Root.forward * FocalPlane;

            focalPlaneObject.transform.position = focalPlanePos;
            focalPlaneObject.transform.rotation = Root.rotation;
            focalPlaneObject.transform.localScale = new Vector3(focalWidth, focalHeight, 1);
        }
    }
}



