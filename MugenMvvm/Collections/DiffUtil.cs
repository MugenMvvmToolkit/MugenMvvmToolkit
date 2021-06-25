using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using MugenMvvm.Extensions;

namespace MugenMvvm.Collections
{
    public static class DiffUtil
    {
#if !NET5_0
        private static readonly Comparer<Diagonal> DiagonalComparer = Comparer<Diagonal>.Create((o1, o2) => o1.X - o2.X);
#endif

        public interface ICallback
        {
            public int GetOldListSize();

            public int GetNewListSize();

            public bool AreItemsTheSame(int oldItemPosition, int newItemPosition);

            public bool AreContentsTheSame(int oldItemPosition, int newItemPosition);
        }

        public interface IListUpdateCallback
        {
            void OnInserted(int position, int finalPosition, int count);

            void OnRemoved(int position, int count);

            void OnMoved(int fromPosition, int toPosition, int fromOriginalPosition, int toFinalPosition);

            void OnChanged(int position, int finalPosition, int count, bool isMove);
        }

        public static ValueTask<DiffResult> CalculateDiffAsync(ICallback cb, bool isAsync, bool detectMoves = true)
        {
            if (isAsync)
            {
                return detectMoves
                    ? Task.Factory.StartNew(o => CalculateDiff((ICallback)o!, true), cb).AsValueTask()
                    : Task.Factory.StartNew(o => CalculateDiff((ICallback)o!, false), cb).AsValueTask();
            }

            return new ValueTask<DiffResult>(CalculateDiff(cb, detectMoves));
        }

        public static DiffResult CalculateDiff(ICallback cb, bool detectMoves = true)
        {
            Should.NotBeNull(cb, nameof(cb));
            var oldSize = cb.GetOldListSize();
            var newSize = cb.GetNewListSize();

            var diagonals = new ListInternal<Diagonal>(0);

            // instead of a recursive implementation, we keep our own stack to avoid potential stack
            // overflow exceptions
            var stack = new ListInternal<Range>(0);

            stack.Add(new Range(0, oldSize, 0, newSize));

            var max = (oldSize + newSize + 1) / 2;
            // allocate forward and backward k-lines. K lines are diagonal lines in the matrix. (see the
            // paper for details)
            // These arrays lines keep the max reachable position for each k-line.
            var forward = new CenteredArray(max * 2 + 1);
            var backward = new CenteredArray(max * 2 + 1);

            // We pool the ranges to avoid allocations for each recursive call.
            var rangePool = new ListInternal<Range>(0);
            while (stack.Count != 0)
            {
                var range = RemoveAt(stack, stack.Count - 1);
                MidPoint(range, cb, forward, backward, out var snake);
                if (snake.IsUndefined)
                    rangePool.Add(range);
                else
                {
                    // if it has a diagonal, save it
                    if (snake.DiagonalSize > 0)
                        diagonals.Add(snake.ToDiagonal());
                    // add new ranges for left and right
                    var left = rangePool.Count == 0 ? new Range() : RemoveAt(rangePool, rangePool.Count - 1);
                    left.OldListStart = range.OldListStart;
                    left.NewListStart = range.NewListStart;
                    left.OldListEnd = snake.StartX;
                    left.NewListEnd = snake.StartY;
                    stack.Add(left);

                    // re-use range for right
                    //noinspection UnnecessaryLocalVariable
                    // Range right = range;
                    // right.OldListEnd = range.OldListEnd;
                    // right.NewListEnd = range.NewListEnd;
                    range.OldListStart = snake.EndX;
                    range.NewListStart = snake.EndY;
                    stack.Add(range);
                }
            }

            // sort snakes
#if NET5_0
            diagonals.Sort((o1, o2) => o1.X - o2.X);
#else
            diagonals.Sort(DiagonalComparer);
#endif

            return new DiffResult(cb, diagonals, forward.Data, backward.Data, detectMoves);
        }

        private static void MidPoint(
            Range range,
            ICallback cb,
            CenteredArray forward,
            CenteredArray backward, out Snake result)
        {
            if (range.OldSize < 1 || range.NewSize < 1)
            {
                result = Snake.Undefined;
                return;
            }

            var max = (range.OldSize + range.NewSize + 1) / 2;
            forward[1] = range.OldListStart;
            backward[1] = range.OldListEnd;
            for (var d = 0; d < max; d++)
            {
                Forward(range, cb, forward, backward, d, out result);
                if (!result.IsUndefined)
                    return;
                Backward(range, cb, forward, backward, d, out result);
                if (!result.IsUndefined)
                    return;
            }

            result = Snake.Undefined;
        }

