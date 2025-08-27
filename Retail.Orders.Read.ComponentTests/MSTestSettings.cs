using Microsoft.VisualStudio.TestTools.UnitTesting;

// MSTest assembly configuration
[assembly: Parallelize(Workers = 0, Scope = ExecutionScope.MethodLevel)]