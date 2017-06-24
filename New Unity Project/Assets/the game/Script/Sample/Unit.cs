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
		return true;
	}
	private void OnAttackDone(NaviUnit unit, int eventCode)
	{
		if (onUnitEvent != null) onUnitEvent(this, 2);
	}
}
