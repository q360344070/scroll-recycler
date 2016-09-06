using System;
using UnityEngine;
using UnityEngine.UI;

namespace UnityEditor.UI
{
    [CanEditMultipleObjects, CustomEditor(typeof(VerticalCellLayout), true)]
    public class VerticalCellLayoutEditor : Editor
    {
        private SerializedProperty m_cellLayout;
        private SerializedProperty m_Padding;
        private SerializedProperty m_Spacing;
        private SerializedProperty m_ChildAlignment;
        private SerializedProperty m_ChildForceExpandWidth;
        private SerializedProperty m_ChildForceExpandHeight;

        protected virtual void OnEnable()
        {
            this.m_cellLayout = base.serializedObject.FindProperty("CellLayout");
            this.m_Padding = base.serializedObject.FindProperty("m_Padding");
            this.m_Spacing = base.serializedObject.FindProperty("m_Spacing");
            this.m_ChildAlignment = base.serializedObject.FindProperty("m_ChildAlignment");
            this.m_ChildForceExpandWidth = base.serializedObject.FindProperty("m_ChildForceExpandWidth");
            this.m_ChildForceExpandHeight = base.serializedObject.FindProperty("m_ChildForceExpandHeight");
        }

        /// <summary>
        ///   <para>See Editor.OnInspectorGUI.</para>
        /// </summary>
        public override void OnInspectorGUI()
        {
            base.serializedObject.Update();
            EditorGUILayout.PropertyField(this.m_cellLayout, true, new GUILayoutOption[0]);
            EditorGUILayout.PropertyField(this.m_Padding, true, new GUILayoutOption[0]);
            EditorGUILayout.PropertyField(this.m_Spacing, true, new GUILayoutOption[0]);
            EditorGUILayout.PropertyField(this.m_ChildAlignment, true, new GUILayoutOption[0]);
            Rect rect = EditorGUILayout.GetControlRect(new GUILayoutOption[0]);
            rect = EditorGUI.PrefixLabel(rect, -1, new GUIContent("Child Force Expand"));
            rect.width = Mathf.Max(50f, (rect.width - 4f) / 3f);
            EditorGUIUtility.labelWidth = 50f;
            this.ToggleLeft(rect, this.m_ChildForceExpandWidth, new GUIContent("Width"));
            rect.x += rect.width + 2f;
            this.ToggleLeft(rect, this.m_ChildForceExpandHeight, new GUIContent("Height"));
            base.serializedObject.ApplyModifiedProperties();
        }

        private void ToggleLeft(Rect position, SerializedProperty property, GUIContent label)
        {
            bool value = property.boolValue;
            EditorGUI.showMixedValue = property.hasMultipleDifferentValues;
            EditorGUI.BeginChangeCheck();
            int indentLevel = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            value = EditorGUI.ToggleLeft(position, label, value);
            EditorGUI.indentLevel = indentLevel;
            if (EditorGUI.EndChangeCheck())
            {
                property.boolValue = (property.hasMultipleDifferentValues || !property.boolValue);
            }
            EditorGUI.showMixedValue = false;
        }
    }
}
