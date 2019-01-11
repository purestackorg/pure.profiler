using Pure.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pure.Profiler.DbProfilingStorage
{
    public class TestEntityEntitysMapper : ClassMapper<TestEntity>
    {
        public TestEntityEntitysMapper()
        {
            Table("Sys_TestData");
            Description("PureProfiling");
            Map(m => m.SEQ).Key(KeyType.Assigned).Description("主键").Size(50);
            Map(m => m.MachineName).Description("所在机器").Size(120);
            Map(m => m.Type).Description("类型").Size(50);
            Map(m => m.Id).Description("编号").Size(50);
            Map(m => m.ParentId).Description("父亲编号").Size(50);
            Map(m => m.Name).Description("名称").Size(1000);
            Map(m => m.Started).Description("开始时间");
            Map(m => m.StartMilliseconds).Description("Gets or sets the start milliseconds since the start of the profling session.");
            Map(m => m.DurationMilliseconds).Description("Gets or sets the duration milliseconds of the timing");
            Map(m => m.Tags).Description("标签").Size(1000);
            Map(m => m.Sort).Description("序号").Size(500);
            Map(m => m.Data).Description("数据").Size(1000);


            AutoMap();
        }
    }
}
