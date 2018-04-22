/*

Class: FirebaseParam.cs
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

using UnityEngine;

namespace SimpleFirebaseUnity
{
	public struct FirebaseParam
	{
		string param;

		/// <summary>
		/// Created parameter for REST API call
		/// </summary>
		public string Parameter
		{
			get
			{
				return param;
			}
		}

        /// <summary>
        /// Created parameter for REST API call with the symbols encoded to url-safe escape characters.
        /// </summary>
        public string SafeParameter
        {
            get
            {
                return WWW.EscapeURL(param);
            }
        }

		/// <summary>
		/// Create new FirebaseQuery
		/// </summary>
		/// <param name="param">REST call parameters on a string. Example: &quot;orderBy=&#92;"$key&#92;"&quot;print=pretty&quot;auth=secret123"></param>
		public FirebaseParam(string _param = "")
		{
			param = _param;
		}

		/// <summary>
		/// For details see https://firebase.google.com/docs/database/rest/retrieve-data
		/// </summary>
		public FirebaseParam Add(string parameter)
		{
			if (param != null && param.Length > 0)
				param += "&";
			param += parameter;

			return this;
		}

		/// <summary>
		/// For details see https://firebase.google.com/docs/database/rest/retrieve-data . Set quoted parameter if necessary
		/// </summary>
		public FirebaseParam Add(string header, string value, bool quoted = true)
		{
			return (quoted) ? Add(header + "=\"" + value + "\"") : Add(header + "=" + value);
		}

		/// <summary>
		/// For details see https://firebase.google.com/docs/database/rest/retrieve-data
		/// </summary>
		public FirebaseParam Add(string header, int value)
		{
			return Add(header + "=" + value);
		}

		/// <summary>
		/// For details see https://firebase.google.com/docs/database/rest/retrieve-data
		/// </summary>
		public FirebaseParam Add(string header, float value)
		{
			return Add(header + "=" + value);
		}

		/// <summary>
		/// For details see https://firebase.google.com/docs/database/rest/retrieve-data
		/// </summary>
		public FirebaseParam Add(string header, bool value)
		{
			return Add(header + "=" + value);
		}

		/// <summary>
		/// For details see https://firebase.google.com/docs/database/rest/retrieve-data
		/// </summary>
		public FirebaseParam OrderByChild(string key)
		{
			return Add("orderBy", key);
		}

		/// <summary>
		/// For details see https://firebase.google.com/docs/database/rest/retrieve-data
		/// </summary>
		public FirebaseParam OrderByKey()
		{
			return Add("orderBy", "$key");
		}

		/// <summary>
		/// For details see https://firebase.google.com/docs/database/rest/retrieve-data
		/// </summary>
		public FirebaseParam OrderByValue()
		{
			return Add("orderBy", "$value");
		}

		/// <summary>
		/// For details see https://firebase.google.com/docs/database/rest/retrieve-data
		/// </summary>
		public FirebaseParam OrderByPriority()
		{
			return Add("orderBy", "$priority");
		}

		/// <summary>
		/// For details see https://firebase.google.com/docs/database/rest/retrieve-data
		/// </summary>
		public FirebaseParam LimitToFirst(int lim)
		{
			return Add("limitToFirst", lim);
		}

		/// <summary>
		/// For details see https://firebase.google.com/docs/database/rest/retrieve-data
		/// </summary>
		public FirebaseParam LimitToLast(int lim)
		{
			return Add("limitToLast", lim);
		}

		/// <summary>
		/// For details see https://firebase.google.com/docs/database/rest/retrieve-data
		/// </summary>
		public FirebaseParam StartAt(string start)
		{
			return Add("startAt", start);
		}

		/// <summary>
		/// For details see https://firebase.google.com/docs/database/rest/retrieve-data
		/// </summary>
		public FirebaseParam StartAt(int start)
		{
			return Add("startAt", start);
		}

		/// <summary>
		/// For details see https://firebase.google.com/docs/database/rest/retrieve-data
		/// </summary>
		public FirebaseParam StartAt(bool start)
		{
			return Add("startAt", start);
		}

		/// <summary>
		/// For details see https://firebase.google.com/docs/database/rest/retrieve-data
		/// </summary>
		public FirebaseParam StartAt(float start)
		{
			return Add("startAt", start);
		}

		/// <summary>
		/// For details see https://firebase.google.com/docs/database/rest/retrieve-data
		/// </summary>
		public FirebaseParam EndAt(string end)
		{
			return Add("endAt", end);
		}

		/// <summary>
		/// For details see https://firebase.google.com/docs/database/rest/retrieve-data
		/// </summary>
		public FirebaseParam EndAt(int end)
		{
			return Add("endAt", end);
		}

		/// <summary>
		/// For details see https://firebase.google.com/docs/database/rest/retrieve-data
		/// </summary>
		public FirebaseParam EndAt(bool end)
		{
			return Add("endAt", end);
		}

		/// <summary>
		/// For details see https://firebase.google.com/docs/database/rest/retrieve-data
		/// </summary>
		public FirebaseParam EndAt(float end)
		{
			return Add("endAt", end);
		}

		/// <summary>
		/// For details see https://firebase.google.com/docs/database/rest/retrieve-data
		/// </summary>
		public FirebaseParam EqualTo(string at)
		{
			return Add("equalTo", at);
		}

		/// <summary>
		/// For details see https://firebase.google.com/docs/database/rest/retrieve-data
		/// </summary>
		public FirebaseParam EqualTo(int at)
		{
			return Add("equalTo", at);
		}

		/// <summary>
		/// For details see https://firebase.google.com/docs/database/rest/retrieve-data
		/// </summary>
		public FirebaseParam EqualTo(bool at)
		{
			return Add("equalTo", at);
		}

		/// <summary>
		/// For details see https://firebase.google.com/docs/database/rest/retrieve-data
		/// </summary>
		public FirebaseParam EqualTo(float at)
		{
			return Add("equalTo", at);
		}

		/// <summary>
		/// For details see https://firebase.google.com/docs/database/rest/retrieve-data
		/// </summary>
		public FirebaseParam PrintPretty()
		{
			return Add("print=pretty");
		}

		/// <summary>
		/// For details see https://firebase.google.com/docs/database/rest/retrieve-data
		/// </summary>
		public FirebaseParam PrintSilent()
		{
			return Add("print=silent");
		}

		/// <summary>
		/// For details see https://firebase.google.com/docs/database/rest/retrieve-data
		/// </summary>
		public FirebaseParam Shallow()
		{
			return Add("shallow=true");
		}

		/// <summary>
		/// For details see https://firebase.google.com/docs/database/rest/retrieve-data
		/// </summary>
		public FirebaseParam Auth(string cred)
		{
			return Add("auth=" + cred);
		}

		public override string ToString()
		{
			return param;
		}

		/// <summary>
		/// Empty paramete or \"\"
		/// </summary>
		public static FirebaseParam Empty
		{
			get
			{
				return new FirebaseParam();
			}
		}
	}
}
