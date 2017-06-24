using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RadiusMarker : MonoBehaviour
{
        
	public GameObject prefab;
	public MapNav.TilesLayout markerLayout = MapNav.TilesLayout.Hex;
	public float markerSize = 1f;			
	public float markerSpacing = 1f;		
	public int markerRadius = 1;			
	public GameObject[] markerNodes;


	void Start()
	{
		HideAll();
	}

	public void HideAll()
	{
		foreach (GameObject go in markerNodes) go.SetActiveRecursively(false);
	}

	public void ShowAll()
	{
		foreach (GameObject go in markerNodes) go.SetActiveRecursively(true);
	}

	public void Show(Vector3 pos, int radius)
	{
		HideAll();
		if (radius <= 0) return;
		if (radius > markerNodes.Length) radius = markerNodes.Length;
		for (int i = 0; i < radius; i++)
		{
			markerNodes[i].SetActiveRecursively(true);
		}
		transform.position = pos;
	}
	public void ShowOutline(Vector3 pos, int radius)
	{
		HideAll();
		if (radius <= 0) return;
		radius--; 
		if (radius > markerNodes.Length) radius = markerNodes.Length;
		markerNodes[radius].SetActiveRecursively(true);
		transform.position = pos;
	}
	public static void UpdateMarker(GameObject markerFab, RadiusMarker marker, MapNav.TilesLayout markerLayout, float markerSpacing, float markerSize, int markerRadius)
	{
		List<GameObject> remove = new List<GameObject>();
		foreach (Transform t in marker.transform) remove.Add(t.gameObject);
		remove.ForEach(go => DestroyImmediate(go));

		if (markerFab == null) return;
		if (markerRadius < 1) markerRadius = 1;
		marker.prefab = markerFab;
		marker.markerLayout = markerLayout;
		marker.markerSpacing = markerSpacing;
		marker.markerSize = markerSize;
		marker.markerRadius = markerRadius;
		CreateMarkerNodes(markerFab, marker, markerLayout, markerSpacing, markerSize, markerRadius);
	}

	public static void CreateMarker(GameObject markerFab, MapNav.TilesLayout markerLayout, float markerSpacing, float markerSize, int markerRadius)
	{
		if (markerFab == null) return;
		if (markerRadius < 1) markerRadius = 1;
		GameObject go = new GameObject();
		go.name = "Marker";
		RadiusMarker marker = go.AddComponent<RadiusMarker>();
		marker.prefab = markerFab;
		marker.markerLayout = markerLayout;
		marker.markerSpacing = markerSpacing;
		marker.markerSize = markerSize;
		marker.markerRadius = markerRadius;
		CreateMarkerNodes(markerFab, marker, markerLayout, markerSpacing, markerSize, markerRadius);
	}
    
	private static void CreateMarkerNodes(GameObject markerFab, RadiusMarker marker, MapNav.TilesLayout markerLayout, float markerSpacing, float markerSize, int markerRadius)
	{
		if (markerLayout == MapNav.TilesLayout.Hex) CreateHexNodes(markerFab, marker, markerLayout, markerSpacing, markerSize, markerRadius);
		else if (markerLayout == MapNav.TilesLayout.Square_4) CreateSquar4Nodes(markerFab, marker, markerLayout, markerSpacing, markerSize, markerRadius);
		else if (markerLayout == MapNav.TilesLayout.Square_8) CreateSquar8Nodes(markerFab, marker, markerLayout, markerSpacing, markerSize, markerRadius);
	}
    
	private static void CreateHexNodes(GameObject markerFab, RadiusMarker marker, MapNav.TilesLayout markerLayout, float markerSpacing, float markerSize, int markerRadius)
	{
       // ‘›Œ¥ µœ÷
	}

	private static void CreateSquar4Nodes(GameObject markerFab, RadiusMarker marker, MapNav.TilesLayout markerLayout, float markerSpacing, float markerSize, int markerRadius)
	{
        
		GameObject go;
		marker.markerNodes = new GameObject[markerRadius];

		for (int i = 0; i < markerRadius; i++)
		{
			GameObject parent = new GameObject();
			parent.name = (i < 10 ? "0" : "") + i.ToString();
			parent.transform.position = marker.transform.position + new Vector3(0f, 0.1f, 0f);
			parent.transform.parent = marker.transform;
			marker.markerNodes[i] = parent;

			// up
			go = (GameObject)GameObject.Instantiate(markerFab);
			go.transform.parent = parent.transform;
			go.transform.localScale = new Vector3(markerSize, markerSize, markerSize);
			go.transform.localPosition = new Vector3(0f, 0f, markerSpacing * (i + 1));
			for (int j = 0; j < i; j++)
			{	// to right
				go = (GameObject)GameObject.Instantiate(markerFab);
				go.transform.parent = parent.transform;
				go.transform.localScale = new Vector3(markerSize, markerSize, markerSize);
				go.transform.localPosition = new Vector3(markerSpacing * (j + 1), 0f, (markerSpacing * i) - (markerSpacing * j));
				// to left
				go = (GameObject)GameObject.Instantiate(markerFab);
				go.transform.parent = parent.transform;
				go.transform.localScale = new Vector3(markerSize, markerSize, markerSize);
				go.transform.localPosition = new Vector3(-markerSpacing * (j + 1), 0f, (markerSpacing * i) - (markerSpacing * j));
			}


			// down
			go = (GameObject)GameObject.Instantiate(markerFab);
			go.transform.parent = parent.transform;
			go.transform.localScale = new Vector3(markerSize, markerSize, markerSize);
			go.transform.localPosition = new Vector3(0f, 0f, -markerSpacing * (i + 1));
			for (int j = 0; j < i; j++)
			{	// to right
				go = (GameObject)GameObject.Instantiate(markerFab);
				go.transform.parent = parent.transform;
				go.transform.localScale = new Vector3(markerSize, markerSize, markerSize);
				go.transform.localPosition = new Vector3(markerSpacing * (j + 1), 0f, -(markerSpacing * i) + (markerSpacing * j));
				// to left
				go = (GameObject)GameObject.Instantiate(markerFab);
				go.transform.parent = parent.transform;
				go.transform.localScale = new Vector3(markerSize, markerSize, markerSize);
				go.transform.localPosition = new Vector3(-markerSpacing * (j + 1), 0f, -(markerSpacing * i) + (markerSpacing * j));
			}

			// right
			go = (GameObject)GameObject.Instantiate(markerFab);
			go.transform.parent = parent.transform;
			go.transform.localScale = new Vector3(markerSize, markerSize, markerSize);
			go.transform.localPosition = new Vector3(+markerSpacing * (i + 1), 0f, 0f);

			// left
			go = (GameObject)GameObject.Instantiate(markerFab);
			go.transform.parent = parent.transform;
			go.transform.localScale = new Vector3(markerSize, markerSize, markerSize);
			go.transform.localPosition = new Vector3(-markerSpacing * (i + 1), 0f, 0f);
		}
	}

	private static void CreateSquar8Nodes(GameObject markerFab, RadiusMarker marker, MapNav.TilesLayout markerLayout, float markerSpacing, float markerSize, int markerRadius)
	{
        
		GameObject go;

		marker.markerNodes = new GameObject[markerRadius];

		for (int i = 0; i < markerRadius; i++)
		{
			GameObject parent = new GameObject();
			parent.name = (i < 10 ? "0" : "") + i.ToString();
			parent.transform.position = marker.transform.position + new Vector3(0f, 0.1f, 0f);
			parent.transform.parent = marker.transform;
			marker.markerNodes[i] = parent;

			// up
			go = (GameObject)GameObject.Instantiate(markerFab);
			go.transform.parent = parent.transform;
			go.transform.localScale = new Vector3(markerSize, markerSize, markerSize);
			go.transform.localPosition = new Vector3(0f, 0f, markerSpacing * (i + 1));
			for (int j = 0; j < i + 1; j++)
			{	// to right
				go = (GameObject)GameObject.Instantiate(markerFab);
				go.transform.parent = parent.transform;
				go.transform.localScale = new Vector3(markerSize, markerSize, markerSize);
				go.transform.localPosition = new Vector3(markerSpacing * (j + 1), 0f, markerSpacing * (i + 1));
				// to left
				go = (GameObject)GameObject.Instantiate(markerFab);
				go.transform.parent = parent.transform;
				go.transform.localScale = new Vector3(markerSize, markerSize, markerSize);
				go.transform.localPosition = new Vector3(-markerSpacing * (j + 1), 0f, markerSpacing * (i + 1));
			}

			// down
			go = (GameObject)GameObject.Instantiate(markerFab);
			go.transform.parent = parent.transform;
			go.transform.localScale = new Vector3(markerSize, markerSize, markerSize);
			go.transform.localPosition = new Vector3(0f, 0f, -markerSpacing * (i + 1));
			for (int j = 0; j < i + 1; j++)
			{	// to right
				go = (GameObject)GameObject.Instantiate(markerFab);
				go.transform.parent = parent.transform;
				go.transform.localScale = new Vector3(markerSize, markerSize, markerSize);
				go.transform.localPosition = new Vector3(markerSpacing * (j + 1), 0f, -markerSpacing * (i + 1));
				// to left
				go = (GameObject)GameObject.Instantiate(markerFab);
				go.transform.parent = parent.transform;
				go.transform.localScale = new Vector3(markerSize, markerSize, markerSize);
				go.transform.localPosition = new Vector3(-markerSpacing * (j + 1), 0f, -markerSpacing * (i + 1));
			}

			// right
			go = (GameObject)GameObject.Instantiate(markerFab);
			go.transform.parent = parent.transform;
			go.transform.localScale = new Vector3(markerSize, markerSize, markerSize);
			go.transform.localPosition = new Vector3(+markerSpacing * (i + 1), 0f, 0f);
			for (int j = 0; j < i; j++)
			{	// up
				go = (GameObject)GameObject.Instantiate(markerFab);
				go.transform.parent = parent.transform;
				go.transform.localScale = new Vector3(markerSize, markerSize, markerSize);
				go.transform.localPosition = new Vector3(markerSpacing * (i + 1), 0f, markerSpacing * (j + 1));
				// down
				go = (GameObject)GameObject.Instantiate(markerFab);
				go.transform.parent = parent.transform;
				go.transform.localScale = new Vector3(markerSize, markerSize, markerSize);
				go.transform.localPosition = new Vector3(markerSpacing * (i + 1), 0f, -markerSpacing * (j + 1));
			}

			// left
			go = (GameObject)GameObject.Instantiate(markerFab);
			go.transform.parent = parent.transform;
			go.transform.localScale = new Vector3(markerSize, markerSize, markerSize);
			go.transform.localPosition = new Vector3(-markerSpacing * (i + 1), 0f, 0f);
			for (int j = 0; j < i; j++)
			{	// up
				go = (GameObject)GameObject.Instantiate(markerFab);
				go.transform.parent = parent.transform;
				go.transform.localScale = new Vector3(markerSize, markerSize, markerSize);
				go.transform.localPosition = new Vector3(-markerSpacing * (i + 1), 0f, markerSpacing * (j + 1));
				// down
				go = (GameObject)GameObject.Instantiate(markerFab);
				go.transform.parent = parent.transform;
				go.transform.localScale = new Vector3(markerSize, markerSize, markerSize);
				go.transform.localPosition = new Vector3(-markerSpacing * (i + 1), 0f, -markerSpacing * (j + 1));
			}
		}
	}
}
