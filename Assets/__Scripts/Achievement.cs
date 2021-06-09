using UnityEngine;


/// <summary>
/// Class for storing individual achievement data. Create new achievements in the AchievementManager class.
/// </summary>
[System.Serializable]
public class Achievement
{
    public string Name;
    public string Description;
    public int CounterTarget;

    [Header("Don't set these attributes.")]
    [SerializeField] private bool _got = false;
    [SerializeField] private int _counter = 0;

    public Achievement(string name, string desr)
    {
        Name = name;
        Description = desr;
    }

    public Achievement(string name, string desr, int counterTarget)
    {
        Name = name;
        Description = desr;
        CounterTarget = counterTarget;
    }

    public bool Complete
    {
        get { return _got; }
        set { _got = value; }
    }

    public int Counter
    {
        get { return _counter; }
        set { _counter = value; }
    }

    public int Target
    {
        get { return CounterTarget; }
    }

    public bool ReachedTarget
    {
        get
        { return _counter >= CounterTarget; }
    }
}
