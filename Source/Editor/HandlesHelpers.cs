#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;

public class HandlesHelpers : MonoBehaviour
{
    public struct LabelData
    {
        public Vector3 Position;
        public string Text;
        public Color Color;
    }

    private readonly ResizableArray<LabelData> _labels = new ResizableArray<LabelData>(32);

    public void OnDrawGizmos()
    {
        for (int i = 0; i < _labels.Count; i++)
        {
            var label = _labels.Values[i];
            var style = new GUIStyle();
            var color = label.Color;
            style.normal.textColor = color;
            UnityEditor.Handles.color = color;
            UnityEditor.Handles.Label(label.Position, label.Text, style);
        }
        _labels.Clear();
    }

    private static HandlesHelpers _default;
    private static HandlesHelpers Default
    {
        get
        {
            if (_default == null)
            {
                var go = new GameObject("Unity_Handles_Dispatcher");
                _default = go.AddComponent<HandlesHelpers>();
            }
            return _default;
        }
    }

    public static void Label(Vector3 pos, string text, Color color)
    {
        Default._labels.Add(new LabelData() { Text = text, Color = color, Position = pos});
    }

    public static void Label(Vector3 pos, string text)
    {
        Label(pos, text, Color.green);
    }

    public class ResizableArray<T>
    {
        public T[] Values;
        public int Count;

        public ResizableArray(int capacity)
        {
            Values = new T[capacity];
            Count = 0;
        }

        public void Add(T item)
        {
            if (Values.Length == Count)
            {
                Array.Resize(ref Values, Values.Length << 1);
            }
            Values[Count++] = item;
        }

        public void Clear()
        {
            Count = 0;
        }
    }
}

#endif