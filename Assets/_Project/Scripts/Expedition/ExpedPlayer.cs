using System.Collections;
using UnityEngine;

public class ExpedPlayer : MonoBehaviour
{
    [SerializeField] private float  moveDuration = 0.1f;
    [SerializeField] private float  heightOffset = 1.0f;
    [SerializeField] public  int    maxMoveDistance = 1;

    public Vector2Int Coord { get; private set; }

    public void Place(Vector2Int coord, Vector3 twpos)
    {
        Coord = coord;
        this.transform.position = twpos + Vector3.up * heightOffset;
    }

    public IEnumerator MoveTo(Vector2Int coord, Vector3 twpos)
    {
        Vector3 start = this.transform.position;
        Vector3 end = twpos + Vector3.up * heightOffset;

        float time = 0.0f;
        while(time < moveDuration)
        {
            time += Time.deltaTime;
            this.transform.position = Vector3.Lerp(start, end, time / moveDuration);
            yield return null;
        }

        this.transform.position = end;
        Coord = coord;
    }
}
