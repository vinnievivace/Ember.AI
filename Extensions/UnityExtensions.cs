using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using EmberAI.Core.Util;
using Object = UnityEngine.Object;

namespace EmberAI.Core
{
	/// <summary>
	/// Various Extension Methods for core Unity classes
	/// </summary>
	public static class UnityExtensions
	{
		#region Int ........................................................................................................

		/// <summary>
		/// Restricts value to supplied min/max
		/// </summary>
		/// <param name="value"></param>
		/// <param name="min"></param>
		/// <param name="max"></param>
		public static void Clamp(this int value, int min, int max)
		{
			if (value < min)
				value = min;

			if (value > max)
				value = max;

		}

		#endregion

		#region Float .......................................................................................................

		/// <summary>
		/// Adds or subtracts delta time to a float based on a bool
		/// </summary>
		/// <param name="timer"></param>
		/// <param name="add"></param>
		public static void IncrementClampedDeltaTime(this ref float timer, bool add)
		{
			timer = add ? Mathf.Clamp01(timer + Time.deltaTime) : Mathf.Clamp01(timer - Time.deltaTime);
		}

		#endregion

		#region List

		/// <summary>
		/// Returns a List of Lists chunked into the specified count
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="source">The original list you want to chunk</param>
		/// <param name="chunkSize">The size of the lists that should be returned</param>
		/// <returns></returns>
		public static List<List<T>> ChunkBy<T>(this List<T> source, int chunkSize)
		{
			return source
				.Select((x, i) => new { Index = i, Value = x })
				.GroupBy(x => x.Index / chunkSize)
				.Select(x => x.Select(v => v.Value).ToList())
				.ToList();
		}
		
		public static void AdjustLength(this List<string> list, int newLength)
		{
			if (list == null) throw new ArgumentNullException(nameof(list));
			if (newLength < 0) throw new ArgumentOutOfRangeException(nameof(newLength), "New length must be non-negative.");

			int currentLength = list.Count;

			if (newLength < currentLength)
			{
				// Truncate the list
				list.RemoveRange(newLength, currentLength - newLength);
			}
			else if (newLength > currentLength)
			{
				// Extend the list with default values (null)
				list.AddRange(new string[newLength - currentLength]);
			}
		}

		#endregion

		#region GameObject / Component .....................................................................................

		/// <summary>
		/// Asserts that the supplied component is defined, if not, an exception is thrown
		/// </summary>
		/// <param name="component"></param>
		private static void AssertDependencyDefined<T>(this Component child, T component) where T : Object
		{
			if (component != null) return;

			var exception = new Exception("Required Component " + typeof(T).Name + " not defined on component: " + child.name);

			throw exception;
		}

		/// <summary>
		/// Asserts that the supplied components are defined, if not, an exception is thrown
		/// </summary>
		/// <param name="component"></param>
		public static void AssertDependenciesDefined(this Component child, params UnityEngine.Object[] components)
		{
			foreach (var component in components)
			{
				child.AssertDependencyDefined(component);
			}
		}


		public static void SetLayerRecursively(this GameObject go, int layer)
		{
			go.layer = layer;
			Transform t = go.transform;
			for (int i = 0; i < t.childCount; i++)
				SetLayerRecursively(t.GetChild(i).gameObject, layer);
		}

		/// <summary>
		/// Returns true if the supplied Component is a prefab, not an instance
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public static bool IsPrefab(this Component obj)
		{
			return obj.gameObject.scene.rootCount == 0;
		}
		
		/// <summary>
		/// Returns the enabled component of the specified type from the supplied GameObject.
		/// </summary>
		/// <typeparam name="T">The type of the component to retrieve.</typeparam>
		/// <param name="gameObject">The GameObject to search for the component.</param>
		/// <returns>The enabled component of the specified type, or null if no such component is found.</returns>
		public static T GetEnabledComponent<T>(this GameObject gameObject) where T : Component
		{
		    T component = gameObject.GetComponent<T>();

		    if (component == null) return null;
		    
		    if (component is Behaviour behaviour)
		    {
		        return behaviour.enabled ? component : null;
		    }
		    if (component is Renderer renderer)
		    {
		        return renderer.enabled ? component : null;
		    }
		    return null;
		}
	
		public static T GetOrAddComponent<T>(this Component child, bool findInChildren = false) where T : Component
		{
			T result;

			if (findInChildren)
			{
				result = child.GetComponentInChildren<T>();
			}
			else
			{
				result = child.GetComponent<T>();
			}

			if (result == null)
			{
				result = child.gameObject.AddComponent<T>();
			}
			return result;
		}

		public static T GetOrAddComponent<T>(this GameObject child, bool findInChildren = false) where T : Component
		{
			T result;

			if (findInChildren)
			{
				result = child.GetComponentInChildren<T>();
			}
			else
			{
				result = child.GetComponent<T>();
			}

			if (result == null)
			{
				result = child.gameObject.AddComponent<T>();
			}
			return result;
		}

