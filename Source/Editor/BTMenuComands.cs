#if UNITY_EDITOR
namespace Nanory.Unity.Entities.BehaviorTree
{
    using UnityEditor;
    using UnityEngine;

    public static class BTMenuComands
    {
        private const string basePath = "Assets/Plugins/Gamefirst/UnityEcsHelpers/BehaviorTree/Editor/Thumbnails/";

        [MenuItem("GameObject/Behavior Tree/Selector", priority = -10, validate = false)]
        public static void CreateSelector()
        {
            var nodeGO = CreateNode<BTSelectorTagAuthoring>();
            nodeGO.name = "Selector";
            BTSceneHierarhyPreview.AssignLabel(nodeGO, AssetDatabase.LoadAssetAtPath<Texture2D>(basePath + "QuestionMark.png"));
        }

        [MenuItem("GameObject/Behavior Tree/Selector", priority = -10, validate = true)]
        public static bool CreateSelectorValidate() => Selection.activeGameObject != null;

        public static GameObject CreateNode<TNodeAuthoring>() where TNodeAuthoring : MonoBehaviour
        {
            if (Selection.activeGameObject != null)
            {
                var node = new GameObject();
                node.transform.SetParent(Selection.activeGameObject.transform, false);
                node.AddComponent<TNodeAuthoring>();
                node.name = nameof(TNodeAuthoring);
                return node;
            }
            return null;
        }
    }
}
#endif
