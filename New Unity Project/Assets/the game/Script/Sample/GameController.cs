using UnityEngine;
using System.Collections.Generic;

public class GameController : TMNController 
{
	#region inspector properties

	public CameraMove camMover;					// 移动相机
	public SelectionIndicator selectionMarker;	// 控制选中单位
	public RadiusMarker attackRangeMarker;		// 攻击距离设置
	public GameObject[] unitFabs;				// 单位列表
	public int spawnCount = 8;					// 单位数量
	public bool hideSelectorOnMove = true;		// 移动时将选中的标记隐藏
	public bool hideMarkersOnMove = true;       // 移动时将选中的单位的可移动打的位置节点隐藏
    public bool useTurns = true;				// 回合制开启/关闭
    public bool combatOn = false;				// 是否只能执行一次攻击

	#endregion

	#region vars

	private enum State : byte { Init = 0, Running, DontRun } //枚举控制单位的状态
	private State state = State.Init;

	private Unit selectedUnit = null;	// 选中的单位
	private TileNode hoverNode = null;	// 选中的节点
	private TileNode prevNode = null;	// 移动时下一个节点
     
	public bool allowInput { get; set; } // 是否允许玩家控制

	private List<Unit>[] units = {
		new List<Unit>(),	//  玩家一的单位列表
		new List<Unit>()	//  玩家二的单位列表
	};

	public int currPlayerTurn  { get; set; }		// 确定每个玩家的回合并记录当前玩家的回合

	#endregion
	
	#region start/init

	public override void Start()
	{
		base.Start(); //将父类初始化
		allowInput = false;
		currPlayerTurn = 0;
		state = State.Init;
	}

	private void SpawnRandomUnits(int count) //随机生成
	{
		for (int i = 0; i < count; i++)
		{
			// 随机选中一个地图块
			int r = Random.Range(0, unitFabs.Length);
			Unit unitFab = unitFabs[r].GetComponent<Unit>();

			// 找到一个未被占用的地图块
			int tries = 0;
			TileNode node = null;
			while (node == null)
			{
				r = Random.Range(0, map.Length);
				if (unitFab.CanStandOn(map[r], true))
				{
					node = map[r];
				}
				tries++;
				if (tries > 10) break;
			}

			if (node == null) continue;
			
			// 创建一个玩家控制的单位
			Unit unit = (Unit)Unit.SpawnUnit(unitFab.gameObject, node);
			unit.Init(OnUnitEvent);

            
            //unit.transform.position.Set(0, 0, 0);
            units[unit.playerSide-1].Add(unit);
		}
	}

	#endregion

	#region update/input

	public void Update()
	{
        /*
        foreach (Unit u in units[0])
        {
                units[0].Remove(u);
        }

        foreach (Unit u in units[1])
        {
                units[0].Remove(u);
        }
        units[0].Clear();*/
        if (state == State.Running) // 状态为移动
		{
            // 控制血量为0单位消失
            foreach(Unit u in units[0])
            {

                if (u.HP <= 0)
                {
                    
                    //units[0].Remove(u);
                    //Destroy(u);
                    //u.node.units.Remove(u);
                    float x = u.transform.position.x;
                    float y = u.transform.position.y;
                    float z = u.transform.position.z;
                    u.transform.localScale = new Vector3(0, 0, 0);
                    //u.transform.position = new Vector3(x, y+100, z);
                }
                    //u.transform.position.y = (10000);
                
                    //Destroy(u);
                    //u.enabled = false;
            }

            foreach (Unit u in units[1])
            {
                if (u.HP <= 0)
                {
                   // units[0].Remove(u);
                    //Destroy(u);
                    //u.node.units.Remove(u);
                    float x = u.transform.position.x;
                    float y = u.transform.position.y;
                    float z = u.transform.position.z;
                    u.transform.localScale = new Vector3(0, 0, 0);
                    //u.transform.position = new Vector3(x, y+100, z);
                }
            }

			//if (allowInput) this.HandleInput(); //测试

		}

		else if (state == State.Init) // 初始化
		{
			state = State.Running;
			SpawnRandomUnits(spawnCount);
			allowInput = true;
		}
	}

	#endregion
	
	#region pub

	public void ChangeTurn() //改变当前玩家
	{
		currPlayerTurn = (currPlayerTurn == 0 ? 1 : 0);

		OnClearNaviUnitSelection(null); //使所有单位变为未选中

		foreach (Unit u in units[currPlayerTurn]) // 将所有单位的状态重置
		{
			u.Reset();
		}
	}

	#endregion
	
	#region input handlers - click tile

