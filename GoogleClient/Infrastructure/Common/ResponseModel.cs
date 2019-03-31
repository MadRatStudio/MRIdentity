using System;

namespace GoogleClient.Infrastructure.Common
{
    public class ResponseModel<T>
        where T : class, new()
    {
        public bool IsSuccess { get; set; }
        public T Data { get; set; }
        public Exception Exception { get; set; }

        public ResponseModel() { }

        public ResponseModel(T data)
        {
            IsSuccess = true;
            Data = data;
        }

        public ResponseModel(Exception ex)
        {
            Exception = ex;
            IsSuccess = false;
        }

        public static ResponseModel<T> FromData(T data)
        {
            return new ResponseModel<T>
            {
                IsSuccess = true,
                Data = data
            };
        }

        public static ResponseModel<T> FromException(Exception ex)
        {
            return new ResponseModel<T>
            {
                Exception = ex,
                IsSuccess = false
            };
        }
    }
}
