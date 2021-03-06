using System.Collections.Generic;
using UnityEngine.Graphing;

namespace UnityEngine.MaterialGraph
{
    public interface IMasterNode : INode
    {
        string GetFullShader(
            GenerationMode mode,
            out List<PropertyGenerator.TextureInfo> configuredTextures);

        string GetSubShader(GenerationMode mode, PropertyGenerator shaderPropertiesVisitor);

        string GetVariableNameForNode();

        bool has3DPreview();
    }
}
