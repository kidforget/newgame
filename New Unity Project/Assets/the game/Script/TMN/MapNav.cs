using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
public class MapNav : MonoBehaviour 
{
	#region inspector properties

	public bool gizmoDrawNodes = true;
	public bool gizmoDrawLinks = true;
	public bool gizmoColorCoding = true;

	public int tilesLayer = 0;							// ��ͼ��
	public int unitsLayer = 0;							// ��λ
	public TilesLayout tilesLayout = TilesLayout.Hex;	// ����
	public float tileSpacing = 1f;						// ���ӿ�λ
	public float tileSize = 1f;							// ��С

	public GameObject[] nodesCache;
	public int nodesXCount = 0, nodesYCount = 0;

	#endregion
	
	#region vars

	public enum TilesLayout : byte { Hex = 0, Square_4 = 1, Square_8 = 2 }

	// ���һ���µ� TileNode
	public TileNode this[int index]
	{ 
		get 
		{
			if (nodesCache == null) return null;
			if (index < 0) return null;
			if (index >= nodesCache.Length) return null;
			if (nodesCache[index] == null) return null;
			return nodesCache[index].GetComponent<TileNode>(); 
		} 
	}
    
	public int Length // ��ȡ�ڵ㳤��
	{ 
		get 
		{
			if (nodesCache == null) return 0;
			return nodesCache.Length; 
		} 
	}
    
	public TileNode[] nodes { get; set; }

	#endregion
	
	#region pub

	void Start() 
	{
		LinkNodes();
		ShowAllTileNodes(false);
	}

	// �����Ƿ�TileNodes����
	public void ShowAllTileNodes(bool show)
	{
		if (nodes == null) return;
		foreach (TileNode n in nodes)
		{
			if (n == null) continue;
			n.Show(show);
		}
	}

	// ��ֹUNITY����ײ����⵽��ЩTileNode
	public void ShowTileNodeMarkers(bool show)
	{
		if (nodesCache == null) return;
		foreach (GameObject go in nodesCache)
		{
			if (go == null) continue;
			go.GetComponent<Renderer>().enabled = show;
		}
	}

	// �õ��ƶ���·��
	public TileNode[] GetPath(TileNode fromNode, TileNode toNode)
	{
		return GetPath(fromNode, toNode, 0);
	}

	// ����Ƿ�·�����в����ƶ���
	public TileNode[] GetPath(TileNode fromNode, TileNode toNode, TileNode.TileType validNodesLayer)
	{
		if (fromNode == null || toNode == null) return null;
		
		foreach (TileNode n in nodes)
		{
			if (n == null) continue;
			n.SetPathParent(null, 0, 0);
		}

		const int DistanceConst = 10;
		List<TileNode> openList = new List<TileNode>();
		List<TileNode> closeList = new List<TileNode>();
		TileNode tn = fromNode;
		openList.Add(tn);

		while (openList.Count > 0)
		{
			//�ҵ�һ�������·
			tn = null;
			foreach (TileNode ol in openList)
			{
				if (tn == null) tn = ol;
				else if (ol.PathF < tn.PathF) tn = ol;
			}

			closeList.Add(tn);
			openList.Remove(tn);

			if (tn == toNode) break; //�ƶ����

			// �ҵ�Ŀ��㸽���Ľڵ�
			foreach (TileNode n in tn.nodeLinks)
			{
				if (n == null) continue;

				if (closeList.Contains(n)) continue;

                // ����Ƿ����ʹ��
                if (validNodesLayer > 0)
				{
					if ((n.tileTypeMask & validNodesLayer) == validNodesLayer)
					{
						if (n.GetUnitInLevel(validNodesLayer) != null) continue;
					}
					else continue;
				}

                // ��������ڵ�֮���Ƿ������ӿ���
                if (tn.linkOnOffSwitch != null)
				{
					if (tn.linkOnOffSwitch.LinkIsOn(n) == 0) continue;
				}
				if (n.linkOnOffSwitch != null)
				{
					if (n.linkOnOffSwitch.LinkIsOn(tn) == 0) continue;
				}


				// ���� g �� H ��ֵ
				int G = tn.PathG + DistanceConst;
				int H = DistanceConst * Mathf.Abs((int)Vector3.Distance(n.transform.position, toNode.transform.position));

                // ����Ƿ����ƶ��޸���
                if (n.movesMod != null)
				{
					foreach (TNEMovementModifier.MovementInfo m in n.movesMod.moveInfos)
					{
						if (m.tileType == validNodesLayer) G += DistanceConst * m.movesModifier;
					}
				}

				// �ƶ�
				if (openList.Contains(n))
				{	
					if (G < n.PathG) n.SetPathParent(tn, G, H); // ȷ��·����ȷ
				}
				else
				{
					n.SetPathParent(tn, G, H);
					openList.Add(n);
				}
			}
		}

        // ���ʼ������·��
        List<TileNode> path = new List<TileNode>();
		tn = toNode;
		while (tn.PathParent != null)
		{
			path.Add(tn);
			tn = tn.PathParent;
		}

		if (path.Count > 0)
		{	
			path.Reverse();
			return (TileNode[])path.ToArray();
		}
		return null;
	}