		/// <summary>
		/// Returns a List of <T> Children Components with names that contain the supplied keyword 
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="keyword"></param>
		/// <param name="includeInactive"></param>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static List<T> GetChildNameContains<T>(this Component parent, string keyword, bool includeInactive) where T : Component
		{
			keyword = keyword.ToLower();

			return parent.GetComponentsInChildren<T>(includeInactive).Where(comp => comp.gameObject.name.ToLower().Contains(keyword)).ToList();
		}
		
		/// <summary>
		/// Returns the first Child of Type <see cref="T"/> with name that contains the supplied keyword
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="keyword"></param>
		/// <param name="includeInactive"></param>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static T GetFirstChildNameContains<T>(this Component parent, string keyword, bool includeInactive) where T : Component
		{
			keyword = keyword.ToLower();
			
			return parent.GetComponentsInChildren<T>(includeInactive).FirstOrDefault(comp => comp.gameObject.name.ToLower().Contains(keyword));
		}
		
		public static GameObject GetChild(this GameObject parent, string name, bool includeInactive)
		{
			return (from child in parent.GetComponentsInChildren<Transform>(includeInactive) where child.name == name select child.gameObject).FirstOrDefault();
		}

		public static GameObject GetChild(this Component parent, string name, bool includeInactive)
		{
			return GetChild(parent.gameObject, name, includeInactive);
		}


		public static GameObject GetOrAddChild(this GameObject parent, string childName, bool includeInactive = false)
		{
			var existing = GetChild(parent, childName, includeInactive);

			if (existing != null) return existing;

			var newChild = new GameObject(childName);

			newChild.transform.parent = parent.transform;

			return newChild;
		}

		public static GameObject GetOrAddChild(this Component parent, string childName, bool includeInactive = true)
		{
			return GetOrAddChild(parent.gameObject, childName, includeInactive);
		}

		public static void RemoveAllChildren(this GameObject parent)
		{
			RemoveAllChildren(parent.transform);
		}

		public static void RemoveAllChildren(Transform parent)
		{
			for (int i = parent.childCount - 1; i >= 0; i--)
			{
				GameObject child = parent.GetChild(i).gameObject;
				Object.Destroy(child);
			}
		}

		public static void SetParent(this GameObject target, Component parent)
		{
			SetParent(target.transform, parent.gameObject);
		}

		public static void SetParent(this Component target, Component parent)
		{
			SetParent(target, parent.gameObject);
		}

		public static void SetParent(this GameObject target, GameObject parent)
		{
			SetParent(target.transform, parent);
		}

		public static void SetParent(this Component target, GameObject parent)
		{
			target.transform.parent = parent.transform;

			target.ResetLocalTransform();
		}

		/// <summary>
		/// Update transform (world coordinates)
		/// </summary>
		/// <param name="target"></param>
		/// <param name="position"></param>
		/// <param name="rotationEuler"></param>
		public static void SetTransform(this GameObject target, Vector3 position, Vector3 rotationEuler)
		{
			SetTransform(target, position, Quaternion.Euler(rotationEuler));
		}

		/// <summary>
		/// Update transform (world coordinates)
		/// </summary>
		/// <param name="target"></param>
		/// <param name="position"></param>
		/// <param name="rotation"></param>
		public static void SetTransform(this GameObject target, Vector3 position, Quaternion rotation)
		{
			target.transform.position = position;
			target.transform.rotation = rotation;
		}

		/// <summary>
		/// Reset local transform to Vector3.zero and optionally, scale to 1
		/// </summary>
		/// <param name="target"></param>
		/// <param name="resetScale"></param>
		public static void ResetLocalTransform(this GameObject target, bool resetScale = false)
		{
			target.transform.localPosition = Vector3.zero;
			target.transform.localRotation = Quaternion.identity;

			if (resetScale)
			{
				target.transform.localScale = Vector3.one;
			}
		}

		/// <summary>
		/// Reset local transform to Vector3.zero and optionally, scale to 1
		/// </summary>
		/// <param name="target"></param>
		/// <param name="resetScale"></param>
		public static void ResetLocalTransform(this MonoBehaviour target, bool resetScale = false)
		{
			ResetLocalTransform(target.gameObject, resetScale);
		}

		/// <summary>
		/// Reset local transform to Vector3.zero and optionally, scale to 1
		/// </summary>
		/// <param name="target"></param>
		/// <param name="resetScale"></param>
		public static void ResetLocalTransform(this Component target, bool resetScale = false)
		{
			ResetLocalTransform(target.gameObject, resetScale);
		}

		/// <summary>
		/// Rotate to face the supplied <see cref="Transform"/>, optionally locking the Y Axis
		/// </summary>
		/// <param name="target"></param>
		/// <param name="transformToFace"></param>
		/// <param name="lockYRotation"></param>
		public static void FaceTransform(this Transform target, Transform transformToFace, bool lockYRotation = true)
		{
			var lookAtPos = new Vector3
			{
				x = transformToFace.position.x,
				y = (lockYRotation) ? target.position.y : transformToFace.position.y,
				z = transformToFace.position.z
			};

			target.LookAt(lookAtPos);
		}

