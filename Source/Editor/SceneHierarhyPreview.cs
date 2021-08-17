namespace Nanory.Unity.Entities.BehaviorTree
{
#if UNITY_EDITOR 
    using UnityEngine;
    using UnityEditor;
    using System;
    using System.Reflection;
    using System.Collections.Generic;

    [InitializeOnLoad]
    class BTSceneHierarhyPreview
    {
        const string IgnoreIcons = "d_Prefab Icon, d_GameObject Icon";

        static BTSceneHierarhyPreview()
        {
            EditorApplication.hierarchyWindowItemOnGUI += HierarchyItemCBNew;
            
        }

        static void HierarchyItemCBNew(int instanceID, Rect selectionRect)
        {
            var go = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
            var content = EditorGUIUtility.ObjectContent(go, null);
            if (content.image != null && !IgnoreIcons.Contains(content.image.name))
            {
                var size = 16;
                var backgroundColor = new Color32(56, 56, 56, 255);
                Texture2D _backroundTexture;
                _backroundTexture = new Texture2D(size, size);
                for (int i = 0; i < size; i++)
                    for (int j = 0; j < size; j++)
                        _backroundTexture.SetPixel(i, j, backgroundColor);

                _backroundTexture.Apply();

                var offset = go.transform.childCount > 0 ? 30 : 18;
                GUI.DrawTexture(new Rect(selectionRect.xMin - 2, selectionRect.yMin, 16, 16), _backroundTexture);
                GUI.DrawTexture(new Rect(selectionRect.xMin, selectionRect.yMin, 14, 14), content.image); 
            }
        }

        public static void AssignLabel(GameObject g, Texture2D label)
        {
            Type editorGUIUtilityType = typeof(EditorGUIUtility);
            BindingFlags bindingFlags = BindingFlags.InvokeMethod | BindingFlags.Static | BindingFlags.NonPublic;
            object[] args = new object[] { g, label };
            editorGUIUtilityType.InvokeMember("SetIconForObject", bindingFlags, null, null, args);
        }
    }
#endif

}