	#endregion
	
	#region create / setup tools
    
	public static void CreateTileNodes(GameObject nodeFab, MapNav map, MapNav.TilesLayout layout, float tileSpacing, float tileSize, TileNode.TileType initialMask, int xCount, int yCount)
	{
		if (xCount <= 0 || yCount <= 0) return;

		map.tilesLayout = layout;
		map.tileSpacing = tileSpacing;
		map.tileSize = tileSize;

		// ɾ���ɵĽڵ�
		List<GameObject> remove = new List<GameObject>();
		foreach (Transform t in map.transform)
		{	
			if (t.name.Contains("node")) remove.Add(t.gameObject); // ȷ���������ǽڵ�
		}
		remove.ForEach(go => DestroyImmediate(go));

		// ������ͼ

		map.nodesXCount = xCount;
		map.nodesYCount = yCount;
		map.nodesCache = new GameObject[map.nodesXCount * map.nodesYCount];

		int count = 0;
		bool atoffs = false;
		float offs = 0f;
		float xOffs = map.tileSpacing;
		float yOffs = map.tileSpacing;

		if (map.tilesLayout == MapNav.TilesLayout.Hex)
        {   // ����ذ��ƫ����
            xOffs = Mathf.Sqrt(3f) * map.tileSpacing * 0.5f;
			offs = xOffs * 0.5f;
			yOffs = yOffs * 0.75f;
		}

		for (int y = 0; y < yCount; y++)
		{
			for (int x = 0; x < xCount; x++)
			{
                // �����ڵ�
                GameObject go = (GameObject)GameObject.Instantiate(nodeFab);
				go.name = "node" + count.ToString();
				go.layer = map.tilesLayer;

                // ��λ�ڵ㲢ʹ�ڵ���MapNav��
                go.transform.parent = map.transform;
				go.transform.localPosition = new Vector3(x * xOffs + (atoffs ? offs : 0f), 0f, y * yOffs);
				go.transform.localScale = new Vector3(map.tileSize, map.tileSize, map.tileSize);

                //����TileNode���
                TileNode n = go.GetComponent<TileNode>();
				n.idx = count;
				n.parent = map;
				n.tileTypeMask = initialMask;

                // �ص�Unity��ײ����
                go.GetComponent<Collider>().enabled = false;

				//����ڵ�
				map.nodesCache[count] = go;
				count++;
			}
			atoffs = !atoffs;
		}
	}

