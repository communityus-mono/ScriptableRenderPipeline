﻿using System;
using System.Collections.Generic;
using UnityEngine.MaterialGraph;
using RMGUI.GraphView;
using UnityEditor.Graphing.Drawing;

namespace UnityEditor.MaterialGraph.Drawing
{
    [Serializable]
    class ConstantsContolPresenter : GraphControlPresenter
    {

        private string[] m_ConstantTypeNames;
        private string[] constantTypeNames
        {
            get
            {
                if (m_ConstantTypeNames == null)
                    m_ConstantTypeNames = Enum.GetNames(typeof(ConstantType));
                return m_ConstantTypeNames;
            }
        }

        public override void OnGUIHandler()
        {
            base.OnGUIHandler();

            var cNode = node as UnityEngine.MaterialGraph.ConstantsNode;
            if (cNode == null)
                return;

            cNode.constant = (ConstantType)EditorGUILayout.Popup((int)cNode.constant, constantTypeNames, EditorStyles.popup);
        }

        public override float GetHeight()
        {
            return EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        }
    }

    [Serializable]
    public class ConstantsNodePresenter : MaterialNodePresenter
    {
        protected override IEnumerable<GraphElementPresenter> GetControlData()
        {
            var instance = CreateInstance<ConstantsContolPresenter>();
            instance.Initialize(node);
            return new List<GraphElementPresenter> { instance };
        }
    }
}
