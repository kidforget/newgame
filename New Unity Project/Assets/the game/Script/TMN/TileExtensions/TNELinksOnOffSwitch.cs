using UnityEngine;
using System.Collections.Generic;

//��չ�򿪽ڵ㼰���ھ�֮�������
public class TNELinksOnOffSwitch : MonoBehaviour
{
	[System.Serializable] // ���������л�
	public class LinkState
	{
		public TileNode neighbour = null;
		public bool isOn = false;
	}

	public List<LinkState> linkStates = new List<LinkState>();

    // ��������Ϣ�Ҵ�Ϊ 1 ������Ϊ0�� û��������ϢΪ-1
    public int LinkIsOn(TileNode node)
	{
		foreach (LinkState l in linkStates)
		{
			if (l.neighbour == node) return (l.isOn?1:0);
		}
		return -1; // û��������Ϣ
	}

	public void SetLinkStateWith(TileNode node, bool on) // ��������
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