        private static void Forward(
            Range range,
            ICallback cb,
            CenteredArray forward,
            CenteredArray backward,
            int d, out Snake result)
        {
            var checkForSnake = Math.Abs(range.OldSize - range.NewSize) % 2 == 1;
            var delta = range.OldSize - range.NewSize;
            for (var k = -d; k <= d; k += 2)
            {
                // we either come from d-1, k-1 OR d-1. k+1
                // as we move in steps of 2, array always holds both current and previous d values
                // k = x - y and each array value holds the max X, y = x - k
                int startX;
                int startY;
                int x, y;
                if (k == -d || k != d && forward[k + 1] > forward[k - 1])
                {
                    // picking k + 1, incrementing Y (by simply not incrementing X)
                    x = startX = forward[k + 1];
                }
                else
                {
                    // picking k - 1, incrementing X
                    startX = forward[k - 1];
                    x = startX + 1;
                }

                y = range.NewListStart + (x - range.OldListStart) - k;
                startY = d == 0 || x != startX ? y : y - 1;
                // now find snake size
                while (x < range.OldListEnd
                       && y < range.NewListEnd
                       && cb.AreItemsTheSame(x, y))
                {
                    x++;
                    y++;
                }

                // now we have furthest reaching x, record it
                forward[k] = x;
                if (checkForSnake)
                {
                    // see if we did pass over a backwards array
                    // mapping function: delta - k
                    var backwardsK = delta - k;
                    // if backwards K is calculated and it passed me, found match
                    if (backwardsK >= -d + 1
                        && backwardsK <= d - 1
                        && backward[backwardsK] <= x)
                    {
                        // match
                        result = new Snake(startX, startY, x, y, false);
                        return;
                    }
                }
            }

            result = Snake.Undefined;
        }