		/// <summary>
		/// Rotate to face the supplied <see cref="Transform"/>, optionally locking the Y Axis
		/// </summary>
		/// <param name="target"></param>
		/// <param name="transformToFace"></param>
		/// <param name="lockYRotation"></param>
		public static void FaceTransform(this Component target, Transform transformToFace, bool lockYRotation = true)
		{
			FaceTransform(target.transform, transformToFace, lockYRotation);
		}

		/// <summary>
		/// Rotate to face the supplied <see cref="Transform"/>, optionally locking the Y Axis
		/// </summary>
		/// <param name="target"></param>
		/// <param name="transformToFace"></param>
		/// <param name="lockYRotation"></param>
		public static void FaceTransform(this MonoBehaviour target, Transform transformToFace, bool lockYRotation = true)
		{
			FaceTransform(target.transform, transformToFace, lockYRotation);
		}

		/// <summary>
		/// Search children of the supplied <see cref="Transform"/> recursively, returning the first child with a matching name
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="name"></param>
		/// <returns></returns>
		public static Transform FindChildTransform(this Transform parent, string name)
		{
			if (parent == null)
			{
				Debug.LogError("Unable to search for child on null parent");

				return null;
			}

			if (string.IsNullOrWhiteSpace(name))
			{
				return null;
			}

			if (parent.name.Contains(name)) return parent;

			foreach (Transform child in parent)
			{
				if (child.name.Contains(name)) return child;

				var result = FindChildTransform(child, name);

				if (result != null) return result;
			}

			return null;
		}

		/// <summary>
		/// return the combined <see cref="Bounds"/> for all Renderers encapsulated in the supplied <see cref="GameObject"/>
		/// </summary>
		/// <param name="source"></param>
		/// <returns></returns>
		public static Bounds GetBounds(this GameObject source)
		{
			var bounds = new Bounds(Vector3.zero, Vector3.zero);

			foreach (var renderer in source.GetComponentsInChildren<Renderer>())
			{
				if(renderer.gameObject.GetComponent<ParticleSystem>() != null) continue;
				
				bounds.Encapsulate(renderer.bounds);
			}

			return bounds;
		}

		/// <summary>
		/// Returns the combined height of all <see cref="SkinnedMeshRenderer"/> vertices. Note the Mesh needs to have read/write
		/// enabled in the import settings to provide access to the vertices data.
		/// </summary>
		/// <param name="source"></param>
		/// <returns></returns>
		public static float GetMeshHeight(this GameObject source)
		{
			float height = 0;

			foreach (var renderer in source.GetComponentsInChildren<SkinnedMeshRenderer>())
			{
				var verts = renderer.sharedMesh.vertices;

				for (var i = 0; i < verts.Length; i++)
				{
					verts[i] = source.transform.TransformPoint(verts[i]);
				}

				for (var i = 1; i < verts.Length; i++)
				{
					if (verts[i].z > height) height = verts[i].z;
				}
			}

			return height;
		}

		/// <summary>
		/// Discover all Renderers in the Source object and toggle visibility
		/// </summary>
		/// <param name="source"></param>
		/// <param name="visible"></param>
		public static void SetVisible(this GameObject source, bool visible)
		{
			foreach (var renderer in source.GetComponentsInChildren<Renderer>(true))
			{
				renderer.enabled = visible;
			}
		}

		public static void SetVisible(this Component source, bool visible)
		{
			SetVisible(source.gameObject, visible);
		}

		public static void RemoveComponent<T>(this Component target) where T : Component
		{
			if(target == null) return;
			
			if (Application.isPlaying)
			{
				Object.Destroy(target);
			}
			else
			{
				Object.DestroyImmediate(target);
			}
			
		}

		#endregion

		#region Meshes .....................................................................................................

		/// <summary>
		/// Centers a mesh to its Parent Transform
		/// </summary>
		/// <param name="target"></param>
		public static void CenterToParent(this MeshFilter target)
		{
			var zCenter = target.mesh.bounds.center.z;

			target.transform.localPosition = new Vector3(0, 0, 0 - zCenter);

		}

		#endregion

		#region RectTransform ..............................................................................................

		public static void SetLeft(this RectTransform rt, float left)
		{
			rt.offsetMin = new Vector2(left, rt.offsetMin.y);
		}

		public static void SetRight(this RectTransform rt, float right)
		{
			rt.offsetMax = new Vector2(-right, rt.offsetMax.y);
		}

		public static void SetTop(this RectTransform rt, float top)
		{
			rt.offsetMax = new Vector2(rt.offsetMax.x, -top);
		}

		public static void SetBottom(this RectTransform rt, float bottom)
		{
			rt.offsetMin = new Vector2(rt.offsetMin.x, bottom);
		}

		#endregion

		#region Color ......................................................................................................



		#endregion

		#region Materials ..................................................................................................

		public static void LoadTexture(this Material material, string path)
		{
			if (!FileUtil.FileExists(path)) throw new Exception("File not found: " + path);

			byte[] fileData = FileUtil.OpenFileAsByteArray(path);

			Texture2D texture = new Texture2D(2, 2);

			texture.LoadImage(fileData);

			material.mainTexture = texture;

		}

		#endregion

	}
}