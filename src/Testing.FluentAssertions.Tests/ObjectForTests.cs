using System;
using System.Diagnostics;

namespace RecShark.Extensions.Testing.FluentAssertions.Tests
{
    [DebuggerDisplay("{ToString()}")]
    public class ObjectForTests
    {
        public ObjectForTests(int id, double value, DateTime date)
        {
            Id = id;
            Value = value;
            Date = date;
        }

        public int Id { get; set; }
        public double Value { get; set; }
        public DateTime Date { get; set; }

        public override string ToString()
        {
            var items = new [] {Id.ToString(), Value.ToString("F2"), Date.ToString("yyyy-MM-dd")};
            return string.Join("|", items);
        }

        public ObjectForTests Clone()
        {
            return new ObjectForTests(Id, Value, Date);
        }
    }
}