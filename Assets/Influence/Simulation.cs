using UnityEngine;
using System.Collections.Generic;

public abstract class Simulation : MonoBehaviour
{
    public float cellSize = 1;

    //	[SerializeField] DiffusionSettings obstacleSetting; // unique case where settings is only a container for the flowvector of the obstacles
    public DiffusionSettings[] settings;

    internal Vector2Int size;

    internal Vector3 boundsMin, boundsMax, boundsSize;

    public virtual void OnEnable()
    {
        var g = GetComponent<MeshRenderer>().bounds;
        boundsMin = g.min;
        boundsMax = g.max;
        boundsSize = g.size;
    }

    public static HashSet<Obstacle> obstacles = new HashSet<Obstacle>();

    public static void RegisterObstacle(Obstacle ob)
    {
        obstacles.Add(ob);
    }

    public static void DeRegisterObstacle(Obstacle ob)
    {
        obstacles.Remove(ob);
    }

    //TODO: send a signal if the thing is out of bounds.
    internal Vector2Int World2Grid(Vector3 pos)
    {
        var v = new Vector2(boundsMax.x - pos.x, -pos.z + boundsMax.z) * size / boundsSize.z;
        //v.x = Mathf.Clamp(v.x, 0, size.x);
        //v.y = Mathf.Clamp(v.y, 0, size.y);
        return new Vector2Int((int) v.x, (int) v.y);
    }

    internal BoundsInt World2Grid(Bounds bounds)
    {
        var min = World2Grid(bounds.min);
        var max = World2Grid(bounds.max);
        var b = new BoundsInt();
        b.min = Vector3Int.Max(Vector3Int.zero, new Vector3Int(min.x, min.y, 0));
        b.max = Vector3Int.Min(new Vector3Int(size.x - 1, size.y - 1, 0), new Vector3Int(max.x, max.y, 0));
        return b;
    }

    public float World2Index(Vector3 pos)
    {
        var gpos = World2Grid(pos);
        if (gpos.x >= 0 && gpos.x < size.x && gpos.y >= 0 && gpos.y < size.y)
            return gpos.x + gpos.y * size.x;
        else
            return Mathf.Infinity;
    }
}