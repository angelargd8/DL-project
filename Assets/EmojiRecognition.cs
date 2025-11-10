using System.Collections;
using System.Collections.Generic;
using Unity.Sentis;
using UnityEngine;

public class EmojiRecognition : MonoBehaviour
{
    [Header("Configuración del modelo")]

    [SerializeField] private ControllerDrawing controllerDrawing;

    // confianza mínima
    public float threshhold = 0.75f;

    // Imagen que se usará como entrada de prueba del modelo
    public Texture2D testPicture;

    // Modelo de IA exportado
    public ModelAsset modelAsset;

    // Encargado de ejecutar el modelo en GPU o CPU
    private Worker worker;

    // Resultados del modelo
    [SerializeField] private float[] results;

    string[] classNames = { "confundido", "enojado", "feliz", "muerto", "neutral", "sorprendida", "triste" };

    void Start()
    {
        // Inicializa IA
        Model model = ModelLoader.Load(modelAsset);
        worker = new Worker(model, BackendType.GPUCompute);
    }

    // Función auxiliar para invertir los colores
    private Texture2D InvertColors(Texture2D source)
    {
        Texture2D inverted = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);
        Color[] pixels = source.GetPixels();

        for (int i = 0; i < pixels.Length; i++)
        {
            Color c = pixels[i];
            inverted.SetPixel(i % source.width, i / source.width, new Color(1f - c.r, 1f - c.g, 1f - c.b, c.a));
        }

        inverted.Apply();
        return inverted;
    }

    // Ejecuta el modelo directamente (versión de prueba)
    public int RunAI(Texture2D picture)
    {
        // Invertir colores antes de enviarlo al modelo
        Texture2D inverted = InvertColors(picture);

        // Transformación para el modelo
        var transform = new TextureTransform()
            .SetDimensions(64, 64, 3)
            .SetTensorLayout(TensorLayout.NHWC);

        // Convertir a tensor
        using Tensor<float> inputTensor = TextureConverter.ToTensor(inverted, transform);

        worker.SetInput("input", inputTensor);
        worker.Schedule();

        Tensor<float> outputTensor = worker.PeekOutput("keras_tensor_153") as Tensor<float>;
        results = outputTensor.DownloadToArray();

        for (int i = 0; i < results.Length; i++)
            Debug.Log($"Clase {i}: {results[i]}");

        int predictedIndex = System.Array.IndexOf(results, Mathf.Max(results));
        Debug.Log($"Predicción: {classNames[predictedIndex]} (Confianza: {results[predictedIndex]:F2})");

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif

        return GetMaxIndex(results);
    }

    // Llamado por controllerDrawing al terminar un trazo
    public string ExecuteModel(Texture2D picture)
    {
        Debug.Log("Ejecutando modelo con textura actual...");

        // Invertir colores antes de la inferencia
        Texture2D inverted = InvertColors(picture);

        var transform = new TextureTransform()
            .SetDimensions(64, 64, 3)
            .SetTensorLayout(TensorLayout.NHWC);

        using Tensor<float> inputTensor = TextureConverter.ToTensor(inverted, transform);

        worker.SetInput("input", inputTensor);
        worker.Schedule();

        using Tensor<float> outputTensor = worker.PeekOutput("keras_tensor_153") as Tensor<float>;
        results = outputTensor.DownloadToArray();

        int maxIndex = GetMaxIndex(results);
        string prediction;

        if (maxIndex >= 0)
        {
            prediction = classNames[maxIndex];
            Debug.Log($"Emoji predicho: {prediction} (Confianza: {results[maxIndex]:F2})");
        }
        else
        {
            prediction = "incierto";
            Debug.Log($"Predicción incierta (Confianza: {Mathf.Max(results):F2})");
        }

        // Limpia el lienzo después de clasificar
        if (controllerDrawing != null)
        {
            controllerDrawing.ClearTexture();
        }
        else
        {
            Debug.LogWarning("No se asignó la referencia a controllerDrawing en el inspector.");
        }

        return prediction;
    }

    // Condición de confianza mínima
    public int GetMaxIndex(float[] array)
    {
        int maxIndex = 0;
        for (int i = 0; i < array.Length; i++)
        {
            if (array[i] > array[maxIndex])
                maxIndex = i;
        }

        if (array[maxIndex] > threshhold)
        {
            Debug.Log($"Predicción aceptada: {classNames[maxIndex]} ({array[maxIndex]:F2})");
            return maxIndex;
        }
        else
        {
            Debug.Log($"Predicción incierta (Confianza: {array[maxIndex]:F2}) — sin clase asignada.");
            return -1;
        }
    }

    // Limpia todo al desactivar
    private void OnDisable()
    {
        worker.Dispose();
    }
}
