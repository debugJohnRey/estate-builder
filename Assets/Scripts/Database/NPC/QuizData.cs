using UnityEngine;

/// <summary>
/// ABM Academy — Quiz Data ScriptableObject.
/// One asset per subject specialist. Holds all questions for that subject.
///
/// CREATE: Right-click in Project > Create > ABM > Quiz Data
/// NAMING: QuizData_FABM, QuizData_BMath, QuizData_BFin,
///         QuizData_Org, QuizData_Mkt, QuizData_BES
/// </summary>
[CreateAssetMenu(fileName = "QuizData", menuName = "ABM/Quiz Data")]
public class QuizData : ScriptableObject
{
    [Header("Identity")]
    [Tooltip("Must match SpecialistNPCController.npcID exactly")]
    public string npcID;            // fabm | bmath | bfin | org | mkt | bes
    public string subjectName;      // e.g. "Fundamentals of Accountancy"
    public Color subjectColor = Color.white; // badge color per subject

    [Header("Questions")]
    public QuizQuestion[] questions;

    [System.Serializable]
    public class QuizQuestion
    {
        [TextArea(2, 4)]
        public string questionText;

        [Tooltip("Always provide exactly 4 choices")]
        public string[] choices = new string[4];

        [Tooltip("Index of the correct choice (0=A, 1=B, 2=C, 3=D)")]
        [Range(0, 3)]
        public int correctIndex;

        [TextArea(1, 3)]
        [Tooltip("Short explanation shown after the player answers")]
        public string explanation;
    }
}