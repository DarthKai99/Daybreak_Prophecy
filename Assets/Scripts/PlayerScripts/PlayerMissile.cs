using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMissile : MonoBehaviour
{
	[SerializeField] private GameObject MissilePrefab;
	[SerializeField] private float spawnDistance = 0.5f;
	[SerializeField] private float missileLifetime = 3f;
	[SerializeField] private int missileDamage = 1;
	[SerializeField] private Key launchKey  = Key.M;
	//Reference the player's movement for direction
	private Player_movement mover;
	private void Awake(){
		//References the player's movement component when initialize
		mover = GetComponent<Player_movement>();
	}
	//The shooting call
	private void Update(){
		if (Keyboard.current != null && Keyboard.current[launchKey].wasPressedThisFrame)
			Fire();
	}
	//The firing logic
	private void Fire(){
		//Prevents firing if Prefab isn't available.
		if (!MissilePrefab) return;
		//The direction. If the player hasn't moved, defaults to right.
		Vector2 dir = mover ? (mover.FacingDir == Vector2.zero ? Vector2.right : mover.FacingDir) : Vector2.right;
		dir = dir.sqrMagnitude > 0.001f ? dir.normalized : Vector2.right;
		Vector3 spawnPos = transform.position + (Vector3)dir * spawnDistance;
		var go = Instantiate(MissilePrefab, spawnPos, Quaternion.identity);
		var homing = go.GetComponent<HomingMissile2D>();
		if (homing != null)
			homing.Prime(missileDamage, missileLifetime, dir);
	}

}
