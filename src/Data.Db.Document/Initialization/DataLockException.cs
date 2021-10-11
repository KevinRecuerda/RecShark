using System;

namespace RecShark.Data.Db.Document.Initialization
{
    public class DataLockException : Exception
    {
        public DataLockException(int count)
            : base($"Could not acquire database lock after {count} retry") { }
    }
}