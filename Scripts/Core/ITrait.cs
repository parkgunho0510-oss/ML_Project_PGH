namespace Wooduduk.Core
{
    public interface ITrait
    {
        string TraitID { get; }
        string DisplayName { get; }
        void Apply(IPlayerStats stats, object source);
        void Remove(IPlayerStats stats, object source);
    }
}
