using System.Collections.Generic;
using EscapeWall.Items;
using UnityEditor;
using UnityEngine;

namespace EscapeWallEditor.Animation
{
    [CustomPropertyDrawer(typeof(TransformAnimation))]
    public class TransformAnimationDrawer : PropertyDrawer
    {
        private static readonly Dictionary<string, int> PreviewIndices = new();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property == null || property.serializedObject == null || property.serializedObject.targetObject == null)
                return;
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            EditorGUI.BeginProperty(position, label, property);

            property.isExpanded = EditorGUI.Foldout(
                new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight),
                property.isExpanded, label, true);

            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;

                float y = position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

                SerializedProperty targetProp = property.FindPropertyRelative("target");
                EditorGUI.PropertyField(new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight), targetProp);
                y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

                SerializedProperty keysProp = property.FindPropertyRelative("keys");
                float keysHeight = EditorGUI.GetPropertyHeight(keysProp, true);
                EditorGUI.PropertyField(new Rect(position.x, y, position.width, keysHeight), keysProp, true);
                y += keysHeight + EditorGUIUtility.standardVerticalSpacing;

                SerializedProperty loopProp = property.FindPropertyRelative("loop");
                EditorGUI.PropertyField(new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight), loopProp);
                y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

                SerializedProperty pingPongProp = property.FindPropertyRelative("pingPong");
                EditorGUI.PropertyField(new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight), pingPongProp);
                y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

                int currentIndex = GetCurrentIndex(property);
                int keyCount = keysProp.arraySize;
                string indexText = keyCount == 0 ? "Current Key: (none)" : $"Current Key: {currentIndex} / {keyCount - 1}";

                EditorGUI.LabelField(new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight), indexText);
                y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

                float buttonWidth = (position.width - 8f) / 5f;

                Rect firstFrameRect = new Rect(position.x, y, buttonWidth, EditorGUIUtility.singleLineHeight);
                Rect prevRect = new Rect(position.x + buttonWidth + 2f, y, buttonWidth, EditorGUIUtility.singleLineHeight);
                Rect addRect = new Rect(position.x + (buttonWidth + 2f) * 2, y, buttonWidth, EditorGUIUtility.singleLineHeight);
                Rect nextRect = new Rect(position.x + (buttonWidth + 2f) * 3, y, buttonWidth, EditorGUIUtility.singleLineHeight);
                Rect lastFrameRect = new Rect(position.x + (buttonWidth + 2f) * 4, y, buttonWidth, EditorGUIUtility.singleLineHeight);

                if (GUI.Button(firstFrameRect, "|◀"))
                    GotoToStart(property);

                if (GUI.Button(prevRect, "◀◀"))
                    CycleKey(property, -1);

                if (GUI.Button(addRect, "+"))
                    AddCurrentKey(property);

                if (GUI.Button(nextRect, "▶▶"))
                    CycleKey(property, 1);

                if (GUI.Button(lastFrameRect, "▶|"))
                    GotoToEnd(property);

                y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

                float playButtonWidth = (position.width - 4f) * 0.5f;
                Rect playRect = new Rect(position.x, y, playButtonWidth, EditorGUIUtility.singleLineHeight);
                Rect stopRect = new Rect(position.x + playButtonWidth + 4f, y, playButtonWidth, EditorGUIUtility.singleLineHeight);

                string path = property.propertyPath;
                bool playing = TransformAnimationPreview.IsPlaying(path);

                GUI.enabled = !playing;
                if (GUI.Button(playRect, "Play"))
                    StartPreview(property);
                GUI.enabled = true;

                GUI.enabled = playing;
                if (GUI.Button(stopRect, "Stop"))
                    TransformAnimationPreview.Stop(path);
                GUI.enabled = true;

                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property == null || property.serializedObject == null || property.serializedObject.targetObject == null)
                return EditorGUIUtility.singleLineHeight;

            if (!property.isExpanded)
                return EditorGUIUtility.singleLineHeight;

            SerializedProperty keysProp = property.FindPropertyRelative("keys");
            float height = EditorGUIUtility.singleLineHeight;
            height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            height += EditorGUI.GetPropertyHeight(keysProp, true) + EditorGUIUtility.standardVerticalSpacing;
            height += EditorGUIUtility.singleLineHeight * 2 + EditorGUIUtility.standardVerticalSpacing * 2;
            height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            return height;
        }

        private int GetCurrentIndex(SerializedProperty property)
        {
            string path = property.propertyPath;
            if (!PreviewIndices.TryGetValue(path, out int index))
                index = 0;

            SerializedProperty keysProp = property.FindPropertyRelative("keys");
            if (keysProp.arraySize == 0)
                return 0;

            return Mathf.Clamp(index, 0, keysProp.arraySize - 1);
        }

        private void SetCurrentIndex(SerializedProperty property, int index)
        {
            PreviewIndices[property.propertyPath] = index;
        }

        private void CycleKey(SerializedProperty property, int direction)
        {
            SerializedProperty keysProp = property.FindPropertyRelative("keys");
            int count = keysProp.arraySize;
            if (count == 0) return;

            int index = GetCurrentIndex(property);
            index = (index + direction + count) % count;

            SetCurrentIndex(property, index);
            ApplyKeyToTransform(property, index);
        }

        private Transform GetTargetTransform(SerializedProperty property)
        {
            SerializedProperty targetProp = property.FindPropertyRelative("target");
            if (targetProp.objectReferenceValue is Transform t)
                return t;

            Object owner = property.serializedObject.targetObject;
            if (owner is Component component)
                return component.transform;
            if (owner is GameObject go)
                return go.transform;

            return null;
        }

        private void ApplyKeyToTransform(SerializedProperty property, int index)
        {
            SerializedProperty keysProp = property.FindPropertyRelative("keys");
            if (index < 0 || index >= keysProp.arraySize) return;

            Transform transform = GetTargetTransform(property);
            if (transform == null) return;

            SerializedProperty key = keysProp.GetArrayElementAtIndex(index);
            Undo.RecordObject(transform, "Preview Key");

            transform.localPosition = key.FindPropertyRelative("localPosition").vector3Value;
            transform.localRotation = key.FindPropertyRelative("localRotation").quaternionValue;

            EditorUtility.SetDirty(transform);
        }

        private void AddCurrentKey(SerializedProperty property)
        {
            Transform transform = GetTargetTransform(property);

            if (transform == null)
            {
                Debug.LogWarning("No target Transform assigned.");
                return;
            }

            SerializedProperty keysProp = property.FindPropertyRelative("keys");
            int currentIndex = GetCurrentIndex(property);
            int insertIndex = keysProp.arraySize == 0 ? 0 : currentIndex + 1;

            keysProp.InsertArrayElementAtIndex(insertIndex);

            SerializedProperty newKey = keysProp.GetArrayElementAtIndex(insertIndex);
            newKey.FindPropertyRelative("localPosition").vector3Value = transform.localPosition;
            newKey.FindPropertyRelative("localRotation").quaternionValue = transform.localRotation;
            newKey.FindPropertyRelative("duration").floatValue = 1f;

            property.serializedObject.ApplyModifiedProperties();
            SetCurrentIndex(property, insertIndex);
        }

        private void GotoToStart(SerializedProperty property)
        {
            SerializedProperty keysProp = property.FindPropertyRelative("keys");
            if (keysProp.arraySize == 0) return;

            ApplyKeyToTransform(property, 0);
            SetCurrentIndex(property, 0);
        }

        private void GotoToEnd(SerializedProperty property)
        {
            SerializedProperty keysProp = property.FindPropertyRelative("keys");
            if (keysProp.arraySize == 0) return;

            ApplyKeyToTransform(property, keysProp.arraySize - 1);
            SetCurrentIndex(property, keysProp.arraySize - 1);
        }

        private void StartPreview(SerializedProperty property)
        {
            SerializedProperty keysProp = property.FindPropertyRelative("keys");
            if (keysProp.arraySize < 2) return;

            Transform transform = GetTargetTransform(property);
            if (transform == null) return;

            var keys = new TransformAnimation.KeyPoint[keysProp.arraySize];
            for (int i = 0; i < keys.Length; i++)
            {
                var keyProp = keysProp.GetArrayElementAtIndex(i);
                keys[i] = new TransformAnimation.KeyPoint
                {
                    localPosition = keyProp.FindPropertyRelative("localPosition").vector3Value,
                    localRotation = keyProp.FindPropertyRelative("localRotation").quaternionValue,
                    duration = keyProp.FindPropertyRelative("duration").floatValue
                };
            }

            TransformAnimationPreview.Play(
                property.propertyPath,
                transform,
                keys,
                property.FindPropertyRelative("loop").boolValue,
                property.FindPropertyRelative("pingPong").boolValue
            );
        }
    }
}