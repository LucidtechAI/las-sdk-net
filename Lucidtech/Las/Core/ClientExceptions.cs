using System;
using RestSharp;

namespace Lucidtech.Las.Core
{
   /// <summary>
   /// A ClientException is raised if the client refuses to
   /// send request due to incorrect usage or bad request data.
   /// </summary>
   public class ClientException : Exception
   {
      public ClientException(string s) : base(s) {}
   }

   /// <summary>
   /// An InvalidCredentialsException is raised if access key id or secret access key is invalid.
   /// </summary>
   public class InvalidCredentialsException : ClientException
   {
      public InvalidCredentialsException(string s) : base(s) {}
   }

   /// <summary>
   /// A TooManyRequestsException is raised if you have reached the number of requests per second limit
   /// associated with your credentials.
   /// </summary>
   public class TooManyRequestsException : ClientException
   {
      public TooManyRequestsException(string s) : base(s) {}
   }

   /// <summary>
   /// A LimitExceededException is raised if you have reached the limit of total requests per month
   /// associated with your credentials.
   /// </summary>
   public class LimitExceededException : ClientException
   {
      public LimitExceededException(string s) : base(s) {}
   }

   /// <summary>
   /// A RequestException is raised if something went wrong with the request.
   /// </summary>
   public class RequestException : ClientException
   {
      public IRestResponse Response { get; }
      public RequestException(string s) : base(s) {}

      public RequestException(IRestResponse response) : base(string.Concat(response.Content, response.ErrorMessage))
      {
         Response = response;
      }
   }

}
