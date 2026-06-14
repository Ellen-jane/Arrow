using UnityEngine;
using System.Collections.Generic;

public class ArrowHeadVisual : MonoBehaviour
{
    public List<GameObject> segments = new List<GameObject>();
    public Direction direction = Direction.Right;
    public bool isMoving;
    public bool isDestroyed;
}
