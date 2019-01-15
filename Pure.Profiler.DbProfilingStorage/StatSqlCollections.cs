using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Pure.Profiler.DbProfilingStorage
{
    public class StatSqlCollections
    {
        public static string GetDataRowValue(DataRow row, string key) {
            if (row != null)
            {
                var o = row[key];
                if ( o != null)
                {
                    return o.ToString();
                }
            }

            return "";
        }

        public static DataTable GetStatWeb( ) {

            string str = @"select
HttpVerb as HttpVerb,
Name as URI, Count(NAME)  as RequestCount,Sum(DurationMilliseconds) as SumDurationMilliseconds, 
Avg(DurationMilliseconds) as AvgDurationMilliseconds,  
Max(DurationMilliseconds) as MaxDurationMilliseconds,
Sum(ErrorCount) as ErrorCount, 
Sum(DbCount) as SumDbCount, 
Sum(DbDuration) as SumDbDurationMilliseconds,
count(case when DurationMilliseconds >=0 and DurationMilliseconds <=1 then SEQ end) as DurationMilliseconds1,
count(case when DurationMilliseconds>1 and DurationMilliseconds <=10 then SEQ end) as DurationMilliseconds2,
count(case when DurationMilliseconds>10 and DurationMilliseconds <=100 then SEQ end) as DurationMilliseconds3,
count(case when DurationMilliseconds>100 and DurationMilliseconds <=1000 then SEQ end) as DurationMilliseconds4,
count(case when DurationMilliseconds>1000 and DurationMilliseconds <=10000 then SEQ end) as DurationMilliseconds5,
count(case when DurationMilliseconds>10000 and DurationMilliseconds <=100000 then SEQ end) as DurationMilliseconds6,
count(case when DurationMilliseconds>100000 then SEQ end) as DurationMilliseconds7
 from sys_pureprofiling  
where Type ='session' group by NAME, HttpVerb order by  RequestCount desc";

            using (var db = new PureProfilingDbContext())
            {
                var dt = db.ExecuteDataTable(str);

                return dt;
            }


            return null;


        }

        public static DataTable GetStatDb()
        {

            string str = @"select
Name as SQLStr, Count(NAME) as RequestCount,Sum(DurationMilliseconds) as SumDurationMilliseconds, 
Avg(DurationMilliseconds) as AvgDurationMilliseconds, 
Max(DurationMilliseconds) as MaxDurationMilliseconds,
ExecuteType as ExecuteType, 
Sum(ErrorCount) as ErrorCount, 
Sum(ExecuteResult) as SumDExecuteResult, 
count(case when DurationMilliseconds >=0 and DurationMilliseconds <=1 then SEQ end) as DurationMilliseconds1,
count(case when DurationMilliseconds>1 and DurationMilliseconds <=10 then SEQ end) as DurationMilliseconds2,
count(case when DurationMilliseconds>10 and DurationMilliseconds <=100 then SEQ end) as DurationMilliseconds3,
count(case when DurationMilliseconds>100 and DurationMilliseconds <=1000 then SEQ end) as DurationMilliseconds4,
count(case when DurationMilliseconds>1000 and DurationMilliseconds <=10000 then SEQ end) as DurationMilliseconds5,
count(case when DurationMilliseconds>10000 and DurationMilliseconds <=100000 then SEQ end) as DurationMilliseconds6,
count(case when DurationMilliseconds>100000 then SEQ end) as DurationMilliseconds7
 from sys_pureprofiling  
where Type ='db' group by NAME, ExecuteType order by  RequestCount desc  ";

            using (var db = new PureProfilingDbContext())
            {
                var dt = db.ExecuteDataTable(str);

                return dt;
            }


            return null;


        }



    }
}
