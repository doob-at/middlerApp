using System;
using System.Collections;

namespace middlerApp.Api.Models.Idp
{
    public class LoginError
    {
        public string Message { get; set; }
        public IDictionary Data { get; set; }

        public LoginError() { }

        public LoginError(string message)
        {
            Message = message;
        }

        public LoginError(string message, IDictionary data)
        {
            Message = message;
            Data = data;
        }

        public static implicit operator LoginError(Exception exception)
        {
            var apiexc = new LoginError();
            apiexc.Data = exception.Data;
            apiexc.Message = exception.Message;
            return apiexc;
        }
    }
}