        private static void Backward(
            Range range,
            ICallback cb,
            CenteredArray forward,
            CenteredArray backward,
            int d, out Snake result)
        {
            var checkForSnake = (range.OldSize - range.NewSize) % 2 == 0;
            var delta = range.OldSize - range.NewSize;
            // same as forward but we go backwards from end of the lists to be beginning
            // this also means we'll try to optimize for minimizing x instead of maximizing it
            for (var k = -d; k <= d; k += 2)
            {
                // we either come from d-1, k-1 OR d-1, k+1
                // as we move in steps of 2, array always holds both current and previous d values
                // k = x - y and each array value holds the MIN X, y = x - k
                // when x's are equal, we prioritize deletion over insertion
                int startX;
                int startY;
                int x, y;

                if (k == -d || k != d && backward[k + 1] < backward[k - 1])
                {
                    // picking k + 1, decrementing Y (by simply not decrementing X)
                    x = startX = backward[k + 1];
                }
                else
                {
                    // picking k - 1, decrementing X
                    startX = backward[k - 1];
                    x = startX - 1;
                }

                y = range.NewListEnd - (range.OldListEnd - x - k);
                startY = d == 0 || x != startX ? y : y + 1;
                // now find snake size
                while (x > range.OldListStart
                       && y > range.NewListStart
                       && cb.AreItemsTheSame(x - 1, y - 1))
                {
                    x--;
                    y--;
                }

                // now we have furthest point, record it (min X)
                backward[k] = x;
                if (checkForSnake)
                {
                    // see if we did pass over a backwards array
                    // mapping function: delta - k
                    var forwardsK = delta - k;
                    // if forwards K is calculated and it passed me, found match
                    if (forwardsK >= -d
                        && forwardsK <= d
                        && forward[forwardsK] >= x)
                    {
                        // match
                        // assignment are reverse since we are a reverse snake
                        result = new Snake(x, y, startX, startY, true);
                        return;
                    }
                }
            }

            result = Snake.Undefined;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Range RemoveAt(ListInternal<Range> list, int index)
        {
            var foo = list.Items[index];
            list.RemoveAt(index);
            return foo;
        }

        [StructLayout(LayoutKind.Auto)]
        public readonly struct DiffResult
        {
            private readonly ICallback _callback;
            private readonly bool _detectMoves;
            private readonly ListInternal<Diagonal> _diagonals;
            private readonly int[] _newItemStatuses;
            private readonly int _newListSize;
            private readonly int[] _oldItemStatuses;
            private readonly int _oldListSize;

            public const int NoPosition = -1;
            private const int FlagNotChanged = 1;
            private const int FlagChanged = FlagNotChanged << 1;
            private const int FlagMovedChanged = FlagChanged << 1;
            private const int FlagMovedNotChanged = FlagMovedChanged << 1;
            private const int FlagMoved = FlagMovedChanged | FlagMovedNotChanged;
            private const int FlagOffset = 4;
            private const int FlagMask = (1 << FlagOffset) - 1;

            internal DiffResult(ICallback callback, ListInternal<Diagonal> diagonals, int[] oldItemStatuses, int[] newItemStatuses, bool detectMoves)
            {
                _diagonals = diagonals;
                _oldItemStatuses = oldItemStatuses;
                _newItemStatuses = newItemStatuses;
                Array.Clear(_oldItemStatuses, 0, _oldItemStatuses.Length);
                Array.Clear(_newItemStatuses, 0, _newItemStatuses.Length);
                _callback = callback;
                _oldListSize = callback.GetOldListSize();
                _newListSize = callback.GetNewListSize();
                _detectMoves = detectMoves;
                AddEdgeDiagonals();
                FindMatchingItems();
            }

            private void AddEdgeDiagonals()
            {
                // see if we should add 1 to the 0,0
                var shouldAdd = _diagonals.Count == 0;
                if (!shouldAdd)
                {
                    var diagonal = _diagonals.Items[0];
                    shouldAdd = diagonal.X != 0 || diagonal.Y != 0;
                }

                if (shouldAdd)
                    _diagonals.Insert(0, new Diagonal(0, 0, 0));
                // always add one last
                _diagonals.Add(new Diagonal(_oldListSize, _newListSize, 0));
            }

            private void FindMatchingItems()
            {
                var count = _diagonals.Count;
                var diagonals = _diagonals.Items;
                for (var index = 0; index < count; index++)
                {
                    var diagonal = diagonals[index];
                    for (var offset = 0; offset < diagonal.Size; offset++)
                    {
                        var posX = diagonal.X + offset;
                        var posY = diagonal.Y + offset;
                        var theSame = _callback.AreContentsTheSame(posX, posY);
                        var changeFlag = theSame ? FlagNotChanged : FlagChanged;
                        _oldItemStatuses[posX] = (posY << FlagOffset) | changeFlag;
                        _newItemStatuses[posY] = (posX << FlagOffset) | changeFlag;
                    }
                }

                // now all matches are marked, lets look for moves
                if (_detectMoves)
                {
                    // traverse each addition / removal from the end of the list, find matching
                    // addition removal from before
                    FindMoveMatches();
                }
            }

            private void FindMoveMatches()
            {
                // for each removal, find matching addition
                var posX = 0;
                var count = _diagonals.Count;
                var items = _diagonals.Items;
                for (var index = 0; index < count; index++)
                {
                    var diagonal = items[index];
                    while (posX < diagonal.X)
                    {
                        if (_oldItemStatuses[posX] == 0)
                        {
                            // there is a removal, find matching addition from the rest
                            FindMatchingAddition(posX);
                        }

                        posX++;
                    }

                    // snap back for the next diagonal
                    posX = diagonal.EndX;
                }
            }

            private void FindMatchingAddition(int posX)
            {
                var posY = 0;
                var count = _diagonals.Count;
                var items = _diagonals.Items;
                for (var i = 0; i < count; i++)
                {
                    var diagonal = items[i];
                    while (posY < diagonal.Y)
                    {
                        // found some additions, evaluate
                        if (_newItemStatuses[posY] == 0)
                        {
                            // not evaluated yet
                            var matching = _callback.AreItemsTheSame(posX, posY);
                            if (matching)
                            {
                                // yay found it, set values
                                var contentsMatching = _callback.AreContentsTheSame(posX, posY);
                                var changeFlag = contentsMatching
                                    ? FlagMovedNotChanged
                                    : FlagMovedChanged;
                                // once we process one of these, it will mark the other one as ignored.
                                _oldItemStatuses[posX] = (posY << FlagOffset) | changeFlag;
                                _newItemStatuses[posY] = (posX << FlagOffset) | changeFlag;
                                return;
                            }
                        }

                        posY++;
                    }

                    posY = diagonal.EndY;
                }
            }

            public int ConvertOldPositionToNew(int oldListPosition)
            {
                if (oldListPosition < 0 || oldListPosition >= _oldListSize)
                    throw new IndexOutOfRangeException($"Index out of bounds - passed position = {oldListPosition}, old list size = {_oldListSize}");
                var status = _oldItemStatuses[oldListPosition];
                if ((status & FlagMask) == 0)
                    return NoPosition;
                return status >> FlagOffset;
            }

            public int ConvertNewPositionToOld(int newListPosition)
            {
                if (newListPosition < 0 || newListPosition >= _newListSize)
                    throw new IndexOutOfRangeException($"Index out of bounds - passed position = {newListPosition}, new list size = {_newListSize}");
                var status = _newItemStatuses[newListPosition];
                if ((status & FlagMask) == 0)
                    return NoPosition;
                return status >> FlagOffset;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void DispatchUpdatesTo(IListUpdateCallback updateCallback, bool ignoreFinalPosition = false)
            {
                var callback = new BatchingListUpdateCallback(updateCallback, ignoreFinalPosition);
                DispatchUpdatesTo(ref callback);
            }

            public void DispatchUpdatesTo(ref BatchingListUpdateCallback batchingCallback)
            {
                // track up to date current list size for moves
                // when a move is found, we record its position from the end of the list (which is
                // less likely to change since we iterate in reverse).
                // Later when we find the match of that move, we dispatch the update
                var currentListSize = _oldListSize;
                // list of postponed moves
                var postponedUpdates = new ListInternal<PostponedUpdate>(0);
                // posX and posY are exclusive
                var posX = _oldListSize;
                var posY = _newListSize;
                // iterate from end of the list to the beginning.
                // this just makes offsets easier since changes in the earlier indices has an effect
                // on the later indices.
                var items = _diagonals.Items;
                for (var diagonalIndex = _diagonals.Count - 1; diagonalIndex >= 0; diagonalIndex--)
                {
                    var diagonal = items[diagonalIndex];
                    var endX = diagonal.EndX;
                    var endY = diagonal.EndY;
                    // dispatch removals and additions until we reach to that diagonal
                    // first remove then add so that it can go into its place and we don't need
                    // to offset values
                    while (posX > endX)
                    {
                        posX--;
                        // REMOVAL
                        var status = _oldItemStatuses[posX];
                        if ((status & FlagMoved) != 0)
                        {
                            var newPos = status >> FlagOffset;
                            // get postponed addition
                            var postponedUpdate = GetPostponedUpdate(postponedUpdates, newPos, false);
                            if (postponedUpdate.IsUndefined)
                            {
                                // first time we are seeing this, we'll see a matching addition
                                postponedUpdates.Add(new PostponedUpdate(
                                    posX,
                                    currentListSize - posX - 1,
                                    true
                                ));
                            }
                            else
                            {
                                // this is an addition that was postponed. Now dispatch it.
                                var updatedNewPos = currentListSize - postponedUpdate.CurrentPos - 1;
                                batchingCallback.OnMoved(posX, updatedNewPos, posX, postponedUpdate.PosInOwnerList);
                                if ((status & FlagMovedChanged) != 0)
                                    batchingCallback.OnChanged(updatedNewPos, postponedUpdate.PosInOwnerList, 1, true);
                            }
                        }
                        else
                        {
                            // simple removal
                            batchingCallback.OnRemoved(posX, 1);
                            currentListSize--;
                        }
                    }

                    while (posY > endY)
                    {
                        posY--;
                        // ADDITION
                        var status = _newItemStatuses[posY];
                        if ((status & FlagMoved) != 0)
                        {
                            // this is a move not an addition.
                            // see if this is postponed
                            var oldPos = status >> FlagOffset;
                            // get postponed removal
                            var postponedUpdate = GetPostponedUpdate(postponedUpdates, oldPos, true);
                            // empty size returns 0 for indexOf
                            if (postponedUpdate.IsUndefined)
                            {
                                // postpone it until we see the removal
                                postponedUpdates.Add(new PostponedUpdate(
                                    posY,
                                    currentListSize - posX,
                                    false
                                ));
                            }
                            else
                            {
                                // oldPosFromEnd = foundListSize - posX
                                // we can find posX if we swap the list sizes
                                // posX = listSize - oldPosFromEnd
                                batchingCallback.OnMoved(currentListSize - postponedUpdate.CurrentPos - 1, posX, postponedUpdate.PosInOwnerList, posY);
                                if ((status & FlagMovedChanged) != 0)
                                    batchingCallback.OnChanged(posX, posY, 1, true);
                            }
                        }
                        else
                        {
                            // simple addition
                            batchingCallback.OnInserted(posX, posY, 1);
                            currentListSize++;
                        }
                    }

                    // now dispatch updates for the diagonal
                    posX = diagonal.X;
                    posY = diagonal.Y;
                    for (var i = 0; i < diagonal.Size; i++)
                    {
                        // dispatch changes
                        if ((_oldItemStatuses[posX] & FlagMask) == FlagChanged)
                            batchingCallback.OnChanged(posX, posY, 1, false);

                        posX++;
                        posY++;
                    }

                    // snap back for the next diagonal
                    posX = diagonal.X;
                    posY = diagonal.Y;
                }

                batchingCallback.DispatchLastEvent();
            }

            private static PostponedUpdate GetPostponedUpdate(ListInternal<PostponedUpdate> postponedUpdates, int posInList, bool removal)
            {
                var postponedUpdate = PostponedUpdate.Undefined;
                var count = postponedUpdates.Count;
                var items = postponedUpdates.Items;
                for (var i = 0; i < count; i++)
                {
                    ref var update = ref items[i];
                    if (postponedUpdate.IsUndefined)
                    {
                        if (update.PosInOwnerList != posInList || update.Removal != removal)
                            continue;

                        postponedUpdate = update;
                        postponedUpdates.RemoveAt(i);
                        --i;
                    }
                    else
                    {
                        if (removal)
                            update.CurrentPos--;
                        else
                            update.CurrentPos++;
                    }
                }

                return postponedUpdate;
            }
        }

        [StructLayout(LayoutKind.Auto)]
        public ref struct BatchingListUpdateCallback
        {
            private readonly IListUpdateCallback _callback;
            private readonly bool _ignoreFinalPosition;
            private int _lastEventCount;
            private int _lastEventPosition;
            private int _lastEventFinalPosition;
            private int _lastEventType;
            private bool _lastMoved;

            private const int TypeNone = 0;
            private const int TypeAdd = 1;
            private const int TypeRemove = 2;
            private const int TypeChange = 3;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public BatchingListUpdateCallback(IListUpdateCallback callback, bool ignoreFinalPosition = false)
            {
                _lastEventCount = -1;
                _lastEventPosition = -1;
                _lastEventFinalPosition = -1;
                _lastMoved = false;
                _lastEventType = TypeNone;
                _callback = callback;
                _ignoreFinalPosition = ignoreFinalPosition;
            }

            [MemberNotNullWhen(false, nameof(_callback))]
            public bool IsEmpty => _callback == null;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private bool IsBatchInsert(int position, int finalPosition, int count) =>
                _lastEventType == TypeAdd && position >= _lastEventPosition && position <= _lastEventPosition + _lastEventCount &&
                (_ignoreFinalPosition || _lastEventFinalPosition >= finalPosition && _lastEventFinalPosition <= finalPosition + count);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private bool IsBatchChange(int position, int finalPosition, int count, bool moved) =>
                _lastEventType == TypeChange && _lastMoved == moved && !(position > _lastEventPosition + _lastEventCount || position + count < _lastEventPosition) &&
                (_ignoreFinalPosition || !(finalPosition > _lastEventFinalPosition + _lastEventCount || finalPosition + count < _lastEventFinalPosition));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void OnInserted(int position, int finalPosition, int count)
            {
                if (IsBatchInsert(position, finalPosition, count))
                {
                    _lastEventCount += count;
                    _lastEventPosition = Math.Min(position, _lastEventPosition);
                    _lastEventFinalPosition = Math.Min(finalPosition, _lastEventFinalPosition);
                }
                else
                {
                    DispatchLastEvent();
                    _lastEventPosition = position;
                    _lastEventFinalPosition = finalPosition;
                    _lastEventCount = count;
                    _lastEventType = TypeAdd;
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void OnRemoved(int position, int count)
            {
                if (_lastEventType == TypeRemove && _lastEventPosition >= position && _lastEventPosition <= position + count)
                {
                    _lastEventCount += count;
                    _lastEventPosition = position;
                }
                else
                {
                    DispatchLastEvent();
                    _lastEventPosition = position;
                    _lastEventCount = count;
                    _lastEventType = TypeRemove;
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void OnMoved(int fromPosition, int toPosition, int fromOriginalPosition, int toFinalPosition)
            {
                DispatchLastEvent();
                _callback.OnMoved(fromPosition, toPosition, fromOriginalPosition, toFinalPosition);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void OnChanged(int position, int finalPosition, int count, bool moved)
            {
                if (IsBatchChange(position, finalPosition, count, moved))
                {
                    var previousEnd = _lastEventPosition + _lastEventCount;
                    _lastEventPosition = Math.Min(position, _lastEventPosition);
                    _lastEventFinalPosition = Math.Min(finalPosition, _lastEventFinalPosition);
                    _lastEventCount = Math.Max(previousEnd, position + count) - _lastEventPosition;
                }
                else
                {
                    DispatchLastEvent();
                    _lastMoved = moved;
                    _lastEventPosition = position;
                    _lastEventFinalPosition = finalPosition;
                    _lastEventCount = count;
                    _lastEventType = TypeChange;
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void DispatchLastEvent()
            {
                switch (_lastEventType)
                {
                    case TypeAdd:
                        _callback.OnInserted(_lastEventPosition, _lastEventFinalPosition, _lastEventCount);
                        break;
                    case TypeRemove:
                        _callback.OnRemoved(_lastEventPosition, _lastEventCount);
                        break;
                    case TypeChange:
                        _callback.OnChanged(_lastEventPosition, _lastEventFinalPosition, _lastEventCount, _lastMoved);
                        break;
                }

                _lastEventType = TypeNone;
            }
        }

        [StructLayout(LayoutKind.Auto)]
        internal readonly struct Diagonal
        {
            public readonly int Size;
            public readonly int X;
            public readonly int Y;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Diagonal(int x, int y, int size)
            {
                X = x;
                Y = y;
                Size = size;
            }

            public int EndX
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => X + Size;
            }

            public int EndY
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => Y + Size;
            }
        }

        [StructLayout(LayoutKind.Auto)]
        private readonly struct Snake
        {
            public readonly int EndX;
            public readonly int EndY;
            private readonly bool _reverse;
            public readonly int StartX;
            public readonly int StartY;

            public static readonly Snake Undefined = new(int.MinValue, int.MinValue, int.MinValue, int.MinValue, false);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Snake(int startX, int startY, int endX, int endY, bool reverse)
            {
                EndX = endX;
                EndY = endY;
                _reverse = reverse;
                StartX = startX;
                StartY = startY;
            }

            public bool IsUndefined
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => EndX == int.MinValue && StartX == int.MinValue;
            }

            private bool HasAdditionOrRemoval
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => EndY - StartY != EndX - StartX;
            }

            private bool IsAddition
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => EndY - StartY > EndX - StartX;
            }

            public int DiagonalSize
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => Math.Min(EndX - StartX, EndY - StartY);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Diagonal ToDiagonal()
            {
                if (HasAdditionOrRemoval)
                {
                    if (_reverse)
                    {
                        // snake edge it at the end
                        return new Diagonal(StartX, StartY, DiagonalSize);
                    }

                    // snake edge it at the beginning
                    if (IsAddition)
                        return new Diagonal(StartX, StartY + 1, DiagonalSize);
                    return new Diagonal(StartX + 1, StartY, DiagonalSize);
                }

                // we are a pure diagonal
                return new Diagonal(StartX, StartY, EndX - StartX);
            }
        }

        [StructLayout(LayoutKind.Auto)]
        private struct Range
        {
            public int NewListStart, NewListEnd;
            public int OldListStart, OldListEnd;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Range(int oldListStart, int oldListEnd, int newListStart, int newListEnd)
            {
                OldListStart = oldListStart;
                OldListEnd = oldListEnd;
                NewListStart = newListStart;
                NewListEnd = newListEnd;
            }

            public int OldSize
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => OldListEnd - OldListStart;
            }

            public int NewSize
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => NewListEnd - NewListStart;
            }
        }

        [StructLayout(LayoutKind.Auto)]
        private struct PostponedUpdate
        {
            public static readonly PostponedUpdate Undefined = new(int.MinValue, int.MinValue, false);

            public readonly bool Removal;
            public readonly int PosInOwnerList;
            public int CurrentPos;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public PostponedUpdate(int posInOwnerList, int currentPos, bool removal)
            {
                PosInOwnerList = posInOwnerList;
                CurrentPos = currentPos;
                Removal = removal;
            }

            public bool IsUndefined
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => PosInOwnerList == int.MinValue;
            }
        }

        [StructLayout(LayoutKind.Auto)]
        private readonly struct CenteredArray
        {
            public readonly int[] Data;
            private readonly int _mid;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public CenteredArray(int size)
            {
                Data = new int[size];
                _mid = Data.Length / 2;
            }

            public int this[int index]
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => Data[index + _mid];
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                set => Data[index + _mid] = value;
            }
        }
    }
}