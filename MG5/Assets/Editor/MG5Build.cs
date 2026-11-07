using System;
using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;
using UnityEditor.Build;

public static class MG5Build
{
    public static void Prepare()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/Main.unity");
        AssetDatabase.DeleteAsset("Assets/Meadow");
        var tags = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        tags.FindProperty("layers").GetArrayElementAtIndex(8).stringValue = "Ground";
        tags.ApplyModifiedProperties();
        const string folder = "Assets/Meadow";
        if (!AssetDatabase.IsValidFolder(folder)) AssetDatabase.CreateFolder("Assets", "Meadow");
        var data = new TerrainData();
        data.heightmapResolution = 129;
        data.alphamapResolution = 128;
        data.size = new Vector3(100f, 14f, 100f);
        float[,] heights = new float[129,129];
        for(int z=0;z<129;z++) for(int x=0;x<129;x++)
        {
            float dx=(x/128f-0.5f)*2f, dz=(z/128f-0.5f)*2f;
            float edge=Mathf.Pow(Mathf.Max(Mathf.Abs(dx),Mathf.Abs(dz)),3f);
            heights[z,x]=0.025f+edge*0.45f+Mathf.PerlinNoise(x*0.045f,z*0.045f)*0.055f;
        }
        data.SetHeights(0,0,heights);
        var layers = new TerrainLayer[3];
        for(int i=0;i<3;i++) layers[i]=AssetDatabase.LoadAssetAtPath<TerrainLayer>("Assets/Holotna/Mountain/Misc/Ground0"+(i+1)+".terrainlayer");
        data.terrainLayers=layers;
        float[,,] paint=new float[128,128,3];
        for(int z=0;z<128;z++) for(int x=0;x<128;x++)
        {
            float path=Mathf.Abs(x/127f-(0.5f+Mathf.Sin(z*0.07f)*0.08f));
            float rock=Mathf.Clamp01((heights[z,x]-0.20f)*5f);
            float trail=1f-Mathf.SmoothStep(0.025f,0.075f,path);
            paint[z,x,0]=(1f-rock)*(1f-trail);
            paint[z,x,1]=(1f-rock)*trail;
            paint[z,x,2]=rock;
        }
        data.SetAlphamaps(0,0,paint);
        string[] plantNames={"Tree01A","Tree01B","Flower01A"};
        var prototypes=new TreePrototype[3];
        for(int i=0;i<3;i++) prototypes[i]=new TreePrototype{prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Holotna/Mountain/Prefabs/"+plantNames[i]+".prefab")};
        data.treePrototypes=prototypes;
        UnityEngine.Random.InitState(31);
        var trees=new System.Collections.Generic.List<TreeInstance>();
        for(int i=0;i<160;i++)
        {
            float x=UnityEngine.Random.Range(0.07f,0.93f), z=UnityEngine.Random.Range(0.07f,0.93f);
            if (Mathf.Abs(x-(0.5f+Mathf.Sin(z*127f*0.07f)*0.08f))<0.085f) continue;
            trees.Add(new TreeInstance{position=new Vector3(x,0,z),prototypeIndex=i%3,widthScale=1f,heightScale=1f,rotation=UnityEngine.Random.Range(0,6.28f),color=Color.white,lightmapColor=Color.white});
        }
        data.treeInstances=trees.ToArray();
        AssetDatabase.CreateAsset(data,folder+"/MeadowTerrain.asset");
        var terrainObject=Terrain.CreateTerrainGameObject(data);
        terrainObject.name="Mountain Meadow Terrain";
        terrainObject.layer=8;
        var terrain=terrainObject.GetComponent<Terrain>();
        terrain.materialTemplate=AssetDatabase.LoadAssetAtPath<Material>("Assets/Holotna/Mountain/Materials/Terrain.mat");
        if(!terrain.materialTemplate)
        {
            var material=new Material(Shader.Find("Universal Render Pipeline/Terrain/Lit"));
            AssetDatabase.CreateAsset(material,folder+"/Terrain.mat");terrain.materialTemplate=material;
        }
        var surface=terrainObject.AddComponent<NavMeshSurface>();
        surface.layerMask=1<<8;
        surface.useGeometry=NavMeshCollectGeometry.PhysicsColliders;
        surface.BuildNavMesh();
        AssetDatabase.CreateAsset(surface.navMeshData,folder+"/MeadowNavMesh.asset");
        var player=UnityEngine.Object.FindFirstObjectByType<Player>();
        player.transform.position=At(50,43,terrain);
        var rb=player.GetComponent<Rigidbody>();if(rb) UnityEngine.Object.DestroyImmediate(rb);
        var agent=player.GetComponent<NavMeshAgent>();
        agent.speed=5f;agent.angularSpeed=360;agent.acceleration=16;agent.radius=0.35f;agent.height=1f;agent.stoppingDistance=0.1f;
        var bird=UnityEngine.Object.FindFirstObjectByType<Bird>();
        Vector2[] spots={new Vector2(49,49),new Vector2(55,53),new Vector2(45,55),new Vector2(53,59),new Vector2(46,63)};
        Bird[] birds=new Bird[spots.Length];
        for(int i=0;i<spots.Length;i++)
        {
            birds[i]=i==0?bird:UnityEngine.Object.Instantiate(bird);
            birds[i].name="Sparrow "+(i+1);
            birds[i].transform.position=At(spots[i].x,spots[i].y,terrain)+Vector3.up*0.1f;
        }
        var manager=new SerializedObject(UnityEngine.Object.FindFirstObjectByType<BirdManager>());
        manager.FindProperty("_player").objectReferenceValue=player.transform;
        manager.FindProperty("_interactDistance").floatValue=5f;
        var array=manager.FindProperty("_birds");array.arraySize=birds.Length;
        for(int i=0;i<birds.Length;i++) array.GetArrayElementAtIndex(i).objectReferenceValue=birds[i];
        manager.ApplyModifiedProperties();
        Camera.main.gameObject.AddComponent<MeadowCamera>().target=player.transform;
        Camera.main.transform.position=player.transform.position+new Vector3(0,17,-14);
        Camera.main.transform.LookAt(player.transform.position+Vector3.forward*2);
        Camera.main.farClipPlane=220;
        PlayerSettings.companyName="Qiliang Wan";
        PlayerSettings.productName="Mountain Meadow MG5";
        PlayerSettings.WebGL.compressionFormat=WebGLCompressionFormat.Disabled;
        PlayerSettings.WebGL.decompressionFallback=false;
        PlayerSettings.defaultWebScreenWidth=960;PlayerSettings.defaultWebScreenHeight=600;
        PlayerSettings.SetScriptingBackend(NamedBuildTarget.WebGL,ScriptingImplementation.IL2CPP);
        EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();
        Debug.Log("MG5_PREPARED terrainLayers="+data.terrainLayers.Length+" plants="+data.treePrototypes.Length+" birds="+birds.Length+" navmesh="+NavMesh.CalculateTriangulation().vertices.Length);
    }
    private static Vector3 At(float x,float z,Terrain terrain) {return new Vector3(x,terrain.SampleHeight(new Vector3(x,0,z))+0.05f,z);}
    public static void PreviewBuild()
    {
        var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/Scenes/Main.unity"},locationPathName="D:/Codex-Storage/MG5-20261107/Preview/Meadow.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.Development});
        if(report.summary.result!=BuildResult.Succeeded) throw new Exception("Preview failed");
    }
    public static void WebBuild()
    {
        var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/Scenes/Main.unity"},locationPathName="D:/Codex-Storage/MG5-20261107/WebBuild",target=BuildTarget.WebGL,options=BuildOptions.None});
        if(report.summary.result!=BuildResult.Succeeded) throw new Exception("MG5 Web build failed");
    }
}




