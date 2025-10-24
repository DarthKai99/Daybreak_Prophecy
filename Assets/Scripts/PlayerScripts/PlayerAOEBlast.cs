using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAOEBlast : MonoBehaviour
{
	[Header("AOE")]
	[SerializeField] private GameObject aoePrefab;
	[SerializeField] private float radius = 1.25f;
	[SerializeField] private float lifetime = 0.2f;
	[SerializeField] private int damage = 2;

	[Header("Cost/Timing")]
	[SerializeField] private float cooldown = 1f;
	[SerializeField] private int mpCost = 3;

	[Header("Input")]
	[SerializeField] private Key castKey = Key.Q;
	private PlayerStats stats;
	private float nextCastTime = 0f;
	void Awake(){
		stats = GetComponent<PlayerStats>();
	}
	void Update(){
		if (Keyboard.current == null) return;
		var keyControl = Keyboard.current[castKey];
		if (keyControl != null && keyControl.wasPressedThisFrame)
			TryCastAOE();
	}
	void TryCastAOE(){
		if (Time.time < nextCastTime) return;
		if (stats != null && !stats.UseMP(mpCost)){
			return;
		}
		Vector3 pos = transform.position;
		var go = Instantiate(aoePrefab, pos, Quaternion.identity);
		var circle = go.GetComponent<CircleCollider2D>();
		if (circle != null) circle.radius = radius;
		var hb = go.GetComponent<AttackHitbox>();
		if (hb != null) hb.Init(damage, lifetime, gameObject);
		nextCastTime = Time.time + cooldown;
	}
}
