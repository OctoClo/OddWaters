using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ECursor
{
    DEFAULT,
    TELESCOPE_PAN_CENTER,
    TELESCOPE_PAN_LEFT,
    TELESCOPE_PAN_RIGHT,
    NAVIGATION_OK,
    NAVIGATION_ISLAND,
    HOVER,
    DRAG,
    COUNT
}

public class CursorManager : Singleton<CursorManager>
{
    [SerializeField]
    [Tooltip("Order : DEFAULT - TELESCOPE_PAN_CENTER - TELESCOPE_PAN_LEFT - TELESCOPE_PAN_RIGHT - NAVIGATION_OK - NAVIGATION_ISLAND - HOVER - DRAG")]
    Texture2D[] cursorSprites;
    Vector2[] cursorOffsets;
    ECursor currentCursor = ECursor.COUNT;

    void Awake()
    {
        DontDestroyOnLoad(transform.gameObject);
    }

    void Start()
    {
        cursorOffsets = new Vector2[(int)ECursor.COUNT];
        cursorOffsets[(int)ECursor.TELESCOPE_PAN_CENTER] = new Vector2(cursorSprites[(int)ECursor.TELESCOPE_PAN_CENTER].width / 2, cursorSprites[(int)ECursor.TELESCOPE_PAN_CENTER].height / 2);
        cursorOffsets[(int)ECursor.TELESCOPE_PAN_LEFT] = new Vector2(cursorSprites[(int)ECursor.TELESCOPE_PAN_LEFT].width / 2, cursorSprites[(int)ECursor.TELESCOPE_PAN_LEFT].height / 2);
        cursorOffsets[(int)ECursor.TELESCOPE_PAN_RIGHT] = new Vector2(cursorSprites[(int)ECursor.TELESCOPE_PAN_RIGHT].width / 2, cursorSprites[(int)ECursor.TELESCOPE_PAN_RIGHT].height / 2);
        cursorOffsets[(int)ECursor.DEFAULT] = Vector2.zero;
        cursorOffsets[(int)ECursor.NAVIGATION_OK] = Vector2.zero;
        cursorOffsets[(int)ECursor.NAVIGATION_ISLAND] = Vector2.zero;

        SetCursor(ECursor.DEFAULT);
    }

    public void SetCursor(ECursor cursor)
    {
        if (currentCursor != cursor)
        {
            Cursor.SetCursor(cursorSprites[(int)cursor], cursorOffsets[(int)cursor], CursorMode.Auto);
            currentCursor = cursor;
        }
    }
}
