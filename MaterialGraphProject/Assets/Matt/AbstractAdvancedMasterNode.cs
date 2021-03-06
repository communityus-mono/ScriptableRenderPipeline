using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine.Graphing;

namespace UnityEngine.MaterialGraph
{
    [Serializable]
    public abstract class AbstractAdvancedMasterNode : AbstractMasterNode
    {
        public const string AlbedoSlotName = "Albedo";
        public const string NormalSlotName = "Normal";
        public const string EmissionSlotName = "Emission";
        public const string SmoothnessSlotName = "Smoothness";
        public const string OcclusionSlotName = "Occlusion";
        public const string AlphaSlotName = "Alpha";

        public const int AlbedoSlotId = 0;
        public const int NormalSlotId = 1;
        public const int EmissionSlotId = 3;
        public const int SmoothnessSlotId = 4;
        public const int OcclusionSlotId = 5;
        public const int AlphaSlotId = 6;

        public const string TangentSlotName = "Tangent";
        public const int TangentSlotId = 7;

        [SerializeField]
        private SurfaceMaterialOptions m_MaterialOptions = new SurfaceMaterialOptions();

        public SurfaceMaterialOptions options
        {
            get { return m_MaterialOptions; }
        }

        public abstract string GetSurfaceOutputName();
        public abstract string GetLightFunction();

        public abstract string GetMaterialID();
        //Deal with custom data packing
        public abstract int[] GetCustomDataSlots();
        public abstract string[] GetCustomData();

        // Check for whether tangents are needed in the BRDF
        public virtual bool RequireTangentCalculation()
        {
            return false;
        }

        public override string GetSubShader(GenerationMode mode, PropertyGenerator shaderPropertiesVisitor)
        {
            var templateLocation = ShaderGenerator.GetTemplatePath("advancedSubshader.template");

            if (!File.Exists(templateLocation))
                return string.Empty;

            var activeNodeList = new List<INode>();
            NodeUtils.DepthFirstCollectNodesFromNode(activeNodeList, this);
            foreach (var node in activeNodeList.OfType<AbstractMaterialNode>())
                node.GeneratePropertyBlock(shaderPropertiesVisitor, mode);

            var templateText = File.ReadAllText(templateLocation);
            var shaderBodyVisitor = new ShaderGenerator();
            var shaderFunctionVisitor = new ShaderGenerator();
            var shaderPropertyUsagesVisitor = new ShaderGenerator();
            var shaderInputVisitor = new ShaderGenerator();
            var vertexShaderBlock = new ShaderGenerator();

            GenerateSurfaceShaderInternal(
                shaderPropertyUsagesVisitor,
                shaderBodyVisitor,
                shaderFunctionVisitor,
                shaderInputVisitor,
                vertexShaderBlock,
                mode);

            var tagsVisitor = new ShaderGenerator();
            var blendingVisitor = new ShaderGenerator();
            var cullingVisitor = new ShaderGenerator();
            var zTestVisitor = new ShaderGenerator();
            var zWriteVisitor = new ShaderGenerator();

            m_MaterialOptions.GetTags(tagsVisitor);
            m_MaterialOptions.GetBlend(blendingVisitor);
            m_MaterialOptions.GetCull(cullingVisitor);
            m_MaterialOptions.GetDepthTest(zTestVisitor);
            m_MaterialOptions.GetDepthWrite(zWriteVisitor);

            var resultShader = templateText.Replace("${ShaderPropertyUsages}", shaderPropertyUsagesVisitor.GetShaderString(2));
            resultShader = resultShader.Replace("${LightingFunctionName}", GetLightFunction());
            resultShader = resultShader.Replace("${SurfaceOutputStructureName}", GetSurfaceOutputName());
            resultShader = resultShader.Replace("${ShaderFunctions}", shaderFunctionVisitor.GetShaderString(2));
            resultShader = resultShader.Replace("${ShaderInputs}", shaderInputVisitor.GetShaderString(3));
            resultShader = resultShader.Replace("${PixelShaderBody}", shaderBodyVisitor.GetShaderString(3));
            resultShader = resultShader.Replace("${Tags}", tagsVisitor.GetShaderString(2));
            resultShader = resultShader.Replace("${Blending}", blendingVisitor.GetShaderString(2));
            resultShader = resultShader.Replace("${Culling}", cullingVisitor.GetShaderString(2));
            resultShader = resultShader.Replace("${ZTest}", zTestVisitor.GetShaderString(2));
            resultShader = resultShader.Replace("${ZWrite}", zWriteVisitor.GetShaderString(2));
            resultShader = resultShader.Replace("${LOD}", ""+m_MaterialOptions.lod);

            resultShader = resultShader.Replace("${VertexShaderDecl}", "vertex:vert");
            resultShader = resultShader.Replace("${VertexShaderBody}", vertexShaderBlock.GetShaderString(3));

            resultShader = resultShader.Replace("${MaterialID}", "#define "+GetMaterialID());

            return resultShader;
        }

