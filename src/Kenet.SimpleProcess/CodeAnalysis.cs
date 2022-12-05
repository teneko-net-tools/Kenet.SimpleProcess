#if !NET5_0_OR_GREATER
namespace System.Diagnostics.CodeAnalysis
{
    [ExcludeFromCodeCoverage]
    [DebuggerNonUserCode]
    [AttributeUsage(AttributeTargets.Constructor, AllowMultiple = false, Inherited = false)]
    internal sealed class SetsRequiredMembersAttribute : Attribute
    {
    }
}
#endif
