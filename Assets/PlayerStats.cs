using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Player Build Stats")]
    [Space(10)]
    [Tooltip(
        "This determines whether or not the player can die.")]
    [SerializeField]
    private bool canDie = true;

    public bool CanDie
    {
        get => canDie;
        set => canDie = value;
    }

    [Tooltip(
        "Max health points that the player starts off with.")]
    [SerializeField]
    private int maxHp = 10;

    public int MaxHp
    {
        get => maxHp;
        set => maxHp = value;
    }

    [Tooltip(
        "Current health points that the player has during the game.")]
    [SerializeField]
    private int currentHp = 10;

    public int CurrentHp
    {
        get => currentHp;
        set => currentHp = value;
    }

    [Tooltip(
        "Damage hit multiplier that determines how much damage the player can do")]
    [SerializeField]
    private int hitMultiplier = 1;

    public int HitMultiplier
    {
        get => hitMultiplier;
        set => hitMultiplier = value;
    }

    void Start()
    {
        currentHp = maxHp;
    }
}