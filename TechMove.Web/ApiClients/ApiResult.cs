namespace TechMove.Web.ApiClients
{
    // small wrapper so the controllers can tell the difference between a real validation
    // error from the api and the api simply being unreachable
    public class ApiResult
    {
        public bool Ok { get; init; }
        public string? Error { get; init; }

        public static ApiResult Success() => new ApiResult { Ok = true };
        public static ApiResult Fail(string error) => new ApiResult { Ok = false, Error = error };
    }

    public class ApiResult<T>
    {
        public bool Ok { get; init; }
        public string? Error { get; init; }
        public T? Value { get; init; }

        public static ApiResult<T> Success(T? value) => new ApiResult<T> { Ok = true, Value = value };
        public static ApiResult<T> Fail(string error) => new ApiResult<T> { Ok = false, Error = error };
    }
}
