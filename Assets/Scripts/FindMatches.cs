using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro; // For TextMeshPro
using UnityEngine.UI;

public class FindMatches : MonoBehaviour
{
    private Board board;
    public List<GameObject> currentMatches = new List<GameObject>();

    // Move count variables
    public TextMeshProUGUI moveCountText; // Assign your TextMeshProUGUI component in the Inspector
    private int movesLeft = 20; // Set the initial move count
    private bool playerMadeMove = false; // Flag to check if player made a move

    void Start()
    {
        board = FindObjectOfType<Board>();
        UpdateMoveCountText(); // Update the UI at the start
    }

    public void FindAllMatches()
    {
        StartCoroutine(FindAllMatchesCo());
    }

    // Call this method when the player makes a move
    public void PlayerMadeMove()
    {
        playerMadeMove = true;
         Debug.Log("Oyuncu bir hamle yaptı.");
        FindObjectOfType<MoveCounter>().DecreaseMoveCounter();
    }

    private void UpdateMoveCount()
    {
        if (playerMadeMove)
        {
            movesLeft--;
            if (movesLeft < 0)
            {
                movesLeft = 0; // Prevent negative move count
            }
            UpdateMoveCountText(); // Update the UI
            playerMadeMove = false; // Reset the flag

            if (movesLeft <= 0)
            {
                EndGame(); // Implement your game over logic here
            }
        }
    }

    private void UpdateMoveCountText()
    {
        if (moveCountText != null)
        {
            moveCountText.text = "Moves Left: " + movesLeft;
        }
    }

    private void EndGame()
    {
        // Implement your game over logic here
        Debug.Log("Game Over!");
    }

    private List<GameObject> IsAdjacentBomb(Dot dot1, Dot dot2, Dot dot3)
    {
        List<GameObject> currentDots = new List<GameObject>();
        if (dot1.isAdjacentBomb)
        {
            currentMatches = currentMatches.Union(GetAdjacentPieces(dot1.column, dot1.row)).ToList();
        }

        if (dot2.isAdjacentBomb)
        {
            currentMatches = currentMatches.Union(GetAdjacentPieces(dot2.column, dot2.row)).ToList();
        }

        if (dot3.isAdjacentBomb)
        {
            currentMatches = currentMatches.Union(GetAdjacentPieces(dot3.column, dot3.row)).ToList();
        }
        return currentDots;
    }

    private List<GameObject> IsRowBomb(Dot dot1, Dot dot2, Dot dot3)
    {
        List<GameObject> currentDots = new List<GameObject>();
        if (dot1.isRowBomb)
        {
            currentMatches = currentMatches.Union(GetRowPieces(dot1.row)).ToList();
        }

        if (dot2.isRowBomb)
        {
            currentMatches = currentMatches.Union(GetRowPieces(dot2.row)).ToList();
        }

        if (dot3.isRowBomb)
        {
            currentMatches = currentMatches.Union(GetRowPieces(dot3.row)).ToList();
        }
        return currentDots;
    }

    private List<GameObject> IsColumnBomb(Dot dot1, Dot dot2, Dot dot3)
    {
        List<GameObject> currentDots = new List<GameObject>();
        if (dot1.isColumnBomb)
        {
            currentMatches = currentMatches.Union(GetColumnPieces(dot1.column)).ToList();
        }

        if (dot2.isColumnBomb)
        {
            currentMatches = currentMatches.Union(GetColumnPieces(dot2.column)).ToList();
        }

        if (dot3.isColumnBomb)
        {
            currentMatches = currentMatches.Union(GetColumnPieces(dot3.column)).ToList();
        }
        return currentDots;
    }

    private void AddToListAndMatch(GameObject dot)
    {
        if (!currentMatches.Contains(dot))
        {
            currentMatches.Add(dot);
        }
        dot.GetComponent<Dot>().isMatched = true;
    }

    private void GetNearbyPieces(GameObject dot1, GameObject dot2, GameObject dot3)
    {
        AddToListAndMatch(dot1);
        AddToListAndMatch(dot2);
        AddToListAndMatch(dot3);
    }

