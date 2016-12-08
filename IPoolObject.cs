namespace GameManagement
{
    public interface IPoolObject
    {
        UsageType   UsageType   { get; }

        void        OnPush          ( );
        void        OnPushFailed    ( );
        void        OnPop           ( );
        void        OnDestroy       ( );
    }
}