using System.ComponentModel;

// Fixes error CS0518: 'IsExternalInit not defined or imported'
// which occurs when trying to use init setters (since C# LangVersion 9) in .NET 5.0 or lower
// Ref: https://stackoverflow.com/questions/64749385/predefined-type-system-runtime-compilerservices-isexternalinit-is-not-defined

// ReSharper disable once CheckNamespace
namespace System.Runtime.CompilerServices
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal static class IsExternalInit
    {
    }
}
