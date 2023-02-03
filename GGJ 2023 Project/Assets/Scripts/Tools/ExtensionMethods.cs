using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// A collection of extension methods
/// </summary>

public static class ExtensionMethods 
{
	/// <summary>
	/// Searches entire child hierarchy for specific object
	/// </summary>
	public static Transform FindChildRecursive(this Transform myTransform, string name)   
	{
		// Check if the current transform is the child we're looking for; if so, return it
		if(myTransform.name == name)
		{
			return myTransform;
		}
		
		// Search through children for the child we're looking for
		for (int i = 0; i < myTransform.childCount; i++)
		{
			// The recursive step; repeat the search one step deeper in the hierarchy
			Transform found = myTransform.GetChild(i).FindChildRecursive(name);
			
			// A transform was returned by the search above that is not null,
			// it must be the child we're looking for
			if(found != null)
			{
				return found;
			}
		}
		
		// Child with name was not found
		return null;
	}

	/// <summary>
	/// Finds the top most game object for the specified child
	/// </summary>
	public static Transform FindUppermostParent(this Transform myTransform)
	{
		if(myTransform.parent == null)
		{
			return myTransform;
		}
		else
		{
			return myTransform.parent.FindUppermostParent();
		}
	}

	/// <summary>
	/// Adds contents of int array
	/// </summary>
	public static int Sum(this int[] array)
	{
		int sum = 0;
		for(int i = 0; i < array.Length; i++)
		{
			sum += array[i];
		}

		return sum;
	}

	/// <summary>
	/// Finds the largest number in an int array
	/// </summary>
	public static int MaxIndex(this int[] array)
	{
		int max = 0;
		for(int i = 1; i < array.Length; i++)
		{
			if(array[i] > array[max])
			{
				max = i;
			}
		}
		
		return max;
	}
	
	public static int MaxElement(this int[] array)
	{
		return array[array.MaxIndex()];
	}

	/*public static List<T> ShuffleList<T>(List<T> list)
	{
		list.Sort
		(
			delegate(T x, T y)
			{
				return Random.Range(-1, 2);
			}
		);

		return list;
	}*/

	/// <summary>
	/// Shuffles contents of int List
	/// </summary>
	public static List<int> ShuffleIntList(List<int> list)
	{
		list.Sort
		(
			delegate(int x, int y)
			{
				return Random.Range(-1, 2);
			}
		);
		
		return list;
	}

	/// <summary>
	/// Shuffles contents of float List
	/// </summary>
	public static List<float> ShuffleFloatList(List<float> list)
	{
		list.Sort
		(
			delegate(float x, float y)
			{
				return Random.Range(-1, 2);
			}
		);
		
		return list;
	}

	/// <summary>
	/// Shuffles contents of string List
	/// </summary>
	public static List<string> ShuffleStringList(List<string> list)
	{
		list.Sort
		(
			delegate(string x, string y)
			{
				return Random.Range(-1, 2);
			}
		);
		
		return list;
	}

	
	public static void Shuffle<T>(this List<T> list)
	{
		list.Sort
		(
			delegate(T x, T y)
			{
				return Random.Range(-1, 2);
			}
		);
	}

	/// <summary>
	/// Rounds a float to the specified number of digits
	/// </summary>
	public static float Round(float value, int digits)
	{
		float mult = Mathf.Pow(10.0F, (float)digits);
		return Mathf.Round(value * mult) / mult;
	}

	/// <summary>
	/// Creates a Vector3 with identical x, y, and z values
	/// </summary>
	public static Vector3 UniformVector(float l_value)
	{
		return new Vector3(l_value, l_value, l_value);
	}

	/// <summary>
	/// Returns the distance between two Vector3s based on only the x and y axes
	/// </summary>
	public static float Distance2D(Vector3 l_source, Vector3 l_target)
	{
		return Mathf.Sqrt(Mathf.Pow((l_target.x - l_source.x), 2) + Mathf.Pow((l_target.y - l_source.y), 2));
	}

	/// <summary>
	/// Returns the closest object to <l_point> with the supplied tag
	/// </summary>
	public static Transform ClosestObjectWithTagToPoint(string l_tag, Vector3 l_point)
	{
		GameObject[] l_objs = GameObject.FindGameObjectsWithTag(l_tag);
		if(l_objs.Length > 0)
		{
			Transform l_closest = null;
			for(int i = 0; i < l_objs.Length; i++)
			{
				if(l_objs[i].activeSelf)
				{
					Transform l_this = l_objs[i].transform;
					if(l_closest == null)
					{
						l_closest = l_this;
					}
					else
					{
						float l_closestDist = Distance2D(l_point, l_closest.position);
						float l_thisDist = Distance2D(l_point, l_this.position);

						if(Mathf.Abs(l_thisDist) < Mathf.Abs(l_closestDist))
						{
							l_closest = l_this;
						}
					}
				}
			}

			return l_closest;
		}

		return null;
	}

