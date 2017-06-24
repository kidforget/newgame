// ====================================================================================================================
//
// Created by Leslie Young
// http://www.plyoung.com/ or http://plyoung.wordpress.com/
// ====================================================================================================================

using UnityEngine;

public class SelectionIndicator : MonoBehaviour 
{

	public Vector3 offset = Vector3.zero; // how it should be offset from position it is placed at

	void Start()
	{
		Hide();
	}

	/// <summary>Hides the selector, also unparent from any transform it might have been following if set</summary>
	public void Hide()
	{
		this.transform.parent = null;
		this.gameObject.SetActiveRecursively(false);
	}

	/// <summary>Hides the selector. Only unlink with transform it is following if unlink is set to true</summary>
	public void Hide(bool unlink)
	{
		if (unlink) this.transform.parent = null;
		this.gameObject.SetActiveRecursively(false);
	}


	/// <summary>Show it at given pos (offset, set in properties, is applied)</summary>
	public void Show(Vector3 pos)
	{
		this.transform.position = pos + offset;
		this.gameObject.SetActiveRecursively(true);
	}

	/// <summary>Show it at the location of the transform it will follow around (offset, set in properties, is applied)</summary>
	public void Show(Transform follow)
	{
		this.transform.parent = follow;
		this.transform.localPosition = offset;
		this.gameObject.SetActiveRecursively(true);
	}

	// ====================================================================================================================
}
