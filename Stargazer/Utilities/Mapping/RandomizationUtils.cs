using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Reactor.Utilities.Extensions;
using UnityEngine;

namespace Stargazer.Mapping;

public class RandomizationUtils
{
    public enum SpawnCellState
    {
        Outside,
        Blocked,
        Door,
        LadderPassage,
        Reachable,
        RoomReachable
    }

    public enum SpawnAreaFilter
    {
        AnyRachable,
        ReachableWitchoutRooms,
        OnlyRooms
    }

    public struct SpawnDebugCell
    {
        public Vector2 Position;
        public SpawnCellState State;

        public SpawnDebugCell(Vector2 position, SpawnCellState state)
        {
            Position = position;
            State = state;
        }
    }

    private static readonly Vector2Int[] Directions =
    [
        new Vector2Int(1, 0),
        new Vector2Int(-1, 0),
        new Vector2Int(0, 1),
        new Vector2Int(0, -1),

        new Vector2Int(1, 1),
        new Vector2Int(1, -1),
        new Vector2Int(-1, 1),
        new Vector2Int(-1, -1)
    ];

    private static readonly Vector2Int[] LadderComponentDirections =
    [
        new Vector2Int(1, 0),
        new Vector2Int(-1, 0),
        new Vector2Int(0, 1),
        new Vector2Int(0, -1),

        new Vector2Int(1, 1),
        new Vector2Int(1, -1),
        new Vector2Int(-1, 1),
        new Vector2Int(-1, -1)
    ];

    private const int LadderTrimBottomCells = 2;
    private const int LadderTrimTopCells = 2;

    private static readonly Dictionary<SpawnAreaFilter, List<Vector2>> CachedPathfinding = new();
    private static bool HasCachedPathfinding;

    private static readonly Dictionary<Vector2Int, Vector2> CachedPathCells = new();
    private static readonly Dictionary<Vector2Int, List<Vector2Int>> CachedPathLinks = new();

    private static float CachedPathCellSize;
    private static Vector2 CachedPathBoundsMin;

    public static bool HasCachedPathMap => HasCachedPathfinding && CachedPathCells.Count > 0;

    public static void SpawnObjectRandomly(GameObject obj, Vector2 size)
    {
        SpawnObjectRandomly(obj, size, true, SpawnAreaFilter.AnyRachable);
    }

    public static void SpawnObjectRandomly(GameObject obj, Vector2 size, bool requirePathFromPlayer)
    {
        SpawnObjectRandomly(obj, size, requirePathFromPlayer, SpawnAreaFilter.AnyRachable);
    }

    public static void SpawnObjectRandomly(
        GameObject obj,
        Vector2 size,
        bool requirePathFromPlayer,
        SpawnAreaFilter areaFilter)
    {
        if (!obj || !ShipStatus.Instance) return;

        if (!requirePathFromPlayer)
        {
            SpawnAtFallback(obj);
            return;
        }

        if (!TryGetCachedPathfinding(areaFilter, out var cachedPositions))
        {
            SpawnAtFallback(obj);
            return;
        }

        if (TryPickCachedSpawnPosition(cachedPositions, size, out var chosen))
        {
            obj.transform.position = new Vector3(chosen.x, chosen.y, 0f);
            return;
        }

        SpawnAtFallback(obj);
    }

    private static bool TryPickCachedSpawnPosition(List<Vector2> positions, Vector2 size, out Vector2 chosen)
    {
        chosen = default;

        if (positions == null || positions.Count == 0)
            return false;

        const int maxChecks = 40;

        if (!PlayerControl.LocalPlayer)
        {
            for (int i = 0; i < maxChecks; i++)
            {
                var pos = positions.Random();

                if (!CanSpawnAt(pos, size))
                    continue;

                chosen = pos;
                return true;
            }

            return false;
        }

        Vector2 playerPos = PlayerControl.LocalPlayer.GetTruePosition();

        var farPositions = positions
            .OrderByDescending(pos => Vector2.Distance(playerPos, pos))
            .Take(Mathf.Max(1, Mathf.CeilToInt(positions.Count * 0.25f)))
            .ToList();

        for (int i = 0; i < maxChecks; i++)
        {
            var pos = farPositions.Random();

            if (!CanSpawnAt(pos, size))
                continue;

            chosen = pos;
            return true;
        }

        return false;
    }
    private static bool TryGetCachedPathfinding(SpawnAreaFilter filter, out List<Vector2> positions)
    {
        positions = null;

        if (!HasCachedPathfinding)
            return false;

        if (!CachedPathfinding.TryGetValue(filter, out positions))
            return false;

        return positions != null && positions.Count > 0;
    }

    public static void ClearCachedPathfinding()
    {
        CachedPathfinding.Clear();
        CachedPathCells.Clear();
        CachedPathLinks.Clear();

        HasCachedPathfinding = false;
        CachedPathCellSize = 0f;
        CachedPathBoundsMin = Vector2.zero;
    }

    private static void AddCachedPathLink(
        Dictionary<Vector2Int, List<Vector2Int>> links,
        Vector2Int a,
        Vector2Int b)
    {
        if (!links.TryGetValue(a, out var aLinks))
        {
            aLinks = new List<Vector2Int>();
            links[a] = aLinks;
        }

        if (!aLinks.Contains(b))
            aLinks.Add(b);

        if (!links.TryGetValue(b, out var bLinks))
        {
            bLinks = new List<Vector2Int>();
            links[b] = bLinks;
        }

        if (!bLinks.Contains(a))
            bLinks.Add(a);
    }

    private static void SetCachedPathGraph(
        HashSet<Vector2Int> reachable,
        Dictionary<Vector2Int, Vector2> cellPositions,
        Dictionary<Vector2Int, List<Vector2Int>> links,
        float cellSize,
        Vector2 boundsMin)
    {
        CachedPathCells.Clear();
        CachedPathLinks.Clear();

        foreach (var cell in reachable)
        {
            if (cellPositions.TryGetValue(cell, out var pos))
                CachedPathCells[cell] = pos;
        }

        foreach (var pair in links)
        {
            if (!reachable.Contains(pair.Key))
                continue;

            List<Vector2Int> filtered = new();

            foreach (var target in pair.Value)
            {
                if (reachable.Contains(target))
                    filtered.Add(target);
            }

            if (filtered.Count > 0)
                CachedPathLinks[pair.Key] = filtered;
        }

        CachedPathCellSize = cellSize;
        CachedPathBoundsMin = boundsMin;
    }
    private static void SetCachedPathfinding(
        List<Vector2> anyRachable,
        List<Vector2> reachableWithoutRooms,
        List<Vector2> onlyRooms)
    {
        CachedPathfinding.Clear();

        CachedPathfinding[SpawnAreaFilter.AnyRachable] = anyRachable;
        CachedPathfinding[SpawnAreaFilter.ReachableWitchoutRooms] = reachableWithoutRooms;
        CachedPathfinding[SpawnAreaFilter.OnlyRooms] = onlyRooms;

        HasCachedPathfinding = true;
    }

