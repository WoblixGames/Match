using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dot : MonoBehaviour
{
    [Header("Board Variables")]
    public int column;
    public int row;
    public int previousColumn;
    public int previousRow;
    public int targetX;
    public int targetY;
    public bool isMatched = false;

    private FindMatches findMatches;
    private Board board;
    public GameObject otherDot;
    private Vector2 firstTouchPosition;
    private Vector2 finalTouchPosition;
    private Vector2 tempPosition;

    [Header("Swipe Stuff")]
    public float swipeAngle = 0;
    public float swipeResist = 1f;

    [Header("Powerup Stuff")]
    public bool isColorBomb;
    public bool isColumnBomb;
    public bool isRowBomb;
    public bool isAdjacentBomb;
    public GameObject adjacentMarker;
    public GameObject rowArrow;
    public GameObject columnArrow;
    public GameObject colorBomb;
    private MoveCounter moveCounter;

    void Start()
    {
        ResetBombs();

        board = FindObjectOfType<Board>();
        findMatches = FindObjectOfType<FindMatches>();
        moveCounter = FindObjectOfType<MoveCounter>();
    }

    private void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(1))
        {
            MakeAdjacentBomb();
        }
    }

    void Update()
    {
        UpdateTargetPosition();

        if (MoveToTargetPosition(ref targetX, ref targetY))
        {
            findMatches.FindAllMatches();
        }
    }

    void UpdateTargetPosition()
    {
        targetX = column;
        targetY = row;
    }

    bool MoveToTargetPosition(ref int targetX, ref int targetY)
    {
        bool moved = false;
        if (Mathf.Abs(targetX - transform.position.x) > .1f)
        {
            MoveToXPosition(targetX);
            moved = true;
        }
        else
        {
            AlignToXPosition(targetX);
        }

        if (Mathf.Abs(targetY - transform.position.y) > .1f)
        {
            MoveToYPosition(targetY);
            moved = true;
        }
        else
        {
            AlignToYPosition(targetY);
        }

        return moved;
    }

    void MoveToXPosition(int targetX)
    {
        tempPosition = new Vector2(targetX, transform.position.y);
        transform.position = Vector2.Lerp(transform.position, tempPosition, .6f);
        UpdateDotPosition();
    }

    void AlignToXPosition(int targetX)
    {
        tempPosition = new Vector2(targetX, transform.position.y);
        transform.position = tempPosition;
        UpdateDotPosition();
    }

    void MoveToYPosition(int targetY)
    {
        tempPosition = new Vector2(transform.position.x, targetY);
        transform.position = Vector2.Lerp(transform.position, tempPosition, .6f);
        UpdateDotPosition();
    }

    void AlignToYPosition(int targetY)
    {
        tempPosition = new Vector2(transform.position.x, targetY);
        transform.position = tempPosition;
        UpdateDotPosition();
    }

    void UpdateDotPosition()
    {
        if (board.allDots[column, row] != this.gameObject)
        {
            board.allDots[column, row] = this.gameObject;
        }
    }

