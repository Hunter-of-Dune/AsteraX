using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class PlayPauseButton : MonoBehaviour
{
    [SerializeField] Sprite pause;
    [SerializeField] Sprite play;
    [SerializeField] Image image;

    private void Start()
    {
        AsterX.GAME_STATE_CHANGED += GameStateChanged;
    }

    private void OnDisable()
    {
        AsterX.GAME_STATE_CHANGED -= GameStateChanged;
    }

    private void GameStateChanged(AsterX.State newState)
    {
        if(newState == AsterX.State.PLAY)
        {
            image.sprite = pause;
            image.color = Color.white;
        }
        else if (newState == AsterX.State.PAUSED)
        {
            image.sprite = play;
            image.color = Color.green;
        }
    }

}
