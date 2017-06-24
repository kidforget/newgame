

using UnityEngine;
using System.Collections;

public class SampleWeapon : MonoBehaviour
{
	public GameObject missileFab;
	public float startOffset = 0f;
	public float fireDelay = 0.6f;
	public float missileSpeed = 3f;
	public float missileHeightGain = 0.3f;

	private Unit.UnitEventDelegate onAttackDone = null;

	public void Init(Unit.UnitEventDelegate callback)
	{
		this.onAttackDone = callback;
	}

	public void Play(Unit target)
	{
		gameObject.SetActiveRecursively(true);

		// 发射炮弹
		Vector3 pos = transform.position + new Vector3(0f, startOffset, 0f);
		Vector3 targetPos = target.transform.position + target.targetingOffset;

		// 炮弹做曲线运动
		Vector3[] path = new Vector3[3];
		float distance = Vector3.Distance(pos, targetPos);
		path[0] = pos;
		path[1] = Vector3.MoveTowards(pos, targetPos, distance / 2.3f);
		path[1].y += missileHeightGain;
		path[2] = targetPos;

		GameObject missileGameObject = (GameObject)GameObject.Instantiate(missileFab, pos, Quaternion.identity);
		iTween.MoveTo(missileGameObject, iTween.Hash(
				"speed", missileSpeed,
				"orienttopath", true,
				"path", path,
				"easetype", "linear",
				"oncomplete", "OnFXMissileReachedTarget",
				"oncompletetarget", gameObject,
				"oncompleteparams", missileGameObject
			));

	}

	private void OnFXMissileReachedTarget(GameObject missile)
	{
		GameObject.Destroy(missile, 0.1f);
		if (onAttackDone != null) onAttackDone(null, 0);
	}


}