	protected override void OnTileNodeClick(GameObject go)
	{
		base.OnTileNodeClick(go);
		TileNode node = go.GetComponent<TileNode>();
		if (selectedUnit != null && node.IsVisible) //可移动
		{
			prevNode = selectedUnit.node; 
			if (selectedUnit.MoveTo(map, node, ref selectedUnit.currMoves))
			{
				allowInput = false;

				if (hideMarkersOnMove) prevNode.ShowNeighbours(((Unit)selectedUnit).maxMoves, false); //移动并隐藏节点

				if (hideSelectorOnMove) selectionMarker.Hide();// 隐藏选择器
				if (combatOn) attackRangeMarker.HideAll(); //隐藏攻击范围

				camMover.Follow(selectedUnit.transform); // 主摄像机跟随
			}
		}
	}

	protected override void OnTileNodeHover(GameObject go)
	{
		base.OnTileNodeHover(go);
		if (go == null) //防止移动方向时，单位发生移动
		{
			if (hoverNode != null)
			{
				hoverNode.OnHover(false);
				hoverNode = null;
			}
			return;
		}

		TileNode node = go.GetComponent<TileNode>();
		if (selectedUnit != null && node.IsVisible)
		{
			if (hoverNode != node)
			{
				if (hoverNode != null) hoverNode.OnHover(false);
				hoverNode = node;
				node.OnHover(true);
			}
		}
		else if (hoverNode != null)
		{
			hoverNode.OnHover(false);
			hoverNode = null;
		}
	}

	#endregion
	
	#region input handlers - click unit

	protected override void OnNaviUnitClick(GameObject go)
	{
		base.OnNaviUnitClick(go);

		Unit unit = go.GetComponent<Unit>();
		camMover.Follow(go.transform);

		if (useTurns)
		{
			if (unit.playerSide == (currPlayerTurn + 1)) //判断为移动
			{
				selectedUnit = go.GetComponent<Unit>();
				selectionMarker.Show(go.transform); //选中
                
				selectedUnit.node.ShowNeighbours(selectedUnit.currMoves, selectedUnit.tileLevel, true, true); // 展开移动地图

				if ( !selectedUnit.didAttack && combatOn) //攻击范围
				{
					attackRangeMarker.Show(selectedUnit.transform.position, selectedUnit.attackRange);
				}
			}
			else if (selectedUnit!=null && combatOn) // 判断为攻击
			{
				if (selectedUnit.Attack(unit))
				{
					allowInput = false;
					attackRangeMarker.HideAll();
				}
			}
		}
		else 
		{
			bool changeUnit = true;
           
			if (selectedUnit != null && combatOn) //先检查攻击
			{
				if (selectedUnit.Attack(unit))
				{
					changeUnit = false;
					allowInput = false;
					selectedUnit.didAttack = false;
					attackRangeMarker.HideAll();
				}
			}

			if (changeUnit)
			{
				selectedUnit = unit;
				selectionMarker.Show(go.transform);
				selectedUnit.node.ShowNeighbours(selectedUnit.currMoves, selectedUnit.tileLevel, true, true);
				if (combatOn) attackRangeMarker.ShowOutline(selectedUnit.transform.position, selectedUnit.attackRange);
				if (!selectedUnit.didAttack && combatOn)
				{
					attackRangeMarker.Show(selectedUnit.transform.position, selectedUnit.attackRange);
				}
			}
		}
	}

	protected override void OnClearNaviUnitSelection(GameObject clickedAnotherUnit) // 清除选中
	{
		base.OnClearNaviUnitSelection(clickedAnotherUnit);
		bool canClear = true;

		if (clickedAnotherUnit != null && selectedUnit != null) // 判断清除目标是否正确
		{
			Unit unit = clickedAnotherUnit.GetComponent<Unit>();
			if (useTurns)
			{
				if (unit.playerSide != selectedUnit.playerSide) canClear = false;
			}
        	else
			{
				if (selectedUnit.CanAttack(unit)) canClear = false;
			}
		}
        
		if (canClear)
		{
			selectionMarker.Hide();
			if (combatOn) attackRangeMarker.HideAll(); //隐藏目标标记

			if (selectedUnit != null)
			{
				selectedUnit.node.ShowNeighbours(((Unit)selectedUnit).maxMoves, false); // 隐藏行走区域
				selectedUnit = null;
			}
			else
			{
				map.ShowAllTileNodes(false);
			}
		}
	}

	#endregion

	#region callbacks
    
	private void OnUnitEvent(NaviUnit unit, int eventCode)
	{
		// eventcode 1 = 完成移动
		if (eventCode == 1)
		{
			if (!useTurns) // 不是你的回合
			{
				Unit u = (unit as Unit);
				u.currMoves = u.maxMoves;
			}

			if (!hideMarkersOnMove && prevNode != null) // 无效移动
			{	
				prevNode.ShowNeighbours(((Unit)selectedUnit).maxMoves, false);
			}

			this.OnNaviUnitClick(unit.gameObject); // 重复选中
			allowInput = true; 
		}

		// eventcode 2 = 攻击完成
		if (eventCode == 2)
		{
			allowInput = true; 
			if (!useTurns)
			{
				this.OnNaviUnitClick(unit.gameObject);
			}
		}
	}

	#endregion
}
