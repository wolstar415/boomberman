using UnityEngine;

[CreateAssetMenu(fileName = "Character Data", menuName = "Scriptable Object/Character Data", order = 1)]
public class CharacterData : ScriptableObject
{
    public int StartPower;
    public int StartBomb;
    public int StartSpeed;
    public int MaxPower;
    public int MaxBomb;
    public int MaxSpeed;
}
