using UnityEngine;
using System;
using System.Collections;

public class CharacterBattle : MonoBehaviour
{
	//Component on the same prefab that handles visuals
	private CharacterBase characterBase;
	//Backing health model
	private HealthSystem healthSystem;
	//Tracks which side this actor belongs to
	private bool isPlayerTeam;
	//Cache to tint/flash on hit.
	private Material cachedMat;
	//Grab required component on spawn
	private void Awake(){
		characterBase = GetComponent<CharacterBase>();
	}
	private void Start(){
		
	}
	//One-time setup called by BattleHandler after instantiation
	//Assigns side, swaps the spritesheet/texture, and creates health
	public void Setup(bool isPlayerTeam){
		this.isPlayerTeam = isPlayerTeam;
		//Get the material from the SpriteRenderer via CharacterBase
		var mat = characterBase.GetMaterial();
		//Front-facing RPG: Swap in the correct sheet based on side
		mat.mainTexture = isPlayerTeam
			? BattleHandler.GetInstance().playerSpritesheet 
			: BattleHandler.GetInstance().enemySpritesheet;
		cachedMat = mat;
		//Start with 100 HP(Tweak later)
		healthSystem = new HealthSystem(100);
	}
	//Public combat API
	//Runs a small attack routine against a target, then calls onAttackComplete
	//So the BattleHandler can advance the turn
	public void Attack(CharacterBattle targetCharacterBattle, Action onAttackComplete){
		StartCoroutine(AttackRoutine(targetCharacterBattle, onAttackComplete));
	}
	//Actual attack sequence
	//play a quick slash "anim" on the attacker
	//Small lunge/timing
	//Target plays hit feedback
	//Apply damage
	//Notify turn system via callback
	private IEnumerator AttackRoutine(CharacterBattle target, Action onComplete){
		//Simple visual trigger (Animator trigger inside CharacterBase if present)
		characterBase.SlashAnim();
		//Optional micro-movement(Kept minimal for front-facing look)
		Vector3 start = transform.position;
		transform.position = start;
		//Let the target flash/shake before damage lands
		yield return target.HitEffect();
		//Apply damage amount
		target.Damage(10);
		//Short delay so effects don't feel instantaneous
		yield return new WaitForSeconds(0.1f);
		//Hand control back to BattleHandler
		if (onComplete != null) onComplete();
	}
	//Quick hit feedback
	//Tint to light red and micro-shake the transform briefly.
	private IEnumerator HitEffect(){
		Color original = cachedMat.color;
		Vector3 basePos = transform.position;
		//Light Red Tint
		cachedMat.color = new Color(1f, 0.35f, 0.35f);
		//Micro shake for 12 microseconds
		float t = 0.12f;
		while (t > 0f){
			t -= Time.deltaTime;
			transform.position = basePos + (Vector3)UnityEngine.Random.insideUnitCircle * 0.03f;
			yield return null;
		}
		//Restore
		transform.position = basePos;
		cachedMat.color = original;
	}


	//Health forwarding helpers
	public void Damage(int damageAmount){
		healthSystem.Damage(damageAmount);
	}
	public bool IsDead(){
		return healthSystem.IsDead();
	}
}
