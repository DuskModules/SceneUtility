#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DuskModules.SceneUtility.DuskEditor {

  /// <summary> Display a Scene Reference object in the editor. </summary>
  [CustomPropertyDrawer(typeof(SceneReference))]
  public class SceneReferencePropertyDrawer : PropertyDrawer {
    
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
      SerializedProperty sceneAssetProperty = property.FindPropertyRelative("sceneAsset");
      SerializedProperty scenePathProperty = property.FindPropertyRelative("scenePath");
      
      label = EditorGUI.BeginProperty(position, label, property);
      EditorGUI.BeginChangeCheck();
      
      Object selectedObject = EditorGUI.ObjectField(position, label, sceneAssetProperty.objectReferenceValue, typeof(SceneAsset), false);
      BuildUtility.BuildScene buildScene = BuildUtility.GetBuildScene(selectedObject);
      
      if (EditorGUI.EndChangeCheck()) {
        sceneAssetProperty.objectReferenceValue = selectedObject;
        
        // If the set scene was invalid, reset the path.
        if (buildScene.scene == null)
          scenePathProperty.stringValue = "";
      }

      EditorGUI.EndProperty();
    }
  }
}
#endif