        public override string GetFullShader(
            GenerationMode mode,
            out List<PropertyGenerator.TextureInfo> configuredTextures)
        {
            var templateLocation = ShaderGenerator.GetTemplatePath("shader.template");

            if (!File.Exists(templateLocation))
            {
                configuredTextures = new List<PropertyGenerator.TextureInfo>();
                return string.Empty;
            }

            var templateText = File.ReadAllText(templateLocation);
            
            var shaderPropertiesVisitor = new PropertyGenerator();
            var resultShader = templateText.Replace("${ShaderName}", GetType() + guid.ToString());
            resultShader = resultShader.Replace("${SubShader}", GetSubShader(mode, shaderPropertiesVisitor));
            resultShader = resultShader.Replace("${ShaderPropertiesHeader}", shaderPropertiesVisitor.GetShaderString(2));
            configuredTextures = shaderPropertiesVisitor.GetConfiguredTexutres();
            return Regex.Replace(resultShader, @"\r\n|\n\r|\n|\r", Environment.NewLine);
        }

        private void GenerateSurfaceShaderInternal(
           ShaderGenerator propertyUsages,
           ShaderGenerator shaderBody,
           ShaderGenerator nodeFunction,
           ShaderGenerator shaderInputVisitor,
           ShaderGenerator vertexShaderBlock,
           GenerationMode mode)
        {
            var activeNodeList = new List<INode>();
            NodeUtils.DepthFirstCollectNodesFromNode(activeNodeList, this);

            foreach (var node in activeNodeList.OfType<AbstractMaterialNode>())
            {
                if (node is IGeneratesFunction)
                    (node as IGeneratesFunction).GenerateNodeFunction(nodeFunction, mode);

                node.GeneratePropertyUsages(propertyUsages, mode);
            }

            // always add color because why not.
            shaderInputVisitor.AddShaderChunk("float4 color : COLOR;", true);

            for (int uvIndex = 0; uvIndex < ShaderGeneratorNames.UVCount; ++uvIndex)
            {
                var channel = (UVChannel)uvIndex;
                if (activeNodeList.OfType<IMayRequireMeshUV>().Any(x => x.RequiresMeshUV(channel)))
                {
                    shaderInputVisitor.AddShaderChunk(string.Format("half4 meshUV{0};", uvIndex), true);
                    vertexShaderBlock.AddShaderChunk(string.Format("o.meshUV{0} = v.texcoord{1};", uvIndex, uvIndex == 0 ? "" : uvIndex.ToString()), true);
                    shaderBody.AddShaderChunk(string.Format("half4 {0} = IN.meshUV{1};", channel.GetUVName(), uvIndex), true);
                }
            }

            if (activeNodeList.OfType<IMayRequireViewDirection>().Any(x => x.RequiresViewDirection()))
            {
                shaderInputVisitor.AddShaderChunk("float3 worldViewDir;", true);
                shaderBody.AddShaderChunk("float3 " + ShaderGeneratorNames.WorldSpaceViewDirection  + " = IN.worldViewDir;", true);
            }
            
            if (activeNodeList.OfType<IMayRequireWorldPosition>().Any(x => x.RequiresWorldPosition()))
            {
                shaderInputVisitor.AddShaderChunk("float3 worldPos;", true);
                shaderBody.AddShaderChunk("float3 " + ShaderGeneratorNames.WorldSpacePosition + " = IN.worldPos;", true);
            }

            if (activeNodeList.OfType<IMayRequireScreenPosition>().Any(x => x.RequiresScreenPosition()))
            {
                shaderInputVisitor.AddShaderChunk("float4 screenPos;", true);
                shaderBody.AddShaderChunk("float4 " + ShaderGeneratorNames.ScreenPosition + " = IN.screenPos;", true);
            }

            // Always need normals/tangents/bitangents for anisotropy
            bool needBitangent = true;//= activeNodeList.OfType<IMayRequireBitangent>().Any(x => x.RequiresBitangent());
            if (needBitangent || activeNodeList.OfType<IMayRequireTangent>().Any(x => x.RequiresTangent()))
            {
                shaderInputVisitor.AddShaderChunk("float4 worldTangent;", true);
                vertexShaderBlock.AddShaderChunk("o.worldTangent = float4(UnityObjectToWorldDir(v.tangent.xyz), v.tangent.w);", true);
                shaderBody.AddShaderChunk("float3 " + ShaderGeneratorNames.WorldSpaceTangent + " = normalize(IN.worldTangent.xyz);", true);
            }

            if (needBitangent || activeNodeList.OfType<IMayRequireNormal>().Any(x => x.RequiresNormal()))
            {
                // is the normal connected?
                var normalSlot = FindInputSlot<MaterialSlot>(NormalSlotId);
                var edges = owner.GetEdges(normalSlot.slotReference);

                shaderInputVisitor.AddShaderChunk("float3 worldNormal;", true);
                if (edges.Any())
                    shaderInputVisitor.AddShaderChunk("INTERNAL_DATA", true);

                shaderBody.AddShaderChunk("float3 " + ShaderGeneratorNames.WorldSpaceNormal + " = normalize(IN.worldNormal);", true);
            }

            if (needBitangent)
            {
                shaderBody.AddShaderChunk(string.Format("float3 {0} = cross({1}, {2}) * IN.worldTangent.w;", ShaderGeneratorNames.WorldSpaceBitangent, ShaderGeneratorNames.WorldSpaceNormal, ShaderGeneratorNames.WorldSpaceTangent), true);
            }

            if (activeNodeList.OfType<IMayRequireVertexColor>().Any(x => x.RequiresVertexColor()))
            {
                shaderBody.AddShaderChunk("float4 " + ShaderGeneratorNames.VertexColor + " = IN.color;", true);
            }

            GenerateNodeCode(shaderBody, mode);
        }

