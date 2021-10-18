using Marten;

namespace RecShark.Data.Db.Document.Initialization
{
    public abstract class DataChange : IDataChange
    {
        public abstract string Id { get; }

        public abstract int Version      { get; }
        public virtual  int PatchVersion { get; } = 0;

        public virtual bool RunAlways { get; } = false;

        public virtual ExecutionMode ExecutionMode { get; set; } = ExecutionMode.PostSchemaChanges;

        public abstract void Run(IDocumentSession session, IDocumentStore store);

        public DataChangeLog ToLog()
        {
            return new DataChangeLog
            {
                Id            = Id,
                Version       = Version,
                PatchVersion  = PatchVersion,
                RunAlways     = RunAlways,
                ExecutionMode = ExecutionMode
            };
        }
    }

    public enum ExecutionMode
    {
        PreSchemaChanges,
        PostSchemaChanges
    }

    public class DataChangeLog : IDataChange
    {
        public string        Id            { get; set; }
        public int           Version       { get; set; }
        public int           PatchVersion  { get; set; }
        public bool          RunAlways     { get; set; }
        public ExecutionMode ExecutionMode { get; set; }
    }

    public interface IDataChange
    {
        string        Id            { get; }
        int           Version       { get; }
        int           PatchVersion  { get; }
        bool          RunAlways     { get; }
        ExecutionMode ExecutionMode { get; }
    }

    public static class DataChangeExtensions
    {
        public static bool IsMoreRecent(this IDataChange change, IDataChange other)
        {
            return change.Version > other.Version
                || change.Version == other.Version && change.PatchVersion > other.PatchVersion;
        }
    }
}