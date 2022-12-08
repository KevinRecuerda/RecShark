// using System;
// using System.Linq;
// using System.Linq.Expressions;
// using Marten;
// using Marten.Linq;
// using Marten.Schema;
// using Marten.Util;
//
// namespace RecShark.Data.Db.Document.MartenExtensions
// {
//     using System.Reflection;
//     using Marten.Internal.Sessions;
//     using Marten.Linq.Parsing;
//     using RecShark.Extensions;
//     using Weasel.Postgresql.Tables;
//
//     public static class MartenRegistryExtensions
//     {
//         // TODO : check still need index methods 
//         public static StoreOptions GinIndex<T>(this StoreOptions options, string shortName, string column, string op = "")
//         {
//             return options.
//                 Index<T>(shortName, column, IndexMethod.gin, op);
//         }
//         
//         public static StoreOptions GistIndex<T>(this StoreOptions options, string shortName, string column, string op = "")
//         {
//             return options.Index<T>(shortName, column, IndexMethod.gist, op);
//         }
//         
//         public static StoreOptions GistIndex<T>(this StoreOptions options, Expression<Func<T, object>> expression, string op = "")
//         {
//             return options.Index(expression, IndexMethod.gist, op);
//         }
//         
//         public static StoreOptions Index<T>(this StoreOptions options, Expression<Func<T, object>> expression, IndexMethod method, string op = "")
//         {
//             var member    = FindMembers.Determine(expression).Single();
//             var shortName = member.ToTableAlias();
//         
//             var doc = options.Storage.MappingFor(typeof(T));
//             var sql = doc.FieldFor(member).RawLocator.Replace("d.", "");
//         
//             return options.Index<T>(shortName, sql, method, op);
//         }
//
//         public static StoreOptions Index<T>(this StoreOptions options, QuerySession session, string shortName, string column, IndexMethod method, string op = "")
//         {
//             var storage   = ((QuerySession) session.DocumentStore.QuerySession()).StorageFor(typeof(T));
//             var doc = (DocumentMapping) storage?.GetType().GetField("_mapping", BindingFlags.NonPublic|BindingFlags.Instance).GetValue(storage);
//             //var doc   = options.Storage.MappingFor(typeof(T));
//             // var index = new SimpleIndex(doc, shortName, column, method, op);
//             
//             doc.Indexes.Add(index);
//             // return options;
//         }
//         
//         // public static MartenRegistry.DocumentMappingExpression<T> PartialUniqueIndex<T>(
//         //     this MartenRegistry.DocumentMappingExpression<T> doc,
//         //     string                                           where,
//         //     params Expression<Func<T, object>>[]             expressions)
//         // {
//         //     return doc.Index(
//         //         expressions.ToList(),
//         //         x =>
//         //         {
//         //             x.IsUnique = true;
//         //             x.Where    = where;
//         //         });
//         // }
//     }
// }
