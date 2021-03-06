using System;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Graphing;
using UnityEngine.MaterialGraph;

namespace UnityEditor.MaterialGraph.UnitTests
{
    [TestFixture]
    public class Function1InputTests
    {
        private class Function1InputTestNode : Function1Input, IGeneratesFunction
        {
            public Function1InputTestNode()
            {
                name = "Function1InputTestNode";
            }

            protected override string GetFunctionName()
            {
                return "unity_test_" + precision;
            }

            public void GenerateNodeFunction(ShaderGenerator visitor, GenerationMode generationMode)
            {
                var outputString = new ShaderGenerator();
                outputString.AddShaderChunk(GetFunctionPrototype("arg"), false);
                outputString.AddShaderChunk("{", false);
                outputString.Indent();
                outputString.AddShaderChunk("return arg;", false);
                outputString.Deindent();
                outputString.AddShaderChunk("}", false);

                visitor.AddShaderChunk(outputString.GetShaderString(0), true);
            }
        }

        private UnityEngine.MaterialGraph.MaterialGraph m_Graph;
        private Vector1Node m_InputOne;
        private Function1InputTestNode m_TestNode;

        [TestFixtureSetUp]
        public void RunBeforeAnyTests()
        {
            Debug.unityLogger.logHandler = new ConsoleLogHandler();
        }

        [SetUp]
        public void TestSetUp()
        {
            m_Graph = new UnityEngine.MaterialGraph.MaterialGraph();
            m_InputOne = new Vector1Node();
            m_TestNode = new Function1InputTestNode();

            m_Graph.AddNode(m_InputOne);
            m_Graph.AddNode(m_TestNode);
            m_Graph.AddNode(new MetallicMasterNode());

            m_InputOne.value = 0.2f;

            m_Graph.Connect(m_InputOne.GetSlotReference(Vector1Node.OutputSlotId), m_TestNode.GetSlotReference(Function1Input.InputSlotId));
            m_Graph.Connect(m_TestNode.GetSlotReference(Function1Input.OutputSlotId), m_Graph.masterNode.GetSlotReference(MetallicMasterNode.NormalSlotId));
        }

        [Test]
        public void TestGenerateNodeCodeGeneratesCorrectCode()
        {
            string expected = string.Format("half {0} = unity_test_half ({1});{2}"
                    , m_TestNode.GetVariableNameForSlot(Function1Input.OutputSlotId)
                    , m_InputOne.GetVariableNameForSlot(Vector1Node.OutputSlotId)
                    , Environment.NewLine
                    );

            ShaderGenerator visitor = new ShaderGenerator();
            m_TestNode.GenerateNodeCode(visitor, GenerationMode.ForReals);
            Assert.AreEqual(expected, visitor.GetShaderString(0));
        }

        [Test]
        public void TestGenerateNodeFunctionGeneratesCorrectCode()
        {
            string expected =
                "inline half unity_test_half (half arg)" + Environment.NewLine
                + "{" + Environment.NewLine
                + "\treturn arg;" + Environment.NewLine
                + "}" + Environment.NewLine;

            ShaderGenerator visitor = new ShaderGenerator();
            m_TestNode.GenerateNodeFunction(visitor, GenerationMode.ForReals);
            Assert.AreEqual(expected, visitor.GetShaderString(0));
        }
    }
}
