using UnityEngine;
using System.Collections.Generic;

// 拓展禁止移动区域的修改
public class TNEMovementModifier : MonoBehaviour 
{
	[System.Serializable]
	public class MovementInfo
	{
        // 设置不可移动区域(TileNode.tileTypeMask)
        public TileNode.TileType tileType = TileNode.TileType.Normal;

		public int movesModifier = 0;
	}

	public List<MovementInfo> moveInfos = new List<MovementInfo>();
}
