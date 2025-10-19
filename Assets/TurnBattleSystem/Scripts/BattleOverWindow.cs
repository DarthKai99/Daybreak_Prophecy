using UnityEngine;
using UnityEngine.UI;
//Handles the "Battle Over" UI panel shown when one side wins.
//Displays the winner's name(Player Wins or Enemy Wins).
public class BattleOverWindow : MonoBehaviour
{
	//Singleton instance(simple static access)
	//Let's other scripts call BattleOverWindow.Show_Static("Player Wins")
	//without needing a reference in the scene
	private static BattleOverWindow instance;
	//Unity's Awake() runs when the object is first created or loaded.
	//Here we assign the singleton and make sure the UI starts hidden.
	private void Awake(){
		instance = this; //Store a reference to this instance
		gameObject.SetActive(false); //hide the "Battle Over" window at start
	}
	//Internal method to display the window and set the text.
	//Takes a string (winnerString) like "Player Wins" or "Enemy Wins".
	private void Show(string winnerString){
		gameObject.SetActive(true); //Show the UI panel
		transform.Find("winnerText").GetComponent<Text>().text = winnerString; //Locate the Text UI element and display the text
	}
	//Public static helper used by BattleHandler
	//Calls the private Show() function on the active instance.
	public static void Show_Static(string winnerString){
		instance.Show(winnerString);
	}


}
