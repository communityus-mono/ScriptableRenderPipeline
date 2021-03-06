using UnityEngine.Graphing;


namespace UnityEngine.MaterialGraph
{
    [Title("Normal/Unpack Normal")]
    internal class UnpackNormalNode : Function1Input
    {
        public UnpackNormalNode()
        {
            name = "UnpackNormal";
        }

        public override bool hasPreview
        {
            get { return false; }
        }

        protected override MaterialSlot GetInputSlot()
        {
            return new MaterialSlot(InputSlotId, GetInputSlotName(), kInputSlotShaderName, SlotType.Input, SlotValueType.Vector4, Vector4.zero);
        }

        protected override MaterialSlot GetOutputSlot()
        {
            return new MaterialSlot(OutputSlotId, GetOutputSlotName(), kOutputSlotShaderName, SlotType.Output, SlotValueType.Vector3, Vector4.zero);
        }

        protected override string GetInputSlotName() {return "PackedNormal"; }
        protected override string GetOutputSlotName() {return "Normal"; }

        protected override string GetFunctionName()
        {
            return "UnpackNormal";
        }
    }
}
