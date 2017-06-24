
using UnityEngine;

public class CameraOrbit : MonoBehaviour 
{
    // 跟随object
    public Transform pivot;
	// 获取的target的pivot的偏移
    public Vector3 pivotOffset = Vector3.zero;
    // 类似选择一个object(used with checking if objects between cam and target)	
    public Transform target;				

    // target使用到的distance变量 (used with zoom)
	public float distance = 10.0f; 
	public float minDis = 2f;
	public float maxDis = 15f;
	public float SpeedOfZoom = 1f;

    public float SpeedOfX = 250.0f;
    public float SpeedOfY = 120.0f;

	public bool IfAllowYTilt = true;
    public float MinLimOfY = 30f;
    public float MaxLimOfY = 80f;

    private float x = 0.0f;
    private float y = 0.0f;

	private float targetX = 0f;
	private float targetY = 0f;
	private float targetDis = 0f;
	private float xVelocity = 1f;
	private float yVelocity = 1f;
	private float zoomVelocity = 1f;

    void Start()
    {
        var angles = transform.eulerAngles;
        targetX = x = angles.x;
		targetY = y = ClampAngle(angles.y, MinLimOfY, MaxLimOfY);
		targetDis = distance;
    }

    void Update()
    {
        if (pivot)
        {
            //使用鼠标滚轮控制放大缩小（即拉近拉远视角）
			float scroll = Input.GetAxis("Mouse ScrollWheel");

            //按照预定的速度拉近或拉远视角，scroll的正负即滚轮滚动方向
			if (scroll > 0.0f) 
            {
                targetDis -= SpeedOfZoom;
            }
			else if (scroll < 0.0f) 
            {
                targetDis += SpeedOfZoom;
            }
			targetDis = Mathf.Clamp(targetDis, minDis, maxDis);
            //两种方式转动视角
            //一是通过按住鼠标邮件再拖动即可
            //二是通过按住ctrl和鼠标左键进行拖动
			if (Input.GetMouseButton(1) || (Input.GetMouseButton(0) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) ))
            {
                //按照预定的移动速度进行移动
                targetX += Input.GetAxis("Mouse X") * SpeedOfX * 0.02f;

				if (IfAllowYTilt)
				{
					targetY -= Input.GetAxis("Mouse Y") * SpeedOfY * 0.02f;
					targetY = ClampAngle(targetY, MinLimOfY, MaxLimOfY);
				}
            }
			x = Mathf.SmoothDampAngle(x, targetX, ref xVelocity, 0.3f);

			if (IfAllowYTilt)
            {
                y = Mathf.SmoothDampAngle(y, targetY, ref yVelocity, 0.3f);
            }
			else
            {
                y = targetY;
            }
			Quaternion rotation = Quaternion.Euler(y, x, 0);
            //smooth只是用于缓冲移动的效果
			distance = Mathf.SmoothDamp(distance, targetDis, ref zoomVelocity, 0.5f);

            //将变动后的数据应用
			Vector3 position = rotation * new Vector3(0.0f, 0.0f, -distance) + pivot.position + pivotOffset;
			transform.rotation = rotation;
			transform.position = position;

        }
    }

	private float ClampAngle(float angle, float min, float max)
	{
		if (angle < -360) angle += 360;
		if (angle > 360) angle -= 360;
		return Mathf.Clamp(angle, min, max);
	}

}
