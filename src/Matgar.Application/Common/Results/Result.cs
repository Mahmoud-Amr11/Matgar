namespace Matgar.Application.Common.Results
{
    public class Result : IResultFactory<Result>
    {
        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        public List<Error> Errors { get; }


        protected Result(bool isSuccess, List<Error> errors)
        {
            IsSuccess = isSuccess;
            Errors = errors;
        }

        public static Result Success => new Result(true, []);

        public static Result Failure(Error error) => new Result(false, [error]);
        public static Result Failure(List<Error> errors) => new Result(false, errors);

        public static implicit operator Result(Error error)
           => Failure(error);
    }
    public class Result<TValue> : Result, IResultFactory<Result<TValue>>
    {
        private readonly TValue value;

        public TValue Value => value!;

        private Result(TValue? value, bool success, List<Error> errors)
           : base(success, errors)
        {
            this.value = value;
        }

        public static Result<TValue> Success(TValue value)
            => new(value, true, []);

        public static new Result<TValue> Failure(Error error)
            => new(default, false, [error]);

        public static new Result<TValue> Failure(List<Error> errors)
            => new(default, false, errors);

        public static implicit operator Result<TValue>(TValue value)
            => Success(value);

        public static implicit operator Result<TValue>(Error error)
            => Failure(error);
    }
}
