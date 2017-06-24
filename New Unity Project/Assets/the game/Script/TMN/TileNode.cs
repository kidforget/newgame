using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// 棋盘用于放置棋子的节点
public class TileNode: MonoBehaviour
{

	public MapNav parent = null;
	public int idx = 0;						
	public TileType tileTypeMask = 0;

    // 设置可以放置棋子类型
	public enum TileType
	{
		Normal		= 0x1,	
		Air			= 0x2,	
		Water		= 0x4,	
		Wall		= 0x8,	
	}
	public TileNode[] nodeLinks { get; set; }
	public List<NaviUnit> units { get; set; }
	public bool IsVisible { get; set; }
	public TNEMovementModifier movesMod { get; private set; }			
	public TNELinksOnOffSwitch linkOnOffSwitch { get; private set; }	
	void Start()
	{
		units = new List<NaviUnit>();

		linkOnOffSwitch = gameObject.GetComponent<TNELinksOnOffSwitch>();
		movesMod = gameObject.GetComponent<TNEMovementModifier>();
	}
	public void OnHover(bool mouseOver)
	{
		if (!GetComponent<Animation>()) return;
		if (mouseOver)
		{
			GetComponent<Animation>().Play();
		}
		else
		{
			GetComponent<Animation>().Stop();
			Color c = GetComponent<Renderer>().material.color;
			c.a = 1f;
			GetComponent<Renderer>().material.color = c;
		}
	}
	public void Show(bool doShow)
	{
		IsVisible = doShow;
		GetComponent<Renderer>().enabled = doShow;
		GetComponent<Collider>().enabled = doShow;
	}
	public void Show()
	{
		Show(true);
	}
	public void Hide()
	{
		Show(false);
	}
	public void ShowNeighbours(int radius, bool show)
	{
		_ShowNeighboursRecursive(radius, show, 0, false, false);
	}
	public void ShowNeighbours(int radius, TileNode.TileType validNodesLayer)
	{
		_ShowNeighboursRecursive(radius, true, validNodesLayer, false, false);
	}
	public void ShowNeighbours(int radius, TileNode.TileType validNodesLayer, bool checkMoveMod, bool checkOnOffSwitch)
	{
		_ShowNeighboursRecursive(radius, true, validNodesLayer, checkMoveMod, checkOnOffSwitch);
	}
	public NaviUnit GetUnitInLevel(TileType level)
	{
		foreach (NaviUnit u in units)
		{
			if (u.tileLevel == level) return u;
		}
		return null;
	}
    // 判断节点是否在范围内
	public bool IsInRange(TileNode targetNode, int radius)
	{
		radius--;
		if (radius < 0) return false;
		foreach (TileNode node in nodeLinks)
		{
			if (node != null)
			{
				if (node == targetNode) return true;
				if (node.IsInRange(targetNode, radius)) return true;
			}
		}
		return false;
	}

    // 接触节点的所有链接
	public void UnlinkWithNode(TileNode node)
	{
		if (nodeLinks != null && node != null)
		{
			for (int i=0; i<nodeLinks.Length; i++)
			{
				if (nodeLinks[i] == node)
				{
					nodeLinks[i] = null;
					break;
				}
			}
		}

	}
	public void Unlink()
	{
		if (units != null)
		{
			foreach (NaviUnit u in units) u.node = null;
		}

		if (nodeLinks != null)
		{
			foreach (TileNode n in nodeLinks)
			{
				if (n!=null) n.UnlinkWithNode(this);
			}
		}

	}
	public bool IsDirectNeighbour(TileNode withNode)
	{
		foreach (TileNode n in nodeLinks)
		{
			if (n == withNode) return true;
		}
		return false;
	}
	public bool LinkIsOnWith(TileNode withNode)
	{
		if (!IsDirectNeighbour(withNode)) return false;
		int state = -1;
		if (linkOnOffSwitch != null)
		{	
			state = linkOnOffSwitch.LinkIsOn(withNode);
		}
		if (state == -1)
		{
			if (withNode.linkOnOffSwitch != null)
			{
				state = withNode.linkOnOffSwitch.LinkIsOn(this);
			}
		}
		return (state == 0?false:true);
	}
	public bool SetLinkState(TileNode withNode, bool on)
	{
		if (!IsDirectNeighbour(withNode)) return false;

		bool needToAdd = false;
		if (linkOnOffSwitch != null)
		{
			linkOnOffSwitch.SetLinkStateWith(withNode, on);
			needToAdd = false;
		}
		if (withNode.linkOnOffSwitch != null)
		{
			withNode.linkOnOffSwitch.SetLinkStateWith(this, on);
			needToAdd = false;
		}
		if (needToAdd)
		{
			linkOnOffSwitch = gameObject.AddComponent<TNELinksOnOffSwitch>();
			linkOnOffSwitch.SetLinkStateWith(withNode, on);
		}
		return true;
	}
	public void SetLinkStateWithAll(bool on)
	{
		if (linkOnOffSwitch==null) return;

		foreach (TNELinksOnOffSwitch.LinkState ls in linkOnOffSwitch.linkStates)
		{
			ls.isOn = on;
			if (ls.neighbour.linkOnOffSwitch = null)
			{
				foreach (TNELinksOnOffSwitch.LinkState lsb in ls.neighbour.linkOnOffSwitch.linkStates)
				{
					if (lsb.neighbour == this)
					{
						lsb.isOn = on;
						break;
					}
				}
			}
		}
	}
	public void _ShowNeighboursRecursive(int radius, bool show, TileNode.TileType validNodesLayer, bool inclMoveMod, bool incLinkCheck)
	{
		radius--;
		if (radius < 0) return;
		List<TileNode> skipList = new List<TileNode>();
		foreach (TileNode node in nodeLinks)
		{
			if (node == null) continue;

			if (show == true)
			{
				if (validNodesLayer > 0)
				{
					if ((node.tileTypeMask & validNodesLayer) != validNodesLayer) continue;
					if (node.GetUnitInLevel(validNodesLayer)) continue;
				}
				if (inclMoveMod && node.movesMod != null)
				{
					int r = radius;
					foreach (TNEMovementModifier.MovementInfo m in node.movesMod.moveInfos)
					{
						if (m.tileType == validNodesLayer) r -= m.movesModifier;
					}
					if (r < 0) continue;
					if (r <= 0) skipList.Add(node);
				}
				if (incLinkCheck)
				{
				    if (!LinkIsOnWith(node))
				    {
				        skipList.Add(node);
				        continue;
				    }
				}
			}

			node.Show(show);
		}
		foreach (TileNode node in nodeLinks)
		{
			if (node == null) continue;

			if (show == true)
			{
				if (validNodesLayer > 0)
				{
					if ((node.tileTypeMask & validNodesLayer) != validNodesLayer) continue;
					if (node.GetUnitInLevel(validNodesLayer)) continue;
				}

				if (skipList.Contains(node)) continue;
			}

			node._ShowNeighboursRecursive(radius, show, validNodesLayer, inclMoveMod, incLinkCheck);
		}
	}

	private TileNode pathParent = null;
	public void SetPathParent(TileNode parent, int g, int h)
	{
		pathParent = parent;
		PathG = g;
		PathH = h;
		PathF = g + h;
	}
	public TileNode PathParent { get { return pathParent; } }
	public int PathF { get; private set; }
	public int PathG { get; private set; }
	public int PathH { get; private set; }
}
