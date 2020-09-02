using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// ReSharper disable once CheckNamespace
namespace MugenMvvm.Internal
{
    public static class DiffUtil
    {
        #region Methods

        public static DiffResult CalculateDiff(ICallback cb, bool detectMoves = true)
        {
            var oldSize = cb.GetOldListSize();
            var newSize = cb.GetNewListSize();

            List<Diagonal> diagonals = new List<Diagonal>();

            // instead of a recursive implementation, we keep our own stack to avoid potential stack
            // overflow exceptions
            List<Range> stack = new List<Range>();

            stack.Add(new Range(0, oldSize, 0, newSize));

            var max = (oldSize + newSize + 1) / 2;
            // allocate forward and backward k-lines. K lines are diagonal lines in the matrix. (see the
            // paper for details)
            // These arrays lines keep the max reachable position for each k-line.
            var forward = new CenteredArray(max * 2 + 1);
            var backward = new CenteredArray(max * 2 + 1);

            // We pool the ranges to avoid allocations for each recursive call.
            List<Range> rangePool = new List<Range>();
            while (stack.Count != 0)
            {
                Range range = RemoveAt(stack, stack.Count - 1);
                MidPoint(range, cb, forward, backward, out var snake);
                if (snake.IsUndefined)
                    rangePool.Add(range);
                else
                {
                    // if it has a diagonal, save it
                    if (snake.DiagonalSize > 0)
                        diagonals.Add(snake.ToDiagonal());
                    // add new ranges for left and right
                    Range left = rangePool.Count == 0 ? new Range() : RemoveAt(rangePool, rangePool.Count - 1);
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
            diagonals.Sort((o1, o2) => o1.X - o2.X);

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
        private static Range RemoveAt(List<Range> list, int index)
        {
            var foo = list[index];
            list.RemoveAt(index);
            return foo;
        }

        #endregion

        #region Nested types

        public interface ICallback
        {
            public int GetOldListSize();

            public int GetNewListSize();

            public bool AreItemsTheSame(int oldItemPosition, int newItemPosition);

            public bool AreContentsTheSame(int oldItemPosition, int newItemPosition);
        }

        public interface IListUpdateCallback
        {
            bool IsUseFinalPosition { get; }

            void OnInserted(int position, int count);

            void OnRemoved(int position, int count);

            void OnMoved(int fromPosition, int toPosition);

            void OnChanged(int position, int count, bool isMove);
        }

        [StructLayout(LayoutKind.Auto)]
        internal readonly struct Diagonal
        {
            #region Fields

            public readonly int Size;
            public readonly int X;
            public readonly int Y;

            #endregion

            #region Constructors

            public Diagonal(int x, int y, int size)
            {
                X = x;
                Y = y;
                Size = size;
            }

            #endregion

            #region Methods

            public int EndX => X + Size;

            public int EndY => Y + Size;

            #endregion
        }

        [StructLayout(LayoutKind.Auto)]
        private readonly struct Snake
        {
            #region Fields

            public readonly int EndX;
            public readonly int EndY;
            private readonly bool _reverse;
            public readonly int StartX;
            public readonly int StartY;

            public static readonly Snake Undefined = new Snake(int.MinValue, int.MinValue, int.MinValue, int.MinValue, false);

            #endregion

            #region Constructors

            public Snake(int startX, int startY, int endX, int endY, bool reverse)
            {
                EndX = endX;
                EndY = endY;
                _reverse = reverse;
                StartX = startX;
                StartY = startY;
            }

            #endregion

            #region Properties

            public bool IsUndefined => EndX == int.MinValue && StartX == int.MinValue;

            private bool HasAdditionOrRemoval => EndY - StartY != EndX - StartX;

            private bool IsAddition => EndY - StartY > EndX - StartX;

            public int DiagonalSize => Math.Min(EndX - StartX, EndY - StartY);

            #endregion

            #region Methods

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

            #endregion
        }

        [StructLayout(LayoutKind.Auto)]
        private struct Range
        {
            #region Fields

            public int NewListStart, NewListEnd;
            public int OldListStart, OldListEnd;

            #endregion

            #region Constructors

            public Range(int oldListStart, int oldListEnd, int newListStart, int newListEnd)
            {
                OldListStart = oldListStart;
                OldListEnd = oldListEnd;
                NewListStart = newListStart;
                NewListEnd = newListEnd;
            }

            #endregion

            #region Properties

            public int OldSize => OldListEnd - OldListStart;

            public int NewSize => NewListEnd - NewListStart;

            #endregion
        }

        [StructLayout(LayoutKind.Auto)]
        public readonly struct DiffResult
        {
            #region Fields

            private readonly ICallback _callback;
            private readonly bool _detectMoves;
            private readonly List<Diagonal> _diagonals;
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

            #endregion

            #region Constructors

            internal DiffResult(ICallback callback, List<Diagonal> diagonals, int[] oldItemStatuses, int[] newItemStatuses, bool detectMoves)
            {
                _diagonals = diagonals;
                _oldItemStatuses = oldItemStatuses;
                _newItemStatuses = newItemStatuses;
                Array.Fill(_oldItemStatuses, 0);
                Array.Fill(_newItemStatuses, 0);
                _callback = callback;
                _oldListSize = callback.GetOldListSize();
                _newListSize = callback.GetNewListSize();
                _detectMoves = detectMoves;
                AddEdgeDiagonals();
                FindMatchingItems();
            }

            #endregion

            #region Methods

            private void AddEdgeDiagonals()
            {
                // see if we should add 1 to the 0,0
                var shouldAdd = _diagonals.Count == 0;
                if (!shouldAdd)
                {
                    var diagonal = _diagonals[0];
                    shouldAdd = diagonal.X != 0 || diagonal.Y != 0;
                }

                if (shouldAdd)
                    _diagonals.Insert(0, new Diagonal(0, 0, 0));
                // always add one last
                _diagonals.Add(new Diagonal(_oldListSize, _newListSize, 0));
            }

            private void FindMatchingItems()
            {
                for (var index = 0; index < _diagonals.Count; index++)
                {
                    var diagonal = _diagonals[index];
                    for (var offset = 0; offset < diagonal.Size; offset++)
                    {
                        var posX = diagonal.X + offset;
                        var posY = diagonal.Y + offset;
                        var theSame = _callback.AreContentsTheSame(posX, posY);
                        var changeFlag = theSame ? FlagNotChanged : FlagChanged;
                        _oldItemStatuses[posX] = posY << FlagOffset | changeFlag;
                        _newItemStatuses[posY] = posX << FlagOffset | changeFlag;
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
                for (var index = 0; index < _diagonals.Count; index++)
                {
                    var diagonal = _diagonals[index];
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
                var diagonalsSize = _diagonals.Count;
                for (var i = 0; i < diagonalsSize; i++)
                {
                    var diagonal = _diagonals[i];
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
                                _oldItemStatuses[posX] = posY << FlagOffset | changeFlag;
                                _newItemStatuses[posY] = posX << FlagOffset | changeFlag;
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

            public void DispatchUpdatesTo(IListUpdateCallback updateCallback)
            {
                var batchingCallback = new BatchingListUpdateCallback(updateCallback);
                var isUseFinalPosition = batchingCallback.IsUseFinalPosition;
                // track up to date current list size for moves
                // when a move is found, we record its position from the end of the list (which is
                // less likely to change since we iterate in reverse).
                // Later when we find the match of that move, we dispatch the update
                var currentListSize = _oldListSize;
                // list of postponed moves
                List<PostponedUpdate> postponedUpdates = new List<PostponedUpdate>();
                // posX and posY are exclusive
                var posX = _oldListSize;
                var posY = _newListSize;
                // iterate from end of the list to the beginning.
                // this just makes offsets easier since changes in the earlier indices has an effect
                // on the later indices.
                for (var diagonalIndex = _diagonals.Count - 1; diagonalIndex >= 0; diagonalIndex--)
                {
                    var diagonal = _diagonals[diagonalIndex];
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
                            PostponedUpdate postponedUpdate = GetPostponedUpdate(postponedUpdates, newPos, false);
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
                                if (isUseFinalPosition)
                                {
                                    batchingCallback.OnMoved(posX, postponedUpdate.PosInOwnerList);
                                    if ((status & FlagMovedChanged) != 0)
                                        batchingCallback.OnChanged(postponedUpdate.PosInOwnerList, 1, true);
                                }
                                else
                                {
                                    var updatedNewPos = currentListSize - postponedUpdate.CurrentPos;
                                    batchingCallback.OnMoved(posX, updatedNewPos - 1);
                                    if ((status & FlagMovedChanged) != 0)
                                        batchingCallback.OnChanged(updatedNewPos - 1, 1, true);
                                }
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
                            PostponedUpdate postponedUpdate = GetPostponedUpdate(postponedUpdates, oldPos, true);
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
                                if (isUseFinalPosition)
                                {
                                    batchingCallback.OnMoved(postponedUpdate.PosInOwnerList, posY);
                                    if ((status & FlagMovedChanged) != 0)
                                        batchingCallback.OnChanged(posY, 1, true);
                                }
                                else
                                {
                                    var updatedOldPos = currentListSize - postponedUpdate.CurrentPos - 1;
                                    batchingCallback.OnMoved(updatedOldPos, posX);
                                    if ((status & FlagMovedChanged) != 0)
                                        batchingCallback.OnChanged(posX, 1, true);
                                }
                            }
                        }
                        else
                        {
                            // simple addition
                            batchingCallback.OnInserted(isUseFinalPosition ? posY : posX, 1);
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
                            batchingCallback.OnChanged(posX, 1, false);

                        posX++;
                        posY++;
                    }

                    // snap back for the next diagonal
                    posX = diagonal.X;
                    posY = diagonal.Y;
                }

                batchingCallback.DispatchLastEvent();
            }

            private static PostponedUpdate GetPostponedUpdate(List<PostponedUpdate> postponedUpdates, int posInList, bool removal)
            {
                var postponedUpdate = PostponedUpdate.Undefined;
                for (var i = 0; i < postponedUpdates.Count; i++)
                {
                    var update = postponedUpdates[i];
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
                        postponedUpdates[i] = update;
                    }
                }

                return postponedUpdate;
            }

            #endregion
        }

        [StructLayout(LayoutKind.Auto)]
        private ref struct BatchingListUpdateCallback
        {
            #region Fields

            private readonly IListUpdateCallback _callback;
            public readonly bool IsUseFinalPosition;
            private int _lastEventCount;
            private int _lastEventPosition;
            private int _lastEventType;
            private bool _lastMoved;

            private const int TypeNone = 0;
            private const int TypeAdd = 1;
            private const int TypeRemove = 2;
            private const int TypeChange = 3;

            #endregion

            #region Constructors

            public BatchingListUpdateCallback(IListUpdateCallback callback)
            {
                _lastEventCount = -1;
                _lastEventPosition = -1;
                _lastMoved = false;
                _lastEventType = TypeNone;
                _callback = callback;
                IsUseFinalPosition = callback.IsUseFinalPosition;
            }

            #endregion

            #region Implementation of interfaces

            public void OnInserted(int position, int count)
            {
                if (_lastEventType == TypeAdd &&
                    (IsUseFinalPosition && _lastEventPosition >= position && _lastEventPosition <= position + count || position >= _lastEventPosition && position <= _lastEventPosition + _lastEventCount))
                {
                    _lastEventCount += count;
                    _lastEventPosition = Math.Min(position, _lastEventPosition);
                }
                else
                {
                    DispatchLastEvent();
                    _lastEventPosition = position;
                    _lastEventCount = count;
                    _lastEventType = TypeAdd;
                }
            }

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

            public void OnMoved(int fromPosition, int toPosition)
            {
                DispatchLastEvent();
                _callback.OnMoved(fromPosition, toPosition);
            }

            public void OnChanged(int position, int count, bool moved)
            {
                if (_lastEventType == TypeChange && _lastMoved == moved && !(position > _lastEventPosition + _lastEventCount || position + count < _lastEventPosition))
                {
                    var previousEnd = _lastEventPosition + _lastEventCount;
                    _lastEventPosition = Math.Min(position, _lastEventPosition);
                    _lastEventCount = Math.Max(previousEnd, position + count) - _lastEventPosition;
                }
                else
                {
                    DispatchLastEvent();
                    _lastMoved = moved;
                    _lastEventPosition = position;
                    _lastEventCount = count;
                    _lastEventType = TypeChange;
                }
            }

            #endregion

            #region Methods

            public void DispatchLastEvent()
            {
                if (_lastEventType == TypeNone)
                    return;
                switch (_lastEventType)
                {
                    case TypeAdd:
                        _callback.OnInserted(_lastEventPosition, _lastEventCount);
                        break;
                    case TypeRemove:
                        _callback.OnRemoved(_lastEventPosition, _lastEventCount);
                        break;
                    case TypeChange:
                        _callback.OnChanged(_lastEventPosition, _lastEventCount, _lastMoved);
                        break;
                }

                _lastEventType = TypeNone;
            }

            #endregion
        }

        [StructLayout(LayoutKind.Auto)]
        private struct PostponedUpdate
        {
            #region Fields

            public static readonly PostponedUpdate Undefined = new PostponedUpdate(int.MinValue, int.MinValue, false);

            public readonly bool Removal;
            public readonly int PosInOwnerList;
            public int CurrentPos;

            #endregion

            #region Constructors

            public PostponedUpdate(int posInOwnerList, int currentPos, bool removal)
            {
                PosInOwnerList = posInOwnerList;
                CurrentPos = currentPos;
                Removal = removal;
            }

            #endregion

            #region Properties

            public bool IsUndefined => PosInOwnerList == int.MinValue;

            #endregion
        }

        [StructLayout(LayoutKind.Auto)]
        private readonly struct CenteredArray
        {
            #region Fields

            public readonly int[] Data;
            private readonly int _mid;

            #endregion

            #region Constructors

            public CenteredArray(int size)
            {
                Data = new int[size];
                _mid = Data.Length / 2;
            }

            #endregion

            #region Properties

            public int this[int index]
            {
                get => Data[index + _mid];
                set => Data[index + _mid] = value;
            }

            #endregion
        }

        #endregion
    }
}