    public static IEnumerator CoMapPathfindingCells(
        float cellSize,
        Action<SpawnDebugCell> onCell,
        int workPerFrame = 500)
    {
        ClearCachedPathfinding();

        if (!ShipStatus.Instance)
            yield break;

        var bounds = GetMapBoundsFromColliders();

        HashSet<Vector2Int> insideCells = new();
        HashSet<Vector2Int> blockedCells = new();
        HashSet<Vector2Int> ladderCells = new();
        Dictionary<Vector2Int, Vector2> cellPositions = new();

        List<Vector2> cachedAnyRachable = new();
        List<Vector2> cachedReachableWithoutRooms = new();
        List<Vector2> cachedOnlyRooms = new();
        Dictionary<Vector2Int, List<Vector2Int>> cachedLinks = new();

        int countX = Mathf.CeilToInt(bounds.size.x / cellSize);
        int countY = Mathf.CeilToInt(bounds.size.y / cellSize);

        int work = 0;

        for (int x = 0; x <= countX; x++)
        {
            for (int y = 0; y <= countY; y++)
            {
                var cell = new Vector2Int(x, y);
                var pos = new Vector2(
                    bounds.min.x + x * cellSize,
                    bounds.min.y + y * cellSize
                );

                cellPositions[cell] = pos;
                insideCells.Add(cell);

                if (IsDoorCell(pos, cellSize))
                    onCell?.Invoke(new SpawnDebugCell(pos, SpawnCellState.Door));

                if (IsLadderCell(pos, cellSize))
                {
                    ladderCells.Add(cell);
                }
                else if (IsCellBlocked(pos, cellSize))
                {
                    blockedCells.Add(cell);
                    onCell?.Invoke(new SpawnDebugCell(pos, SpawnCellState.Blocked));
                }

                work++;

                if (work >= workPerFrame)
                {
                    work = 0;
                    yield return null;
                }
            }
        }

        var ladderPassageCells = GetMainLadderPassageCells(ladderCells);
        var elevatorLinks = BuildSimpleElevatorLinks(
            cellPositions,
            insideCells,
            blockedCells,
            ladderPassageCells
        );

        foreach (var ladderCell in ladderPassageCells)
        {
            if (!cellPositions.TryGetValue(ladderCell, out var pos))
                continue;

            onCell?.Invoke(new SpawnDebugCell(pos, SpawnCellState.LadderPassage));

            work++;

            if (work >= workPerFrame)
            {
                work = 0;
                yield return null;
            }
        }

        if (!PlayerControl.LocalPlayer)
            yield break;

        var startCell = FindClosestFreeCell(
            PlayerControl.LocalPlayer.GetTruePosition(),
            insideCells,
            blockedCells,
            ladderPassageCells,
            cellPositions
        );

        if (!startCell.HasValue)
            yield break;

        HashSet<Vector2Int> reachable = new();
        Queue<Vector2Int> queue = new();

        queue.Enqueue(startCell.Value);
        reachable.Add(startCell.Value);

        if (cellPositions.TryGetValue(startCell.Value, out var startPos))
        {
            var state = GetReachableState(startPos);
            AddToPathfindingCache(startPos, state, cachedAnyRachable, cachedReachableWithoutRooms, cachedOnlyRooms);
            onCell?.Invoke(new SpawnDebugCell(startPos, state));
        }

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (elevatorLinks.TryGetValue(current, out var elevatorTarget))
            {
                if (!reachable.Contains(elevatorTarget) &&
                    insideCells.Contains(elevatorTarget) &&
                    !blockedCells.Contains(elevatorTarget) &&
                    cellPositions.TryGetValue(elevatorTarget, out var elevatorTargetPos))
                {
                    reachable.Add(elevatorTarget);
                    queue.Enqueue(elevatorTarget);
                    // AddCachedPathLink(cachedLinks, current, elevatorTarget);

                    var state = GetReachableState(elevatorTargetPos);

                    AddToPathfindingCache(
                        elevatorTargetPos,
                        state,
                        cachedAnyRachable,
                        cachedReachableWithoutRooms,
                        cachedOnlyRooms
                    );

                    onCell?.Invoke(new SpawnDebugCell(elevatorTargetPos, state));
                }
            }
            foreach (var dir in Directions)
            {
                var next = current + dir;

                if (reachable.Contains(next))
                    continue;

                if (!insideCells.Contains(next))
                    continue;

                if (blockedCells.Contains(next))
                    continue;

                if (!cellPositions.TryGetValue(current, out var currentPos))
                    continue;

                if (!cellPositions.TryGetValue(next, out var nextPos))
                    continue;

                if (!CanMoveBetweenCells(
                        current,
                        next,
                        dir,
                        currentPos,
                        nextPos,
                        ladderPassageCells))
                {
                    continue;
                }

                reachable.Add(next);
                queue.Enqueue(next);
                // AddCachedPathLink(cachedLinks, current, next);

                var state = GetReachableState(nextPos);
                AddToPathfindingCache(nextPos, state, cachedAnyRachable, cachedReachableWithoutRooms, cachedOnlyRooms);
                onCell?.Invoke(new SpawnDebugCell(nextPos, state));

                work++;

                if (work >= workPerFrame)
                {
                    work = 0;
                    yield return null;
                }
            }
        }

