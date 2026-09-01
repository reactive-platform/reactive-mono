namespace Reactive.Compiler;

/// <summary>
/// Specifies a raw property that should be excluded from the automatic state generation.
/// Used for cases when you need a complex in-out flow or just want to observe the state manually for certain reasons.
/// Note that by using this attribute you take the responsibility of managing state lifetimes, hence you have to bind and unbind them properly. 
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class RawStateAttribute : Attribute;