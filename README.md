# pure.profiler
PureProfiler is an open source .NET performance debugging library which trace web or sql execute info.
support netstandard


Pure.Profiler does not attach itself to every single method call; that would be too invasive and wouldn't focus on the biggest performance issues. Instead, it provides:

- An ADO.NET profiler, capable of profiling calls on raw ADO.NET (SQL Server, Oracle, etc), LINQ-to-SQL, Entity Framework (including Code First and EF Core), and a range of other data access scenarios.
- A pragmatic Step instrumentation that you can add to code you want to explicitly profile.

Simple. Fast. Pragmatic. Useful. 
