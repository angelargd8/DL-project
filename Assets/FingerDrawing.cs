//using UnityEngine;
//using UnityEngine.UI;
//using System.Collections;

//public class FingerDrawing : MonoBehaviour
//{
//    [SerializeField] private RawImage displayImage;
//    [SerializeField] private EmojiRecognition classifier;
//    [SerializeField] private Transform fingerTipMarkerTransform;
//    [SerializeField] private float delayToSend = 1f;
//    [SerializeField] private float distanceToCanvas = 0.02f; // 2 cm

//    private bool hasDrawn = false;
//    private float lastDrawnTime;
//    private Camera drawCamera;            
//    private Texture2D drawingTexture;
//    private Coroutine checkForSendCoroutine;
//    private Canvas rootCanvas;

//    void Start()
//    {
//        // Obtener Canvas y su cámara
//        rootCanvas = displayImage.canvas;
//        drawCamera = rootCanvas.worldCamera != null ? rootCanvas.worldCamera : Camera.main;

//        // Crear textura donde dibujar
//        drawingTexture = new Texture2D(256, 256, TextureFormat.ARGB32, false);
//        drawingTexture.filterMode = FilterMode.Point;
//        drawingTexture.wrapMode = TextureWrapMode.Clamp;
//        displayImage.texture = drawingTexture;
//        Debug.Log("Canvas camera: " + rootCanvas.worldCamera);
//        Debug.Log("Main camera: " + Camera.main);
        
//        ClearTexture(); // limpia en negro para que el blanco se vea
//    }

//    void Update()
//    {
//        // Distancia del dedo al plano del RawImage (usamos su posición en mundo)
//        float dist = Vector3.Distance(fingerTipMarkerTransform.position, displayImage.transform.position);

//        // DIBUJAR CUANDO ESTÁ CERCA
//        bool isDrawing = dist < distanceToCanvas;
//        Debug.Log($"Distance to canvas: {dist}");
//        if (isDrawing)
//        {
//            if (checkForSendCoroutine != null) { StopCoroutine(checkForSendCoroutine); checkForSendCoroutine = null; }
//            Draw(fingerTipMarkerTransform.position);
//            hasDrawn = true;
//            lastDrawnTime = Time.time;
//        }
//        else if (hasDrawn && Time.time - lastDrawnTime > delayToSend && checkForSendCoroutine == null)
//        {
//            checkForSendCoroutine = StartCoroutine(CheckForSend());
//        }
//    }

//    private void Draw(Vector3 fingerTipWorldPos)
//    {
//        // De mundo → pantalla → coords locales del RawImage
//        Vector2 screenPoint = drawCamera.WorldToScreenPoint(fingerTipWorldPos);
//        Debug.Log($"Drawing at screen point: {screenPoint}");

//        RectTransform rt = displayImage.rectTransform;
//        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, screenPoint, drawCamera, out Vector2 local))
//        {
//            // Local → [0..1] normalizado dentro del rect
//            Vector2 norm = Rect.PointToNormalized(rt.rect, local);

//            // Validar dentro del rect
//            if (norm.x >= 0 && norm.x <= 1 && norm.y >= 0 && norm.y <= 1)
//            {
//                AddPixels(norm);
//            }
//        }
//    }

//    private void AddPixels(Vector2 norm)
//    {
//        int x = Mathf.RoundToInt(norm.x * (drawingTexture.width - 1));
//        int y = Mathf.RoundToInt(norm.y * (drawingTexture.height - 1));

//        // Texturas tienen eje Y invertido a veces; si te sale “espejado”, invierte:
//        // y = drawingTexture.height - 1 - y;

//        // Trazo de 3x3 para que se note
//        for (int dx = -1; dx <= 1; dx++)
//            for (int dy = -1; dy <= 1; dy++)
//            {
//                int px = x + dx, py = y + dy;
//                if (px >= 0 && px < drawingTexture.width && py >= 0 && py < drawingTexture.height)
//                    drawingTexture.SetPixel(px, py, Color.white);
//            }
//        drawingTexture.Apply(false);
//    }

//    private IEnumerator CheckForSend()
//    {
//        yield return new WaitForSeconds(delayToSend);

//        if (classifier != null)
//        {
//            string predictedEmoji = classifier.ExecuteModel(drawingTexture);
//            Debug.Log($"Resultado del modelo: {predictedEmoji}");
//        }

//        hasDrawn = false;
//        checkForSendCoroutine = null;
//    }

//    public void ClearTexture()
//    {
//        // Fondo negro para que el blanco se vea
//        Color[] clear = new Color[drawingTexture.width * drawingTexture.height];
//        for (int i = 0; i < clear.Length; i++) clear[i] = Color.black;
//        drawingTexture.SetPixels(clear);
//        drawingTexture.Apply(false);
//    }
//}
