using Pure.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pure.Profiler.DbProfilingStorage
{
    public class PureProfilingEntitysMapper : ClassMapper<PureProfilingEntity>
    {
        public PureProfilingEntitysMapper()
        {
            Table("Sys_PureProfiling");
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

            Map(m => m.ExecuteType).Description("执行类型").Size(50);
            Map(m => m.ExecuteResult).Description("执行影响行数").Size(50);
            Map(m => m.Parameters).Description("参数").Size(1000);
            Map(m => m.HttpVerb).Description("Http动作").Size(50);
            Map(m => m.IsAjax).Description("是否Ajax").Size(50);
            Map(m => m.ClientIp).Description("客户端IP").Size(50);
            Map(m => m.DbCount).Description("数据库执行次数").Size(50);
            Map(m => m.DbDuration).Description("数据库耗时毫秒").Size(50);
            Map(m => m.RequestType).Description("请求类型").Size(50);
            Map(m => m.ErrorCount).Description("错误数量").Size(50);
            

            AutoMap();
        }
    }
}
