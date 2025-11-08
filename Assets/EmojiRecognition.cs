using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Sentis;

public class EmojiRecognition : MonoBehaviour
{
    public ModelAsset modelAsset;

    private Worker worker;

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
    public void runAI()
    {

    }

    private void OnDisable()
    {
        
    }
}
