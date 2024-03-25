﻿using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.Build.Content;
using UnityEngine;


public class Board : MonoBehaviour
{
    [SerializeField]
    private GameObject tile;
    [SerializeField]
    private GameObject sliderPuzzle;
    [SerializeField]
    private Transform boardTrm;

    [SerializeField]
    private TextMeshProUGUI numberOfMoves;
    [SerializeField]
    private TextMeshProUGUI minnumNumMoves; // 최소 횟수를 나타낼 TMP

    private List<Tile> tileList;                         

    private int puzzleSize = 3;
    private int minNum = 0;
    private float neighborTileDistance = 182;              

    public Vector3 EmptyTilePosition { set; get; }        
    public int Playtime { private set; get; } = 0;     
    public int MoveCount { private set; get; } = 0;

    private IEnumerator Start()
    {
        // 게임이 시작될 때 A* 알고리즘을 사용하여 최소 이동 횟수를 계산
        int[,] initialState = GetInitialState(); // 초기 상태
        int[,] goalState = GetGoalState(); // 목표 상태

        int minMoves = PuzzleSolver.CalculateMinimumMoves(initialState, goalState);
        minNum = 100;
        //Debug.Log("Minimum moves to solve the puzzle: " + minMoves);

        minnumNumMoves.text = $"최소 횟수 : {minNum}";

        tileList = new List<Tile>();

        //SpawnTiles();
        SetupTilesFromState(goalState);

        UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(boardTrm.GetComponent<RectTransform>());

        yield return new WaitForEndOfFrame();

        tileList.ForEach(x => x.SetPosition());

        //StartCoroutine(Suffle());
    }

    private int[,] GetGoalState()
    {
        int[,] initialState = new int[3, 3] {
        {1, 4, 7},
        {8, 9, 5},
        {6, 2, 3}  // 9가 빈 타일
        };

        return initialState;
    }

    private int[,] GetInitialState()
    {
        int[,] initialState = new int[3, 3] {
        {1, 2, 3},
        {4, 5, 6},
        {7, 8, 9}  
        };

        return initialState;
    }

    private void SetupTilesFromState(int[,] state)
    {
        for (int y = 0; y < puzzleSize; ++y)
        {
            for (int x = 0; x < puzzleSize; ++x)
            {
                GameObject clone = Instantiate(this.tile, boardTrm);
                Tile tile = clone.GetComponent<Tile>();

                tile.Setup(this, puzzleSize * puzzleSize, state[y, x]);

                tileList.Add(tile);
            }
        }
    }


    //private void SpawnTiles()
    //{
    //    for (int y = 0; y < puzzleSize; ++y)
    //    {
    //        for (int x = 0; x < puzzleSize; ++x)
    //        {
    //            GameObject clone = Instantiate(this.tile, boardTrm);
    //            Tile tile = clone.GetComponent<Tile>();

    //            tile.Setup(this, puzzleSize * puzzleSize, y * puzzleSize + x + 1);

    //            tileList.Add(tile);
    //        }
    //    }
    //}

    //private IEnumerator Suffle()
    //{
    //    float current = 0;
    //    float percent = 0;

    //    while (percent < 1)
    //    {
    //        current += Time.deltaTime;
    //        percent = current / 0.1f;

    //        int index = UnityEngine.Random.Range(0, puzzleSize * puzzleSize);
    //        tileList[index].transform.SetAsLastSibling();

    //        yield return null;
    //    }
    //    EmptyTilePosition = tileList[tileList.Count - 1].GetComponent<RectTransform>().localPosition;
    //}

    public void IsMoveTile(Tile tile)
    {
        if (Vector3.Distance(EmptyTilePosition, tile.GetComponent<RectTransform>().localPosition) == neighborTileDistance)
        {
            Vector3 goalPosition = EmptyTilePosition;

            EmptyTilePosition = tile.GetComponent<RectTransform>().localPosition;

            tile.OnMoveTo(goalPosition);

            MoveCount++;
            numberOfMoves.text = $"이동 횟수 : {MoveCount}";

            if(minNum <= MoveCount)
            {
                Destroy(sliderPuzzle);
            }
        }
    }

    
}
