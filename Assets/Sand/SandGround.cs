using UnityEngine;
using System.Collections.Generic;

public class SandGround : MonoBehaviour {

    // Parameters
    public Vector2 _scale = Vector2.one * 10;
    public Transform[] _targets;

    public enum Resolution : int {
        _16x16 = 16,
        _32x32 = 32,
        _64_64 = 64,
        _128_128 = 128,
        _256x256 = 256,
        _512x512 = 512,
        _1024x1024 = 1024,
        _2048x2048 = 2048,
        _4096x4096 = 4096,
		_8192x8192 = 8192,
	}

    public Resolution _resolution = Resolution._16x16;

    FrameBuffer frameBuffer;

    [System.Serializable]
    public class Brush {
        public float _size = 0.02f;
        public float _opacity = 0.5f;
    }

    public Brush _brush = new Brush();

    public class Drawer
    {
        public const float step = 0.05f;
        public const int maxStep = 16;
        Transform _transform;
        Vector3 _lastPosition;

        float _earnedSnow = 0;

        AddDrawPointHandler _hAddDrawPoint;
        public Drawer(Transform transform, AddDrawPointHandler handler) {
            _transform = transform;
            _lastPosition = _transform.position;
            _hAddDrawPoint = handler;
        }
        
        public void Refresh() 
		{
            Vector3 newPosition = _transform.position;
            float dist = Vector3.Distance(newPosition, _lastPosition);
            while (dist > step) {
                _earnedSnow += step;
                _lastPosition = Vector3.MoveTowards(_lastPosition, newPosition, step);
                dist = Vector3.Distance(newPosition, _lastPosition);
                // Add point
                _hAddDrawPoint(_lastPosition);
            }
        }
    }

    public List<Drawer> _drawers = new List<Drawer>();

    // Rendering 
    public Material material;
    private Material drawerShader;
    private Material cleanerShader;
    private RenderTexture output;

    // Timing
    [Range(0.0f, 1.0f)]
    public float delay = 0.1f;
    private float last = 0f;

    // Draw points
    List<Vector4> drawPointBuffer = new List<Vector4>();

    public void Start() {
        Initialise();
    }

    public void FixedUpdate() {
        foreach(var drawer in _drawers) {
            drawer.Refresh();
        }
    }

    public void Initialise() {
        Camera.onPreRender += onPreRender;

        // Generate base texture
        Texture2D baseTexture = new Texture2D((int)_resolution, (int)_resolution, TextureFormat.ARGB32, false);
        Color[] colorArray = new Color[(int)_resolution * (int)_resolution];
        for(int i = 0; i < colorArray.Length; ++i) 
		{
            colorArray[i] = Color.white;
        }
        baseTexture.SetPixels(colorArray);
        baseTexture.Apply();

        if (material)
        {
            material.mainTexture = output;
            material.SetTexture("_DispTex", output);
        }

        // Setup frame buffer
        frameBuffer = new FrameBuffer();
        frameBuffer.Create((int)_resolution, (int)_resolution);
        output = frameBuffer.Get();
        frameBuffer.Swap();
        Graphics.Blit(baseTexture, frameBuffer.Get());
        
        // Setup target
        foreach (var target in _targets) {
            AddDrawer(target);
        }
    }

    public void OnDrawGizmos() {
        GizmosDrawZone();
    }

    public void GizmosDrawZone() {
        // B--C
        // |<>|
        // A--D
        Vector3 a, b, c, d;
        a = new Vector3(-(_scale.x / 2), 0, -(_scale.y / 2));
        b = new Vector3(-(_scale.x / 2), 0, +(_scale.y / 2));
        c = new Vector3(+(_scale.x / 2), 0, +(_scale.y / 2));
        d = new Vector3(+(_scale.x / 2), 0, -(_scale.y / 2));
        a += transform.position;
        b += transform.position;
        c += transform.position;
        d += transform.position;
        Gizmos.DrawLine(a, b);
        Gizmos.DrawLine(b, c);
        Gizmos.DrawLine(c, d);
        Gizmos.DrawLine(d, a);
    }

    public void AddDrawer(Transform drawer) {
        Drawer newDrawer = new Drawer(drawer, AddDrawPoint);
        _drawers.Add(newDrawer);
    }

    private void onPreRender(Camera camera) {
        if (drawerShader == null) {
            drawerShader = new Material(Shader.Find("Sand/SandGroundDrawer"));
            //shader.SetVector("_Resolution", new Vector2((int)_resolution, (int)_resolution));
            drawerShader.SetFloat("_BrushSize", _brush._size);
            drawerShader.SetFloat("_BrushOpacity", _brush._opacity);
        }
        // Cleaner
        if (cleanerShader == null) {
            cleanerShader = new Material(Shader.Find("Sand/SandGroundCleaner"));
        }


        if (last + delay < Time.time) {
            last = Time.time;
            
            // Drawer
            foreach(var point in drawPointBuffer) {
                drawerShader.SetVector("_BrushLocation", point);
                Graphics.Blit(frameBuffer.Get(), output, drawerShader);
                output = frameBuffer.Get();
                frameBuffer.Swap();

            }
            drawPointBuffer.Clear();

            // Cleaner
            Graphics.Blit(frameBuffer.Get(), output, cleanerShader);
            output = frameBuffer.Get();
            frameBuffer.Swap();

            if (material) {
                material.mainTexture = output;
                material.SetTexture("_DispTex", output);
            }
        }
    }

    public delegate void AddDrawPointHandler(Vector3 position);

    public void AddDrawPoint(Vector3 position) {
        Vector3 localPosition = (transform.position - position);
        Vector4 newPoint = new Vector4((localPosition.x / _scale.x) + 0.5f, (localPosition.z / _scale.y) + 0.5f, 0, 0);
        drawPointBuffer.Add(newPoint);
    }

}
