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
    }

    public class Control
    {
        public Control() { }

        public Control(DateTime date, string itemId, int result, params Log[] logs)
        {
            this.Date   = date;
            this.ItemId = itemId;
            this.Result = result;
            this.Logs   = logs;
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
            this.Description = description;
        }

        public string Description { get; set; }
    }
}