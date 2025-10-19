using UnityEngine;
using System;

public class BattleHandler : MonoBehaviour
{
	private static BattleHandler instance;
	public static BattleHandler GetInstance(){
		return instance;
	}
	//Prefab and art references
	//pfCharacterBattle is the prefab that has CharacterBattle + CharacterBase on it
	//playerSpritesheet/enemtSprite sheet are the front-facing textures we swap in the runtime
	[SerializeField] private Transform pfCharacterBattle;
	public Texture2D enemySpritesheet;
	public Texture2D playerSpritesheet;
	//Simple front-facing layout.
	[Header("Front-Facing Layout")]
	public Vector3 playerSpawnPos = new Vector3(0f, -2f, 0f);
	public Vector3 enemySpawnPos = new Vector3(0f, 1.5f, 0f);
	// Runtime references to the two fighters.
	private CharacterBattle playerCharacterBattle;
	private CharacterBattle enemyCharacterBattle;
	//Track who's turn it is.
	private CharacterBattle activeCharacterBattle;
	//Turn-state for input gating
	//WaitingForPlayer = waiting for player to press space
	//Busy = an attack animation/callback is still running
	private enum State{ WaitingForPlayer, Busy }
	private State state;
	//Set the singleton on load
	private void Awake(){ instance = this; }
	//Scene setup: spawn both characters, make it the player's turn
	private void Start(){
		playerCharacterBattle = SpawnCharacter(true, playerSpawnPos);
		enemyCharacterBattle = SpawnCharacter(false, enemySpawnPos);
		SetActiveCharacterBattle(playerCharacterBattle);
		state = State.WaitingForPlayer;
	}

	//Turn input loop:
	//Only active when WaitingForPlayer
	//Press space to make the player attack, then hand control to AI
	private void Update()
	{
		if (state == State.WaitingForPlayer){
			if (Input.GetKeyDown(KeyCode.Space)){
				state = State.Busy; //lock input until the action finishes
				playerCharacterBattle.Attack(enemyCharacterBattle, () => {
					ChooseNextActiveCharacter(); //after the player's attack completes, move to the next actor/turn
				});
			}
		}
	}
	//Instantiates a CharacterBattle prefab at a given position
	//and initializes it for the player/enemy side.
	private CharacterBattle SpawnCharacter(bool isPlayerTeam, Vector3 position){
		Transform characterTransform = Instantiate(pfCharacterBattle, position, Quaternion.identity);
		CharacterBattle characterBattle = characterTransform.GetComponent<CharacterBattle>();
		characterBattle.Setup(isPlayerTeam);
		return characterBattle;
	}
	//Small helper to track who's turn it is
	private void SetActiveCharacterBattle(CharacterBattle characterBattle){
		activeCharacterBattle = characterBattle;
	}
	//Turn progression
	//If Battle is over, stop
	//If player just acted, trigger the enemy's AI attack
	//Otherwise, give control back to player
	private void ChooseNextActiveCharacter(){
		if (TestBattleOver()) return;
		
		
		if (activeCharacterBattle == playerCharacterBattle){
			SetActiveCharacterBattle(enemyCharacterBattle);//Enemy's turn: Attack the player, then recurse back to this method
			state = State.Busy;
			enemyCharacterBattle.Attack(playerCharacterBattle, () => {
				ChooseNextActiveCharacter();
			});
		} else {
			SetActiveCharacterBattle(playerCharacterBattle); //Back to player turn: wait for input.
			state = State.WaitingForPlayer;
		}
	}
	//Win/Lose check
	//If player HP <= 0 Enemy wins
	//If enemy HP <= 0 player wins
	//Return true if someone has won.
	private bool TestBattleOver(){
		if(playerCharacterBattle.IsDead()){
			BattleOverWindow.Show_Static("Enemy Wins");
			return true;
		}
		if(enemyCharacterBattle.IsDead()){
			BattleOverWindow.Show_Static("Player Wins");
			return true;
		}
		return false;
	}


}
