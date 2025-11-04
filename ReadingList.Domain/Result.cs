using System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ReadingList.Domain
{
    public sealed class Result<T> //vezi sealed class
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

       public bool TryGetValue(out T value)
       {
            if(IsSuccess && Value is not null)
            {
                value = Value;
                return true;
            }

            value = default!;
            return false;
       }

       // Map the contained value if Success == true; otherwise propagate the failure.
       public Result<U> Map<U>(Func<T, U> mapper)
        {
            if(mapper is null)
            {
                throw new ArgumentNullException(nameof(mapper));
            }

            return IsSuccess ? 
                Result<U>.Ok(mapper(Value!)) :
                Result<U>.Fail(ErrorMessage!);
        }

        // Bind/FlatMap: chain operations that also return Result&lt;U&gt;
        public Result<U> Bind<U>(Func<T, Result<U>> binder)
        {
            if (binder is null) throw new ArgumentNullException(nameof(binder));
            return IsSuccess ? binder(Value!) : Result<U>.Fail(ErrorMessage!);
        }



    }
}
