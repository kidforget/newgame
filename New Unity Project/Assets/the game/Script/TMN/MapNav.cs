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

	public int tilesLayer = 0;							// 地图格
	public int unitsLayer = 0;							// 单位
	public TilesLayout tilesLayout = TilesLayout.Hex;	// 布局
	public float tileSpacing = 1f;						// 格子空位
	public float tileSize = 1f;							// 大小

	public GameObject[] nodesCache;
	public int nodesXCount = 0, nodesYCount = 0;

	#endregion
	
	#region vars

	public enum TilesLayout : byte { Hex = 0, Square_4 = 1, Square_8 = 2 }

	// 获得一个新的 TileNode
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
    
	public int Length // 获取节点长度
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

	// 设置是否TileNodes可视
	public void ShowAllTileNodes(bool show)
	{
		if (nodes == null) return;
		foreach (TileNode n in nodes)
		{
			if (n == null) continue;
			n.Show(show);
		}
	}

	// 防止UNITY的碰撞检测检测到这些TileNode
	public void ShowTileNodeMarkers(bool show)
	{
		if (nodesCache == null) return;
		foreach (GameObject go in nodesCache)
		{
			if (go == null) continue;
			go.GetComponent<Renderer>().enabled = show;
		}
	}

	// 得到移动的路径
	public TileNode[] GetPath(TileNode fromNode, TileNode toNode)
	{
		return GetPath(fromNode, toNode, 0);
	}

	// 检测是否路径上有不可移动点
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
			//找到一条最近的路
			tn = null;
			foreach (TileNode ol in openList)
			{
				if (tn == null) tn = ol;
				else if (ol.PathF < tn.PathF) tn = ol;
			}

			closeList.Add(tn);
			openList.Remove(tn);

			if (tn == toNode) break; //移动完成

			// 找到目标点附近的节点
			foreach (TileNode n in tn.nodeLinks)
			{
				if (n == null) continue;

				if (closeList.Contains(n)) continue;

                // 检查是否可以使用
                if (validNodesLayer > 0)
				{
					if ((n.tileTypeMask & validNodesLayer) == validNodesLayer)
					{
						if (n.GetUnitInLevel(validNodesLayer) != null) continue;
					}
					else continue;
				}

                // 检查两个节点之间是否有连接开关
                if (tn.linkOnOffSwitch != null)
				{
					if (tn.linkOnOffSwitch.LinkIsOn(n) == 0) continue;
				}
				if (n.linkOnOffSwitch != null)
				{
					if (n.linkOnOffSwitch.LinkIsOn(tn) == 0) continue;
				}


				// 计算 g 和 H 的值
				int G = tn.PathG + DistanceConst;
				int H = DistanceConst * Mathf.Abs((int)Vector3.Distance(n.transform.position, toNode.transform.position));

                // 检查是否有移动修改器
                if (n.movesMod != null)
				{
					foreach (TNEMovementModifier.MovementInfo m in n.movesMod.moveInfos)
					{
						if (m.tileType == validNodesLayer) G += DistanceConst * m.movesModifier;
					}
				}

				// 移动
				if (openList.Contains(n))
				{	
					if (G < n.PathG) n.SetPathParent(tn, G, H); // 确保路径正确
				}
				else
				{
					n.SetPathParent(tn, G, H);
					openList.Add(n);
				}
			}
		}

        // 从最开始，构建路径
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

		// 删除旧的节点
		List<GameObject> remove = new List<GameObject>();
		foreach (Transform t in map.transform)
		{	
			if (t.name.Contains("node")) remove.Add(t.gameObject); // 确保创建的是节点
		}
		remove.ForEach(go => DestroyImmediate(go));

		// 创建地图

		map.nodesXCount = xCount;
		map.nodesYCount = yCount;
		map.nodesCache = new GameObject[map.nodesXCount * map.nodesYCount];

		int count = 0;
		bool atoffs = false;
		float offs = 0f;
		float xOffs = map.tileSpacing;
		float yOffs = map.tileSpacing;

		if (map.tilesLayout == MapNav.TilesLayout.Hex)
        {   // 计算地板的偏移量
            xOffs = Mathf.Sqrt(3f) * map.tileSpacing * 0.5f;
			offs = xOffs * 0.5f;
			yOffs = yOffs * 0.75f;
		}

		for (int y = 0; y < yCount; y++)
		{
			for (int x = 0; x < xCount; x++)
			{
                // 创建节点
                GameObject go = (GameObject)GameObject.Instantiate(nodeFab);
				go.name = "node" + count.ToString();
				go.layer = map.tilesLayer;

                // 定位节点并使节点在MapNav下
                go.transform.parent = map.transform;
				go.transform.localPosition = new Vector3(x * xOffs + (atoffs ? offs : 0f), 0f, y * yOffs);
				go.transform.localScale = new Vector3(map.tileSize, map.tileSize, map.tileSize);

                //更新TileNode组件
                TileNode n = go.GetComponent<TileNode>();
				n.idx = count;
				n.parent = map;
				n.tileTypeMask = initialMask;

                // 关掉Unity碰撞检测机
                go.GetComponent<Collider>().enabled = false;

				//缓存节点
				map.nodesCache[count] = go;
				count++;
			}
			atoffs = !atoffs;
		}
	}

    // 连接TileNodes和它们的相邻节点
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

        // 链接节点与它们的邻居
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
					nodes[i] = n; // 缓存组件
                    n.nodeLinks = new TileNode[6] { null, null, null, null, null, null };

                    // 链接上一个节点
                    if (x - 1 >= 0) n.nodeLinks[0] = nodesCache[i - 1] == null ? null : nodesCache[i - 1].GetComponent<TileNode>();
                    // 链接下一个节点
                    if (x + 1 < nodesXCount) n.nodeLinks[1] = nodesCache[i + 1] == null ? null : nodesCache[i + 1].GetComponent<TileNode>();

					// 链接到前一行中的节点
					if (y > 0)
					{
						// 前一行, 同一列
						n.nodeLinks[2] = nodesCache[i - nodesXCount] == null ? null : nodesCache[i - nodesXCount].GetComponent<TileNode>(); ;
						if (atoffs)
                        {   // 前一行, 下一列
                            if (x + 1 < nodesXCount) n.nodeLinks[4] = nodesCache[i - nodesXCount + 1] == null ? null : nodesCache[i - nodesXCount + 1].GetComponent<TileNode>();
						}
						else
                        {   // 前一行, 前一列
                            if (x - 1 >= 0) n.nodeLinks[4] = nodesCache[i - nodesXCount - 1] == null ? null : nodesCache[i - nodesXCount - 1].GetComponent<TileNode>();
						}
					}

                    // 链接到下一行中的节点
                    if (y + 1 < nodesYCount)
					{
                        // 下一行,同一列
                        n.nodeLinks[3] = nodesCache[i + nodesXCount] == null ? null : nodesCache[i + nodesXCount].GetComponent<TileNode>();
						if (atoffs)
                        {   //下一行,下一列
                            if (x + 1 < nodesXCount) n.nodeLinks[5] = nodesCache[i + nodesXCount + 1] == null ? null : nodesCache[i + nodesXCount + 1].GetComponent<TileNode>();
						}
						else
                        {   // 下一行,前一列
                            if (x - 1 >= 0) n.nodeLinks[5] = nodesCache[i + nodesXCount - 1] == null ? null : nodesCache[i + nodesXCount - 1].GetComponent<TileNode>();
						}
					}
				}
				atoffs = !atoffs;
			}
		}

        // 链接节点与它们的邻居(4个方向)
        if (tilesLayout == TilesLayout.Square_4)
		{
			for (int y = 0; y < nodesYCount; y++)
			{
				for (int x = 0; x < nodesXCount; x++)
				{
					i = y * nodesXCount + x;
					if (nodesCache[i] == null) { nodes[i] = null; continue; }

					TileNode n = nodesCache[i].GetComponent<TileNode>();
					nodes[i] = n; // 缓存组件
                    n.nodeLinks = new TileNode[4] { null, null, null, null };

                    // 链接前一个节点
                    if (x - 1 >= 0) n.nodeLinks[0] = nodesCache[i - 1] == null ? null : nodesCache[i - 1].GetComponent<TileNode>();
                    // 链接下一个节点
                    if (x + 1 < nodesXCount) n.nodeLinks[1] = nodesCache[i + 1] == null ? null : nodesCache[i + 1].GetComponent<TileNode>();
                    // 链接上一行，同一列
                    if (y > 0) n.nodeLinks[2] = nodesCache[i - nodesXCount] == null ? null : nodesCache[i - nodesXCount].GetComponent<TileNode>();
                    // 链接下一行，同一列
                    if (y + 1 < nodesYCount) n.nodeLinks[3] = nodesCache[i + nodesXCount] == null ? null : nodesCache[i + nodesXCount].GetComponent<TileNode>(); ;
				}
			}
		}

        //  链接节点与它们的邻居(8个方向)
        if (tilesLayout == TilesLayout.Square_8)
		{
			for (int y = 0; y < nodesYCount; y++)
			{
				for (int x = 0; x < nodesXCount; x++)
				{
					i = y * nodesXCount + x;
					if (nodesCache[i] == null) { nodes[i] = null; continue; }

					TileNode n = nodesCache[i].GetComponent<TileNode>();
					nodes[i] = n; // 缓存组件
                    n.nodeLinks = new TileNode[8] { null, null, null, null, null, null, null, null };

                    // 链接前一个节点
                    if (x - 1 >= 0) n.nodeLinks[0] = nodesCache[i - 1] == null ? null : nodesCache[i - 1].GetComponent<TileNode>();
                    // 链接下一个节点
                    if (x + 1 < nodesXCount) n.nodeLinks[1] = nodesCache[i + 1] == null ? null : nodesCache[i + 1].GetComponent<TileNode>();

                    // 链接到前一行中的节点
                    if (y > 0)
					{
                        // 前一行, 同一列
                        n.nodeLinks[2] = nodesCache[i - nodesXCount] == null ? null : nodesCache[i - nodesXCount].GetComponent<TileNode>();
                        // 前一行, 前一列
                        if (x - 1 >= 0) n.nodeLinks[4] = nodesCache[i - nodesXCount - 1] == null ? null : nodesCache[i - nodesXCount - 1].GetComponent<TileNode>();
                        // 前一行, 下一列
                        if (x + 1 < nodesXCount) n.nodeLinks[6] = nodesCache[i - nodesXCount + 1] == null ? null : nodesCache[i - nodesXCount + 1].GetComponent<TileNode>();
					}

                    // 链接到下一行中的节点
                    if (y + 1 < nodesYCount)
					{
                        // 下一行, 同一列
                        n.nodeLinks[3] = nodesCache[i + nodesXCount] == null ? null : nodesCache[i + nodesXCount].GetComponent<TileNode>();
                        // 下一行, 前一列
                        if (x - 1 >= 0) n.nodeLinks[5] = nodesCache[i + nodesXCount - 1] == null ? null : nodesCache[i + nodesXCount - 1].GetComponent<TileNode>();
                        // 下一行, 下一列
                        if (x + 1 < nodesXCount) n.nodeLinks[7] = nodesCache[i + nodesXCount + 1] == null ? null : nodesCache[i + nodesXCount + 1].GetComponent<TileNode>();
					}

				}
			}
		}

	}

    // 重置所有TileNode的类型
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

		//打开碰撞机
		foreach (GameObject go in nodesCache)
		{
			if (go == null) continue;
			go.GetComponent<Collider>().enabled = true;
		}

        // 通过子节点运行
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

        // 完成,禁用对撞机
        foreach (GameObject go in nodesCache)
		{
			if (go == null) continue;
			go.GetComponent<Collider>().enabled = false;
		}
	}

	public void AddOrSetTileNodeMasks(bool isAdd, TileNode.TileType mask, LayerMask testAgainstCollidersLayerMask, float offsetY)
	{
		if (nodesCache == null) return;

        // 遍历节点，进行光线转换，并在需要的情况下对节点进行修改
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


   // 从每个节点(节点高度+一个小高度)向下照射，检查是否击中了一个对撞机。
   // 节点的高度被调整到被击中的偏移量，如果有任何东西在这个层。
   // 如果发生碰撞，那么没有触及任何层的节点将被删除。
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
				// 删除无用的多余节点
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
