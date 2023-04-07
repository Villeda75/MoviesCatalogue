namespace MoviesCatalogue.Classes.Wrappers
{
    public class Response<T>
    {
        public Response()
        {
        }

        public Response(string message, string error, T data) { 
            Success = false;
            Status = 400;
            Message = message;
            Data =  data;
            Error = error;
        }

        public Response(string message, T data)
        {
            Success = true;
            Status = 200;
            Message = message;
            Data = data;
            Error = "";
        }

        public bool Success { get; set; }
        public int Status { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
        public string Error { get; set; }
    }
}
