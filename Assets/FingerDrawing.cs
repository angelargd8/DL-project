using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;

public class FingerDrawing : MonoBehaviour
{
    [SerializeField] private RawImage displayImage;
    [SerializeField] private EmojiRecognition classifier;
    [SerializeField] private Transform fingerTipMarkerTransform;
    [SerializeField] private float delayToSend = 1f;
    [SerializeField] private float distanceToCanvas = 0.07f;

    private bool hasDrawn = false;
    private float lastDrawnTime;
    private Camera mainCamera;
    private Texture2D drawningTexture;
    private Coroutine checkForSendCoroutine;


    // Start is called before the first frame update
    void Start()
    {
        drawningTexture = new Texture2D(64, 64, TextureFormat.ARGB32, false);
        displayImage.texture = drawningTexture;
        mainCamera = Camera.main;
        ClearTexture();

    }

    // Update is called once per frame
    void Update()
    {
     bool isDrawing = Vector3.Distance(fingerTipMarkerTransform.position, displayImage.transform.position) > distanceToCanvas;

        if (isDrawing) {

            if (checkForSendCoroutine != null)
            {
                StopCoroutine(checkForSendCoroutine);
                checkForSendCoroutine = null;
            }

            Draw(fingerTipMarkerTransform.position);
            hasDrawn = true;
            lastDrawnTime = Time.time;
        }
        else if (hasDrawn && Time.time - lastDrawnTime > delayToSend && checkForSendCoroutine==null)
        {
            checkForSendCoroutine = StartCoroutine(CheckForSend());

        }

        
    }


    private void Draw(Vector3 fingerTipPos)
    {
        Vector2 screenPoint = mainCamera.WorldToScreenPoint(fingerTipPos);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(displayImage.rectTransform, screenPoint, mainCamera, out Vector2 localPoint);
        Vector2 normalizedPoint = Rect.PointToNormalized(displayImage.rectTransform.rect, localPoint);
        AddPixels(normalizedPoint);

    }

    private void AddPixels(Vector2 normalizedPoint)
    {
        int texX = (int)(normalizedPoint.x * drawningTexture.width);
        int texY = (int)(normalizedPoint.y * drawningTexture.height);

        if (texX >= 0 && texX < drawningTexture.width && texY >=0 && texY< drawningTexture.height)
        {
            drawningTexture.SetPixel(texX, texY, Color.white);
            drawningTexture.Apply();
        }
    }

    //private IEnumerator CheckForSend()
    //{
    //    yield return new WaitForSeconds(delayToSend);
    //    classifier.ExecuteModel(drawningTexture);
    //    hasDrawn = false;
    //    checkForSendCoroutine = null;
    //}
    private IEnumerator CheckForSend()
    {
        yield return new WaitForSeconds(delayToSend);

        // Llamada al modelo Sentiss
        string predictedEmoji = classifier.ExecuteModel(drawningTexture);

        Debug.Log($"Resultado del modelo: {predictedEmoji}");

        hasDrawn = false;
        checkForSendCoroutine = null;
    }

    public void ClearTexture()
    {
        Color[] clearColors = new Color[drawningTexture.width * drawningTexture.height];
        for (int i=0; i <clearColors.Length; i++)
            clearColors[i] = Color.white;
        drawningTexture.SetPixels(clearColors);
        drawningTexture.Apply ();
    }

}
