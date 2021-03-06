using System.IO;
using UnityEditor.ProjectWindowCallback;
using UnityEngine.MaterialGraph;

namespace UnityEditor.MaterialGraph
{
    public class CreateShaderGraph : EndNameEditAction
    {
        [MenuItem("Assets/Create/Shader Graph", false, 208)]
        public static void CreateMaterialGraph()
        {
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, CreateInstance<CreateShaderGraph>(),
                "New Shader Graph.ShaderGraph", null, null);
        }

        public override void Action(int instanceId, string pathName, string resourceFile)
        {
			var graph = new UnityEngine.MaterialGraph.MaterialGraph();
			File.WriteAllText(pathName, EditorJsonUtility.ToJson(graph));
			AssetDatabase.Refresh ();
        }
    }
}
