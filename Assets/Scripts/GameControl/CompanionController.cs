using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompanionController : MonoBehaviour
{
    public static CompanionController Instance { get { return instance; } }
    private static CompanionController instance;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    public List<CompanionObject> Companions { get { return companions; } }
    private List<CompanionObject> companions = new List<CompanionObject>();

    public CompanionObject AddCompanion()
    {
        CompanionObject companion = (CompanionObject)PoolController.Pop("COMPANION");
        companion.transform.position = Player.Instance.transform.position;
        companion.Init();
        companions.Add(companion);

        return companion;
    }

    public void RemoveCompanion(CompanionObject companion)
    {
        UIController.Instance.RemoveCompanion(companion);
        PoolController.Push(companion.name, companion);
        companions.Remove(companion);
    }
}