        SetCachedPathfinding(
            cachedAnyRachable,
            cachedReachableWithoutRooms,
            cachedOnlyRooms
        );
        cachedLinks = BuildCachedPathLinks(
            reachable,
            cellPositions,
            ladderPassageCells,
            elevatorLinks
        );
        SetCachedPathGraph(
            reachable,
            cellPositions,
            cachedLinks,
            cellSize,
            bounds.min
        );
    }

    private static Dictionary<Vector2Int, List<Vector2Int>> BuildCachedPathLinks(
        HashSet<Vector2Int> reachable,
        Dictionary<Vector2Int, Vector2> cellPositions,
        Dictionary<Vector2Int, Vector2Int> elevatorLinks)
    {
        return BuildCachedPathLinks(reachable, cellPositions, new HashSet<Vector2Int>(), elevatorLinks);
    }

    private static Dictionary<Vector2Int, List<Vector2Int>> BuildCachedPathLinks(
        HashSet<Vector2Int> reachable,
        Dictionary<Vector2Int, Vector2> cellPositions,
        HashSet<Vector2Int> ladderPassageCells,
        Dictionary<Vector2Int, Vector2Int> elevatorLinks)
    {
        Dictionary<Vector2Int, List<Vector2Int>> links = new();

        foreach (var current in reachable)
        {
            if (!cellPositions.TryGetValue(current, out var currentPos))
                continue;

            if (elevatorLinks.TryGetValue(current, out var elevatorTarget))
            {
                if (reachable.Contains(elevatorTarget))
                    AddCachedPathLink(links, current, elevatorTarget);
            }

            foreach (var dir in Directions)
            {
                var next = current + dir;

                if (!reachable.Contains(next))
                    continue;

                if (!cellPositions.TryGetValue(next, out var nextPos))
                    continue;

                if (!CanMoveBetweenCells(
                        current,
                        next,
                        dir,
                        currentPos,
                        nextPos,
                        ladderPassageCells))
                {
                    continue;
                }

                if (IsDiagonalMove(dir) && IsCuttingCorner(current, dir, reachable, links, cellPositions, ladderPassageCells))
                    continue;

                AddCachedPathLink(links, current, next);
            }
        }

        return links;
    }

    private static bool IsDiagonalMove(Vector2Int dir)
    {
        return dir.x != 0 && dir.y != 0;
    }

    private static bool IsCuttingCorner(
        Vector2Int current,
        Vector2Int dir,
        HashSet<Vector2Int> reachable,
        Dictionary<Vector2Int, List<Vector2Int>> links,
        Dictionary<Vector2Int, Vector2> cellPositions,
        HashSet<Vector2Int> ladderPassageCells)
    {
        var sideA = new Vector2Int(current.x + dir.x, current.y);
        var sideB = new Vector2Int(current.x, current.y + dir.y);

        if (!reachable.Contains(sideA) || !reachable.Contains(sideB))
            return true;

        if (!cellPositions.TryGetValue(current, out var currentPos))
            return true;

        if (!cellPositions.TryGetValue(sideA, out var sideAPos))
            return true;

        if (!cellPositions.TryGetValue(sideB, out var sideBPos))
            return true;

        if (!CanMoveBetweenCells(current, sideA, new Vector2Int(dir.x, 0), currentPos, sideAPos, ladderPassageCells))
            return true;

        if (!CanMoveBetweenCells(current, sideB, new Vector2Int(0, dir.y), currentPos, sideBPos, ladderPassageCells))
            return true;

        return false;
    }
    private static SpawnCellState GetReachableState(Vector2 pos)
    {
        return IsInAnyRoomArea(pos)
            ? SpawnCellState.RoomReachable
            : SpawnCellState.Reachable;
    }

    private static void AddToPathfindingCache(
        Vector2 pos,
        SpawnCellState state,
        List<Vector2> anyRachable,
        List<Vector2> reachableWithoutRooms,
        List<Vector2> onlyRooms)
    {
        switch (state)
        {
            case SpawnCellState.Reachable:
                anyRachable.Add(pos);
                reachableWithoutRooms.Add(pos);
                break;

            case SpawnCellState.RoomReachable:
                anyRachable.Add(pos);
                onlyRooms.Add(pos);
                break;
        }
    }

    public static bool CanSpawnAt(Vector2 pos, Vector2 size)
    {
        return !IsBoxTouchingObstacle(pos, size + Vector2.one * 0.0005f);
    }

    private static bool CanMoveBetweenCells(
        Vector2Int current,
        Vector2Int next,
        Vector2Int dir,
        Vector2 currentPos,
        Vector2 nextPos,
        HashSet<Vector2Int> ladderPassageCells)
    {
        bool currentIsLadderPassage = ladderPassageCells.Contains(current);
        bool nextIsLadderPassage = ladderPassageCells.Contains(next);

        if (currentIsLadderPassage || nextIsLadderPassage)
            return IsVerticalMove(dir);

        return !IsPathBetweenCellsBlocked(currentPos, nextPos);
    }

    private static Vector2Int? FindClosestFreeCell(
        Vector2 targetPos,
        HashSet<Vector2Int> insideCells,
        HashSet<Vector2Int> blockedCells,
        HashSet<Vector2Int> ladderPassageCells,
        Dictionary<Vector2Int, Vector2> cellPositions)
    {
        Vector2Int? bestCell = null;
        float bestDistance = float.MaxValue;

        foreach (var cell in insideCells)
        {
            if (blockedCells.Contains(cell))
                continue;

            if (!cellPositions.TryGetValue(cell, out var pos))
                continue;

            float distance = Vector2.Distance(targetPos, pos);

            if (ladderPassageCells.Contains(cell))
                distance += 0.2f;

            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestCell = cell;
            }
        }

        return bestCell;
    }

    private static bool IsVerticalMove(Vector2Int dir)
    {
        return dir.x == 0 && dir.y != 0;
    }

    private static HashSet<Vector2Int> GetMainLadderPassageCells(HashSet<Vector2Int> ladderCells)
    {
        HashSet<Vector2Int> result = new();
        HashSet<Vector2Int> visited = new();

        foreach (var start in ladderCells)
        {
            if (visited.Contains(start))
                continue;

            var component = GetConnectedLadderComponent(start, ladderCells, visited);
            var bestRun = GetLongestVerticalRun(component);
            var trimmedRun = TrimLadderRun(bestRun, LadderTrimBottomCells, LadderTrimTopCells);

            foreach (var cell in trimmedRun)
                result.Add(cell);
        }

        return result;
    }

    private static List<Vector2Int> GetConnectedLadderComponent(
        Vector2Int start,
        HashSet<Vector2Int> ladderCells,
        HashSet<Vector2Int> visited)
    {
        List<Vector2Int> component = new();
        Queue<Vector2Int> queue = new();

        queue.Enqueue(start);
        visited.Add(start);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            component.Add(current);

            foreach (var dir in LadderComponentDirections)
            {
                var next = current + dir;

                if (visited.Contains(next))
                    continue;

                if (!ladderCells.Contains(next))
                    continue;

                visited.Add(next);
                queue.Enqueue(next);
            }
        }

        return component;
    }

    private static List<Vector2Int> GetLongestVerticalRun(List<Vector2Int> component)
    {
        List<Vector2Int> bestRun = new();

        if (component.Count == 0)
            return bestRun;

        float averageX = (float)component.Average(c => c.x);

        foreach (var group in component.GroupBy(c => c.x))
        {
            var ordered = group.OrderBy(c => c.y).ToList();
            List<Vector2Int> currentRun = new();

            for (int i = 0; i < ordered.Count; i++)
            {
                if (currentRun.Count == 0)
                {
                    currentRun.Add(ordered[i]);
                    continue;
                }

                var previous = currentRun[currentRun.Count - 1];

                if (ordered[i].y == previous.y + 1)
                {
                    currentRun.Add(ordered[i]);
                }
                else
                {
                    bestRun = PickBetterLadderRun(bestRun, currentRun, averageX);
                    currentRun = new List<Vector2Int> { ordered[i] };
                }
            }

            bestRun = PickBetterLadderRun(bestRun, currentRun, averageX);
        }

        return bestRun;
    }

    private static List<Vector2Int> PickBetterLadderRun(
        List<Vector2Int> bestRun,
        List<Vector2Int> currentRun,
        float averageX)
    {
        if (currentRun.Count == 0)
            return bestRun;

        if (bestRun.Count == 0)
            return new List<Vector2Int>(currentRun);

        if (currentRun.Count > bestRun.Count)
            return new List<Vector2Int>(currentRun);

        if (currentRun.Count < bestRun.Count)
            return bestRun;

        float currentX = (float)currentRun.Average(c => c.x);
        float bestX = (float)bestRun.Average(c => c.x);

        return Mathf.Abs(currentX - averageX) < Mathf.Abs(bestX - averageX)
            ? new List<Vector2Int>(currentRun)
            : bestRun;
    }

    private static List<Vector2Int> TrimLadderRun(List<Vector2Int> run, int trimBottom, int trimTop)
    {
        if (run.Count == 0)
            return run;

        var ordered = run.OrderBy(c => c.y).ToList();

        int start = Mathf.Clamp(trimBottom, 0, ordered.Count);
        int end = Mathf.Clamp(ordered.Count - trimTop, 0, ordered.Count);

        if (start >= end)
            return new List<Vector2Int>();

        return ordered.Skip(start).Take(end - start).ToList();
    }

    private static bool IsPathBetweenCellsBlocked(Vector2 from, Vector2 to)
    {
        var hits = Physics2D.LinecastAll(from, to);
        HashSet<Collider2D> counted = new();

        foreach (var hit in hits)
        {
            var col = hit.collider;

            if (!IsMovementBlockingCollider(col))
                continue;

            if (!counted.Add(col))
                continue;

            return true;
        }

        return false;
    }

    private static bool IsCellBlocked(Vector2 pos, float cellSize)
    {
        var hits = Physics2D.OverlapBoxAll(pos, new Vector2(cellSize * 0.5f, cellSize * 0.5f), 0f);

        foreach (var hit in hits)
        {
            if (IsStaticBlockingCollider(hit))
                return true;
        }

        return false;
    }

    private static bool IsDoorCell(Vector2 pos, float cellSize)
    {
        var hits = Physics2D.OverlapBoxAll(pos, new Vector2(cellSize * 1.15f, cellSize * 1.15f), 0f);

        foreach (var hit in hits)
        {
            if (IsDoorCollider(hit))
                return true;
        }

        return false;
    }

    private static bool IsLadderCell(Vector2 pos, float cellSize)
    {
        var hits = Physics2D.OverlapBoxAll(pos, new Vector2(cellSize * 0.7f, cellSize * 0.7f), 0f);

        foreach (var hit in hits)
        {
            if (IsLadderCollider(hit))
                return true;
        }

        return false;
    }

    private static bool IsBoxTouchingObstacle(Vector2 pos, Vector2 size)
    {
        var hits = Physics2D.OverlapBoxAll(pos, size, 0f);

        foreach (var hit in hits)
        {
            if (IsMovementBlockingCollider(hit))
                return true;
        }

        return false;
    }

    private static bool IsInAnyRoomArea(Vector2 pos)
    {
        if (!ShipStatus.Instance)
            return false;

        foreach (var room in ShipStatus.Instance.AllRooms)
        {
            if (!room || !room.roomArea)
                continue;

            if (room.roomArea.OverlapPoint(pos))
                return true;
        }

        return false;
    }

    private static Bounds GetMapBoundsFromColliders()
    {
        var colliders = UnityEngine.Object.FindObjectsOfType<Collider2D>()
            .Where(c => c && c.enabled)
            .Where(IsUsefulMapColliderForBounds)
            .ToList();

        if (colliders.Count > 0)
        {
            var bounds = colliders[0].bounds;

            for (int i = 1; i < colliders.Count; i++)
                bounds.Encapsulate(colliders[i].bounds);

            bounds.Expand(2f);
            return bounds;
        }

        if (ShipStatus.Instance && ShipStatus.Instance.DummyLocations != null && ShipStatus.Instance.DummyLocations.Count > 0)
        {
            var fallback = new Bounds(ShipStatus.Instance.DummyLocations[0].transform.position, Vector3.one);

            foreach (var location in ShipStatus.Instance.DummyLocations)
            {
                if (location)
                    fallback.Encapsulate(location.transform.position);
            }

            fallback.Expand(20f);
            return fallback;
        }

        if (PlayerControl.LocalPlayer)
            return new Bounds(PlayerControl.LocalPlayer.transform.position, new Vector3(50f, 50f, 1f));

        return new Bounds(Vector3.zero, new Vector3(50f, 50f, 1f));
    }

    private static bool IsUsefulMapColliderForBounds(Collider2D col)
    {
        if (!col) return false;
        if (!col.enabled) return false;

        if (col.GetComponentInParent<PlayerControl>()) return false;
        if (col.GetComponentInParent<DeadBody>()) return false;
        if (col.GetComponentInParent<Vent>()) return false;

        if (IsRoomAreaCollider(col)) return false;
        if (IsIgnoredByNameCollider(col)) return false;

        return true;
    }

    private static bool IsStaticBlockingCollider(Collider2D col)
    {
        if (!col) return false;
        if (!col.enabled) return false;
        if (col.isTrigger) return false;

        if (IsRoomAreaCollider(col)) return false;

        if (col.GetComponentInParent<PlayerControl>()) return false;
        if (col.GetComponentInParent<DeadBody>()) return false;
        if (col.GetComponentInParent<Vent>()) return false;

        if (IsDoorCollider(col)) return false;
        if (IsLadderCollider(col)) return false;
        if (IsIgnoredByNameCollider(col)) return false;

        return true;
    }

    private static bool IsMovementBlockingCollider(Collider2D col)
    {
        if (!col) return false;
        if (!col.enabled) return false;
        if (col.isTrigger) return false;

        if (IsRoomAreaCollider(col)) return false;

        if (col.GetComponentInParent<PlayerControl>()) return false;
        if (col.GetComponentInParent<DeadBody>()) return false;
        if (col.GetComponentInParent<Vent>()) return false;

        if (IsDoorCollider(col)) return false;
        if (IsLadderCollider(col)) return false;
        if (IsIgnoredByNameCollider(col)) return false;

        return true;
    }

    private static bool IsDoorCollider(Collider2D col)
    {
        return HasNameInParents(col ? col.transform : null, "door");
    }

    private static bool IsLadderCollider(Collider2D col)
    {
        return HasNameInParents(col ? col.transform : null, "ladder");
    }

    private static bool IsIgnoredByNameCollider(Collider2D col)
    {
        if (!col) return false;

        return IsHudCollider(col)
            || HasNameInParents(col.transform, "OnewayShadow-Top")
            || HasNameInParents(col.transform, "OnewayShadow-Top+Ledge")
            || HasNameInParents(col.transform, "minigame")
            || HasNameInParents(col.transform, "DarkRoomShadow");
    }

    private static bool IsHudCollider(Collider2D col)
    {
        if (!col) return false;

        var transform = col.transform;

        if (HudManager.Instance && transform.IsChildOf(HudManager.Instance.transform))
            return true;

        if (Camera.main && transform.IsChildOf(Camera.main.transform))
        {
            string path = GetFullPath(transform).ToLowerInvariant();

            if (path.Contains("/hud") || path.Contains("hudmanager") || path.Contains("hud"))
                return true;
        }

        string fullPath = GetFullPath(transform).ToLowerInvariant();

        return fullPath.Contains("main camera/hud")
            || fullPath.Contains("maincamera/hud")
            || fullPath.Contains("/hud/")
            || fullPath.EndsWith("/hud")
            || fullPath.Contains("hudmanager");
    }

    private static string GetFullPath(Transform transform)
    {
        if (!transform) return "null";

        string path = transform.name;

        while (transform.parent)
        {
            transform = transform.parent;
            path = transform.name + "/" + path;
        }

        return path;
    }

    private static bool HasNameInParents(Transform transform, string text)
    {
        while (transform)
        {
            if (transform.name.ToLowerInvariant().Contains(text.ToLowerInvariant()))
                return true;

            transform = transform.parent;
        }

        return false;
    }

    private static bool IsRoomAreaCollider(Collider2D col)
    {
        if (!ShipStatus.Instance) return false;

        foreach (var room in ShipStatus.Instance.AllRooms)
        {
            if (room && room.roomArea == col)
                return true;
        }

        return false;
    }

    private static Dictionary<Vector2Int, Vector2Int> BuildSimpleElevatorLinks(
        Dictionary<Vector2Int, Vector2> cellPositions,
        HashSet<Vector2Int> insideCells,
        HashSet<Vector2Int> blockedCells,
        HashSet<Vector2Int> ladderPassageCells)
    {
        Dictionary<Vector2Int, Vector2Int> links = new();

        var elevatorParents = FindElevatorParents();

        foreach (var parent in elevatorParents)
        {
            Transform upper = FindDirectOrChildByNameContains(parent, "UpperElevator");
            Transform lower = FindDirectOrChildByNameContains(parent, "LowerElevator");

            if (!upper || !lower)
                continue;

            var upperCell = FindClosestFreeCellToPosition(
                upper.position,
                insideCells,
                blockedCells,
                ladderPassageCells,
                cellPositions
            );

            var lowerCell = FindClosestFreeCellToPosition(
                lower.position,
                insideCells,
                blockedCells,
                ladderPassageCells,
                cellPositions
            );

            if (!upperCell.HasValue || !lowerCell.HasValue)
                continue;

            if (upperCell.Value == lowerCell.Value)
                continue;

            links[upperCell.Value] = lowerCell.Value;
            links[lowerCell.Value] = upperCell.Value;
        }

        return links;
    }

    private static List<Transform> FindElevatorParents()
    {
        List<Transform> result = new();

        var transforms = UnityEngine.Object.FindObjectsOfType<Transform>();

        foreach (var transform in transforms)
        {
            if (!transform)
                continue;

            string name = transform.name.ToLowerInvariant();

            if (!name.Contains("elevator"))
                continue;

            Transform upper = FindDirectOrChildByNameContains(transform, "UpperElevator");
            Transform lower = FindDirectOrChildByNameContains(transform, "LowerElevator");

            if (!upper || !lower)
                continue;

            if (!result.Contains(transform))
                result.Add(transform);
        }

        return result;
    }

    private static Transform FindDirectOrChildByNameContains(Transform parent, string text)
    {
        if (!parent) return null;

        string lowerText = text.ToLowerInvariant();

        var children = parent.GetComponentsInChildren<Transform>(true);

        foreach (var child in children)
        {
            if (!child)
                continue;

            if (child == parent)
                continue;

            if (child.name.ToLowerInvariant().Contains(lowerText))
                return child;
        }

        return null;
    }

    private static Vector2Int? FindClosestFreeCellToPosition(
        Vector2 position,
        HashSet<Vector2Int> insideCells,
        HashSet<Vector2Int> blockedCells,
        HashSet<Vector2Int> ladderPassageCells,
        Dictionary<Vector2Int, Vector2> cellPositions)
    {
        Vector2Int? bestCell = null;
        float bestDistance = float.MaxValue;

        foreach (var cell in insideCells)
        {
            if (blockedCells.Contains(cell) && !ladderPassageCells.Contains(cell))
                continue;

            if (!cellPositions.TryGetValue(cell, out var cellPos))
                continue;

            float distance = Vector2.Distance(position, cellPos);

            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestCell = cell;
            }
        }

        return bestCell;
    }

    private static void AddNamedTravelPair(
        Dictionary<Vector2Int, Vector2Int> links,
        Dictionary<Vector2Int, Vector2> cellPositions,
        string fromName,
        string toName,
        bool twoWay)
    {
        Transform from = FindBestTransformByNameContains(fromName);
        Transform to = FindBestTransformByNameContains(toName);

        if (!from || !to)
            return;

        var fromCell = FindClosestCellToPosition(from.position, cellPositions);
        var toCell = FindClosestCellToPosition(to.position, cellPositions);

        if (!fromCell.HasValue || !toCell.HasValue)
            return;

        links[fromCell.Value] = toCell.Value;

        if (twoWay)
            links[toCell.Value] = fromCell.Value;
    }

    private static Transform FindBestTransformByNameContains(string text)
    {
        string lowerText = text.ToLowerInvariant();
        var transforms = UnityEngine.Object.FindObjectsOfType<Transform>();

        Transform best = null;
        int bestScore = int.MaxValue;

        foreach (var transform in transforms)
        {
            if (!transform)
                continue;

            string name = transform.name.ToLowerInvariant();

            if (!name.Contains(lowerText))
                continue;

            int score = Mathf.Abs(name.Length - lowerText.Length);

            if (score < bestScore)
            {
                bestScore = score;
                best = transform;
            }
        }

        return best;
    }

    private static Transform FindTransformByNameContains(string text)
    {
        var transforms = UnityEngine.Object.FindObjectsOfType<Transform>();

        foreach (var transform in transforms)
        {
            if (transform.name.ToLowerInvariant().Contains(text.ToLowerInvariant()))
                return transform;
        }

        return null;
    }

    private static Vector2Int? FindClosestCellToPosition(
        Vector2 position,
        Dictionary<Vector2Int, Vector2> cellPositions)
    {
        Vector2Int? bestCell = null;
        float bestDistance = float.MaxValue;

        foreach (var pair in cellPositions)
        {
            float distance = Vector2.Distance(position, pair.Value);

            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestCell = pair.Key;
            }
        }

        return bestCell;
    }
    private static Vector2 PickFarPosition(List<Vector2> positions)
    {
        if (!PlayerControl.LocalPlayer)
            return positions.Random();

        Vector2 playerPos = PlayerControl.LocalPlayer.GetTruePosition();

        var ordered = positions
            .OrderByDescending(pos => Vector2.Distance(playerPos, pos))
            .ToList();

        int count = Mathf.Max(1, Mathf.CeilToInt(ordered.Count * 0.25f));

        return ordered.Take(count).ToList().Random();
    }

    private static void SpawnAtFallback(GameObject obj)
    {
        if (ShipStatus.Instance && ShipStatus.Instance.DummyLocations != null && ShipStatus.Instance.DummyLocations.Count > 0)
        {
            var fallback = ShipStatus.Instance.DummyLocations.Random().transform.position;
            obj.transform.position = new Vector3(fallback.x, fallback.y, 0f);
            return;
        }

        if (PlayerControl.LocalPlayer)
        {
            var pos = PlayerControl.LocalPlayer.GetTruePosition();
            obj.transform.position = new Vector3(pos.x, pos.y, 0f);
        }
    }

    public static AnimationCurve GetRandomAnimationCurve(float min, float max, float start, float end, int keyframeCount)
    {
        Keyframe[] keyframes = [];
        keyframes = keyframes.Append(new Keyframe(start, 0)).ToArray();

        for (int i = 0; i < keyframeCount - 2; i++)
        {
            float val = UnityEngine.Random.RandomRange(min, max);
            keyframes = keyframes.Append(new Keyframe(val, i / (float)keyframeCount)).ToArray();
        }

        keyframes = keyframes.Append(new Keyframe(end, 1)).ToArray();

        return new AnimationCurve(keyframes);
    }

    public static bool TryFindPathOnCachedMap(
        Vector2 startPos,
        Vector2 targetPos,
        out List<Vector2> path,
        out Vector2 usedTargetPos)
    {
        path = new List<Vector2>();
        usedTargetPos = targetPos;

        if (!HasCachedPathMap)
            return false;

        var startCell = FindClosestCachedPathCell(startPos);
        var targetCell = FindClosestCachedPathCell(targetPos);

        if (!startCell.HasValue || !targetCell.HasValue)
            return false;

        if (!CachedPathCells.TryGetValue(targetCell.Value, out usedTargetPos))
            usedTargetPos = targetPos;

        return TryFindCachedPath(startCell.Value, targetCell.Value, out path);
    }

    public static bool TryFindPartialPathOnCachedMap(
        Vector2 startPos,
        Vector2 targetPos,
        int maxExpandedCells,
        out List<Vector2> path,
        out Vector2 usedTargetPos)
    {
        path = new List<Vector2>();
        usedTargetPos = targetPos;

        if (!HasCachedPathMap)
            return false;

        var startCell = FindClosestCachedPathCell(startPos);
        var targetCell = FindClosestCachedPathCell(targetPos);

        if (!startCell.HasValue || !targetCell.HasValue)
            return false;

        return TryFindPartialCachedPath(
            startCell.Value,
            targetCell.Value,
            maxExpandedCells,
            out path,
            out usedTargetPos
        );
    }
    private static bool TryFindPartialCachedPath(
        Vector2Int start,
        Vector2Int target,
        int maxExpandedCells,
        out List<Vector2> path,
        out Vector2 usedTargetPos)
    {
        path = new List<Vector2>();
        usedTargetPos = CachedPathCells.TryGetValue(target, out var targetPos)
            ? targetPos
            : Vector2.zero;

        if (!CachedPathCells.ContainsKey(start))
            return false;

        if (!CachedPathCells.ContainsKey(target))
            return false;

        List<Vector2Int> open = new() { start };
        HashSet<Vector2Int> openSet = new() { start };
        HashSet<Vector2Int> closed = new();

        Dictionary<Vector2Int, Vector2Int> cameFrom = new();
        Dictionary<Vector2Int, float> gScore = new();

        gScore[start] = 0f;

        Vector2Int bestCell = start;
        float bestDistanceToTarget = Vector2Int.Distance(start, target);

        int expanded = 0;

        while (open.Count > 0 && expanded < maxExpandedCells)
        {
            var current = GetBestPartialCell(open, gScore, target);

            open.Remove(current);
            openSet.Remove(current);
            closed.Add(current);
            expanded++;

            float distanceToTarget = Vector2Int.Distance(current, target);

            if (distanceToTarget < bestDistanceToTarget)
            {
                bestDistanceToTarget = distanceToTarget;
                bestCell = current;
            }

            if (current == target)
            {
                bestCell = current;
                break;
            }

            if (!CachedPathLinks.TryGetValue(current, out var neighbours))
                continue;

            var orderedNeighbours = neighbours
                .Where(n => !closed.Contains(n))
                .OrderBy(n => Vector2Int.Distance(n, target))
                .ToList();

            foreach (var next in orderedNeighbours)
            {
                float moveCost = GetMoveCost(current, next);
                float newGScore = gScore[current] + moveCost;

                if (!gScore.TryGetValue(next, out var oldScore) || newGScore < oldScore)
                {
                    cameFrom[next] = current;
                    gScore[next] = newGScore;

                    if (!openSet.Contains(next))
                    {
                        open.Add(next);
                        openSet.Add(next);
                    }
                }
            }
        }

        path = ReconstructCachedPath(bestCell, cameFrom);
        // DO NOT TOUCH THAT OR SIMPLIFY PATH USED... Simplifying can cut corners!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!, WE DON'T WANT THAT

        if (CachedPathCells.TryGetValue(bestCell, out var bestPos))
            usedTargetPos = bestPos;

        return path.Count > 1;
    }

    private static Vector2Int GetBestPartialCell(
        List<Vector2Int> open,
        Dictionary<Vector2Int, float> gScore,
        Vector2Int target)
    {
        var best = open[0];

        float bestScore = GetPartialScore(best, gScore, target);

        for (int i = 1; i < open.Count; i++)
        {
            var cell = open[i];
            float score = GetPartialScore(cell, gScore, target);

            if (score < bestScore)
            {
                best = cell;
                bestScore = score;
            }
        }

        return best;
    }

    private static float GetPartialScore(
        Vector2Int cell,
        Dictionary<Vector2Int, float> gScore,
        Vector2Int target)
    {
        float g = gScore.TryGetValue(cell, out var value) ? value : 999999f;
        float h = Vector2Int.Distance(cell, target);

        return g + h;
    }

    private static Vector2Int? FindClosestCachedPathCell(Vector2 pos)
    {
        if (CachedPathCells.Count == 0 || CachedPathCellSize <= 0f)
            return null;

        var center = new Vector2Int(
            Mathf.RoundToInt((pos.x - CachedPathBoundsMin.x) / CachedPathCellSize),
            Mathf.RoundToInt((pos.y - CachedPathBoundsMin.y) / CachedPathCellSize)
        );

        Vector2Int? bestCell = null;
        float bestDistance = float.MaxValue;

        for (int radius = 0; radius <= 80; radius++)
        {
            bool foundInRadius = false;

            for (int x = -radius; x <= radius; x++)
            {
                for (int y = -radius; y <= radius; y++)
                {
                    if (Mathf.Abs(x) != radius && Mathf.Abs(y) != radius)
                        continue;

                    var cell = center + new Vector2Int(x, y);

                    if (!CachedPathCells.TryGetValue(cell, out var cellPos))
                        continue;

                    float distance = Vector2.Distance(pos, cellPos);

                    if (distance < bestDistance)
                    {
                        bestDistance = distance;
                        bestCell = cell;
                    }

                    foundInRadius = true;
                }
            }

            if (foundInRadius)
                return bestCell;
        }

        foreach (var pair in CachedPathCells)
        {
            float distance = Vector2.Distance(pos, pair.Value);

            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestCell = pair.Key;
            }
        }

        return bestCell;
    }

    private static bool TryFindCachedPath(Vector2Int start, Vector2Int end, out List<Vector2> path)
    {
        path = new List<Vector2>();

        if (!CachedPathCells.ContainsKey(start))
            return false;

        if (!CachedPathCells.ContainsKey(end))
            return false;

        Queue<Vector2Int> queue = new();
        HashSet<Vector2Int> visited = new();
        Dictionary<Vector2Int, Vector2Int> cameFrom = new();

        queue.Enqueue(start);
        visited.Add(start);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();

            if (current == end)
            {
                path = ReconstructCachedPath(current, cameFrom);
                return true;
            }

            if (!CachedPathLinks.TryGetValue(current, out var neighbours))
                continue;

            foreach (var next in neighbours)
            {
                if (visited.Contains(next))
                    continue;

                visited.Add(next);
                cameFrom[next] = current;
                queue.Enqueue(next);
            }
        }

        return false;
    }

    private static List<Vector2> SimplifyPath(List<Vector2> path)
    {
        if (path == null || path.Count <= 2)
            return path;

        List<Vector2> result = new();

        result.Add(path[0]);

        Vector2 previousDirection = GetDirection(path[0], path[1]);

        for (int i = 2; i < path.Count; i++)
        {
            Vector2 direction = GetDirection(path[i - 1], path[i]);

            if (direction != previousDirection)
            {
                result.Add(path[i - 1]);
                previousDirection = direction;
            }
        }

        result.Add(path[^1]);

        return result;
    }

    private static Vector2 GetDirection(Vector2 from, Vector2 to)
    {
        Vector2 dir = to - from;

        return new Vector2(
            Mathf.Round(dir.x * 100f) / 100f,
            Mathf.Round(dir.y * 100f) / 100f
        );
    }
    private static Vector2Int GetLowestScoreCell(List<Vector2Int> open, Dictionary<Vector2Int, float> fScore)
    {
        var best = open[0];
        float bestScore = fScore.TryGetValue(best, out var score) ? score : float.MaxValue;

        for (int i = 1; i < open.Count; i++)
        {
            var cell = open[i];
            float cellScore = fScore.TryGetValue(cell, out var value) ? value : float.MaxValue;

            if (cellScore < bestScore)
            {
                best = cell;
                bestScore = cellScore;
            }
        }

        return best;
    }

    private static List<Vector2> ReconstructCachedPath(
        Vector2Int current,
        Dictionary<Vector2Int, Vector2Int> cameFrom)
    {
        List<Vector2> result = new();

        if (CachedPathCells.TryGetValue(current, out var pos))
            result.Add(pos);

        while (cameFrom.TryGetValue(current, out var previous))
        {
            current = previous;

            if (CachedPathCells.TryGetValue(current, out var previousPos))
                result.Add(previousPos);
        }

        result.Reverse();
        return result;
    }

    public static IEnumerator CoFindPathOnCachedMap(
        Vector2 startPos,
        Vector2 targetPos,
        int workPerFrame,
        Action<bool, List<Vector2>, Vector2> onComplete)
    {
        List<Vector2> path = new();
        Vector2 usedTargetPos = targetPos;

        if (!HasCachedPathMap)
        {
            onComplete?.Invoke(false, path, usedTargetPos);
            yield break;
        }

        var startCell = FindClosestCachedPathCell(startPos);
        var targetCell = FindClosestCachedPathCell(targetPos);

        if (!startCell.HasValue || !targetCell.HasValue)
        {
            onComplete?.Invoke(false, path, usedTargetPos);
            yield break;
        }

        if (CachedPathCells.TryGetValue(targetCell.Value, out var targetCellPos))
            usedTargetPos = targetCellPos;

        bool success = false;

        yield return CoFindCachedPathAStar(
            startCell.Value,
            targetCell.Value,
            workPerFrame,
            result =>
            {
                path = result;
                success = result != null && result.Count > 1;
            }
        );

        onComplete?.Invoke(success, path, usedTargetPos);
    }

    private static IEnumerator CoFindCachedPathAStar(
        Vector2Int start,
        Vector2Int end,
        int workPerFrame,
        Action<List<Vector2>> onComplete)
    {
        if (!CachedPathCells.ContainsKey(start) || !CachedPathCells.ContainsKey(end))
        {
            onComplete?.Invoke(new List<Vector2>());
            yield break;
        }

        PathNodeQueue open = new();
        HashSet<Vector2Int> closed = new();

        Dictionary<Vector2Int, Vector2Int> cameFrom = new();
        Dictionary<Vector2Int, float> gScore = new();

        gScore[start] = 0f;
        open.Enqueue(start, GetAStarHeuristic(start, end));

        Vector2Int bestCell = start;
        float bestDistance = GetAStarHeuristic(start, end);
        // bool blockedByRuntimeDoor = false;

        int work = 0;

        while (open.Count > 0)
        {
            var current = open.Dequeue();

            if (closed.Contains(current))
                continue;

            float currentDistance = GetAStarHeuristic(current, end);

            if (currentDistance < bestDistance)
            {
                bestDistance = currentDistance;
                bestCell = current;
            }

            if (current == end)
            {
                var path = ReconstructCachedPath(current, cameFrom);
                onComplete?.Invoke(path);
                yield break;
            }

            closed.Add(current);

            if (!CachedPathLinks.TryGetValue(current, out var neighbours))
                continue;

            foreach (var next in neighbours)
            {
                if (closed.Contains(next))
                    continue;

                if (!CanMoveBetweenCachedCells(current, next))
                    continue;

                if (!IsSpecialCachedLink(current, next) &&
                    CachedPathCells.TryGetValue(current, out var currentPos) &&
                    CachedPathCells.TryGetValue(next, out var nextPos) &&
                    IsRuntimeDoorBlockingSegment(currentPos, nextPos))
                {
                    continue;
                }

                float moveCost = GetMoveCost(current, next);
                float currentG = gScore.TryGetValue(current, out var g) ? g : 999999f;
                float newGScore = currentG + moveCost;

                if (!gScore.TryGetValue(next, out var oldGScore) || newGScore < oldGScore)
                {
                    cameFrom[next] = current;
                    gScore[next] = newGScore;

                    float fScore = newGScore + GetAStarHeuristic(next, end);
                    open.Enqueue(next, fScore);
                }
            }

            work++;

            if (work >= workPerFrame)
            {
                work = 0;
                yield return null;
            }
        }

        if (bestCell != start)
        {
            var fallbackPath = ReconstructCachedPath(bestCell, cameFrom);

            if (!IsRuntimePathBlocked(fallbackPath))
            {
                onComplete?.Invoke(fallbackPath);
                yield break;
            }
        }

        onComplete?.Invoke(new List<Vector2>());
    }
    private static bool IsRuntimePathBlocked(List<Vector2> path)
    {
        if (path == null || path.Count < 2)
            return false;

        for (int i = 0; i < path.Count - 1; i++)
        {
            if (IsRuntimePathSegmentBlocked(path[i], path[i + 1]))
                return true;
        }

        return false;
    }
    private static bool CanMoveBetweenCachedCells(Vector2Int from, Vector2Int to)
    {
        int dx = to.x - from.x;
        int dy = to.y - from.y;

        if (Mathf.Abs(dx) != 1 || Mathf.Abs(dy) != 1)
            return true;

        var sideA = new Vector2Int(from.x + dx, from.y);
        var sideB = new Vector2Int(from.x, from.y + dy);

        return CachedPathLinks.TryGetValue(from, out var links)
            && links.Contains(sideA)
            && links.Contains(sideB);
    }
    private static bool IsSpecialCachedLink(Vector2Int from, Vector2Int to)
    {
        int dx = Mathf.Abs(to.x - from.x);
        int dy = Mathf.Abs(to.y - from.y);

        return dx > 1 || dy > 1;
    }
    private static Vector2Int GetBestAStarCell(
        List<Vector2Int> open,
        Dictionary<Vector2Int, float> fScore,
        Dictionary<Vector2Int, float> gScore,
        Vector2Int target)
    {
        var best = open[0];

        float bestF = fScore.TryGetValue(best, out var firstF) ? firstF : float.MaxValue;
        float bestH = GetAStarHeuristic(best, target);
        float bestG = gScore.TryGetValue(best, out var firstG) ? firstG : float.MaxValue;

        for (int i = 1; i < open.Count; i++)
        {
            var cell = open[i];

            float cellF = fScore.TryGetValue(cell, out var f) ? f : float.MaxValue;
            float cellH = GetAStarHeuristic(cell, target);
            float cellG = gScore.TryGetValue(cell, out var g) ? g : float.MaxValue;

            if (cellF < bestF ||
                Mathf.Approximately(cellF, bestF) && cellH < bestH ||
                Mathf.Approximately(cellF, bestF) && Mathf.Approximately(cellH, bestH) && cellG < bestG)
            {
                best = cell;
                bestF = cellF;
                bestH = cellH;
                bestG = cellG;
            }
        }

        return best;
    }

    private static float GetAStarHeuristic(Vector2Int from, Vector2Int to)
    {
        int dx = Mathf.Abs(from.x - to.x);
        int dy = Mathf.Abs(from.y - to.y);

        int min = Mathf.Min(dx, dy);
        int max = Mathf.Max(dx, dy);

        return 1.414f * min + (max - min);
    }

    private static float GetMoveCost(Vector2Int from, Vector2Int to)
    {
        int dx = Mathf.Abs(from.x - to.x);
        int dy = Mathf.Abs(from.y - to.y);

        if (dx != 0 && dy != 0)
            return 1.414f;

        return 1f;
    }

    private class PathNodeQueue
    {
        private readonly List<(Vector2Int Cell, float Score)> items = new();

        public int Count => items.Count;

        public void Enqueue(Vector2Int cell, float score)
        {
            items.Add((cell, score));

            int child = items.Count - 1;

            while (child > 0)
            {
                int parent = (child - 1) / 2;

                if (items[parent].Score <= items[child].Score)
                    break;

                (items[parent], items[child]) = (items[child], items[parent]);
                child = parent;
            }
        }

        public Vector2Int Dequeue()
        {
            var result = items[0].Cell;

            int last = items.Count - 1;
            items[0] = items[last];
            items.RemoveAt(last);

            int parent = 0;

            while (true)
            {
                int left = parent * 2 + 1;
                int right = left + 1;
                int best = parent;

                if (left < items.Count && items[left].Score < items[best].Score)
                    best = left;

                if (right < items.Count && items[right].Score < items[best].Score)
                    best = right;

                if (best == parent)
                    break;

                (items[parent], items[best]) = (items[best], items[parent]);
                parent = best;
            }

            return result;
        }

        public void Clear()
        {
            items.Clear();
        }
    }

    public static bool IsRuntimePathSegmentBlocked(Vector2 from, Vector2 to)
    {
        return IsRuntimeDoorBlockingSegment(from, to);
    }
    private static bool IsRuntimeStaticBlockingSegment(Vector2 from, Vector2 to)
    {
        Vector2 dir = to - from;
        float distance = dir.magnitude;

        if (distance <= 0.001f)
            return false;

        var hits = Physics2D.LinecastAll(from, to);

        foreach (var hit in hits)
        {
            var col = hit.collider;

            if (!col) continue;
            if (!col.enabled) continue;
            if (col.isTrigger) continue;

            if (IsRoomAreaCollider(col)) continue;

            if (col.GetComponentInParent<PlayerControl>()) continue;
            if (col.GetComponentInParent<DeadBody>()) continue;
            if (col.GetComponentInParent<Vent>()) continue;

            if (IsLadderCollider(col)) continue;
            if (IsIgnoredByNameCollider(col)) continue;

            if (IsDoorCollider(col)) continue;

            return true;
        }

        return false;
    }
    private static bool IsRuntimeDoorBlockingSegment(Vector2 from, Vector2 to)
    {
        Vector2 dir = to - from;
        float distance = dir.magnitude;

        if (distance <= 0.001f)
            return false;
            
        if (distance > CachedPathCellSize * 3f)
            return false;

        Vector2 mid = Vector2.Lerp(from, to, 0.5f);

        var hits = Physics2D.OverlapBoxAll(
            mid,
            new Vector2(distance + 0.15f, 0.25f),
            Vector2.SignedAngle(Vector2.right, dir)
        );

        foreach (var hit in hits)
        {
            if (!hit) continue;

            if (IsRuntimeDoorBlockingCollider(hit))
                return true;
        }

        return false;
    }

    private static bool IsRuntimeDoorBlockingCollider(Collider2D col)
    {
        if (!col) return false;
        if (!col.enabled) return false;
        if (!col.gameObject.activeInHierarchy) return false;

        ManualDoor manualDoor = col.GetComponentInParent<ManualDoor>();

        if (manualDoor)
        {
            if (manualDoor.myCollider && col != manualDoor.myCollider)
                return false;

            return !manualDoor.Opening;
        }

        SomeKindaDoor someDoor = col.GetComponentInParent<SomeKindaDoor>();

        if (someDoor)
        {
            if (col.isTrigger)
                return false;

            return true;
        }

        if (IsDoorCollider(col))
        {
            if (col.isTrigger)
                return false;

            return true;
        }

        return false;
    }
    public static bool IsWalkable(Vector2 position)
    {
        if (CachedPathCells == null || CachedPathCells.Count == 0)
            return true;

        Vector2Int cell = new(
            Mathf.RoundToInt((position.x - CachedPathBoundsMin.x) / CachedPathCellSize),
            Mathf.RoundToInt((position.y - CachedPathBoundsMin.y) / CachedPathCellSize)
        );

        return CachedPathCells.ContainsKey(cell);
    }
    public class CachedPathMapFileData
    {
        public string MapName;
        public float CellSize;
        public Vector2 BoundsMin;

        public List<SpawnDebugCell> DebugCells = new();
        public List<CachedPathNode> Nodes = new();
        public List<CachedPathLink> Links = new();
    }

    public struct CachedPathNode
    {
        public Vector2Int Cell;
        public Vector2 Position;

        public CachedPathNode(Vector2Int cell, Vector2 position)
        {
            Cell = cell;
            Position = position;
        }
    }

    public struct CachedPathLink
    {
        public Vector2Int From;
        public Vector2Int To;

        public CachedPathLink(Vector2Int from, Vector2Int to)
        {
            From = from;
            To = to;
        }
    }

    public static CachedPathMapFileData CreateCachedPathMapFileData(
        string mapName,
        float cellSize,
        List<SpawnDebugCell> debugCells)
    {
        CachedPathMapFileData data = new()
        {
            MapName = mapName,
            CellSize = cellSize,
            BoundsMin = CachedPathBoundsMin,
            DebugCells = debugCells
        };

        foreach (var pair in CachedPathCells)
        {
            data.Nodes.Add(new CachedPathNode(pair.Key, pair.Value));
        }

        foreach (var pair in CachedPathLinks)
        {
            foreach (var target in pair.Value)
            {
                data.Links.Add(new CachedPathLink(pair.Key, target));
            }
        }

        return data;
    }

    public static void LoadCachedPathMapFileData(CachedPathMapFileData data)
    {
        ClearCachedPathfinding();

        List<Vector2> anyReachable = new();
        List<Vector2> reachableWithoutRooms = new();
        List<Vector2> onlyRooms = new();

        HashSet<Vector2Int> reachable = new();
        Dictionary<Vector2Int, Vector2> cellPositions = new();
        Dictionary<Vector2Int, List<Vector2Int>> links = new();

        foreach (var node in data.Nodes)
        {
            reachable.Add(node.Cell);
            cellPositions[node.Cell] = node.Position;
        }

        foreach (var link in data.Links)
        {
            AddCachedPathLink(links, link.From, link.To);
        }

        foreach (var cell in data.DebugCells)
        {
            if (cell.State == SpawnCellState.Reachable)
            {
                anyReachable.Add(cell.Position);
                reachableWithoutRooms.Add(cell.Position);
            }
            else if (cell.State == SpawnCellState.RoomReachable)
            {
                anyReachable.Add(cell.Position);
                onlyRooms.Add(cell.Position);
            }
        }

        SetCachedPathfinding(anyReachable, reachableWithoutRooms, onlyRooms);
        SetCachedPathGraph(reachable, cellPositions, links, data.CellSize, data.BoundsMin);
    }
}
