namespace Reactive.Compiler;

/// <summary>
/// Generates versions of the marked method or type taking 2 to <see cref="MaxArity"/> instances of the growing elements.
/// The template itself is the version for one. Methods get their overloads in the same type, which has to be partial,
/// types get copies with more type parameters next to them. Both have to be top-level.
/// </summary>
/// <example>
/// <code>
/// [VariadicGen(3, "T{}", "IState&lt;T{}&gt; state{}", "state{}")]
/// public static void Log&lt;T1&gt;(IState&lt;T1&gt; state1) => Print(state1);
/// </code>
/// Generates
/// <code>
/// public static void Log&lt;T1, T2&gt;(IState&lt;T1&gt; state1, IState&lt;T2&gt; state2) => Print(state1, state2)
/// </code>
/// <code>
/// public static void Log&lt;T1, T2, T3&gt;(IState&lt;T1&gt; state1, IState&lt;T2&gt; state2, IState&lt;T3&gt; state3) => Print(state1, state2, state3)
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Struct)]
public class VariadicGenAttribute : Attribute {
    /// <param name="maxArity">The largest number of instances an overload gets.</param>
    /// <param name="patterns">
    /// The elements that grow, with <c>{}</c> standing for the index. An element matching a pattern with <c>{}</c>
    /// set to 1 is repeated for every index, elements matching nothing are searched for growing elements inside them.
    /// Elements are the items of type parameter, parameter, type argument and argument lists, the members of a type
    /// and the statements of a block. Fields and local declarations are matched without modifiers and the semicolon
    /// (<c>IState&lt;T{}&gt; _state{}</c>), expression statements without the semicolon (<c>_state{} = state{}</c>).
    /// </param>
    public VariadicGenAttribute(int maxArity, params string[] patterns) {
        MaxArity = maxArity;
        Patterns = patterns;
    }

    public int MaxArity { get; }

    public string[] Patterns { get; }
}
