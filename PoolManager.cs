using UnityEngine;
using UnityEngine.SceneManagement;

using System.Collections.Generic;
using System.Linq;

using System.IO;
using System.Text.RegularExpressions;

namespace GameManagement
{
    public static class PoolManager
    {
        private     const       string              pattern = @"\b(?<=Resources\u005C)(.*)(?=.prefab)";

        private     static      List<MonoBehaviour>             prefabs;

        private     static      List<IPool>                     pools;


                    static                          PoolManager                     ( )
        {
            pools                       =   new List<IPool>( );

            SceneManager.sceneLoaded    +=  SceneLoadedEventHandler;
            SceneManager.sceneUnloaded  +=  SceneUnloadedEventHandler;

            LoadPrefabs( );
        }

        private     static      void                SceneUnloadedEventHandler       ( Scene scene )
        {
            pools.Clear( );
            Resources.UnloadUnusedAssets( );
            System.GC.Collect( );
        }

        private     static      void                SceneLoadedEventHandler         ( Scene scene, LoadSceneMode mode )
        {
            if ( prefabs == null ) LoadPrefabs( );
        }

        private     static      void                LoadPrefabs                     ( )
        {
            prefabs         = new List<MonoBehaviour>( );
            Regex regex     = new Regex( pattern );

            foreach ( string item in Directory.GetFiles( Application.dataPath + "/Resources", "*.prefab", SearchOption.AllDirectories ) )
            {
                Match match = regex.Match( item );

                if ( match.Success )
                {
                    string          path     = match.Groups[ 0 ].Value;
                    MonoBehaviour   mono    = Resources.Load<MonoBehaviour>( path );

                    if ( mono && mono is IPoolObject )
                    {
                        prefabs.Add( mono );
                    }
                }
            }
        }

        private     static      IPool               AddPoolOfType<T>                ( UsageType usageType )                                         where T : MonoBehaviour, IPoolObject
        {
            IPool pool = new Pool<T>( usageType ) as IPool;

            pools.Add( pool );

            return pool;
        }

        public      static      bool                HavePoolOfType<T>               ( )                                                             where T : MonoBehaviour, IPoolObject
        {
            return pools.Any( pool => pool.TypeOf<T>( ) );
        }

        private     static      IPool               GetPoolOfType<T>                ( )                                                             where T : MonoBehaviour, IPoolObject
        {
            return pools.FirstOrDefault( pool => pool.TypeOf<T>( ) );
        }

        public      static      void                ClearPoolOfType<T>              ( )                                                             where T : MonoBehaviour, IPoolObject
        {
            IPool pool = GetPoolOfType<T>( );

            if ( pool != null ) pool.Clear( );
        }


        public      static      bool                Push<T>                         ( T item )                                                      where T : MonoBehaviour, IPoolObject
        {
            if ( item == null ) throw new System.NullReferenceException( "The thing you want to push is null." );

            IPool pool = GetPoolOfType<T>( );

            if ( pool != null )
            {
                if ( !pool.Push( item ) )
                {
                    // OnDestroy( ) calls automatically as MonoBehaviour message!
                    MonoBehaviour.Destroy( item.gameObject );
                    return false;
                }

                return true;
            }

            pool = AddPoolOfType<T>( item.UsageType );

            if ( !pool.Push( item ) )
            {
                // OnDestroy( ) calls automatically as MonoBehaviour message!
                MonoBehaviour.Destroy( item.gameObject );
                return false;
            }

            return true;
        }


        private     static      T                   Extract<T>                      ( )                                                             where T : MonoBehaviour, IPoolObject
        {
            IPool pool = GetPoolOfType<T>( );

            T item = default( T );

            if ( pool != null )
            {
                item = pool.Pop( ) as T;

                if ( item == null )
                {
                    if ( !pool.CanCache ) return null;

                    item = Create<T>( );

                    pool.Cache( item );
                }

                return item;
            }

            item = Create<T>( );

            AddPoolOfType<T>( item.UsageType );

            return item;
        }

        public      static      T                   Pop<T>                          ( )                                                             where T : MonoBehaviour, IPoolObject
        {
            T item = Extract<T>( );

            if ( item ) item.OnPop( );

            return item;
        }

        public      static      T                   Pop<T>                          ( Vector3 position, Quaternion rotation )                       where T : MonoBehaviour, IPoolObject
        {
            T item = Extract<T>( );

            if ( item )
            {
                item.transform.position = position;
                item.transform.rotation = rotation;
                
                item.OnPop( );
            }

            return item;
        }

        public      static      T                   PopAsChild<T>                   ( Transform parent, Vector3 position, Quaternion rotation )     where T : MonoBehaviour, IPoolObject
        {
            T item = Extract<T>( );

            if ( item )
            {
                item.transform.position = position;
                item.transform.rotation = rotation;

                item.transform.SetParent( parent.transform );

                item.OnPop( );
            }

            return item;
        }

        private     static      T                   Create<T>                       ( ) where T : MonoBehaviour, IPoolObject
        {
            T prefab = GetPrefabOfType<T>( );

            return MonoBehaviour.Instantiate( prefab ) as T;
        }

        private     static      T                   Create<T>                       ( Vector3 position, Quaternion rotation ) where T : MonoBehaviour, IPoolObject
        {
            T prefab = GetPrefabOfType<T>( );

            return MonoBehaviour.Instantiate( prefab, position, rotation ) as T;
        }

        public      static      bool                Remove<T>                       ( T item ) where T : MonoBehaviour, IPoolObject
        {
            if ( item == null ) throw new System.NullReferenceException( "The thing you want to remove is null." );

            IPool pool = GetPoolOfType<T>( );

            if ( pool != null )
            {
                if ( pool.Remove( item ) ) return true;

                Trace.Warning( "You trying to remove from the pool not pooled item." );
                return false;
            }

            Trace.Warning( "You trying to remove item from the nonexistent pool. WAT? o_O" );
            return false;
        }

        public      static      bool                Destroy<T>                      ( T item ) where T : MonoBehaviour, IPoolObject
        {
            if ( item == null ) throw new System.NullReferenceException( "The thing you want to destroy is null." );

            IPool pool = GetPoolOfType<T>( );

            if ( pool != null && pool.Destroy( item ) )
            {
                return true;
            }

            Trace.Warning( "You trying to destory item, that not controlled by PoolManager." );
            return false;
        }

        private     static      T                   GetPrefabOfType<T>              ( ) where T : MonoBehaviour, IPoolObject
        {
            T prefab = prefabs.FirstOrDefault( p => p.GetType( ) == typeof( T ) ) as T;

            if ( !prefab ) throw new System.NullReferenceException( "Prefab not found." );

            return prefab;
        }

        public      static      void                ClearAll                        ( )
        {
            foreach ( IPool pool in pools )
            {
                pool.Clear( );
            }
            
            Resources.UnloadUnusedAssets( );
            System.GC.Collect( );
        }
    }

}