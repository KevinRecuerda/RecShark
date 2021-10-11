using RecShark.Data.Db.Document.Initialization;

namespace RecShark.Data.Db.Document.Tests.Initialization
{
    public class ObjectForTests
    {
        public string Id { get; set; }
    }

    public class ObjectDataChange : ItemsDataChange
    {
        public override string   Id           => "change";
        public override int      Version      => 1;
        public override object[] BuildItems() => new object[] {new ObjectForTests() {Id = "id"}};
    }

    public class ObjectDataChangeExecuteBeforeSchemaChanges : ItemsDataChange
    {
        public override string        Id            => "change_before_schema_changes";
        public override int           Version       => 1;
        public override ExecutionMode ExecutionMode => ExecutionMode.PreSchemaChanges;

        public override object[] BuildItems() => new object[] {new ObjectForTests() {Id = "id"}};
    }

    public class AnotherObjectForTests : ObjectForTests { }

    public class AnotherObjectDataChange : ItemsDataChange
    {
        public override string   Id           => "another_change";
        public override int      Version      => 1;
        public override object[] BuildItems() => new object[] {new AnotherObjectForTests() {Id = "another id"}};
    }
}