using System;

namespace RecShark.Data.Db.Document.Tests.MartenExtensions
{
    public enum ItemType
    {
        A,
        B
    }

    public class Item
    {
        public string   Id   { get; set; }
        public string   Name { get; set; }
        public ItemType Type { get; set; }

        public Guid FirstControlId { get; set; }
        public Guid LastControlId  { get; set; }
    }

    public class Control
    {
        public Control() { }

        public Control(DateTime date, string itemId, int result, params Log[] logs)
        {
            Date   = date;
            ItemId = itemId;
            Result = result;
            Logs   = logs;
        }

        public Guid     Id     { get; set; }
        public DateTime Date   { get; set; }
        public string   ItemId { get; set; }
        public int      Result { get; set; }

        public Log[] Logs { get; set; }
    }

    public class Log
    {
        public Log(string description)
        {
            Description = description;
        }

        public string Description { get; set; }
    }

    public class Aggregate
    {
        public Guid      Id     { get; set; }
        public int       Result { get; set; }
        public ItemType? Type   { get; set; }
        public string    Name   { get; set; }
        public Item      Item   { get; set; }
    }
}
