using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Weapon")]
public class Weapon : ScriptableObject
{
    public string displayName;
    public int damage;
    public GameObject prefab;
    public float cooldownTime;

    [Header("Animation")]
    public Avatar animationAvatar;
    public AnimationClip idleAnimation;
    public AnimationClip attackAnimation;
    public AnimationClip blockAnimation;
}