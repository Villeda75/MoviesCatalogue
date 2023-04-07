using NuGet.Common;
using System.Drawing.Printing;

namespace MoviesCatalogue.Classes.Wrappers
{
    public class AuthenticationResponse<T> : Response<T>
    {
        public AuthenticationResponse(string message, string error, T data, T token)
        {
            Success = false;
            Status = 400;
            Message = message;
            Token = token;
            Data = data;
            Error = error;
        }

        public AuthenticationResponse(string message, T data, T token)
        {
            Success = true;
            Status = 200;
            Message = message;
            Token = token;
            Data = data;
            Error = "";
        }

        public T Token { get; set; }
    }
}
