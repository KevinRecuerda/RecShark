﻿using System.Linq;
using Weasel.Postgresql.SqlGeneration;

namespace RecShark.Data.Db.Document.MartenExtensions
{
    public class CollectionWhereFragment : CustomizableWhereFragment
    {
        public CollectionWhereFragment(string sql, params object[] parameters) : base(
            sql,
            "??",
            parameters.Select(x => new CommandParameter(x)).ToArray()) { }
    }
}
