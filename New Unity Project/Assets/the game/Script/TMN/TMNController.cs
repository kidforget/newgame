using UnityEngine;

public abstract class TMNController : MonoBehaviour 
{
    #region inspector properties
    //  main game camera
    public Camera rayCam;
    // the MapNav used by this controller
    public MapNav map;
    #endregion

    #region vars
    //  the selected unit
    private GameObject _selectedUnitGo = null;
    //  the tile node when a mouse is on it
    private GameObject _hoverNodeGo = null;
    //  used in TNEMovementModifier.cs and TNELinksOnOffSwitch.cs to present different UI when conditions are different
    private LayerMask _rayMask = 0;
	#endregion

	#region start/init

	public virtual void Start()
	{
		if (map == null)
		{
            Debug.LogWarning("The map is not set, attempting to find a MapNav in the scene.");
			Object obj = GameObject.FindObjectOfType(typeof(MapNav));
            //  get the map
            if (obj != null) map = obj as MapNav;

            //  fail to get the map
			if (map == null) Debug.LogError("Could not find a MapNav in the scene. You gonna get NullRef errors soon!");
		}

		_rayMask = (1<<map.tilesLayer | 1<<map.unitsLayer);
	}

	#endregion

	#region update/input

	/// <summary>Call this every frame to handle input (detect clicks on units and tiles)</summary>
	protected void HandleInput()
	{
        //  return when no click and no unit is selected
		if (!Input.GetMouseButtonUp(0) && _selectedUnitGo == null) return;

        //  click happened or not
		bool isClick = (Input.GetMouseButtonUp(0) ? true : false);

        //  摄像头的光源点和鼠标单击的光源点形成的射线，摄像头光源点在显示的中心点
		Ray ray = rayCam.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
        //  http://www.manew.com/blog-42778-2527.html
        //  public static bool Raycast(Ray ray, RaycastHit hitInfo, float distance, int layerMask);
        //  hit携带了射线碰撞到那个物体的信息,这里是鼠标所在位置的信息，可以是tile和unit
        if (Physics.Raycast(ray, out hit, 500f, _rayMask))
		{
			//  ray hit a tile
			if (hit.collider.gameObject.layer == map.tilesLayer)
			{
                //  click the mouse
				if (Input.GetMouseButtonUp(0))
                {
					isClick = false;
					OnTileNodeClick(hit.collider.gameObject);
				}
                //  no click
                else
                {
					OnTileNodeHover(hit.collider.gameObject);
				}
			}
			else if (_hoverNodeGo != null)
			{
				OnTileNodeHover(null);
			}

			// ray hit a unit
			if (hit.collider.gameObject.layer == map.unitsLayer)
			{
                //  click the mouse
                if (Input.GetMouseButtonUp(0))
				{
					isClick = false;

					//  clear the previous selection and then select unit
					if (_selectedUnitGo != null)
					{
						OnTileNodeHover(null);
						OnClearNaviUnitSelection(hit.collider.gameObject);
					}
					OnNaviUnitClick(hit.collider.gameObject);
				}
			}
		}
        //  没有发生碰撞，游戏运行时不会发生
		else if (_hoverNodeGo != null)
		{
			OnTileNodeHover(null);
		}		

        //  handle finished
		if (isClick)
		{
			OnTileNodeHover(null);
			OnClearNaviUnitSelection(null);
		}
    }

	//  click tile
    protected virtual void OnTileNodeClick(GameObject nodeGo)
	{
	}

    //  when mouse push over the tile
	protected virtual void OnTileNodeHover(GameObject nodeGo)
	{
		_hoverNodeGo = nodeGo;
	}

    //  click a unit
	protected virtual void OnNaviUnitClick(GameObject unitGo)
	{
		_selectedUnitGo = unitGo;
	}

	//  clear selection
	protected virtual void OnClearNaviUnitSelection(GameObject clickedAnotherUnit)
	{
		_selectedUnitGo = null;
	}

	#endregion
}
