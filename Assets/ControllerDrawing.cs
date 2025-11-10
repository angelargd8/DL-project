using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class ControllerDrawing : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private RawImage displayImage;      // Canvas donde se dibuja
    [SerializeField] private Transform controllerTip;    // Punta del control derecho
    [SerializeField] private Camera drawCamera;          // Cámara (CenterEyeAnchor)
    [SerializeField] private float distanceToCanvas = 0.04f;

    [Header("Pincel")]
    [SerializeField] private Color drawColor = new Color(1f, 1f, 1f, 1f);
    [SerializeField] private int brushSize = 3;
    [SerializeField] private float minStepDistance = 0.003f;

    [Header("Línea 3D")]
    [SerializeField] private Material lineMaterial;      // Material unlit para la línea
    [SerializeField] private float lineWidth = 0.001f;   // Grosor de línea 3D

    [Header("IA (opcional)")]
    [SerializeField] private EmojiRecognition classifier;
    [SerializeField] private float delayToSend = 1f;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI resultLabel;

    private Texture2D drawingTexture;
    private List<Vector2Int> drawBuffer = new List<Vector2Int>();
    private Vector2? lastPixel = null;
    private LineRenderer currentLine = null;
    private Vector3 lastWorldPos;
    private bool isDrawing = false;
    private Coroutine checkForSendCoroutine;
    private Coroutine showResultCoroutine;

    void Start()
    {
        if (drawCamera == null)
            drawCamera = Camera.main;

        // Mayor resolución para trazos más suaves
        drawingTexture = new Texture2D(512, 512, TextureFormat.ARGB32, false);
        drawingTexture.filterMode = FilterMode.Point;
        drawingTexture.wrapMode = TextureWrapMode.Clamp;
        displayImage.texture = drawingTexture;
        ClearTexture();

        // Borrar automáticamente cada 20 segundos
        StartCoroutine(AutoClearEvery20Seconds());
    }

    void Update()
    {
        bool triggerPressed = OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch);
        float dist = Vector3.Distance(controllerTip.position, displayImage.transform.position);
        bool close = dist < distanceToCanvas;

        if (triggerPressed && close)
        {
            isDrawing = true;
            Draw(controllerTip.position);
        }
        else
        {
            StopDrawing();
        }
    }

    void LateUpdate()
    {
        // Aplica todos los píxeles acumulados en un solo frame -> trazo fluido
        if (drawBuffer.Count > 0)
        {
            Color[] colors = drawingTexture.GetPixels();

            foreach (var p in drawBuffer)
            {
                int index = p.y * drawingTexture.width + p.x;
                if (index >= 0 && index < colors.Length)
                    colors[index] = drawColor;
            }

            drawingTexture.SetPixels(colors);
            drawingTexture.Apply(false);
            drawBuffer.Clear();
        }
    }

    private void Draw(Vector3 worldPos)
    {
        // ======== DIBUJO 3D FLUIDO ========
        if (currentLine == null)
        {
            GameObject go = new GameObject("Line3D");
            currentLine = go.AddComponent<LineRenderer>();
            currentLine.material = lineMaterial;
            currentLine.startWidth = lineWidth;
            currentLine.endWidth = lineWidth;
            currentLine.positionCount = 0;
            currentLine.useWorldSpace = true;
        }

        float dist = Vector3.Distance(lastWorldPos, worldPos);
        if (currentLine.positionCount == 0 || dist > minStepDistance)
        {
            currentLine.positionCount++;
            currentLine.SetPosition(currentLine.positionCount - 1, worldPos);
            lastWorldPos = worldPos;
        }

        // ======== DIBUJO 2D EN CANVAS ========
        RectTransform rt = displayImage.rectTransform;
        Vector3 localPoint = rt.InverseTransformPoint(worldPos);
        Vector2 norm = Rect.PointToNormalized(rt.rect, new Vector2(localPoint.x, localPoint.y));

        if (norm.x >= 0f && norm.x <= 1f && norm.y >= 0f && norm.y <= 1f)
        {
            int x = Mathf.RoundToInt(norm.x * (drawingTexture.width - 1));
            int y = Mathf.RoundToInt(norm.y * (drawingTexture.height - 1));

            Vector2 current = new Vector2(x, y);
            if (lastPixel.HasValue)
                InterpolateLine(lastPixel.Value, current);
            else
                AddPixels(x, y);

            lastPixel = current;
        }
    }

    private void StopDrawing()
    {
        if (isDrawing && currentLine != null)
        {
            currentLine = null;
            lastPixel = null;

            if (checkForSendCoroutine == null)
                checkForSendCoroutine = StartCoroutine(CheckForSend());
        }
        isDrawing = false;
    }

    private void InterpolateLine(Vector2 start, Vector2 end)
    {
        float distance = Vector2.Distance(start, end);
        int steps = Mathf.CeilToInt(distance);

        for (int i = 0; i <= steps; i++)
        {
            float t = (float)i / steps;
            int x = Mathf.RoundToInt(Mathf.Lerp(start.x, end.x, t));
            int y = Mathf.RoundToInt(Mathf.Lerp(start.y, end.y, t));
            AddPixels(x, y);
        }
    }

    private void AddPixels(int cx, int cy)
    {
        for (int dx = -brushSize; dx <= brushSize; dx++)
        {
            for (int dy = -brushSize; dy <= brushSize; dy++)
            {
                int px = cx + dx;
                int py = cy + dy;

                if (px >= 0 && px < drawingTexture.width && py >= 0 && py < drawingTexture.height)
                {
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    if (dist <= brushSize)
                        drawBuffer.Add(new Vector2Int(px, py));
                }
            }
        }
    }

    private IEnumerator CheckForSend()
    {
        yield return new WaitForSeconds(delayToSend);

        if (classifier != null)
        {
            string result = classifier.ExecuteModel(drawingTexture);
            Debug.Log($"Resultado del modelo: {result}");
            DisplayResult(result);
        }

        checkForSendCoroutine = null;
    }

    // Actualiza el texto del resultado de forma persistente
    private void DisplayResult(string text)
    {
        if (showResultCoroutine != null)
            StopCoroutine(showResultCoroutine);

        showResultCoroutine = StartCoroutine(ShowResult($"Predicción: {text}"));
    }

    private IEnumerator ShowResult(string text)
    {
        if (resultLabel != null)
        {
            resultLabel.text = text;
            resultLabel.alpha = 1f;

            // Pequeño efecto de cambio de color para indicar actualización
            Color original = resultLabel.color;
            resultLabel.color = Color.yellow;
            yield return new WaitForSeconds(0.2f);
            resultLabel.color = original;
        }
    }

    // Borra automáticamente cada 20 segundosss
    private IEnumerator AutoClearEvery20Seconds()
    {
        while (true)
        {
            yield return new WaitForSeconds(11f);
            ClearTexture();
            Debug.Log("Lienzo limpiado automáticamente después de 20 segundos.");
        }
    }

    public void ClearTexture()
    {
        Color[] clear = new Color[drawingTexture.width * drawingTexture.height];
        for (int i = 0; i < clear.Length; i++)
            clear[i] = Color.black;
        drawingTexture.SetPixels(clear);
        drawingTexture.Apply(false);

        // Mantener visible el label (no se borra el texto)
        if (resultLabel != null)
        {
            resultLabel.alpha = 1f;
        }
    }
}
