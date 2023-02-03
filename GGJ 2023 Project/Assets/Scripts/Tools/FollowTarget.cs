using UnityEngine;
using System.Collections;

/// <summary>
/// Follow the movement of a supplied target Transform 1:1
/// Can follow x, y, and z axes separately
/// </summary>

public class FollowTarget : MonoBehaviour 
{
	/// <summary>
	/// The Transform to follow
	/// </summary>
	[SerializeField] private Transform _target;

	/// <summary>
	/// If true, follow the target along the x axis
	/// </summary>
	[SerializeField] private bool _followX;

	/// <summary>
	/// If true, follow the target along the y axis
	/// </summary>
	[SerializeField] private bool _followY;

	/// <summary>
	/// If true, follow the target along the z axis
	/// </summary>
	[SerializeField] private bool _followZ;

	/// <summary>
	/// Vector3 of the previous frame
	/// </summary>
	private Vector3 _previousVector;

	/// <summary>
	/// Difference between previous frame Vector3 and current Vector3
	/// </summary>
	private Vector3 _deltaVector;
	
	/// <summary>
	/// Cached Transform on Game Object
	/// </summary>
	private Transform _transform;

	// Use this for initialization
	void Awake()
	{
		// cache our transform for performance
		_transform = transform;
	}

	// Initialize must be called on both Start and OnEnable to recycle correctly
	void Start ()
	{
		Initialize();
	}
	
	void OnEnable()
	{
		Initialize();
	}
	
	// Initializes variables 
	public void Initialize() 
	{
		if(_target != null)
		{
			_previousVector = _target.position;
		}
	}
	
	// Update is called once per frame
	void Update ()
	{
		_deltaVector = _target.position - _previousVector;
		if(!_followX)
		{
			_deltaVector.x = 0;	
		}

		if(!_followY)
		{
			_deltaVector.y = 0;	
		}

		if(!_followZ)
		{
			_deltaVector.z = 0;	
		}

		_transform.Translate(_deltaVector, Space.World);
		_previousVector = _target.position;
	}
}