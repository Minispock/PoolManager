using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace GameManagement
{
    class Pool<T> : IPool where T : class, IPoolObject
    {
        private         System.Type         type;

        private         List<T>             items;
        private         int                 size;
        private         int                 Size
        {
            get { return size; }
        }

        private         List<T>             cache;
        private         int                 cacheSize;

        public          bool                CanPush
        {
            get { return items.Count + 1 < size; }
        }

        public          bool                IsEmpty
        {
            get { return items.Count == 0; }
        }

        public          bool                CanCache
        {
            get { return cache.Count + 1 < cacheSize; }
        }


        //__________ CONSTRUCTORS __________

        public                              Pool            (int size)
        {
            type = typeof( T );

            this.size       = size;
            this.cacheSize  = size;

            items           = new List<T>( size );
            cache           = new List<T>( size );
        }

        public                              Pool            ( UsageType usageType ) : this( ( int ) usageType )
        {

        }

        //__________ METHODS __________

        public          bool                Push            ( IPoolObject item )
        {
            // Check for null performed outside.

            if ( CanPush )
            {
                if ( cache.Contains( item as T ) )
                {
                    cache.Remove( item as T );
                }

                items.Add( item as T );
                item.OnPush( );

                return true;
            }

            if ( cache.Contains( item as T ) )
            {
                cache.Remove( item as T );
            }

            item.OnPushFailed( );
            // Destroy( ) needs to be called externally by PoolManager.
            // OnDestroy( ) calls automatically as MonoBehaviour message!

            return false;
        }

        public          IPoolObject         Pop             ( )
        {
            if ( IsEmpty ) return null;

            T item = items.FirstOrDefault( );

            // The item.OnPop( ) needs to be called externally by PoolManager.

            items.Remove( item as T );
            cache.Add( item );
            return item;
        }

        public          bool                Cache           ( IPoolObject item )
        {
            if ( item == null || !CanCache ) return false;

            cache.Add( item as T );

            return true;
        }

        public          bool                Remove          ( IPoolObject item )
        {
            // Check for null performed outside.

            if ( items.Contains( item as T ) )
            {
                items.Remove( item as T );
                MonoBehaviour.Destroy( ( item as MonoBehaviour ).gameObject );
                // OnDestroy( ) calls automatically as MonoBehaviour message!

                return true;
            }

            return false;
        }

        public          bool                Destroy         ( IPoolObject item )
        {
            // Check for null performed outside.

            if ( !Remove( item ) && cache.Contains( item as T ) )
            {
                cache.Remove( item as T );
                MonoBehaviour.Destroy( ( item as MonoBehaviour ).gameObject );
                // OnDestroy( ) calls automatically as MonoBehaviour message!

                return true;
            }

            return false;
        }

        public          void                Clear           ( )
        {
            foreach ( IPoolObject item in items )
            {
                MonoBehaviour.Destroy( ( item as MonoBehaviour ).gameObject );
                // OnDestroy( ) calls automatically as MonoBehaviour message!
            }

            items.Clear( );

            Resources.UnloadUnusedAssets( );
        }

        public          bool                Contains        ( IPoolObject item )
        {
            return items.Contains( item as T );
        }

        public          List<IPoolObject>   ExtractAll      ( )
        {
            throw new NotImplementedException( );
        }

                        bool                IPool.TypeOf<R> ( )
        {
            return type == typeof( R );
        }

        public static implicit operator bool( Pool<T> pool )
        {
            return pool != null;
        }
    }
}