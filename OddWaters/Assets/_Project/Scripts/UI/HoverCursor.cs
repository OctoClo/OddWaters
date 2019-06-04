﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class HoverCursor : MonoBehaviour, IPointerExitHandler, IPointerEnterHandler
{
    [SerializeField]
    bool forceDefaultCursorOnDisable = false;
    [SerializeField]
    bool playHover = true;

    EventSystem eventSystem;

    void Start()
    {
        eventSystem = EventSystem.current;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (playHover)
            AkSoundEngine.PostEvent("Play_Dots", gameObject);
        CursorManager.Instance.SetCursor(ECursor.HOVER);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        CursorManager.Instance.SetCursor(ECursor.DEFAULT);
    }

    void OnDisable()
    {
        if (forceDefaultCursorOnDisable || !eventSystem.IsPointerOverGameObject())
            CursorManager.Instance.SetCursor(ECursor.DEFAULT);
    }
}
