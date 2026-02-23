using System.Reflection;

namespace OspreyPulseAPI.Modules.Identity.Application;

/// <summary>
/// Marker type for assembly discovery (e.g. MediatR, AutoMapper).
/// Prevents brittle typeof(SomeHandler) references.
/// </summary>
public static class AssemblyReference
{
    public static Assembly Assembly => typeof(AssemblyReference).Assembly;
}
