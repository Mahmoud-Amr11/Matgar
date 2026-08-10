namespace Matgar.Application.Common.Results
{
    public interface IResultFactory<TSelf> where TSelf : IResultFactory<TSelf>
    {
        static abstract TSelf Failure(Error error);
        static abstract TSelf Failure(List<Error> errors);
    }

}
