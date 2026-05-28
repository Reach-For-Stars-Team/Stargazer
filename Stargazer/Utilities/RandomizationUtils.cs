using System.Linq;
using Reactor.Utilities.Extensions;
using UnityEngine;

namespace Stargazer.Utilities;

public class RandomizationUtils
{
    public static void SpawnObjectRandomly(GameObject obj, Vector2 size)
    {
        var col = obj.AddComponent<BoxCollider2D>();
        col.size = size;
        var room = ShipStatus.Instance.AllRooms.Random();
        obj.transform.position = room.roomArea.transform.position;
        
        var obstacles = room.GetComponentsInChildren<Collider2D>()
            .Where(x => x != room.roomArea)
            .ToList();
        
        var bounds = room.roomArea.bounds;
        bounds.size -= new Vector3(size.x/2, size.y/2, 0f);

        int maxAttempts = 1000;
        while (maxAttempts-- > 0)
        {
            bool isTouchingObstacle = obstacles.Any(x => x.IsTouching(col));
            bool isInsideRoom = room.roomArea.IsTouching(col);

            if (!isTouchingObstacle && isInsideRoom)
                break;

            obj.transform.position = new Vector3(
                UnityRandom.RandomRange(bounds.min.x, bounds.max.x),
                UnityRandom.RandomRange(bounds.min.y, bounds.max.y),
                0
            );
        }

        if (maxAttempts == 0)
        {
            obj.transform.position = ShipStatus.Instance.DummyLocations.Random().transform.position;
        }
        col.Destroy();
    }

    public static AnimationCurve GetRandomAnimationCurve(float min, float max, float start, float end, int keyframeCount)
    {
        Keyframe[] keyframes = [];
        keyframes = keyframes.Append(new Keyframe(start, 0)).ToArray();
        for (int i = 0; i < keyframeCount - 2; i++)
        {
            float val = UnityRandom.RandomRange(min, max);
            keyframes = keyframes.Append(new Keyframe(val, i/keyframeCount)).ToArray();
        }
        keyframes = keyframes.Append(new Keyframe(end, 1)).ToArray();
        return new AnimationCurve(keyframes);
    }
}