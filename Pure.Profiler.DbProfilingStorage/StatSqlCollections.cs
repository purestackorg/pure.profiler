using Pure.Profiler.Configuration;
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

        public static List<StatDTO> GetStatWeb( ) {

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
                var dt = db.ExecuteList<StatDTO>(str);// db.ExecuteDataTable(str);

                return dt;
            }


            return null;


        }

        public static List<StatDTO> GetStatDb()
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

            var config = ConfigurationHelper.LoadPureProfilerConfigurationSection();
            if (config.DbType == "oracle")
            {
                str = @"select
Name as SQLStr, Count(NAME) as RequestCount,Sum(DurationMilliseconds) as SumDurationMilliseconds, 
ROUND(Avg(DurationMilliseconds),2) as AvgDurationMilliseconds, 
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
            }

            using (var db = new PureProfilingDbContext())
            {
                var dt = db.ExecuteList<StatDTO>(str);// db.ExecuteDataTable(str);

                return dt;
            }


            return null;


        }



    }
}



public class StatDTO
{
    public string SQLStr { get; set; }
    public int RequestCount { get; set; }
    public double SumDurationMilliseconds { get; set; }
    public double AvgDurationMilliseconds { get; set; }
    public double MaxDurationMilliseconds { get; set; }
    public string ExecuteType { get; set; }
    public int ErrorCount { get; set; }
    public int SumDExecuteResult { get; set; }
    public int DurationMilliseconds1 { get; set; }
    public int DurationMilliseconds2 { get; set; }
    public int DurationMilliseconds3 { get; set; }
    public int DurationMilliseconds4 { get; set; }
    public int DurationMilliseconds5 { get; set; }
    public int DurationMilliseconds6 { get; set; }
    public int DurationMilliseconds7 { get; set; }

 


    public string HttpVerb { get; set; }
    public string URI { get; set; }
    public int SumDbCount { get; set; }
    public int SumDbDurationMilliseconds { get; set; }
     


}