using UnityEngine;
using System.Collections;

/// <summary>
/// Keeps Game Object at a constant point on screen
/// </summary>
public class StayAtScreenPoint : MonoBehaviour 
{
	private Vector3 _screenPoint;
	private Transform _transform;
	private Camera _camera;

	void Start () 
	{
		_transform = transform;
		_camera = Camera.main;
		_screenPoint = _camera.WorldToScreenPoint(_transform.position);
	}
	
	// Update is called once per frame
	void Update() 
	{
		Vector3 correctWorldPoint = _camera.ScreenToWorldPoint(_screenPoint);

		_transform.position = correctWorldPoint;
	}
}