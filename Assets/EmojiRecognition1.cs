//using System.Collections;
//using System.Collections.Generic;
//using Unity.Sentis;
//using UnityEngine;

//public class EmojiRecognition : MonoBehaviour
//{
//    //confianza minima
//    public float threshhold = 0.9f;

//    //Imagen que se usará como entrada de prueba del model
//    public Texture2D testPicture;

//    //Es el modelo de IA exportado
//    public ModelAsset modelAsset;

//    //Es el objeto encargado de ejecutar el modelo en GPU o CPU
//    private Worker worker;

//    //Guarda los valores numéricos de probabilidad de cada clase
//    //[SerializeField] permite verlo en el Inspector aunque sea private
//    [SerializeField] private float[] results;

//    string[] classNames = { "confundido", "enojado", "feliz", "muerto", "neutral", "sorprendida", "triste" };


//    void Start()
//    {
//        // init IA
//        Model model = ModelLoader.Load(modelAsset);
//        worker = new Worker(model, BackendType.GPUCompute);

//        Debug.Log(RunAI(testPicture));
//    }




//    //run the model
//    public int RunAI(Texture2D picture)
//    {
//        //crear una transformacion que se ajusta a la imagen
//        var transform = new TextureTransform()
//            .SetDimensions(64, 64, 3) //64x64 pixeles y 3 canales RGB
//            .SetTensorLayout(TensorLayout.NHWC); //formato NHWC (batch, height, width, channels)

//        //Convierte la imagen Texture2D en un tensor de floats normalizado (0 a 1)
//        using Tensor<float> inputTensor = TextureConverter.ToTensor(picture, transform);

//        //Asigna el tensor de entrada al modelo
//        worker.SetInput("input", inputTensor);

//        //lanza la ejecución asíncrona del modelo en la GPU.
//        worker.Schedule();

//        //running the worker with a tensor
//        //Recupera el tensor de salida del modelo (vector con las probabilidades de cada clase).
//        Tensor<float> outputTensor = worker.PeekOutput("keras_tensor_153") as Tensor<float>;
//        //guardar los resultados del tensor en el array
//        results = outputTensor.DownloadToArray();

//        // mostrar la probabilidad de cada clase
//        for (int i = 0; i < results.Length; i++)
//            Debug.Log($"Clase {i}: {results[i]}");

//        //busca el indice con el valor maximo de probabilidad
//        int predictedIndex = System.Array.IndexOf(results, Mathf.Max(results));
//        Debug.Log($"Predicción: {classNames[predictedIndex]} (Confianza: {results[predictedIndex]:F2})");

//        // forzar refresco visual en el inspector
//#if UNITY_EDITOR
//        UnityEditor.EditorUtility.SetDirty(this);
//#endif

//        return GetMaxIndex(results);
//    }

//    //clean everything on disable, libera los recursos del worker
//    private void OnDisable()
//    {
//        worker.Dispose();
//    }

//    // condición de confianza mínima
//    public int GetMaxIndex(float[] array)
//    {
//        int maxIndex = 0;
//        for (int i = 0; i < array.Length; i++)
//        {
//            if (array[i] > array[maxIndex])
//            {
//                maxIndex = i;
//            }
//        }

//        if (array[maxIndex] > threshhold)
//        {
//            Debug.Log($"Predicción aceptada: {classNames[maxIndex]} ({array[maxIndex]:F2})");
//            return maxIndex;
//        }
//        else
//        {
//            Debug.Log($"Predicción incierta (Confianza: {array[maxIndex]:F2}) — sin clase asignada.");
//            return -1;
//        }
//    }


//}
