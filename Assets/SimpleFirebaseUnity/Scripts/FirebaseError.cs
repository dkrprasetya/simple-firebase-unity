/*

Class: FirebaseError.cs
==============================================
Last update: 2018-05-20  (by Dikra)
==============================================


 * MIT LICENSE
 *
 * Permission is hereby granted, free of charge, to any person obtaining
 * a copy of this software and associated documentation files (the
 * "Software"), to deal in the Software without restriction, including
 * without limitation the rights to use, copy, modify, merge, publish,
 * distribute, sublicense, and/or sell copies of the Software, and to
 * permit persons to whom the Software is furnished to do so, subject to
 * the following conditions:
 *
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
 * MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
 * IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
 * CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
 * TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
 * SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

*/

using System;
using System.Net;

namespace SimpleFirebaseUnity
{
    public class FirebaseError : Exception
    {
		const string MESSAGE_ERROR_400 = "Firebase request has an error / bad request. See https://firebase.google.com/docs/reference/rest/database/ for more details.";
		const string MESSAGE_ERROR_401 = "Firebase request's authorization has failed. See https://firebase.google.com/docs/reference/rest/database/ for more details.";
		const string MESSAGE_ERROR_404 = "The specified Realtime Database was not found.";
		const string MESSAGE_ERROR_500 = "The server returned an error.";
		const string MESSAGE_ERROR_503 = "The specified Firebase Realtime Database is temporarily unavailable, which means the request was not attempted.";
		const string MESSAGE_ERROR_412 = "The request's specified ETag value in the if-match header did not match the server's value.";
		const string MESSAGE_ERROR_UNDEFINED = "Undefined error: ";

		protected HttpStatusCode m_Status;


		public FirebaseError(HttpStatusCode status, string message) : base(message)
		{
			m_Status = status;
		}

		public FirebaseError(HttpStatusCode status, string message, Exception inner) : base(message, inner)
		{
			m_Status = status;
		}

		public FirebaseError(string message) : base(message)
		{
		}

		public FirebaseError(string message, Exception inner) : base(message, inner)
		{
		}


		/// <summary>
		/// Create the FirebaseError initialized based on the given WebException.
		/// </summary>
		/// <param name="webEx">Web exception.</param>
		public static FirebaseError Create(WebException webEx)
		{
			string message;
			HttpStatusCode status = 0;
			bool isStatusAvailable = false;

			if (webEx.Status == WebExceptionStatus.ProtocolError)
			{
				HttpWebResponse response = webEx.Response as HttpWebResponse;
				if (response != null) 
				{
					status = response.StatusCode;
					isStatusAvailable = true;
				}
			}

			if (!isStatusAvailable)
				return new FirebaseError(webEx.Message, webEx);

			switch (status) 
			{
				case HttpStatusCode.Unauthorized:
					message = MESSAGE_ERROR_401;
					break;
				case HttpStatusCode.BadRequest:
					message = MESSAGE_ERROR_400;
					break;
				case HttpStatusCode.NotFound:
					message = MESSAGE_ERROR_404;
					break;
				case HttpStatusCode.InternalServerError:
					message = MESSAGE_ERROR_500 + "\n(" + webEx.Message + ")";
                    break;
				case HttpStatusCode.ServiceUnavailable:
					message = MESSAGE_ERROR_503;
                    break;
				case HttpStatusCode.PreconditionFailed:
					message = MESSAGE_ERROR_412;
					break;
				default:
					message = webEx.Message;
					break;
			}

			return new FirebaseError(status, message, webEx);
		}
			
		/// <summary>
		/// Create the FirebaseError initialized based on the given http status code.
		/// </summary>
		/// <param name="status">Http status code.</param>
		public static FirebaseError Create(HttpStatusCode status)
		{
			string message;

			switch (status)
            {
                case HttpStatusCode.Unauthorized:
                    message = MESSAGE_ERROR_401;
                    break;
                case HttpStatusCode.BadRequest:
                    message = MESSAGE_ERROR_400;
                    break;
                case HttpStatusCode.NotFound:
                    message = MESSAGE_ERROR_404;
                    break;
                case HttpStatusCode.InternalServerError:
					message = MESSAGE_ERROR_500 + ". See the error message for further details.";
                    break;
                case HttpStatusCode.ServiceUnavailable:
                    message = MESSAGE_ERROR_503;
                    break;
                case HttpStatusCode.PreconditionFailed:
                    message = MESSAGE_ERROR_412;
                    break;
                default:
					message = MESSAGE_ERROR_UNDEFINED + status.ToString();
                    break;
            }

			return  new FirebaseError (status, message);
		}

		/// <summary>
		/// Gets the status code. 
		/// Tips: Typecast to integer to get the code. You could also typecast to string to print as it is.
		/// </summary>
		/// <value>The status.</value>
		public HttpStatusCode Status
		{
			get{
				return m_Status;
			}
		}
    }
}
