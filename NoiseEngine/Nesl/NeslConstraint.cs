using System.Collections.Generic;

namespace NoiseEngine.Nesl;

public class NeslConstraint {

    public NeslGenericTypeParameter GenericTypeParameter { get; }
    public IReadOnlyList<NeslType> Constraints { get; }

    public NeslConstraint(NeslGenericTypeParameter genericTypeParameter, params NeslType[] constraints) {
        GenericTypeParameter = genericTypeParameter;
        Constraints = constraints;
    }

}
