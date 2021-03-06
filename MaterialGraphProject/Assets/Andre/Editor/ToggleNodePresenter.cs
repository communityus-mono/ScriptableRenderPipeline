﻿using System;
using System.Collections.Generic;
using RMGUI.GraphView;
using UnityEditor.Graphing.Drawing;
using UnityEngine;
using UnityEngine.MaterialGraph;

namespace UnityEditor.MaterialGraph.Drawing
{
    [Serializable]
    class ToggleNodeControlPresenter : GraphControlPresenter
    {
        public override void OnGUIHandler()
        {
            base.OnGUIHandler();

			var cNode = node as ToggleNode;
            if (cNode == null)
                return;

			cNode.value = EditorGUILayout.Toggle(cNode.value);
			cNode.exposedState = (PropertyNode.ExposedState)EditorGUILayout.EnumPopup(new GUIContent("Exposed"), cNode.exposedState);
        }

 /*       public override float GetHeight()
        {
            return EditorGUIUtility.singleLineHeight + 2 * EditorGUIUtility.standardVerticalSpacing;
        }
 */
        public override float GetHeight()
        {
            return 2 * (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) + EditorGUIUtility.standardVerticalSpacing;
        }
    }

    [Serializable]
    public class ToggleNodePresenter : MaterialNodePresenter
    {
        protected override IEnumerable<GraphElementPresenter> GetControlData()
        {
            var instance = CreateInstance<ToggleNodeControlPresenter>();
            instance.Initialize(node);
            return new List<GraphElementPresenter> { instance };
        }
    }
}