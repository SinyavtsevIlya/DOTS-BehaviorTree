#if UNITY_EDITOR
namespace Nanory.Unity.Entities.BehaviorTree
{
    using UnityEditor;
    using UnityEngine;

    public static class BTMenuComands
    {
        private const string PackageName = "com.nanory.unity.entities.bt";

        private const string basePath = "Packages/" + PackageName + "/Source/Editor/Thumbnails/";

        [MenuItem("GameObject/Behavior Tree/Repeater", priority = -10, validate = false)]
        public static void CreateRepeater()
        {
            CreateNode<BTNodeProcessAuthoring>().name = "Repeater";
        }

        [MenuItem("GameObject/Behavior Tree/Repeater", priority = -10, validate = true)]
        public static bool CreateRepeaterValidate() => Selection.activeGameObject != null;

        [MenuItem("GameObject/Behavior Tree/Selector", priority = -10, validate = false)]
        public static void CreateSelector()
        {
            CreateNode<BTSelectorTagAuthoring>().name = "Selector";
        }

        [MenuItem("GameObject/Behavior Tree/Selector", priority = -10, validate = true)]
        public static bool CreateSelectorValidate() => Selection.activeGameObject != null;


        [MenuItem("GameObject/Behavior Tree/Sequence", priority = -10, validate = false)]
        public static void CreateSequence()
        {
            CreateNode<BTSequenceAuthoring>().name = "Sequence";
        }

        [MenuItem("GameObject/Behavior Tree/Sequence", priority = -10, validate = true)]
        public static bool CreateSequenceValidate() => Selection.activeGameObject != null;

        [MenuItem("GameObject/Behavior Tree/Root", priority = -10, validate = false)]
        public static void CreateRoot()
        {
            var authoring = new GameObject().AddComponent<BTNodeProcessAuthoring>();
            authoring.name = "BtRoot";
            BTSceneHierarhyPreview.AssignLabel(authoring.gameObject, authoring.DefaultThumbnail);
            Selection.activeGameObject = authoring.gameObject;
        }

        [MenuItem("GameObject/Behavior Tree/Root", priority = -10, validate = true)]
        public static bool CreateRootValidate() => Selection.activeGameObject == null;

        public static GameObject CreateNode<TNodeAuthoring>() where TNodeAuthoring : BTCompositeNodeAuthoring
        {
            if (Selection.activeGameObject != null)
            {
                var node = new GameObject();
                node.transform.SetParent(Selection.activeGameObject.transform, false);
                var authoring = node.AddComponent<TNodeAuthoring>();
                node.name = typeof(TNodeAuthoring).Name;
                BTSceneHierarhyPreview.AssignLabel(node, authoring.DefaultThumbnail);
                Selection.activeGameObject = node.gameObject;
                
                return node;
            }
            return null;
        }
    }
}
#endif
