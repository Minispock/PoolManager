using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameManagement
{
    interface IPool
    {
        bool        CanPush { get; }
        bool        IsEmpty { get; }
        bool        CanCache { get; }

        bool        Push        ( IPoolObject item );
        IPoolObject Pop         ( );

        bool        Cache       ( IPoolObject item );

        bool        Remove      ( IPoolObject item );
        bool        Destroy     ( IPoolObject item );
        void        Clear       ( );

        bool        Contains    ( IPoolObject item );

        bool        TypeOf<R>   ( )     where R : class, IPoolObject;
    }
}
