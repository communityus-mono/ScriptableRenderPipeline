using NUnit.Framework;
using UnityEngine;
using UnityEngine.Graphing;
using UnityEngine.MaterialGraph;

namespace UnityEditor.MaterialGraph.UnitTests
{
    [TestFixture]
    public class ShaderGeneratorTests
    {
        [TestFixtureSetUp]
        public void RunBeforeAnyTests()
        {
            Debug.unityLogger.logHandler = new ConsoleLogHandler();
        }

        class TestNode : AbstractMaterialNode
        {
            public const int V1Out = 0;
            public const int V2Out = 1;
            public const int V3Out = 2;
            public const int V4Out = 3;

            public TestNode()
            {
                AddSlot(new MaterialSlot(V1Out, "V1Out", "V1Out", SlotType.Output, SlotValueType.Vector1, Vector4.zero));
                AddSlot(new MaterialSlot(V2Out, "V2Out", "V2Out", SlotType.Output, SlotValueType.Vector2, Vector4.zero));
                AddSlot(new MaterialSlot(V3Out, "V3Out", "V3Out", SlotType.Output, SlotValueType.Vector3, Vector4.zero));
                AddSlot(new MaterialSlot(V4Out, "V4Out", "V4Out", SlotType.Output, SlotValueType.Vector4, Vector4.zero));
            }
        }

        [Test]
        public void AdaptNodeOutput1To1Works()
        {
            var node = new TestNode();
            var result = ShaderGenerator.AdaptNodeOutput(node, TestNode.V1Out, ConcreteSlotValueType.Vector1);
            Assert.AreEqual(string.Format("{0}", node.GetVariableNameForSlot(TestNode.V1Out)), result);
        }

        [Test]
        public void AdaptNodeOutput1To2Works()
        {
            var node = new TestNode();
            var result = ShaderGenerator.AdaptNodeOutput(node, TestNode.V1Out, ConcreteSlotValueType.Vector2);
            Assert.AreEqual(string.Format("({0})", node.GetVariableNameForSlot(TestNode.V1Out)), result);
        }

        [Test]
        public void AdaptNodeOutput1To3Works()
        {
            var node = new TestNode();
            var result = ShaderGenerator.AdaptNodeOutput(node, TestNode.V1Out, ConcreteSlotValueType.Vector3);
            Assert.AreEqual(string.Format("({0})", node.GetVariableNameForSlot(TestNode.V1Out)), result);
        }

        [Test]
        public void AdaptNodeOutput1To4Works()
        {
            var node = new TestNode();
            var result = ShaderGenerator.AdaptNodeOutput(node, TestNode.V1Out, ConcreteSlotValueType.Vector4);
            Assert.AreEqual(string.Format("({0})", node.GetVariableNameForSlot(TestNode.V1Out)), result);
        }

        [Test]
        public void AdaptNodeOutput2To1Works()
        {
            var node = new TestNode();
            var result = ShaderGenerator.AdaptNodeOutput(node, TestNode.V2Out, ConcreteSlotValueType.Vector1);
            Assert.AreEqual(string.Format("({0}).x", node.GetVariableNameForSlot(TestNode.V2Out)), result);
        }

        [Test]
        public void AdaptNodeOutput2To2Works()
        {
            var node = new TestNode();
            var result = ShaderGenerator.AdaptNodeOutput(node, TestNode.V2Out, ConcreteSlotValueType.Vector2);
            Assert.AreEqual(string.Format("{0}", node.GetVariableNameForSlot(TestNode.V2Out)), result);
        }

        [Test]
        public void AdaptNodeOutput2To3Fails()
        {
            var node = new TestNode();
            var result = ShaderGenerator.AdaptNodeOutput(node, TestNode.V2Out, ConcreteSlotValueType.Vector3);
            Assert.AreEqual("ERROR!", result);
        }

        [Test]
        public void AdaptNodeOutput2To4Fails()
        {
            var node = new TestNode();
            var result = ShaderGenerator.AdaptNodeOutput(node, TestNode.V2Out, ConcreteSlotValueType.Vector4);
            Assert.AreEqual("ERROR!", result);
        }

        [Test]
        public void AdaptNodeOutput3To1Works()
        {
            var node = new TestNode();
            var result = ShaderGenerator.AdaptNodeOutput(node, TestNode.V3Out, ConcreteSlotValueType.Vector1);
            Assert.AreEqual(string.Format("({0}).x", node.GetVariableNameForSlot(TestNode.V3Out)), result);
        }