	/// <summary>
	/// Returns the closest object to <l_point> with any of the supplied tags
	/// </summary>
	public static Transform ClosestObjectWithTagsToPoint(string[] l_tags, Vector3 l_point)
	{
		List<Transform> l_closestObjects = new List<Transform>();
		for(int i = 0; i < l_tags.Length; i++)
		{
			Transform l_transform = ClosestObjectWithTagToPoint(l_tags[i], l_point);
			if(l_transform != null)
			{
				l_closestObjects.Add(l_transform);
			}
		}
		
		if(l_closestObjects.Count >= 1)
		{
			Transform l_closest = null;
			for(int i = 0; i < l_closestObjects.Count; i++)
			{
				Transform l_this = l_closestObjects[i].transform;
				
				if(l_closest == null)
				{
					l_closest = l_this;
				}
				else
				{
					float l_closestDist = Distance2D(l_point, l_closest.position);
					float l_thisDist = Distance2D(l_point, l_this.position);
					
					if(Mathf.Abs(l_thisDist) < Mathf.Abs(l_closestDist))
					{
						l_closest = l_this;
					}
				}
			}
			
			return l_closest;
		}
		
		return null;
	}

	/*public static Vector3 MousePointToGunPoint(Vector3 l_position, Transform l_gunTransform = null)
	{
		Vector3 l_ret = l_position;
		l_ret.z = -CameraManager.instance.tapCamera.transform.position.z;
		l_ret = CameraManager.instance.tapCamera.ScreenToWorldPoint(l_ret);

		if(l_gunTransform != null)
		{
			l_ret.y = Mathf.Max(l_gunTransform.position.y, l_ret.y);
			l_ret.z = l_gunTransform.position.z;
		}

		return l_ret;
	}*/

	/// <summary>
	/// Sets the layer of the supplied game object and all its children to the specified layer
	/// </summary>
	public static void SetLayerRecursive(GameObject l_object, int l_layer)
	{
		l_object.layer = l_layer;
		Transform l_transform = l_object.transform;
		for(int i = 0; i < l_transform.childCount; i++)
		{
			SetLayerRecursive(l_transform.GetChild(i).gameObject, l_layer);
		}
	}

	/// <summary>
	/// Converts a RenderTexture to Texture2D
	/// </summary>
	public static Texture2D ToTexture2D(this RenderTexture rTex)
	{
		Texture2D tex = new Texture2D(rTex.width, rTex.height, TextureFormat.RGBA32, false);
		var old_rt = RenderTexture.active;
		RenderTexture.active = rTex;

		tex.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
		tex.Apply();

		RenderTexture.active = old_rt;
		return tex;
	}

	/// <summary>
	/// Returns a string in MM:SS format for a supplied number of seconds
	/// </summary>
	public static string SecondsToTimecode (float p_seconds)
	{
		bool l_isPositive = (p_seconds >= 0);
		int l_timeInSecondsInt = (int)p_seconds;  //We don't care about fractions of a second, so easy to drop them by just converting to an int
		int l_minutes = (int)(p_seconds / 60f);  //Get total minutes
		int l_seconds = l_timeInSecondsInt - (l_minutes * 60);  //Get seconds for display alongside minutes
		return (l_isPositive ? "" : "- ") + Mathf.Abs(l_minutes).ToString("D2") + ":" + Mathf.Abs(l_seconds).ToString("D2");  //Create the string representation, where both seconds and minutes are at minimum 2 digits
	}

	/// <summary>
	/// Returns a string formatted to display currency with suffixes up to M
	/// </summary>

	public static string FloatToCurrency (float p_amount)
	{
		float l_displayValue = p_amount;

			string l_suffix = "";

			if (p_amount >= 1000000)
			{
				l_displayValue = l_displayValue / 1000000f;
				l_suffix = "M";
			} 
			else if (p_amount < 1000000 && p_amount >= 1000)
			{
				l_displayValue = l_displayValue / 1000f;
				l_suffix = "K";
			}

			return "$" + l_displayValue.ToString("F2") + l_suffix;
	}
}