namespace RecShark.AspNetCore.ApiClient
{
    public interface IApiClientConfig<T>
    {
        public ApiClientConfig Value { get; set; }
    }

    public class ApiClientConfig<T> : IApiClientConfig<T>
    {
        public ApiClientConfig Value { get; set; }
    }
}
