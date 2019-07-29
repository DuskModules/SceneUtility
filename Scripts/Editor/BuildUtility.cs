using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.VersionControl;
using UnityEditor;
using System.Linq;

namespace DuskModules.SceneUtility {

  /// <summary> Utiltiy for build settings </summary>
  public static class BuildUtility {

    // time in seconds that we have to wait before we query again when IsReadOnly() is called.
    public static float minCheckWait = 3;

    private static float lastTimeChecked = 0;
    private static bool cachedReadonlyVal = true;

    /// <summary> A small container for tracking scene data BuildSettings  </summary>
    public struct BuildScene {
      public int buildIndex;
      public GUID assetGUID;
      public string assetPath;
      public EditorBuildSettingsScene scene;
    }

    /// <summary> Check if the build settings asset is readonly. Caches value and only queries state a max of every 'minCheckWait' seconds. </summary>
    public static bool IsReadOnly() {
      float curTime = Time.realtimeSinceStartup;
      float timeSinceLastCheck = curTime - lastTimeChecked;

      if (timeSinceLastCheck > minCheckWait) {
        lastTimeChecked = curTime;
        cachedReadonlyVal = QueryBuildSettingsStatus();
      }

      return cachedReadonlyVal;
    }
    
    /// <summary> A blocking call to the Version Control system to see if the build settings asset is readonly. </summary>
    /// <summary> Use BuildSettingsIsReadOnly for version that caches the value for better responsivenes. </summary>
    private static bool QueryBuildSettingsStatus() {
      // If no version control provider, assume not readonly
      if (Provider.enabled == false)
        return false;

      // If we cannot checkout, then assume we are not readonly
      if (Provider.hasCheckoutSupport == false)
        return false;
      
      // Try to get status for file
      Task status = Provider.Status("ProjectSettings/EditorBuildSettings.asset", false);
      status.Wait();

      // If no status listed we can edit
      if (status.assetList == null || status.assetList.Count != 1)
        return true;

      // If is checked out, we can edit
      if (status.assetList[0].IsState(Asset.States.CheckedOutLocal))
        return false;

      return true;
    }

    /// <summary> For a given Unity Scene Asset object reference, extract its build settings data, including buildIndex. </summary>
    public static BuildScene GetBuildScene(Object sceneObject) {
      BuildScene entry = new BuildScene() {
        buildIndex = -1,
        assetGUID = new GUID(string.Empty)
      };

      if (((SceneAsset)sceneObject) == null)
        return entry;

      entry.assetPath = AssetDatabase.GetAssetPath(sceneObject);
      entry.assetGUID = new GUID(AssetDatabase.AssetPathToGUID(entry.assetPath));

      for (int index = 0; index < EditorBuildSettings.scenes.Length; ++index) {
        if (entry.assetGUID.Equals(EditorBuildSettings.scenes[index].guid)) {
          entry.scene = EditorBuildSettings.scenes[index];
          entry.buildIndex = index;
          return entry;
        }
      }

      return entry;
    }

    /// <summary> Enable/Disable a given scene in the buildSettings. </summary>
    public static void SetBuildSceneState(BuildScene buildScene, bool enabled) {
      bool modified = false;
      EditorBuildSettingsScene[] scenesToModify = EditorBuildSettings.scenes;
      for (int i =0; i < scenesToModify.Length; i++) {
        EditorBuildSettingsScene curScene = scenesToModify[i];
        if (curScene.guid.Equals(buildScene.assetGUID)) {
          curScene.enabled = enabled;
          modified = true;
          break;
        }
      }
      if (modified)
        EditorBuildSettings.scenes = scenesToModify;
    }

    /// <summary> Display Dialog to add a scene to build settings  </summary>
    public static void AddBuildScene(BuildScene buildScene, bool force = false, bool enabled = true) {
      if (force == false) {
        bool selection = EditorUtility.DisplayDialog(
          "Add Scene To Build",
          "Are you sure you want to add " + buildScene.assetPath + " to the Build Settings?",
          "Yes", "No");

        if (!selection)
          return;
      }

      EditorBuildSettingsScene newScene = new EditorBuildSettingsScene(buildScene.assetGUID, enabled);
      List<EditorBuildSettingsScene> tempScenes = EditorBuildSettings.scenes.ToList();
      tempScenes.Add(newScene);
      EditorBuildSettings.scenes = tempScenes.ToArray();
    }

    /// <summary> Display Dialog to remove a scene from build settings (or just disable it) </summary>
    public static void RemoveBuildScene(BuildScene buildScene, bool force = false) {
      if (force == false) {

        bool selection = EditorUtility.DisplayDialog(
          "Add Scene To Build",
          "Are you sure you want to remove " + buildScene.assetPath + " at index " + buildScene.buildIndex + " from the Build Settings?",
          "Yes", "No");

        if (!selection)
          return;
      }
      
      List<EditorBuildSettingsScene> tempScenes = EditorBuildSettings.scenes.ToList();
      tempScenes.RemoveAll(scene => scene.guid.Equals(buildScene.assetGUID));
      EditorBuildSettings.scenes = tempScenes.ToArray();
    }

    /// <summary> Open the default Unity Build Settings window  </summary>
    public static void OpenBuildSettings() {
      EditorWindow.GetWindow(typeof(BuildPlayerWindow));
    }
  }

}