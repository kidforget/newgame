
using UnityEngine;

public class CameraMove : MonoBehaviour
{
	public float speed = 10f;
	// 用于跟随target，当其不为NULL时cam固定跟随围绕
    public Transform target;
	//用于判断是否跟随target(target不为null时)
    public bool followTarget = false;
    //用于控制cam是否接受控制输入
	public bool allowInput = true;
	public Transform camTr;
	public Vector2 min_xz;
	public Vector2 max_xz;
	private Transform tr;

	public delegate void CamMaunallyMoved();
	public CamMaunallyMoved OnCamManuallyMoved = null;

	private bool moved = false;

	void Start()
	{
		tr = this.transform;
		if (target && followTarget) tr.position = target.position;
	}

    void OnGUI()
    {
        //用于在屏幕右下角生成控制视角移动的"W","S","A","D"按钮
        //并设定按下按钮时的反应
        if (GUI.Button(new Rect(Screen.width-400, Screen.height-150, 150f, 150f), "A")) {
            moved = true;
            Translate(Vector3.left * Time.deltaTime * speed * 10);
        };
        if (GUI.Button(new Rect(Screen.width - 250, Screen.height - 300, 150f, 150f), "W"))
        {
            moved = true;
            Translate(Vector3.forward * Time.deltaTime * speed * 10);
        };
        if (GUI.Button(new Rect(Screen.width - 250, Screen.height - 150, 150f, 150f), "S"))
        {
            moved = true;
            Translate(Vector3.back * Time.deltaTime * speed * 10);
        };
        if (GUI.Button(new Rect(Screen.width-100, Screen.height - 150, 150f, 150f), "D"))
        {
            moved = true;
            Translate(Vector3.right * Time.deltaTime * speed * 10);
        };

    }

    void Update()
	{
		if (Input.anyKey && allowInput)
		{
            //先将moved的状态还原为false
			moved = false;
            //利用键盘方向键控制视角移动
			if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
            { 
                moved = true; Translate(Vector3.forward * Time.deltaTime * speed);
            }
			if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
            { 
                moved = true; Translate(Vector3.back * Time.deltaTime * speed);
            }
			if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
            { 
                moved = true; Translate(Vector3.left * Time.deltaTime * speed);
            }
			if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
            {
                moved = true; Translate(Vector3.right * Time.deltaTime * speed); 
            }

            //根据输入控制更改对象的状态
			if (OnCamManuallyMoved != null && moved)
			{
				Vector3 pos = tr.position;
				if (pos.x < min_xz.x)
                {
                    pos.x = min_xz.x;
                }
				if (pos.x > max_xz.x)
                {
                    pos.x = max_xz.x;
                }
				if (pos.z < min_xz.y)
                {
                    pos.z = min_xz.y;
                }
				if (pos.z > max_xz.y)
                {
                    pos.z = max_xz.y;
                }
				tr.position = pos;

                //回调
				OnCamManuallyMoved();
			}
		}
	}

	void LateUpdate()
	{
        //更新所有状态
		if (target && followTarget)
		{
			Vector3 difference = target.position - tr.position;
			tr.position = Vector3.Slerp(tr.position, target.position, Time.deltaTime * Mathf.Clamp(difference.magnitude, 0f, 2f));
		}
	}

	private void Translate(Vector3 pos)
	{
        //如果手动移动了，则停止跟随mode
		followTarget = false;

        //如果按住shift键则按照双倍速度移动
		if (Input.GetKey(KeyCode.RightShift) || Input.GetKey(KeyCode.LeftShift))
        {
            pos *= 2.5f;
        }

		// 应用更改后的数据
		Vector3 r = camTr.eulerAngles;
		r.x = 0; tr.position += Quaternion.Euler(r) * pos;
	}

    //定义Follow内容
	public void Follow(bool doFollowCurrentTarget)
	{
		followTarget = doFollowCurrentTarget;
	}

	public void Follow(Transform t)
	{
		target = t;
		followTarget = true;
	}
}
