namespace NoiseEngine.Nesl.Emit;

public enum OpCode : ushort {
    #region BranchOperations

    [OpCodeValidation(typeof(uint), typeof(NeslMethod), typeof(uint[]))]
    Call,
    [OpCodeValidation]
    Return,
    [OpCodeValidation(typeof(uint))]
    ReturnValue,

    #endregion
    #region DefOperations

    [OpCodeValidation(typeof(NeslType))]
    DefVariable,

    #endregion
    #region LoadOperations

    [OpCodeValidation(typeof(uint), typeof(uint))]
    Load,
    [OpCodeValidation(typeof(uint), typeof(uint))]
    LoadUInt32,
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
    SetElement

    #endregion
}
