using UnityEngine;
using System.Collections.Generic;

//扩展打开节点及其邻居之间的链接
public class TNELinksOnOffSwitch : MonoBehaviour
{
	[System.Serializable] // 将对象序列化
	public class LinkState
	{
		public TileNode neighbour = null;
		public bool isOn = false;
	}

	public List<LinkState> linkStates = new List<LinkState>();

    // 有连接信息且打开为 1 ，否则为0， 没有连接信息为-1
    public int LinkIsOn(TileNode node)
	{
		foreach (LinkState l in linkStates)
		{
			if (l.neighbour == node) return (l.isOn?1:0);
		}
		return -1; // 没有链接信息
	}

	public void SetLinkStateWith(TileNode node, bool on) // 设置链接
	{
		foreach (LinkState l in linkStates)
		{
			if (l.neighbour == node)
			{
				l.isOn = on;
				return;
			}
		}
		LinkState ls = new LinkState();
		ls.neighbour = node;
		ls.isOn = on;
		linkStates.Add(ls);
	}
}
