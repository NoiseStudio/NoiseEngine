namespace NoiseEngine.Nesl.Emit;

/// <summary>
/// Op codes of NESIL (NoiseEngine Shader Intermediate Language).
/// </summary>
/// <remarks>
/// Glossary of terms:
/// <list type="number">
///     <item>
///         <term>index of a field/variable</term>
///         <description>
///         Valid index of field or variable. Firsts indexes (starts from zero) refers to fields in declaration order.
///         Next is parameters of current method and later variables in scope, also in declaration order.<br />
///
///         Max value of unsigned 32 bit integer (4,294,967,295) is the discarded value.
///         Support of discarded value must be explicit noted.
///         </description>
///     </item>
///     <item>
///         <term>supported index type</term>
///         <description>Currently only unsigned 32 bit integer.</description>
///     </item>
/// </list>
/// </remarks>
public enum OpCode : ushort {
    #region BranchOperations

    /// <summary>
    /// Calls method given in second argument with parameters in third argument and
    /// loads return value to first argument.
    /// </summary>
    /// <remarks>
    /// First argument - a valid index of a field/variable with the type of return type from second argument's method.
    /// Or discarded value.<br />
    /// Second argument - a valid assembly method reference.<br />
    /// Third argument - a valid indexes of a field/variable with the type of next parameters from
    /// second argument's method. Number of indexes must be same as number of parameters.<br />
    /// </remarks>
    [OpCodeValidation(typeof(uint), typeof(NeslMethod), typeof(uint[]))]
    Call,

    /// <summary>
    /// Returns and ends current method. Current method must do not have a return type.
    /// </summary>
    [OpCodeValidation]
    Return,

    /// <summary>
    /// Returns and ends current method with returning a value from first argument.
    /// </summary>
    /// <remarks>
    /// First argument - a valid index of a field/variable with the type of return type from method.<br />
    /// </remarks>
    [OpCodeValidation(typeof(uint))]
    ReturnValue,

    #endregion
    #region DefOperations

    /// <summary>
    /// Defines new variable of first argument type. Index of this variable will be next free index.
    /// </summary>
    /// <remarks>
    /// First argument - a valid assembly type reference.<br />
    /// </remarks>
    [OpCodeValidation(typeof(NeslType))]
    DefVariable,

    #endregion
    #region LoadOperations

    /// <summary>
    /// Loads to first argument, value from second argument.
    /// </summary>
    /// <remarks>
    /// First argument - a valid index of a field/variable.<br />
    /// Second argument - a valid index of a field/variable with the type of first argument.<br />
    /// </remarks>
    [OpCodeValidation(typeof(uint), typeof(uint))]
    Load,

    /// <summary>
    /// Loads to first argument, constant 32 bit unsigned integer from second argument.
    /// </summary>
    /// <remarks>
    /// First argument - a valid index of a field/variable with the 32 bit unsigned integer type.<br />
    /// Second argument - constant 32 bit unsigned integer value.<br />
    /// </remarks>
    [OpCodeValidation(typeof(uint), typeof(uint))]
    LoadUInt32,

    /// <summary>
    /// Loads to first argument, constant 32 bit float from second argument.
    /// </summary>
    /// <remarks>
    /// First argument - a valid index of a field/variable with the 32 bit float type.<br />
    /// Second argument - constant 32 bit float value.<br />
    /// </remarks>
    [OpCodeValidation(typeof(uint), typeof(float))]
    LoadFloat32,

    #endregion
    #region LoadElementOperations

    /// <summary>
    /// Loads to first argument, value from second argument at specified index given as third argument.
    /// </summary>
    /// <remarks>
    /// First argument - a valid index of a field/variable with the element type.<br />
    /// Second argument - a valid index of a field/variable with the buffer type of the element type.<br />
    /// Third argument - a valid index of a field/variable with the supported index type.<br />
    /// </remarks>
    [OpCodeValidation(typeof(uint), typeof(uint), typeof(uint))]
    LoadElement,

    /// <summary>
    /// Sets a specified in second argument index in buffer given as first argument to the value of the third argument.
    /// </summary>
    /// <remarks>
    /// First argument - a valid index of a field/variable with the buffer type of the element type.<br />
    /// Second argument - a valid index of a field/variable with the supported index type.<br />
    /// Third argument - a valid index of a field/variable with the element type.<br />
    /// </remarks>
    [OpCodeValidation(typeof(uint), typeof(uint), typeof(uint))]
    SetElement,

    #endregion
    #region LoadFieldOperations

    /// <summary>
    /// Loads to first argument, value from second argument at specified second argument object's field index given as
    /// third argument.
    /// </summary>
    /// <remarks>
    /// First argument - a valid index of a field/variable with type of third argument's field.<br />
    /// Second argument - a valid index of a field/variable.<br />
    /// Third argument - a valid index of a field with type of second argument.<br />
    /// </remarks>
    [OpCodeValidation(typeof(uint), typeof(uint), typeof(uint))]
    LoadField,

    /// <summary>
    /// Sets a specified in second argument object's field index in object given as first argument to the value of the
    /// third argument.
    /// </summary>
    /// <remarks>
    /// First argument - a valid index of a field/variable.<br />
    /// Second argument - a valid index of a field with type of second argument.<br />
    /// Third argument - a valid index of a field/variable with type of second argument's field.<br />
    /// </remarks>
    [OpCodeValidation(typeof(uint), typeof(uint), typeof(uint))]
    SetField

    #endregion
}
