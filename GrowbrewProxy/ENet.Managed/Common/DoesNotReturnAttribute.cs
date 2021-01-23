using System;

namespace ENet.Managed.Common
{
// Only declare this class for target frameworks that lack System.Diagnostics.CodeAnalysis.DoesNotReturnAttribute
#if NET46 || NETSTANDARD2_0
    [AttributeUsage(AttributeTargets.Method)]
    internal sealed class DoesNotReturnAttribute : Attribute { }
#endif
}
