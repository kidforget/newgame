using UnityEngine;
using System.Collections.Generic;

public class GameController : TMNController 
{
	#region inspector properties

	public CameraMove camMover;					// �ƶ����
	public SelectionIndicator selectionMarker;	// ����ѡ�е�λ
	public RadiusMarker attackRangeMarker;		// ������������
	public GameObject[] unitFabs;				// ��λ�б�
	public int spawnCount = 8;					// ��λ����
	public bool hideSelectorOnMove = true;		// �ƶ�ʱ��ѡ�еı������
	public bool hideMarkersOnMove = true;       // �ƶ�ʱ��ѡ�еĵ�λ�Ŀ��ƶ����λ�ýڵ�����
    public bool useTurns = true;				// �غ��ƿ���/�ر�
    public bool combatOn = false;				// �Ƿ�ֻ��ִ��һ�ι���

	#endregion

	#region vars

	private enum State : byte { Init = 0, Running, DontRun } //ö�ٿ��Ƶ�λ��״̬
	private State state = State.Init;

	private Unit selectedUnit = null;	// ѡ�еĵ�λ
	private TileNode hoverNode = null;	// ѡ�еĽڵ�
	private TileNode prevNode = null;	// �ƶ�ʱ��һ���ڵ�
     
	public bool allowInput { get; set; } // �Ƿ�������ҿ���

	private List<Unit>[] units = {
		new List<Unit>(),	//  ���һ�ĵ�λ�б�
		new List<Unit>()	//  ��Ҷ��ĵ�λ�б�
	};

	public int currPlayerTurn  { get; set; }		// ȷ��ÿ����ҵĻغϲ���¼��ǰ��ҵĻغ�

	#endregion
	
	#region start/init

	public override void Start()
	{
		base.Start(); //�������ʼ��
		allowInput = false;
		currPlayerTurn = 0;
		state = State.Init;
	}

	private void SpawnRandomUnits(int count) //�������
	{
		for (int i = 0; i < count; i++)
		{
			// ���ѡ��һ����ͼ��
			int r = Random.Range(0, unitFabs.Length);
			Unit unitFab = unitFabs[r].GetComponent<Unit>();

			// �ҵ�һ��δ��ռ�õĵ�ͼ��
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
			
			// ����һ����ҿ��Ƶĵ�λ
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
        if (state == State.Running) // ״̬Ϊ�ƶ�
		{
            // ����Ѫ��Ϊ0��λ��ʧ
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

			//if (allowInput) this.HandleInput(); //����

		}

		else if (state == State.Init) // ��ʼ��
		{
			state = State.Running;
			SpawnRandomUnits(spawnCount);
			allowInput = true;
		}
	}

	#endregion
	
	#region pub

	public void ChangeTurn() //�ı䵱ǰ���
	{
		currPlayerTurn = (currPlayerTurn == 0 ? 1 : 0);

		OnClearNaviUnitSelection(null); //ʹ���е�λ��Ϊδѡ��

		foreach (Unit u in units[currPlayerTurn]) // �����е�λ��״̬����
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
		if (selectedUnit != null && node.IsVisible) //���ƶ�
		{
			prevNode = selectedUnit.node; 
			if (selectedUnit.MoveTo(map, node, ref selectedUnit.currMoves))
			{
				allowInput = false;

				if (hideMarkersOnMove) prevNode.ShowNeighbours(((Unit)selectedUnit).maxMoves, false); //�ƶ������ؽڵ�

				if (hideSelectorOnMove) selectionMarker.Hide();// ����ѡ����
				if (combatOn) attackRangeMarker.HideAll(); //���ع�����Χ

				camMover.Follow(selectedUnit.transform); // �����������
			}
		}
	}

	protected override void OnTileNodeHover(GameObject go)
	{
		base.OnTileNodeHover(go);
		if (go == null) //��ֹ�ƶ�����ʱ����λ�����ƶ�
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
			if (unit.playerSide == (currPlayerTurn + 1)) //�ж�Ϊ�ƶ�
			{
				selectedUnit = go.GetComponent<Unit>();
				selectionMarker.Show(go.transform); //ѡ��
                
				selectedUnit.node.ShowNeighbours(selectedUnit.currMoves, selectedUnit.tileLevel, true, true); // չ���ƶ���ͼ

				if ( !selectedUnit.didAttack && combatOn) //������Χ
				{
					attackRangeMarker.Show(selectedUnit.transform.position, selectedUnit.attackRange);
				}
			}
			else if (selectedUnit!=null && combatOn) // �ж�Ϊ����
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
           
			if (selectedUnit != null && combatOn) //�ȼ�鹥��
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

	protected override void OnClearNaviUnitSelection(GameObject clickedAnotherUnit) // ���ѡ��
	{
		base.OnClearNaviUnitSelection(clickedAnotherUnit);
		bool canClear = true;

		if (clickedAnotherUnit != null && selectedUnit != null) // �ж����Ŀ���Ƿ���ȷ
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
			if (combatOn) attackRangeMarker.HideAll(); //����Ŀ����

			if (selectedUnit != null)
			{
				selectedUnit.node.ShowNeighbours(((Unit)selectedUnit).maxMoves, false); // ������������
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
		// eventcode 1 = ����ƶ�
		if (eventCode == 1)
		{
			if (!useTurns) // ������Ļغ�
			{
				Unit u = (unit as Unit);
				u.currMoves = u.maxMoves;
			}

			if (!hideMarkersOnMove && prevNode != null) // ��Ч�ƶ�
			{	
				prevNode.ShowNeighbours(((Unit)selectedUnit).maxMoves, false);
			}

			this.OnNaviUnitClick(unit.gameObject); // �ظ�ѡ��
			allowInput = true; 
		}

		// eventcode 2 = �������
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
