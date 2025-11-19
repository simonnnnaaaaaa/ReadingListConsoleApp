
namespace ReadingList.Domain
{
    public sealed class Result<T> 
    {
       public bool IsSuccess { get; }
       public T? Value { get; }

       public string? ErrorMessage { get; }

       private Result(bool isSuccess, T? value, string? errorMessage)
       {
           IsSuccess = isSuccess;
           Value = value;
           ErrorMessage = errorMessage;
       }

       public static Result<T> Ok (T value) => new Result<T>(true, value, null);

       public static Result<T> Fail (string errorMessage) => new Result<T>(false, default, errorMessage);

    }
}
