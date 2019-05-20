using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace DuskModules.SceneUtility {

	/// <summary> Object storing a scene path, which can be set by setting a scene asset reference.</summary>
	[System.Serializable]
	public class SceneReference : ISerializationCallbackReceiver {

		[SerializeField]
		private string _scenePath = "";

		/// <summary> Asset path leading to the scene </summary>
		public string scenePath {
			get {
#if UNITY_EDITOR
				// In editor, the asset is always known. Fetch the path using the asset reference.
				return GetPathFromAsset();
#else
      // At runtime, the asset reference isn't set, so just use the stored path value.
      return _scenePath;
#endif
			}
			set {
				_scenePath = value;
#if UNITY_EDITOR
				sceneAsset = GetAssetFromPath();
#endif
			}
		}

		[SerializeField]
		private string _sceneName;
		/// <summary> Name of the scene, can be used to load the scene </summary>
		public string sceneName => _sceneName;

		/// <summary> Build index of the scene, can be used to load the scene. </summary>
		public int buildIndex => UnityEngine.SceneManagement.SceneUtility.GetBuildIndexByScenePath(scenePath);

		/// <summary> Before serializing data of this object, call method if in editor. </summary>
		public void OnBeforeSerialize() {
#if UNITY_EDITOR
			BeforeSerialize();
#endif
		}

		/// <summary> Sets up the data of this object after deserialization, with method if in editor. </summary>
		public void OnAfterDeserialize() {
#if UNITY_EDITOR
			EditorApplication.update += AfterDeserialize;
#endif
		}


#if UNITY_EDITOR
		[SerializeField]
		private Object sceneAsset = null;
		bool sceneAssetIsSet {
			get {
				if (sceneAsset == null) return false;
				return sceneAsset.GetType().Equals(typeof(SceneAsset));
			}
		}

		private void BeforeSerialize() {
			// Asset has not been set, but path is known. Get asset with the path.
			if (!sceneAssetIsSet && _scenePath != null && _scenePath != "") {
				sceneAsset = GetAssetFromPath();
				if (sceneAsset == null) _scenePath = "";

				// Set scene name
				_sceneName = null;
				if (sceneAsset != null) {
					string path = scenePath.Substring(scenePath.LastIndexOf('/') + 1);
					_sceneName = path.Substring(0, path.LastIndexOf('.'));
				}

				EditorSceneManager.MarkAllScenesDirty();
			}
			else {
				// If asset has been set, just use that.
				_scenePath = GetPathFromAsset();
			}
		}

		private void AfterDeserialize() {
			EditorApplication.update -= AfterDeserialize;

			// Asset has already been set, and will be used in editor instead of the path.
			if (sceneAssetIsSet)
				return;

			// Asset has not been set, so attempt to find the asset with the known path
			if (_scenePath != null && _scenePath != "") {
				sceneAsset = GetAssetFromPath();

				// If no scene asset found, reset the scene path to avoid using outdated paths.
				if (sceneAsset == null)
					_scenePath = "";

				// Set scene name
				_sceneName = null;
				if (sceneAsset != null) {
					string path = scenePath.Substring(scenePath.LastIndexOf('/') + 1);
					_sceneName = path.Substring(0, path.LastIndexOf('.'));
				}

				if (Application.isPlaying == false)
					EditorSceneManager.MarkAllScenesDirty();
			}
		}

		private SceneAsset GetAssetFromPath() {
			if (_scenePath == null || _scenePath == "") return null;
			return AssetDatabase.LoadAssetAtPath<SceneAsset>(_scenePath);
		}

		private string GetPathFromAsset() {
			if (sceneAsset == null) return "";
			return AssetDatabase.GetAssetPath(sceneAsset);
		}
#endif
	}
}