    // ����TileNodes�����ǵ����ڽڵ�
    public void LinkNodes()
	{
		if (nodesCache == null) return;
		if (nodesCache.Length == 0) return;
		if (nodesCache.Length != nodesXCount * nodesYCount)
		{
			Debug.LogWarning(string.Format("The number of cached nodes {0} != {1} which was expected", nodesCache.Length, (nodesXCount * nodesYCount)));
			return;
		}

		nodes = new TileNode[nodesCache.Length];

		bool atoffs = false;
		int i = 0;

        // ���ӽڵ������ǵ��ھ�
        if (tilesLayout == TilesLayout.Hex)
		{
			atoffs = false;
			for (int y = 0; y < nodesYCount; y++)
			{
				for (int x = 0; x < nodesXCount; x++)
				{
					i = y * nodesXCount + x;
					if (nodesCache[i] == null) { nodes[i] = null; continue; }

					TileNode n = nodesCache[i].GetComponent<TileNode>();
					nodes[i] = n; // �������
                    n.nodeLinks = new TileNode[6] { null, null, null, null, null, null };

                    // ������һ���ڵ�
                    if (x - 1 >= 0) n.nodeLinks[0] = nodesCache[i - 1] == null ? null : nodesCache[i - 1].GetComponent<TileNode>();
                    // ������һ���ڵ�
                    if (x + 1 < nodesXCount) n.nodeLinks[1] = nodesCache[i + 1] == null ? null : nodesCache[i + 1].GetComponent<TileNode>();

					// ���ӵ�ǰһ���еĽڵ�
					if (y > 0)
					{
						// ǰһ��, ͬһ��
						n.nodeLinks[2] = nodesCache[i - nodesXCount] == null ? null : nodesCache[i - nodesXCount].GetComponent<TileNode>(); ;
						if (atoffs)
                        {   // ǰһ��, ��һ��
                            if (x + 1 < nodesXCount) n.nodeLinks[4] = nodesCache[i - nodesXCount + 1] == null ? null : nodesCache[i - nodesXCount + 1].GetComponent<TileNode>();
						}
						else
                        {   // ǰһ��, ǰһ��
                            if (x - 1 >= 0) n.nodeLinks[4] = nodesCache[i - nodesXCount - 1] == null ? null : nodesCache[i - nodesXCount - 1].GetComponent<TileNode>();
						}
					}

                    // ���ӵ���һ���еĽڵ�
                    if (y + 1 < nodesYCount)
					{
                        // ��һ��,ͬһ��
                        n.nodeLinks[3] = nodesCache[i + nodesXCount] == null ? null : nodesCache[i + nodesXCount].GetComponent<TileNode>();
						if (atoffs)
                        {   //��һ��,��һ��
                            if (x + 1 < nodesXCount) n.nodeLinks[5] = nodesCache[i + nodesXCount + 1] == null ? null : nodesCache[i + nodesXCount + 1].GetComponent<TileNode>();
						}
						else
                        {   // ��һ��,ǰһ��
                            if (x - 1 >= 0) n.nodeLinks[5] = nodesCache[i + nodesXCount - 1] == null ? null : nodesCache[i + nodesXCount - 1].GetComponent<TileNode>();
						}
					}
				}
				atoffs = !atoffs;
			}
		}

        // ���ӽڵ������ǵ��ھ�(4������)
        if (tilesLayout == TilesLayout.Square_4)
		{
			for (int y = 0; y < nodesYCount; y++)
			{
				for (int x = 0; x < nodesXCount; x++)
				{
					i = y * nodesXCount + x;
					if (nodesCache[i] == null) { nodes[i] = null; continue; }

					TileNode n = nodesCache[i].GetComponent<TileNode>();
					nodes[i] = n; // �������
                    n.nodeLinks = new TileNode[4] { null, null, null, null };

                    // ����ǰһ���ڵ�
                    if (x - 1 >= 0) n.nodeLinks[0] = nodesCache[i - 1] == null ? null : nodesCache[i - 1].GetComponent<TileNode>();
                    // ������һ���ڵ�
                    if (x + 1 < nodesXCount) n.nodeLinks[1] = nodesCache[i + 1] == null ? null : nodesCache[i + 1].GetComponent<TileNode>();
                    // ������һ�У�ͬһ��
                    if (y > 0) n.nodeLinks[2] = nodesCache[i - nodesXCount] == null ? null : nodesCache[i - nodesXCount].GetComponent<TileNode>();
                    // ������һ�У�ͬһ��
                    if (y + 1 < nodesYCount) n.nodeLinks[3] = nodesCache[i + nodesXCount] == null ? null : nodesCache[i + nodesXCount].GetComponent<TileNode>(); ;
				}
			}
		}

        //  ���ӽڵ������ǵ��ھ�(8������)
        if (tilesLayout == TilesLayout.Square_8)
		{
			for (int y = 0; y < nodesYCount; y++)
			{
				for (int x = 0; x < nodesXCount; x++)
				{
					i = y * nodesXCount + x;
					if (nodesCache[i] == null) { nodes[i] = null; continue; }

					TileNode n = nodesCache[i].GetComponent<TileNode>();
					nodes[i] = n; // �������
                    n.nodeLinks = new TileNode[8] { null, null, null, null, null, null, null, null };

                    // ����ǰһ���ڵ�
                    if (x - 1 >= 0) n.nodeLinks[0] = nodesCache[i - 1] == null ? null : nodesCache[i - 1].GetComponent<TileNode>();
                    // ������һ���ڵ�
                    if (x + 1 < nodesXCount) n.nodeLinks[1] = nodesCache[i + 1] == null ? null : nodesCache[i + 1].GetComponent<TileNode>();

                    // ���ӵ�ǰһ���еĽڵ�
                    if (y > 0)
					{
                        // ǰһ��, ͬһ��
                        n.nodeLinks[2] = nodesCache[i - nodesXCount] == null ? null : nodesCache[i - nodesXCount].GetComponent<TileNode>();
                        // ǰһ��, ǰһ��
                        if (x - 1 >= 0) n.nodeLinks[4] = nodesCache[i - nodesXCount - 1] == null ? null : nodesCache[i - nodesXCount - 1].GetComponent<TileNode>();
                        // ǰһ��, ��һ��
                        if (x + 1 < nodesXCount) n.nodeLinks[6] = nodesCache[i - nodesXCount + 1] == null ? null : nodesCache[i - nodesXCount + 1].GetComponent<TileNode>();
					}

                    // ���ӵ���һ���еĽڵ�
                    if (y + 1 < nodesYCount)
					{
                        // ��һ��, ͬһ��
                        n.nodeLinks[3] = nodesCache[i + nodesXCount] == null ? null : nodesCache[i + nodesXCount].GetComponent<TileNode>();
                        // ��һ��, ǰһ��
                        if (x - 1 >= 0) n.nodeLinks[5] = nodesCache[i + nodesXCount - 1] == null ? null : nodesCache[i + nodesXCount - 1].GetComponent<TileNode>();
                        // ��һ��, ��һ��
                        if (x + 1 < nodesXCount) n.nodeLinks[7] = nodesCache[i + nodesXCount + 1] == null ? null : nodesCache[i + nodesXCount + 1].GetComponent<TileNode>();
					}

				}
			}
		}

	}

