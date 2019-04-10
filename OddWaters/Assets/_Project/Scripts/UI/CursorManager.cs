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
    NAVIGATION_KO,
    HOVER,
    DRAG,
    COUNT
}

public class CursorManager : Singleton<CursorManager>
{
    [SerializeField]
    [Tooltip("Order : DEFAULT - TELESCOPE_PAN_CENTER - TELESCOPE_PAN_LEFT - TELESCOPE_PAN_RIGHT - NAVIGATION_OK - NAVIGATION_ISLAND - NAVIGATION_KO - HOVER - DRAG")]
    Sprite[] cursorSprites;
    Vector2[] cursorOffsets;
    ECursor currentCursor = ECursor.COUNT;

    void Start()
    {
        Vector2 offsetTopLeft = new Vector2(0, 0);
        Vector2 offsetCenter = new Vector2(cursorSprites[0].texture.width / 2, cursorSprites[0].texture.height / 2);
        //Vector2 offsetCenterLeft = new Vector2(0, cursorSprites[0].texture.height / 2);
        //Vector2 offsetCenterRight = new Vector2(cursorSprites[0].texture.width, cursorSprites[0].texture.height / 2);

        cursorOffsets = new Vector2[(int)ECursor.COUNT];
        cursorOffsets[(int)ECursor.TELESCOPE_PAN_CENTER] = offsetCenter;
        cursorOffsets[(int)ECursor.TELESCOPE_PAN_LEFT] = offsetCenter;
        cursorOffsets[(int)ECursor.TELESCOPE_PAN_RIGHT] = offsetCenter;
        cursorOffsets[(int)ECursor.DEFAULT] = offsetTopLeft;
        cursorOffsets[(int)ECursor.NAVIGATION_OK] = offsetTopLeft;
        cursorOffsets[(int)ECursor.NAVIGATION_ISLAND] = offsetTopLeft;
        cursorOffsets[(int)ECursor.NAVIGATION_KO] = offsetTopLeft;

        SetCursor(ECursor.DEFAULT);
    }

    public void SetCursor(ECursor cursor)
    {
        if (currentCursor != cursor)
        {
            Cursor.SetCursor(cursorSprites[(int)cursor].texture, cursorOffsets[(int)cursor], CursorMode.Auto);
            currentCursor = cursor;
        }
    }
}
