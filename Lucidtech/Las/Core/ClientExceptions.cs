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
     /// <summary>
     /// Constructor..
     /// <param name="s">Error message.</param>
     /// </summary>
      public ClientException(string s) : base(s) {}
   }

   /// <summary>
   /// An InvalidCredentialsException is raised if access key id or secret access key is invalid.
   /// </summary>
   public class InvalidCredentialsException : ClientException
   {
     /// <summary>
     /// Constructor..
     /// <param name="s">Error message.</param>
     /// </summary>
      public InvalidCredentialsException(string s) : base(s) {}
   }

   /// <summary>
   /// A TooManyRequestsException is raised if you have reached the number of requests per second limit
   /// associated with your credentials.
   /// </summary>
   public class TooManyRequestsException : ClientException
   {
     /// <summary>
     /// Constructor..
     /// <param name="s">Error message.</param>
     /// </summary>
      public TooManyRequestsException(string s) : base(s) {}
   }

   /// <summary>
   /// A LimitExceededException is raised if you have reached the limit of total requests per month
   /// associated with your credentials.
   /// </summary>
   public class LimitExceededException : ClientException
   {
     /// <summary>
     /// Constructor..
     /// <param name="s">Error message.</param>
     /// </summary>
      public LimitExceededException(string s) : base(s) {}
   }

   /// <summary>
   /// A RequestException is raised if something went wrong with the request.
   /// </summary>
   public class RequestException : ClientException
   {
      /// <summary>
      /// Server-side response.
      /// </summary>
      public IRestResponse? Response { get; }

     /// <summary>
     /// Constructor..
     /// <param name="s">Error message.</param>
     /// </summary>
      public RequestException(string s) : base(s) {}

      /// <summary>
      /// Constructor..
      /// <param name="response">Server-side response.</param>
      /// </summary>
      public RequestException(IRestResponse response) : base(string.Concat(response.Content, response.ErrorMessage))
      {
         Response = response;
      }
   }

}
