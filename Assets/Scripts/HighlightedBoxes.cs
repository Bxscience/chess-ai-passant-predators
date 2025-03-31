using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HighlightedBoxes : MonoBehaviour
{
    [SerializeField] private GameObject highlightPrefab, lessHighlightPrefab;
    private static List<GameObject> currentHighlights = new List<GameObject>();
    void Start()
    {
        if (BoardManager.instance != null){BoardManager.instance.PlayedPly += OnPlayedPly;}
    }
    void OnDestroy()
    {
        if (BoardManager.instance != null){BoardManager.instance.PlayedPly -= OnPlayedPly;}
    }

    public void OnPlayedPly(Ply newPly)
    {
        ClearHighlights();
        Vector3 startPos = Board.IdxToPos(newPly.Start);
        Vector3 endPos = Board.IdxToPos(newPly.End);
        GameObject startHighlight = Instantiate(lessHighlightPrefab, startPos, Quaternion.identity);
        GameObject endHighlight = Instantiate(highlightPrefab, endPos, Quaternion.identity);

        if (startHighlight != null){currentHighlights.Add(startHighlight);}
        if (endHighlight != null){currentHighlights.Add(endHighlight);}
    }

    public static void ClearHighlights()
    {
        foreach (GameObject highlight in currentHighlights)
        {
            Destroy(highlight);
        }
        currentHighlights.Clear();
    }
}