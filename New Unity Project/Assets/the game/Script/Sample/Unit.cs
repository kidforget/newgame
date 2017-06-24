using System.Collections;
using UnityEngine;

public class Unit : NaviUnit
{
	public int playerSide = 1;			
	public int maxMoves = 1;			
	public int attackRange = 1;			
	public int attackDamage = 1;		
	public Vector3 targetingOffset = Vector3.zero; 
    public int HP = 100;

	[HideInInspector]
	public int currMoves = 0;			
	public bool didAttack { get; set; }
	private SampleWeapon weapon;

	public override void Start()
	{
		base.Start();
		weapon = gameObject.GetComponent<SampleWeapon>();
		weapon.Init(OnAttackDone);
	}
	public override void Init(UnitEventDelegate callback)
	{
		base.Init(callback);
		this.Reset();		
	}
    // 回合结束，重置信息
	public void Reset()
	{
		currMoves = maxMoves;
		didAttack = false;
	}
    // 本回合可以攻击
	public bool CanAttack(Unit target)
	{
		if (didAttack) return false; 
		if (target.playerSide == this.playerSide) return false; 
		if (this.node.units.Contains(target)) return false; 
		return this.node.IsInRange(target.node, this.attackRange);
	}
    // 攻击
	public bool Attack(Unit target)
	{
		if (!CanAttack(target)) return false;

		didAttack = true;
		Vector3 direction = target.transform.position - transform.position; direction.y = 0f;
		transform.rotation = Quaternion.LookRotation(direction);

		weapon.Play(target);
        int del = Random.Range(30, 40);
        target.UnderAttack(del);
		return true;
	}
    private int ddd;

    // 延迟执行，和攻击弹道同步
    IEnumerator MyMethod()
    {
        Debug.Log("Before Waiting 2 seconds");
        yield return new WaitForSeconds(1);
        Debug.Log("After Waiting 2 Seconds");
        HP -= ddd;
        if (HP <= 0)
        {
            HP = 0;
        }
    }
    // 受到攻击
    public void UnderAttack(int del)
    {
        ddd = del;
        StartCoroutine("MyMethod");
        //MyMethod();
        //for (int i = 1; i <= 200000000; i++) tot++;
        if (HP <= 0)
        {
            HP = 0;
            //this.node.units.Remove(this);
            //transform.position = new Vector3(0, 0, 0);
            //float x = transform.position.x;
            //float y = transform.position.y;
            //float z = transform.position.z;
            //transform.position.Set(x, y+100, z);
        }
    }

	private void OnAttackDone(NaviUnit unit, int eventCode)
	{
		if (onUnitEvent != null) onUnitEvent(this, 2);
	}
}
