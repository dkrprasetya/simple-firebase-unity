/*

Class: FirebaseError.cs
==============================================
Last update: 2016-06-23  (by Dikra)
==============================================

Copyright (c) 2016  M Dikra Prasetya

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
		const string MESSAGE_ERROR_400 = "Firebase request has invalid child names or invalid/missing/too large data";
		const string MESSAGE_ERROR_401 = "Firebase request's authorization has failed";
		const string MESSAGE_ERROR_403 = "Firebase request violates Firebase Realtime Database Rules";
		const string MESSAGE_ERROR_404 = "Firebase request made over HTTP instead of HTTPS";
		const string MESSAGE_ERROR_417 = "Firebase request doesn't specify a Firebase database name";
		const string MESSAGE_ERROR_UNDOCUMENTED = "Firebase request's error is not yet documented on Firebase";

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
				case HttpStatusCode.ExpectationFailed:
					message = MESSAGE_ERROR_417;
					break;
				case HttpStatusCode.Forbidden:
					message = MESSAGE_ERROR_403;
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
				case HttpStatusCode.ExpectationFailed:
					message = MESSAGE_ERROR_417;
					break;
				case HttpStatusCode.Forbidden:
					message = MESSAGE_ERROR_403;
					break;
				default:
					message = MESSAGE_ERROR_UNDOCUMENTED;
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
