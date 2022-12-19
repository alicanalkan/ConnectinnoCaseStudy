using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanGrids : MonoBehaviour
{
    /// <summary>
    /// Create Ingredients Positions for ingredint placement.
    /// </summary>
    public List<PanGridDatas> panPositions = new List<PanGridDatas>();
    [SerializeField] private MeshFilter meshFilter;
    [SerializeField] private Transform panPosition;
    private void Start()
    {
        for (int i = 0; i < meshFilter.sharedMesh.vertices.Length; i++)
        {
            var gridData = new PanGridDatas() { gridPos = panPosition.TransformPoint(meshFilter.sharedMesh.vertices[i])};
            panPositions.Add(gridData);

        }
    }
}

public class PanGridDatas
{
    public Vector3 gridPos;
}
