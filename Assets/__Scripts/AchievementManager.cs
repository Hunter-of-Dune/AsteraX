using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.UI;


public class AchievementManager : MonoBehaviour
{
    static private AchievementManager _S; // A private singleton for AchievementManager

    static private AchievementManager S
    {
        get
        {
            if (_S == null)
            {
                Debug.LogError("AchievementManager:S getter - Attempt to get "
                               + "value of S before it has been set.");
                return null;
            }
            return _S;
        }
        set
        {
            if (_S != null)
            {
                Debug.LogError("AchievementManager:S setter - Attempt to set S "
                               + "when it has already been set.");
            }
            _S = value;
        }
    }

    [SerializeField] private Text achievementPopupName;
    [SerializeField] private Text achievementPopupDescription;

    [Header("Set in Inspector")]
    public List<Achievement> achievements = new List<Achievement>();

    static public Dictionary<string, Achievement> AchievementDictionary = new Dictionary<string, Achievement>();

    private bool activePopup = false;
    private List<Achievement> popupBuffer = new List<Achievement>();

    private void Awake()
    {
        S = this;

        foreach (Achievement a in achievements)
        {
            if (!AchievementDictionary.ContainsKey(a.Name))
            {
                AchievementDictionary[a.Name] = a;
            }
        }
    }

    internal static void LoadDataFromSaveFile(SaveFile saveFile)
    {
        foreach (Achievement a in saveFile.achievements)
        {
            AchievementDictionary[a.Name] = a;
        }
    }

    public static Achievement[] GetAchievements()
    {
        List<Achievement> ach = new List<Achievement>();
        foreach (Achievement a in AchievementDictionary.Values)
        {
            ach.Add(a);
        }
        return ach.ToArray();
    }

    private void Start()
    {
        Bullet.HIT += BulletHitAchievements;
        Bullet.FIRED += BulletFiredAchievements;
        AsterX.SCORE_CHANGED += ScoreAchievements;
        AsterX.ASTEROID_COUNT += AsteroidAmountAchievements;
        PlayerShip.Timer += TimeAchievements;
    }



    private void OnDisable()
    {
        Bullet.HIT -= BulletHitAchievements;
        Bullet.FIRED -= BulletFiredAchievements;
        AsterX.SCORE_CHANGED -= ScoreAchievements;
        AsterX.ASTEROID_COUNT -= AsteroidAmountAchievements;
        PlayerShip.Timer -= TimeAchievements;
    }

    private void Popup(Achievement a)
    {
        // save user data when they earn a new achievement
        SaveGameManager.Save(false);

        AnalyticsEvent.AchievementUnlocked(a.Name, new Dictionary<string, object>
        {
            {"User Info", "Player: " +AsterX.Instance.playerName+ ", Chiev: "  +a.Name+ ", Time: " +DateTime.Now}
        });
        Analytics.FlushEvents();

        if (activePopup)
        {
            popupBuffer.Add(a);
            return;
        }
        else
        {
            activePopup = true;
            achievementPopupName.text = a.Name;
            achievementPopupDescription.text = a.Description;
            AsterX.Instance.animator.SetTrigger("gotAchievement");
        }

    }

    internal static void ResetAchievements()
    {
        AchievementDictionary.Clear();
        foreach (Achievement a in S.achievements)
        {
            if (!AchievementDictionary.ContainsKey(a.Name))
            {
                AchievementDictionary[a.Name] = a;
            }
        }
    }

    public void PopupDone()
    {
        activePopup = false;
        if (popupBuffer.Count > 0)
        {
            Debug.Log("Found popup in buffer.");
            Popup(popupBuffer[0]);
            popupBuffer.RemoveAt(0);
        }
    }

    public void BulletHitAchievements(bool luckyshot, int hitInRow)
    {
        // First dust
        if (!AchievementDictionary["FIRST DUST"].Complete)
        {
            AchievementDictionary["FIRST DUST"].Complete = true;
            Popup(AchievementDictionary["FIRST DUST"]);
        }

        if (luckyshot)
        {
            AchievementDictionary["SPRAY AND PRAY"].Counter++;
            if (!AchievementDictionary["LUCKY SHOT"].Complete)
            {
                AchievementDictionary["LUCKY SHOT"].Complete = true;
                Popup(AchievementDictionary["LUCKY SHOT"]);
            }
            if (!AchievementDictionary["SPRAY AND PRAY"].Complete && AchievementDictionary["SPRAY AND PRAY"].ReachedTarget)
            {
                AchievementDictionary["SPRAY AND PRAY"].Complete = true;
                Popup(AchievementDictionary["SPRAY AND PRAY"]);
            }
        }

        if (!AchievementDictionary["EAGLE EYE"].Complete && hitInRow >= AchievementDictionary["EAGLE EYE"].Target)
        {
            AchievementDictionary["EAGLE EYE"].Complete = true;
            Popup(AchievementDictionary["EAGLE EYE"]);
        }
    }

    private void BulletFiredAchievements()
    {
        AchievementDictionary["TRIGGER HAPPY"].Counter++;
        if (!AchievementDictionary["TRIGGER HAPPY"].Complete && AchievementDictionary["TRIGGER HAPPY"].ReachedTarget)
        {
            AchievementDictionary["TRIGGER HAPPY"].Complete = true;
            Popup(AchievementDictionary["TRIGGER HAPPY"]);
        }
    }

    private void ScoreAchievements(int score)
    {
        if (!AchievementDictionary["ROOKIE PILOT"].Complete && score >= AchievementDictionary["ROOKIE PILOT"].Target)
        {
            AchievementDictionary["ROOKIE PILOT"].Complete = true;
            Popup(AchievementDictionary["ROOKIE PILOT"]);
        }

        if (!AchievementDictionary["EXPERT PILOT"].Complete && score >= AchievementDictionary["EXPERT PILOT"].Target)
        {
            AchievementDictionary["EXPERT PILOT"].Complete = true;
            Popup(AchievementDictionary["EXPERT PILOT"]);
        }

        if (!AchievementDictionary["JEDI MASTER"].Complete && score >= AchievementDictionary["JEDI MASTER"].Target)
        {
            AchievementDictionary["JEDI MASTER"].Complete = true;
            Popup(AchievementDictionary["JEDI MASTER"]);
        }
    }

    public void TimeAchievements(float timer)
    {
        if (!AchievementDictionary["PACIFIST"].Complete && timer >= AchievementDictionary["PACIFIST"].Target)
        {
            AchievementDictionary["PACIFIST"].Complete = true;
            Popup(AchievementDictionary["PACIFIST"]);
        }
    }

    private void AsteroidAmountAchievements(int rootAsteroids)
    {
        if (!AchievementDictionary["MORE THE MERRIER"].Complete && rootAsteroids >= AchievementDictionary["MORE THE MERRIER"].Target)
        {
            AchievementDictionary["MORE THE MERRIER"].Complete = true;
            Popup(AchievementDictionary["MORE THE MERRIER"]);
        }
    }

}
