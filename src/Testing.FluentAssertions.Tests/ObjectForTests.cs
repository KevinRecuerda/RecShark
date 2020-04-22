using System;

namespace RecShark.Extensions.Testing.FluentAssertions.Tests
{
    public class ObjectForTests
    {
        public ObjectForTests(int id, double value, DateTime date)
        {
            this.Id = id;
            this.Value = value;
            this.Date = date;
        }

        public int Id { get; }
        public double Value { get; }
        public DateTime Date { get; }
    }
}