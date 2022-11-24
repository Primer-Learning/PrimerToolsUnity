#if !POSITIONAL_RECORD_CLASSES_WORK
#define POSITIONAL_RECORD_CLASSES_WORK

using System.ComponentModel;

// This is required because of a bug in .NET core 5.0
// In order to make positional record classes work such as
//
// public record class Person(string FirstName, string LastName);
//
// https://stackoverflow.com/questions/62648189/testing-c-sharp-9-0-in-vs2019-cs0518-isexternalinit-is-not-defined-or-imported

// ReSharper disable once CheckNamespace
namespace System.Runtime.CompilerServices
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class IsExternalInit {}
}

#endif