        [Test]
        public void AdaptNodeOutput3To2Works()
        {
            var node = new TestNode();
            var result = ShaderGenerator.AdaptNodeOutput(node, TestNode.V3Out, ConcreteSlotValueType.Vector2);
            Assert.AreEqual(string.Format("({0}.xy)", node.GetVariableNameForSlot(TestNode.V3Out)), result);
        }

        [Test]
        public void AdaptNodeOutput3To3Works()
        {
            var node = new TestNode();
            var result = ShaderGenerator.AdaptNodeOutput(node, TestNode.V3Out, ConcreteSlotValueType.Vector3);
            Assert.AreEqual(string.Format("{0}", node.GetVariableNameForSlot(TestNode.V3Out)), result);
        }

        [Test]
        public void AdaptNodeOutput3To4Fails()
        {
            var node = new TestNode();
            var result = ShaderGenerator.AdaptNodeOutput(node, TestNode.V3Out, ConcreteSlotValueType.Vector4);
            Assert.AreEqual("ERROR!", result);
        }

        [Test]
        public void AdaptNodeOutput4To1Works()
        {
            var node = new TestNode();
            var result = ShaderGenerator.AdaptNodeOutput(node, TestNode.V4Out, ConcreteSlotValueType.Vector1);
            Assert.AreEqual(string.Format("({0}).x", node.GetVariableNameForSlot(TestNode.V4Out)), result);
        }

        [Test]
        public void AdaptNodeOutput4To2Works()
        {
            var node = new TestNode();
            var result = ShaderGenerator.AdaptNodeOutput(node, TestNode.V4Out, ConcreteSlotValueType.Vector2);
            Assert.AreEqual(string.Format("({0}.xy)", node.GetVariableNameForSlot(TestNode.V4Out)), result);
        }

        [Test]
        public void AdaptNodeOutput4To3Works()
        {
            var node = new TestNode();
            var result = ShaderGenerator.AdaptNodeOutput(node, TestNode.V4Out, ConcreteSlotValueType.Vector3);
            Assert.AreEqual(string.Format("({0}.xyz)", node.GetVariableNameForSlot(TestNode.V4Out)), result);
        }

        [Test]
        public void AdaptNodeOutput4To4Works()
        {
            var node = new TestNode();
            var result = ShaderGenerator.AdaptNodeOutput(node, TestNode.V4Out, ConcreteSlotValueType.Vector4);
            Assert.AreEqual(string.Format("{0}", node.GetVariableNameForSlot(TestNode.V4Out)), result);
        }
        
        [Test]
        public void AdaptNodeOutput1To4PreviewWorks()
        {
            var node = new TestNode();
            var result = ShaderGenerator.AdaptNodeOutputForPreview(node, TestNode.V1Out);
            Assert.AreEqual(string.Format("half4({0}, {0}, {0}, 1.0)", node.GetVariableNameForSlot(TestNode.V1Out)), result);
        }
        
        [Test]
        public void AdaptNodeOutput2To4PreviewWorks()
        {
            var node = new TestNode();
            var expected = string.Format("half4({0}.x, {0}.y, 0.0, 1.0)", node.GetVariableNameForSlot(TestNode.V2Out));
            var result = ShaderGenerator.AdaptNodeOutputForPreview(node, TestNode.V2Out);
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void AdaptNodeOutput3To4PreviewWorks()
        {
            var node = new TestNode();
            var expected = string.Format("half4({0}.x, {0}.y, {0}.z, 1.0)", node.GetVariableNameForSlot(TestNode.V3Out));
            var result = ShaderGenerator.AdaptNodeOutputForPreview(node, TestNode.V3Out);
            Assert.AreEqual(expected, result);
        }
        
        [Test]
        public void AdaptNodeOutput4To4PreviewWorks()
        {
            var node = new TestNode();
            var expected = string.Format("half4({0}.x, {0}.y, {0}.z, 1.0)", node.GetVariableNameForSlot(TestNode.V4Out));
            var result = ShaderGenerator.AdaptNodeOutputForPreview(node, TestNode.V4Out);
            Assert.AreEqual(expected, result);
        }
    }
}
