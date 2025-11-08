using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Sentis;

public class EmojiRecognition : MonoBehaviour
{
    public ModelAsset modelAsset;

    private Worker worker;

    public float[] results;

    // Start is called before the first frame update
    void Start()
    {
        // init IA
        Model model = ModelLoader.Load(modelAsset);

        //graph 
        FunctionalGraph graph = new FunctionalGraph();
        FunctionalTensor[] inputs = graph.AddInputs(model);
        FunctionalTensor[] outputs = Functional.Forward(model, inputs);

        Model runtimeModel = graph.Compile(outputs);
        worker = new Worker(runtimeModel, BackendType.GPUCompute);


    }

    //run the model
    public void runAI(Texture2D picture)
    {
        //running the worker with a tensor
        using Tensor<float> inputTensor = TextureConverter.ToTensor(picture, 64, 64, 1);
        worker.Schedule(inputTensor);
        Tensor<float> outputTensor = worker.PeekOutput() as Tensor<float>;
        results = outputTensor.DownloadToArray();

    }

    //clean everything on disable
    private void OnDisable()
    {
        worker.Dispose();
    }
}
