using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Telescope : MonoBehaviour
{
    [SerializeField]
    Sprite cursorCenter;
    [SerializeField]
    Sprite cursorLeft;
    [SerializeField]
    Sprite cursorRight;
    
    GameObject cursorBegin;
    Vector2 cursorOffset;
    Vector3 cursorScale;
    
    [SerializeField]
    GameObject telescope1;
    GameObject telescope2;
    GameObject[] telescopes;
    Sprite sprite1;
    Sprite sprite2;
    float telescopeOffsetX;

    const float dragSpeedMultiplier = 0.01f;
    float dragSpeed;

    [SerializeField]
    GameObject zonesFolder;

    [SerializeField]
    Animator fadeAnimator;
    [SerializeField]
    float fadeAnimHalfTime;
    bool firstAnim;

    void Start()
    {
        cursorOffset = new Vector2(cursorCenter.texture.width / 2, cursorCenter.texture.height / 2);
        cursorScale = new Vector3(1.5f, 1.5f, 0);

        GameObject completeZone1 = telescope1.transform.GetChild(0).gameObject;
        sprite1 = completeZone1.transform.GetComponent<SpriteRenderer>().sprite;
        telescopeOffsetX = sprite1.texture.width * completeZone1.transform.localScale.x / sprite1.pixelsPerUnit;

        telescope2 = Instantiate(telescope1, transform);
        telescope2.name = "Telescope2";
        sprite2 = telescope2.transform.GetComponentInChildren<SpriteRenderer>().sprite;

        Vector3 telescope2Pos = telescope1.transform.position;
        telescope2Pos.x = telescopeOffsetX;
        telescope2.transform.position = telescope2Pos;

        telescopes = new GameObject[2];
        telescopes[0] = telescope1;
        telescopes[1] = telescope2;

        dragSpeed = 0;

        firstAnim = true;
    }

    public void BeginDrag(Vector3 beginPos)
    {
        cursorBegin = new GameObject("CursorBegin");
        beginPos.y = 0;
        cursorBegin.transform.position = beginPos;
        cursorBegin.transform.rotation = Quaternion.Euler(90, 0, 0);
        cursorBegin.transform.localScale = cursorScale;
        SpriteRenderer renderer = cursorBegin.AddComponent<SpriteRenderer>();
        renderer.sprite = cursorCenter;
    }

    public void EndDrag()
    {
        dragSpeed = 0;
        Destroy(cursorBegin);
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    public void UpdateSpeed(float speed)
    {
        dragSpeed = speed * dragSpeedMultiplier;
        if (dragSpeed == 0)
            Cursor.SetCursor(cursorCenter.texture, cursorOffset, CursorMode.Auto);
        else if (dragSpeed < 0)
            Cursor.SetCursor(cursorLeft.texture, cursorOffset, CursorMode.Auto);
        else
            Cursor.SetCursor(cursorRight.texture, cursorOffset, CursorMode.Auto);
    }

    void Update()
    {
        Vector3 move = new Vector3(dragSpeed * Time.deltaTime, 0, 0);
        telescope1.transform.position += move;
        telescope2.transform.position += move;
        
        if (dragSpeed < 0 && telescopes[1].transform.position.x <= 0)
        {
            Vector3 newPos = telescopes[0].transform.position;
            newPos.x = telescopes[1].transform.position.x + telescopeOffsetX;
            telescopes[0].transform.position = newPos;
            SwapTelescopes();
        }
        else if (dragSpeed > 0 && telescopes[0].transform.position.x >= 0)
        {
            Vector3 newPos = telescopes[1].transform.position;
            newPos.x = telescopes[0].transform.position.x - telescopeOffsetX;
            telescopes[1].transform.position = newPos;
            SwapTelescopes();
        }
    }

    void SwapTelescopes()
    {
        GameObject temp = telescopes[0];
        telescopes[0] = telescopes[1];
        telescopes[1] = temp;
    }

    public IEnumerator ChangeSprite(Sprite sprite)
    {
        if (firstAnim)
        {
            firstAnim = false;
            sprite1 = sprite;
            sprite2 = sprite;
            fadeAnimator.Play("Base Layer.TelescopeFadeOut");
        }
        else
        {
            fadeAnimator.Play("Base Layer.TelescopeFadeInOut");
            yield return new WaitForSeconds(fadeAnimHalfTime);
            sprite1 = sprite;
            sprite2 = sprite;
        }
    }
}
