using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Add to gameObjects to toggle active. Set desired active states in the inspector.
/// </summary>
public class ActiveOnlyDuringSomeGameStates : MonoBehaviour
{
    [EnumFlags]
    public AsterX.State activeStates;

    [SerializeField] List<GameObject> managedObjects = new List<GameObject>();

    [SerializeField] bool activeOnlyInEditor;
    
    
    // Start is called before the first frame update
    void Start()
    {
        AsterX.GAME_STATE_CHANGED += GameStateChanged;

        if (activeOnlyInEditor && !Application.isEditor)
        {
            SetListActive(false);
        }
    }

    private void OnDisable()
    {
        AsterX.GAME_STATE_CHANGED -= GameStateChanged;
    }

    void GameStateChanged(AsterX.State newState)
    {
        if (ReturnSelectedElements().Contains((int)newState))
        {
            SetListActive(true);
        }
        else
        {
            SetListActive(false);
        }
    }

    List<int> ReturnSelectedElements()
    {

        List<int> selectedElements = new List<int>();
        for (int i = 0; i < System.Enum.GetValues(typeof(AsterX.State)).Length; i++)
        {
            int layer = 1 << i;
            if (((int)activeStates & layer) != 0)
            {
                selectedElements.Add(i);
            }
        }

        return selectedElements;
    }

    void SetListActive(bool active)
    {
        foreach(GameObject g in managedObjects)
        {
            g.SetActive(active);
        }
    }
}