    // ��������TileNode������
    public void SetAllNodeMasksTo(TileNode.TileType mask)
	{
		if (nodesCache == null) return;
		foreach (GameObject go in nodesCache)
		{
			if (go == null) continue;
			TileNode n = go.GetComponent<TileNode>();
			if (n != null) n.tileTypeMask = mask;
		}
	}

	public void SetTileNodeMasks(TileNode.TileType mask, Transform parent)
	{
		AddOrSetTileNodeMasks(false, mask, parent, 10f);
	}

	public void SetTileNodeMasks(TileNode.TileType mask, int testAgainstCollidersLayer)
	{
		AddOrSetTileNodeMasks(false, mask, 1<<testAgainstCollidersLayer, 100f);
	}

	public void AddToTileNodeMasks(TileNode.TileType mask, Transform parent)
	{
		AddOrSetTileNodeMasks(true, mask, parent, 10f);
	}

	public void AddToTileNodeMasks(TileNode.TileType mask, int testAgainstCollidersLayer)
	{
		AddOrSetTileNodeMasks(true, mask, 1<<testAgainstCollidersLayer, 100f);
	}

	public void AddOrSetTileNodeMasks(bool isAdd, TileNode.TileType mask, Transform parent, float offsetY)
	{
		if (nodesCache == null || parent == null) return;

		//����ײ��
		foreach (GameObject go in nodesCache)
		{
			if (go == null) continue;
			go.GetComponent<Collider>().enabled = true;
		}

        // ͨ���ӽڵ�����
        foreach (Transform tr in parent.transform)
		{
			LayerMask rayMask = (1 << tilesLayer);
			Vector3 pos = tr.position; pos.y = offsetY;
			RaycastHit hit;
			if (Physics.Raycast(pos, -Vector3.up, out hit, offsetY * 2f, rayMask))
			{
				TileNode node = hit.collider.GetComponent<TileNode>();
				if (node != null)
				{
					if (isAdd) node.tileTypeMask = (node.tileTypeMask | mask);
					else node.tileTypeMask = mask;
				}
			}
		}

        // ���,���ö�ײ��
        foreach (GameObject go in nodesCache)
		{
			if (go == null) continue;
			go.GetComponent<Collider>().enabled = false;
		}
	}

	public void AddOrSetTileNodeMasks(bool isAdd, TileNode.TileType mask, LayerMask testAgainstCollidersLayerMask, float offsetY)
	{
		if (nodesCache == null) return;

        // �����ڵ㣬���й���ת����������Ҫ������¶Խڵ�����޸�
        foreach (GameObject go in nodesCache)
		{
			if (go == null) continue;

			Vector3 pos = go.transform.position; pos.y = offsetY;
			RaycastHit hit;
			if (Physics.Raycast(pos, -Vector3.up, out hit, offsetY * 2f, testAgainstCollidersLayerMask))
			{
				TileNode node = go.GetComponent<TileNode>();
				if (node != null)
				{
					if (isAdd) node.tileTypeMask = (node.tileTypeMask | mask);
					else node.tileTypeMask = mask;
				}
			}
		}
	}


   // ��ÿ���ڵ�(�ڵ�߶�+һ��С�߶�)�������䣬����Ƿ������һ����ײ����
   // �ڵ�ĸ߶ȱ������������е�ƫ������������κζ���������㡣
   // ���������ײ����ôû�д����κβ�Ľڵ㽫��ɾ����
	public void SetupNodeHeights(LayerMask checkAgainstLayer, bool unlinkIsActive)
	{
		if (nodesCache == null) return;
		if (nodesCache.Length == 0) return;

		foreach (GameObject go in nodesCache)
		{
			if (go == null) continue;
			Transform tr = go.transform;
			LayerMask rayMask = (1 << checkAgainstLayer);
			Vector3 pos = tr.position; pos.y = 100f;
			RaycastHit hit;
			if (Physics.Raycast(pos, -Vector3.up, out hit, 200f, rayMask))
			{
				pos.y = hit.point.y;
				tr.position = pos;
			}
			else if (unlinkIsActive)
			{
				// ɾ�����õĶ���ڵ�
				TileNode node = go.GetComponent<TileNode>();
				node.Unlink();
#if UNITY_EDITOR
				DestroyImmediate(go);
#else
				Destroy(go);
#endif
			}
		}
	}

	#endregion
}
