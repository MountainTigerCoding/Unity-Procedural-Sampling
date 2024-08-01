using UnityEditor;
using Editors.Shared;

namespace Editors.ProceduralSampling
{
    public static class ProcedualSamplingEditorUtils
    {
        public static void PaintNoiseSettings (SerializedObject serializedObject, string path, string name)
        {
            SerializedProperty property = serializedObject.FindProperty(path);
            EditorGUIUtils.PaintField(property, inspectorName: name);
        }
    }
}