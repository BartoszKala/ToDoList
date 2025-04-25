namespace ToDoList.Application
{
    public class Result<T>
    {
        public bool IsSuccess { get; set; }

        // The actual result or value of the operation (if successful)
        public T? Value { get; set; }

        public string? Error { get; set; }

        // A static method to create a successful result
        public static Result<T> Success(T value) => new Result<T> { IsSuccess = true, Value = value };

        // A static method to create a failed result
        public static Result<T> Failure(string error) => new Result<T> { IsSuccess = false, Error = error };
    }
}