    private IEnumerator FindAllMatchesCo()
    {
        yield return new WaitForSeconds(.2f);
        for (int i = 0; i < board.width; i++)
        {
            for (int j = 0; j < board.height; j++)
            {
                GameObject currentDot = board.allDots[i, j];

                if (currentDot != null)
                {
                    Dot currentDotDot = currentDot.GetComponent<Dot>();
                    if (i > 0 && i < board.width - 1)
                    {
                        GameObject leftDot = board.allDots[i - 1, j];
                        GameObject rightDot = board.allDots[i + 1, j];

                        if (leftDot != null && rightDot != null)
                        {
                            Dot rightDotDot = rightDot.GetComponent<Dot>();
                            Dot leftDotDot = leftDot.GetComponent<Dot>();
                            if (leftDot.tag == currentDot.tag && rightDot.tag == currentDot.tag)
                            {
                                currentMatches = currentMatches.Union(IsRowBomb(leftDotDot, currentDotDot, rightDotDot)).ToList();
                                currentMatches = currentMatches.Union(IsColumnBomb(leftDotDot, currentDotDot, rightDotDot)).ToList();
                                currentMatches = currentMatches.Union(IsAdjacentBomb(leftDotDot, currentDotDot, rightDotDot)).ToList();

                                GetNearbyPieces(leftDot, currentDot, rightDot);
                            }
                        }
                    }

                    if (j > 0 && j < board.height - 1)
                    {
                        GameObject upDot = board.allDots[i, j + 1];
                        GameObject downDot = board.allDots[i, j - 1];

                        if (upDot != null && downDot != null)
                        {
                            Dot upDotDot = upDot.GetComponent<Dot>();
                            Dot downDotDot = downDot.GetComponent<Dot>();
                            if (upDot.tag == currentDot.tag && downDot.tag == currentDot.tag)
                            {
                                currentMatches = currentMatches.Union(IsColumnBomb(upDotDot, currentDotDot, downDotDot)).ToList();
                                currentMatches = currentMatches.Union(IsRowBomb(upDotDot, currentDotDot, downDotDot)).ToList();
                                currentMatches = currentMatches.Union(IsAdjacentBomb(upDotDot, currentDotDot, downDotDot)).ToList();

                                GetNearbyPieces(upDot, currentDot, downDot);
                            }
                        }
                    }
                }
            }
        }

        // After finding all matches, if the player made a move, update the move count
        UpdateMoveCount();
    }

    public void MatchPiecesOfColor(string color)
    {
        for (int i = 0; i < board.width; i++)
        {
            for (int j = 0; j < board.height; j++)
            {
                // Check if that piece exists
                if (board.allDots[i, j] != null)
                {
                    // Check the tag on that dot
                    if (board.allDots[i, j].tag == color)
                    {
                        // Set that dot to be matched
                        board.allDots[i, j].GetComponent<Dot>().isMatched = true;
                    }
                }
            }
        }
    }

    List<GameObject> GetAdjacentPieces(int column, int row)
    {
        List<GameObject> dots = new List<GameObject>();
        for (int i = column - 1; i <= column + 1; i++)
        {
            for (int j = row - 1; j <= row + 1; j++)
            {
                // Check if the piece is inside the board
                if (i >= 0 && i < board.width && j >= 0 && j < board.height)
                {
                    if (board.allDots[i, j] != null)
                    {
                        dots.Add(board.allDots[i, j]);
                        board.allDots[i, j].GetComponent<Dot>().isMatched = true;
                    }
                }
            }
        }
        return dots;
    }

    List<GameObject> GetColumnPieces(int column)
    {
        List<GameObject> dots = new List<GameObject>();
        for (int i = 0; i < board.height; i++)
        {
            if (board.allDots[column, i] != null)
            {
                dots.Add(board.allDots[column, i]);
                board.allDots[column, i].GetComponent<Dot>().isMatched = true;
            }
        }
        return dots;
    }

    List<GameObject> GetRowPieces(int row)
    {
        List<GameObject> dots = new List<GameObject>();
        for (int i = 0; i < board.width; i++)
        {
            if (board.allDots[i, row] != null)
            {
                dots.Add(board.allDots[i, row]);
                board.allDots[i, row].GetComponent<Dot>().isMatched = true;
            }
        }
        return dots;
    }


    public void CheckBombs()
    {
        // Did the player move something?
        if (board.currentDot != null)
        {
            // Is the piece they moved matched?
            if (board.currentDot.isMatched)
            {
                // Make it unmatched
                board.currentDot.isMatched = false;
                if ((board.currentDot.swipeAngle > -45 && board.currentDot.swipeAngle <= 45)
                || (board.currentDot.swipeAngle < -135 || board.currentDot.swipeAngle >= 135))
                {
                    board.currentDot.MakeRowBomb();
                }
                else
                {
                    board.currentDot.MakeColumnBomb();
                }
            }
            // Is the other piece matched?
            else if (board.currentDot.otherDot != null)
            {
                Dot otherDot = board.currentDot.otherDot.GetComponent<Dot>();
                // Is the other dot matched?
                if (otherDot.isMatched)
                {
                    // Make it unmatched
                    otherDot.isMatched = false;
                    if ((otherDot.swipeAngle > -45 && otherDot.swipeAngle <= 45)
                    || (otherDot.swipeAngle < -135 || otherDot.swipeAngle >= 135))
                    {
                        otherDot.MakeRowBomb();
                    }
                    else
                    {
                        otherDot.MakeColumnBomb();
                    }
                }
            }
        }
    }

}