public IEnumerator CheckMoveCo()
{
    if (isColorBomb)
    {
        findMatches.MatchPiecesOfColor(otherDot.tag);
        isMatched = true;
    }
    else if (otherDot.GetComponent<Dot>().isColorBomb)
    {
        findMatches.MatchPiecesOfColor(this.gameObject.tag);
        otherDot.GetComponent<Dot>().isMatched = true;
    }

    yield return new WaitForSeconds(.5f);

    if (otherDot != null)
    {
        if (!isMatched && !otherDot.GetComponent<Dot>().isMatched)
        {
            SwapBack();
        }
        else
        {
            board.DestroyMatches();
            if (moveCounter != null)
            {
                moveCounter.DecreaseMoveCounter(); // Hamle sayısını azalt
            }
        }
    }
}


    private void SwapBack()
    {
        otherDot.GetComponent<Dot>().row = row;
        otherDot.GetComponent<Dot>().column = column;
        row = previousRow;
        column = previousColumn;
        StartCoroutine(SwapDelay());
    }

    private IEnumerator SwapDelay()
    {
        yield return new WaitForSeconds(.5f);
        board.currentDot = null;
        board.currentState = GameState.move;
    }

    private void OnMouseDown()
    {
        if (board.currentState == GameState.move)
        {
            firstTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }
    }

    private void OnMouseUp()
    {
        if (board.currentState == GameState.move)
        {
            finalTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            CalculateAngle();
        }
    }

    void CalculateAngle()
    {
        if (IsSwipe())
        {
            board.currentState = GameState.wait;
            swipeAngle = Mathf.Atan2(finalTouchPosition.y - firstTouchPosition.y, finalTouchPosition.x - firstTouchPosition.x) * 180 / Mathf.PI;
            MovePieces();
            board.currentDot = this;
        }
        else
        {
            board.currentState = GameState.move;
        }
    }

    bool IsSwipe()
    {
        return Mathf.Abs(finalTouchPosition.y - firstTouchPosition.y) > swipeResist || Mathf.Abs(finalTouchPosition.x - firstTouchPosition.x) > swipeResist;
    }

    void MovePieces()
    {
        if (swipeAngle > -45 && swipeAngle <= 45 && column < board.width - 1)
        {
            MovePiecesActual(Vector2.right);
        }
        else if (swipeAngle > 45 && swipeAngle <= 135 && row < board.height - 1)
        {
            MovePiecesActual(Vector2.up);
        }
        else if ((swipeAngle > 135 || swipeAngle <= -135) && column > 0)
        {
            MovePiecesActual(Vector2.left);
        }
        else if (swipeAngle < -45 && swipeAngle >= -135 && row > 0)
        {
            MovePiecesActual(Vector2.down);
        }
        else
        {
            board.currentState = GameState.move;
        }
    }

    void MovePiecesActual(Vector2 direction)
    {
        otherDot = board.allDots[column + (int)direction.x, row + (int)direction.y];
        previousRow = row;
        previousColumn = column;
        if (otherDot != null)
        {
            otherDot.GetComponent<Dot>().column += -1 * (int)direction.x;
            otherDot.GetComponent<Dot>().row += -1 * (int)direction.y;
            column += (int)direction.x;
            row += (int)direction.y;
            StartCoroutine(CheckMoveCo());
        }
        else
        {
            board.currentState = GameState.move;
        }
    }

    void FindMatches()
    {
        CheckForMatches(Vector2.left, Vector2.right);
        CheckForMatches(Vector2.up, Vector2.down);
    }

    void CheckForMatches(Vector2 dir1, Vector2 dir2)
    {
        GameObject dot1 = board.allDots[column + (int)dir1.x, row + (int)dir1.y];
        GameObject dot2 = board.allDots[column + (int)dir2.x, row + (int)dir2.y];

        if (dot1 != null && dot2 != null && dot1.tag == this.gameObject.tag && dot2.tag == this.gameObject.tag)
        {
            dot1.GetComponent<Dot>().isMatched = true;
            dot2.GetComponent<Dot>().isMatched = true;
            isMatched = true;
        }
    }

    public void MakeRowBomb()
    {
        InstantiateBomb(ref isRowBomb, rowArrow);
    }

    public void MakeColumnBomb()
    {
        InstantiateBomb(ref isColumnBomb, columnArrow);
    }

    public void MakeColorBomb()
    {
        InstantiateBomb(ref isColorBomb, colorBomb);
    }

    public void MakeAdjacentBomb()
    {
        InstantiateBomb(ref isAdjacentBomb, adjacentMarker);
    }

    private void InstantiateBomb(ref bool bombType, GameObject bombPrefab)
    {
        bombType = true;
        GameObject bomb = Instantiate(bombPrefab, transform.position, Quaternion.identity);
        bomb.transform.parent = this.transform;
    }

    private void ResetBombs()
    {
        isColumnBomb = false;
        isRowBomb = false;
        isColorBomb = false;
        isAdjacentBomb = false;
    }
}
