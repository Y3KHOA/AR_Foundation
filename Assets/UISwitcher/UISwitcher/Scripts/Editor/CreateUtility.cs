using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
namespace UISwitcher {
	public static class CreateUtility {
		private const string CANVAS_PREFAB_NAME_IN_RESOURCES = "Canvas";
		public static void CreateUIElement(string path) {
			var selectedObject = Selection.activeGameObject;

			if (selectedObject != null) {
				var hasCanvasIsParent = selectedObject.GetComponentInParent<Canvas>();
				//If has parent use he, or create new canvas, and use new parent canvas
				var parent = hasCanvasIsParent ? selectedObject.transform : CreateAndPlace(CANVAS_PREFAB_NAME_IN_RESOURCES, selectedObject.transform).transform;

				CreateAndPlace(path, parent);
			}
			else {
				var canvas = CreateAndPlace(CANVAS_PREFAB_NAME_IN_RESOURCES);
				CreateAndPlace(path, canvas.transform);
			}
		}

		private static GameObject CreateAndPlace(string path, Transform parent = null) {
			var gameObject = Object.Instantiate(Resources.Load(path), parent) as GameObject;
			if (gameObject == null)
				throw new Exception($"Object {path} not found in Resources");
			gameObject.name = path;
			Place(gameObject);
			return gameObject;
		}

		private static void Place(GameObject gameObject) {
		//	SceneView lastView = SceneView.lastActiveSceneView;

		//	gameObject.transform.position = /*lastView ? lastView.pivot : */Vector3.zero;

			StageUtility.PlaceGameObjectInCurrentStage(gameObject);
			GameObjectUtility.EnsureUniqueNameForSibling(gameObject);

			Undo.RegisterCreatedObjectUndo(gameObject, $"Create object: {gameObject.name}");
			Selection.activeGameObject = gameObject;

			if (!EditorApplication.isPlaying)
				EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
		}
	}
}
