using UnityEngine;
//This script handles visual behavior for a single character:
//ensures a SpriteRenerer and Animator are present
//Provides access for the material for texture swapping
//Contains a basic animation trigger(slash)
[RequireComponent(typeof(SpriteRenderer))] //Forces every GameObject using this script to have a SpriteRenderer
public class CharacterBase : MonoBehaviour
{
	//References to Unity components for visuals and animation
	private SpriteRenderer spriteRenderer;
	private Animator animator;
	//Called automatically by Unity when the object is initialized
	//Grabs references to require components on this GameObject
	private void Awake(){
		spriteRenderer = GetComponent<SpriteRenderer>(); //Gets the visual component
		animator = GetComponent<Animator>(); //gets the animation controller if attached
	}
	//Returns the material used by this sprite
	//BattleHandler and CharacterBattle use this to swap the player's or enemy's texture
	public Material GetMaterial(){
		return spriteRenderer != null ? spriteRenderer.material : null;
	}
	//Triggers a simple "Slash" animation if an Animator is attached
	//BattleHandler calls this when a character attacks
	public void SlashAnim(){
		if (animator != null){
			animator.SetTrigger("Slash");
		}
	}
}
