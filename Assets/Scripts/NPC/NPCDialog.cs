using UnityEngine;

[CreateAssetMenu(fileName = "NPCDialog", menuName = "NPC/Dialog")]
public class NPCDialog : ScriptableObject
{
    public string npcName;

    [TextArea(2, 5)]
    public string[] lines;
}