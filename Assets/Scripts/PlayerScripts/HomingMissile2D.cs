using UnityEngine;

public class HomingMissile2D : MonoBehaviour
{
	[SerializeField] private float speed = 6f;
	[SerializeField] private float turnRateDegPerSec = 360f;
	[SerializeField] private float lifeTime = 3f;
	[SerializeField] private float seekRadius = 8f;
	[SerializeField] private LayerMask targetMask;
	[SerializeField] private int damage = 1;
	private Rigidbody2D rb;
	private Transform target;
	private float lifeTimer;
	private Vector2 initialDir = Vector2.right;
	//Initialization data passed from the Launcher
	//Called by PlayerMissile when said thing spawns
	public void Prime(int dmg, float lifetime, Vector2 dir){
		damage = dmg;
		lifeTime = lifetime > 0 ? lifetime : lifeTime;
		//Normalize direction
		initialDir = dir.sqrMagnitude > 0.0001f ? dir.normalized : Vector2.right;
	}
	
	private void Awake(){
		rb = GetComponent<Rigidbody2D>();
		//lifetime counter
		lifeTimer = lifeTime;
		//Give the missile the velocity in the starting direction
		rb.linearVelocity = initialDir * speed;
		//Responsible for finding targets.
		AcquireTarget();
	}
	//Responsible for destroying the missile when time runs out
	private void Update(){
		lifeTimer -= Time.deltaTime;
		if (lifeTimer <= 0f) Destroy(gameObject);
	}
	//Flies towards the target. Will go through a straight line if no target exists
	private void FixedUpdate(){
		if (target == null) {
			rb.linearVelocity = initialDir * speed;
			return;
		}
		Vector2 toTarget = ((Vector2)target.position - rb.position).normalized;
		float angle = Mathf.Atan2(toTarget.y, toTarget.x) * Mathf.Rad2Deg;
		rb.MoveRotation(Mathf.LerpAngle(rb.rotation, angle, turnRateDegPerSec * Time.fixedDeltaTime));
		rb.linearVelocity = rb.transform.right * speed;
	}
	//Responsible for locking onto the closest target.
	private void AcquireTarget(){
		Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, seekRadius, targetMask);
		if (hits.Length == 0) return;
		float minDist = float.MaxValue;
		foreach (var hit in hits){
			float d =(hit.transform.position - transform.position).sqrMagnitude;
			if (d < minDist){
				minDist = d;
				target = hit.transform;
			}
		}
	}
}