        public void GenerateNodeCode(ShaderGenerator shaderBody, GenerationMode generationMode)
        {
            var nodes = ListPool<INode>.Get();

            //shaderBody.AddShaderChunk(, false); // TODO - Switch to defines
            //Get the rest of the nodes for all the other slots
            NodeUtils.DepthFirstCollectNodesFromNode(nodes, this, NodeUtils.IncludeSelf.Exclude);
            for (var i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];
                if (node is IGeneratesBodyCode)
                    (node as IGeneratesBodyCode).GenerateNodeCode(shaderBody, generationMode);
            }
            ListPool<INode>.Release(nodes);
            
            foreach (var slot in GetInputSlots<MaterialSlot>())
            {
                if (!CheckForCustomData(slot.id)) // Check to ignore writing surface data for items getting packed into custom data
                {
                    foreach (var edge in owner.GetEdges(slot.slotReference))
                    {
                        var outputRef = edge.outputSlot;
                        var fromNode = owner.GetNodeFromGuid<AbstractMaterialNode>(outputRef.nodeGuid);
                        if (fromNode == null)
                            continue;

                        var remapper = fromNode as INodeGroupRemapper;
                        if (remapper != null && !remapper.IsValidSlotConnection(outputRef.slotId))
                            continue;

                        shaderBody.AddShaderChunk("o." + slot.shaderOutputName + " = " + fromNode.GetVariableNameForSlot(outputRef.slotId) + ";", true);

                        if (slot.id == NormalSlotId)
                            shaderBody.AddShaderChunk("o." + slot.shaderOutputName + " += 1e-6;", true);
                    }

                    // If tangents are needed for the BRDF
                    // Write tangent data to WorldVectors input on SurfaceOutput
                    if (slot.id == TangentSlotId)
                    {
                        if (RequireTangentCalculation())
                        {
                            var edges = owner.GetEdges(slot.slotReference);
                            if (edges.Any())
                            {
                                shaderBody.AddShaderChunk("float3x3 worldToTangent;", false);
                                shaderBody.AddShaderChunk("worldToTangent[0] = float3(1, 0, 0);", false);
                                shaderBody.AddShaderChunk("worldToTangent[1] = float3(0, 1, 0);", false);
                                shaderBody.AddShaderChunk("worldToTangent[2] = float3(0, 0, 1);", false);
                                shaderBody.AddShaderChunk("float3 tangentTWS = mul(o." + slot.shaderOutputName + ", worldToTangent);", false);
                                shaderBody.AddShaderChunk("o.WorldVectors = float3x3(tangentTWS, " + ShaderGeneratorNames.WorldSpaceBitangent + ", " + ShaderGeneratorNames.WorldSpaceNormal + ");", false);
                            }
                            else
                            {
                                shaderBody.AddShaderChunk("o.WorldVectors = float3x3(" + ShaderGeneratorNames.WorldSpaceTangent + ", " + ShaderGeneratorNames.WorldSpaceBitangent + ", " + ShaderGeneratorNames.WorldSpaceNormal + ");", false);
                            }
                        }
                    }
                }
            }

            string[] customData = GetCustomData();
            foreach(string data in customData)
            {
                shaderBody.AddShaderChunk(data, false);
            }
        }

        public MaterialSlot GetMaterialSlot(int id)
        {
            foreach (var slot in GetInputSlots<MaterialSlot>())
            {
                if (slot.id == id)
                    return slot;
            }
            return null;
        }

        public string GetVariableNameForSlotAtId(int id)
        {
            MaterialSlot slot = GetMaterialSlot(id);
            if (slot != null)
            {
                foreach (var edge in owner.GetEdges(slot.slotReference))
                {
                    var outputRef = edge.outputSlot;
                    var fromNode = owner.GetNodeFromGuid<AbstractMaterialNode>(outputRef.nodeGuid);
                    if (fromNode == null)
                        continue;

                    var remapper = fromNode as INodeGroupRemapper;
                    if (remapper != null && !remapper.IsValidSlotConnection(outputRef.slotId))
                        continue;

                    return fromNode.GetVariableNameForSlot(outputRef.slotId);
                    //shaderBody.AddShaderChunk("o." + slot.shaderOutputName + " = " + fromNode.GetVariableNameForSlot(outputRef.slotId) + ";", true);
                }
            }
            return "";
        }

        bool CheckForCustomData(int id)
        {
            foreach (var custom in GetCustomDataSlots())
            {
                if (custom == id)
                    return true;
            }
            return false;
        }
    }
}
