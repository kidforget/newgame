using UnityEngine;
using System.Collections.Generic;

// ��չ��ֹ�ƶ�������޸�
public class TNEMovementModifier : MonoBehaviour 
{
	[System.Serializable]
	public class MovementInfo
	{
        // ���ò����ƶ�����(TileNode.tileTypeMask)
        public TileNode.TileType tileType = TileNode.TileType.Normal;

		public int movesModifier = 0;
	}

	public List<MovementInfo> moveInfos = new List<MovementInfo>();
}
