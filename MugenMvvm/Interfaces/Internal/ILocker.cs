﻿using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.Internal
{
    public interface ILocker : IHasPriority
    {
        void Enter(ref bool lockTaken);

        void TryEnter(int timeout, ref bool lockTaken);

        void Exit();
    }
}