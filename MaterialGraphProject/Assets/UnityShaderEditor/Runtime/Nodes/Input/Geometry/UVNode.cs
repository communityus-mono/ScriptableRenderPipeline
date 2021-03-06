using UnityEngine.Graphing;

namespace UnityEngine.MaterialGraph
{
    interface IMayRequireMeshUV
    {
        bool RequiresMeshUV(UVChannel channel);
    }

    [Title("Input/Geometry/UV")]
	public class UVNode : AbstractMaterialNode, IGeneratesBodyCode, IMayRequireMeshUV
    {
        public const int OutputSlotId = 0;
        private const string kOutputSlotName = "UV";

        [SerializeField]
        private UVChannel m_OutputChannel;
        public UVChannel uvChannel
        {
            get { return m_OutputChannel; }
            set
            {
                if (m_OutputChannel == value)
                    return;

                m_OutputChannel = value;
                if (onModified != null)
                {
                    onModified(this, ModificationScope.Graph);
                }
            }
        }

        public override bool hasPreview { get { return true; } }
        
        public UVNode()
        {
            name = "UV";
            UpdateNodeAfterDeserialization();
        }

        public override void UpdateNodeAfterDeserialization()
        {
            AddSlot(new MaterialSlot(OutputSlotId, kOutputSlotName, kOutputSlotName, SlotType.Output, SlotValueType.Vector4, Vector4.zero));
            RemoveSlotsNameNotMatching(new[] { OutputSlotId });
        }

        public void GenerateNodeCode(ShaderGenerator visitor, GenerationMode generationMode)
        {
            visitor.AddShaderChunk(precision + "4 " + GetVariableNameForSlot(OutputSlotId) + " = " + m_OutputChannel.GetUVName() + ";", true);
        }

        public bool RequiresMeshUV(UVChannel channel)
        {
            return channel == uvChannel;
        }
    }
}
