using UnityEngine;

public class NaviUnit : MonoBehaviour 
{
	public TileNode.TileType tileLevel = TileNode.TileType.Normal;
	public float move_speed = 4f;
	public float move_speedMulti = 0.1f;
	public bool tiltToPath = false;
	public bool adjustToCollider = false;
	public bool adjustNormals = false; 
	public LayerMask adjustmentLayerMask = 0;
	public TileNode node { get; set; }
	public delegate void UnitEventDelegate(NaviUnit unit, int eventCode);
	protected UnitEventDelegate onUnitEvent = null;
	protected bool isMoving = false;					
	protected Vector3 endPointToReach = new Vector3();	
	protected Transform _tr;
    private Vector3[] _jump_points = null;
    private int _jump_pointIdx = 0;
    private Vector3 smoothed_normal = Vector3.up;

    public static NaviUnit SpawnUnit(GameObject unitFab, TileNode node)
	{
		GameObject go = (GameObject)GameObject.Instantiate(unitFab);
		go.transform.position = node.transform.position;
		NaviUnit unit = go.GetComponent<NaviUnit>();
		unit.LinkWith(node);
		return unit;
	}
	public virtual void Init(UnitEventDelegate callback)
	{
		onUnitEvent = callback;
	}
	public virtual void Start()
	{
		_tr = gameObject.transform;
	}
    // 绑定物体和node
	public void LinkWith(TileNode targetNode)
	{
		if (node != null)
		{
			node.units.Remove(this);
		}
		node = targetNode;
		if (node != null)
		{
			node.units.Add(this);
		}
	}
    // 判断能否生成在某个位置
	public bool CanStandOn(TileNode targetNode, bool andLevelIsOpen)
	{
		if (targetNode == null) return false;
		if ((targetNode.tileTypeMask & tileLevel) == tileLevel)
		{
			if (andLevelIsOpen)
			{
				if (targetNode.GetUnitInLevel(tileLevel) == null) return true;
			}
			else
			{
				return true;
			}
		}
		return false;
	}

	
	protected virtual void OnMovementComplete() { }

	
	public TileNode[] GetPathTo(MapNav map, TileNode targetNode)
	{
		return map.GetPath(node, targetNode, tileLevel);
	}

    // 移动 断开和当前节点链接->移动->链接至目标节点
	public bool MoveTo(MapNav map, TileNode targetNode, ref int moves)
	{
		if (moves == 0) return false;

		TileNode[] path = map.GetPath(node, targetNode, tileLevel);
		if (path == null) return false;
		if (path.Length == 0) return false;

		int useMoves = path.Length;
		if (moves >= 0)
		{
			if (moves <= useMoves) { useMoves = moves; moves = 0; }
			else moves -= useMoves;
		}
		else moves = path.Length;

		node.units.Remove(this);
		node = path[useMoves-1];
		node.units.Add(this);

		isMoving = true;

		System.Collections.Hashtable opt = iTween.Hash(
					"orienttopath", true,
					"easetype", "linear",
					"oncomplete", "_OnCompleteMoveTo",
					"oncompletetarget", gameObject);

		if (!tiltToPath) opt.Add("axis", "y");

		if ((adjustNormals || adjustToCollider) && adjustmentLayerMask != 0)
		{
			opt.Add("onupdate", "_OnMoveToUpdate");
			opt.Add("onupdatetarget", gameObject);
		}

		if (path.Length > 1)
		{
			Vector3[] points = new Vector3[useMoves];
			for (int i = 0; i < useMoves; i++) points[i] = path[i].transform.position;
			endPointToReach = points[points.Length - 1];
			opt.Add("path", points);
			opt.Add("speed", ((move_speedMulti * useMoves) + 1f) * move_speed);
			iTween.MoveTo(gameObject, opt);
		}
		else
		{
			endPointToReach = path[0].transform.position;
			opt.Add("position", endPointToReach);
			opt.Add("speed", move_speed);
			iTween.MoveTo(gameObject, opt);
		}
		return true;
	}
	private void _OnCompleteMoveTo()
	{
		UpdateUnitNormal();

		isMoving = false;
		OnMovementComplete();
		if (onUnitEvent != null) onUnitEvent(this, 1);
	}
	private void _OnMoveToUpdate()
	{
		UpdateUnitNormal();
	}
	private void UpdateUnitNormal()
	{
		if ( (!adjustNormals && !adjustToCollider) || adjustmentLayerMask==0 ) return;

		Vector3 pos = _tr.position; pos.y += 10f;
		RaycastHit hit;
		if (Physics.Raycast(pos, -Vector3.up, out hit, 50f, adjustmentLayerMask))
		{
			pos.y -= hit.distance; pos.y += 0.01f;
			_tr.position = pos;

			if (adjustNormals)
			{
				smoothed_normal = Vector3.Lerp(smoothed_normal, hit.normal, 10f * Time.deltaTime);
				Quaternion tilt = Quaternion.FromToRotation(Vector3.up, smoothed_normal);
				transform.rotation = tilt * Quaternion.Euler(0f, transform.rotation.eulerAngles.y, 0f);
			}
		}
	}
}
