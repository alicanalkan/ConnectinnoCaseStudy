using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BorderController : MonoBehaviour
{
    [SerializeField] private List<BoxCollider> boxColliders;

    /// <summary>
    /// Set World colliders
    /// </summary>
    /// <param name="position"></param>
    /// <param name="topZPadding"></param>
    public void RecalculateBorder(Vector2 position, float topZPadding)
    {
        boxColliders[0].transform.position = new Vector2(position.x + boxColliders[0].size.x/2, 0.5f);
        boxColliders[1].transform.position = new Vector2(-position.x - boxColliders[1].size.x / 2, 0.5f);
        boxColliders[2].transform.position = new Vector3(0, 0.5f, position.y - topZPadding + boxColliders[2].size.z / 2);
        boxColliders[3].transform.position = new Vector3(0, 0.5f, - position.y -(boxColliders[3].size.z / 2));
    }
}
