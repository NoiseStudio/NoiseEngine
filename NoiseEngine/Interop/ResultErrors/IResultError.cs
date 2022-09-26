using System;

namespace NoiseEngine.Interop.ResultErrors;

internal interface IResultError {

    public Exception ToException();

}
