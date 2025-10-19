using System;
using UnityEngine;
//Handles character gealth logic in the battle system
//Tracks HP, triggers events when HP changes or reaches zero.
//Used by CharacterBattle to manage taking damage and dying
public class HealthSystem : MonoBehaviour
{
//Events: Allow other scripts to react to health updates.
//OnHealthChanged: Called when HP value changes
//OnDead: Called when HP reaches 0
    public event EventHandler OnHealthChanged;
    public event EventHandler OnDead;
// Maximum and currenr Health values
    private readonly int healthmax;
    private int health;
//Constructor: Sets up initial health when the object is created
    public HealthSystem(int healthmax)
    {
        this.healthmax = healthmax; //Store max HP
        health = healthmax; //Start at full health
    }
//Manually set health to a new value (for healing or debugging)
//and trigger the OnHealthChanged event
    public void SetHealAmount(int health)
    {
        this.health = health;
        OnHealthChanged?.Invoke(this, EventArgs.Empty); //Safely notify listeners
    }
//Returns the ratio of current health to max health.
//Useful for health bars or UI fill amounts.
    public float SetHealthAmount()
    {
        return (float)health / healthmax;
    }
//Returns the raw integer value of current HP.
    public int GetHealAmount()
    {
        return health;
    }
//Apply dammage to this unit.
//subtract hp
//clamp to zero
//Trigger health update event
//Call Die() if HP <= 0
    public void Damage(int amount)
    {
        health -= amount;
        if (health < 0) health = 0;

        OnHealthChanged?.Invoke(this, EventArgs.Empty);

        if (health <= 0)
        {
            Die(); //Character has died
        }
    }
//Handles death logic
//The event can trigger death animations or battle-end logic
    private void Die()
    {
        OnDead?.Invoke(this, EventArgs.Empty);
    }
//Returns trie if HP <= 0
//Used by BattleHandler to detect victory/defeat
    public bool IsDead()
    {
        return health <= 0;
    }
}
