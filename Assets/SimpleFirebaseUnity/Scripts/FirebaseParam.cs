/*

Class: FirebaseParam.cs
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

using System.Collections.Generic;
using UnityEngine;

namespace SimpleFirebaseUnity
{
	public struct FirebaseParam
	{
        public enum HttpMethodName
        {
            GET, POST, PUT, PATCH, DELETE
        }

        Dictionary<string, string> header;
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
		/// Created HttpHeader for REST API call
		/// </summary>
		public Dictionary<string, string> HttpHeader
        {
            get
            {
                return header;
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
		/// <param name="_param">REST call parameters on a string. Example: &quot;orderBy=&#92;"$key&#92;"&quot;print=pretty&quot;auth=secret123"></param>
		public FirebaseParam(string _param = "")
		{
			param = _param;
            header = new Dictionary<string, string>();
        }

        /// <summary>
		/// Create new FirebaseQuery
		/// </summary>
		/// <param name="_param">REST call parameters on a string. Example: &quot;orderBy=&#92;"$key&#92;"&quot;print=pretty&quot;auth=secret123"></param>
        /// <param name="_header">REST call Http Headers."></param>
		public FirebaseParam(string _param, Dictionary<string, string> _header)
        {
            param = _param;
            header = _header;
        }
        
        /// <summary>
		/// Create new FirebaseQuery
		/// </summary>
		/// <param name="copy">Firebase parameter to copy.</param>
		public FirebaseParam(FirebaseParam copy)
        {
            param = copy.Parameter;
            header = new Dictionary<string, string>();

            foreach (var kv in header){
                header.Add(kv.Key, kv.Value);
            }
        }

        /// <summary>
        /// For details see https://firebase.google.com/docs/reference/rest/database/
        /// </summary>
        public FirebaseParam Add(string parameter)
		{
			if (param != null && param.Length > 0)
				param += "&";
			param += parameter;

			return this;
		}

		/// <summary>
		/// For details see https://firebase.google.com/docs/reference/rest/database/ . Set quoted parameter if necessary
		/// </summary>
		public FirebaseParam Add(string name, string value, bool quoted = true)
		{
			return (quoted) ? Add(name + "=\"" + value + "\"") : Add(name + "=" + value);
		}

		/// <summary>
		/// For details see https://firebase.google.com/docs/reference/rest/database/
		/// </summary>
		public FirebaseParam Add(string name, int value)
		{
			return Add(name + "=" + value);
		}

		/// <summary>
		/// For details see https://firebase.google.com/docs/reference/rest/database/
		/// </summary>
		public FirebaseParam Add(string name, float value)
		{
			return Add(name + "=" + value);
		}

		/// <summary>
		/// For details see https://firebase.google.com/docs/reference/rest/database/
		/// </summary>
		public FirebaseParam Add(string name, bool value)
		{
			return Add(name + "=" + value);
		}


        /// <summary>
        /// For details see https://firebase.google.com/docs/reference/rest/database/
        /// </summary>
        public FirebaseParam OrderByChild(string key)
		{
			return Add("orderBy", key);
		}

		/// <summary>
		/// For details see https://firebase.google.com/docs/reference/rest/database/
		/// </summary>
		public FirebaseParam OrderByKey()
		{
			return Add("orderBy", "$key");
		}

		/// <summary>
		/// For details see https://firebase.google.com/docs/reference/rest/database/
		/// </summary>
		public FirebaseParam OrderByValue()
		{
			return Add("orderBy", "$value");
		}

		/// <summary>
		/// For details see https://firebase.google.com/docs/reference/rest/database/
		/// </summary>
		public FirebaseParam OrderByPriority()
		{
			return Add("orderBy", "$priority");
		}

		/// <summary>
		/// For details see https://firebase.google.com/docs/reference/rest/database/
		/// </summary>
		public FirebaseParam LimitToFirst(int lim)
		{
			return Add("limitToFirst", lim);
		}

		/// <summary>
		/// For details see https://firebase.google.com/docs/reference/rest/database/
		/// </summary>
		public FirebaseParam LimitToLast(int lim)
		{
			return Add("limitToLast", lim);
		}

		/// <summary>
		/// For details see https://firebase.google.com/docs/reference/rest/database/
		/// </summary>
		public FirebaseParam StartAt(string start)
		{
			return Add("startAt", start);
		}

		/// <summary>
		/// For details see https://firebase.google.com/docs/reference/rest/database/
		/// </summary>
		public FirebaseParam StartAt(int start)
		{
			return Add("startAt", start);
		}

		/// <summary>
		/// For details see https://firebase.google.com/docs/reference/rest/database/
		/// </summary>
		public FirebaseParam StartAt(bool start)
		{
			return Add("startAt", start);
		}

		/// <summary>
		/// For details see https://firebase.google.com/docs/reference/rest/database/
		/// </summary>
		public FirebaseParam StartAt(float start)
		{
			return Add("startAt", start);
		}

		/// <summary>
		/// For details see https://firebase.google.com/docs/reference/rest/database/
		/// </summary>
		public FirebaseParam EndAt(string end)
		{
			return Add("endAt", end);
		}

		/// <summary>
		/// For details see https://firebase.google.com/docs/reference/rest/database/
		/// </summary>
		public FirebaseParam EndAt(int end)
		{
			return Add("endAt", end);
		}

		/// <summary>
		/// For details see https://firebase.google.com/docs/reference/rest/database/
		/// </summary>
		public FirebaseParam EndAt(bool end)
		{
			return Add("endAt", end);
		}

		/// <summary>
		/// For details see https://firebase.google.com/docs/reference/rest/database/
		/// </summary>
		public FirebaseParam EndAt(float end)
		{
			return Add("endAt", end);
		}

		/// <summary>
		/// For details see https://firebase.google.com/docs/reference/rest/database/
		/// </summary>
		public FirebaseParam EqualTo(string at)
		{
			return Add("equalTo", at);
		}

		/// <summary>
		/// For details see https://firebase.google.com/docs/reference/rest/database/
		/// </summary>
		public FirebaseParam EqualTo(int at)
		{
			return Add("equalTo", at);
		}

		/// <summary>
		/// For details see https://firebase.google.com/docs/reference/rest/database/
		/// </summary>
		public FirebaseParam EqualTo(bool at)
		{
			return Add("equalTo", at);
		}

		/// <summary>
		/// For details see https://firebase.google.com/docs/reference/rest/database/
		/// </summary>
		public FirebaseParam EqualTo(float at)
		{
			return Add("equalTo", at);
		}

		/// <summary>
		/// For details see https://firebase.google.com/docs/reference/rest/database/
		/// </summary>
		public FirebaseParam PrintPretty()
		{
			return Add("print=pretty");
		}

		/// <summary>
		/// For details see https://firebase.google.com/docs/reference/rest/database/
		/// </summary>
		public FirebaseParam PrintSilent()
		{
			return Add("print=silent");
		}

		/// <summary>
		/// For details see https://firebase.google.com/docs/reference/rest/database/
		/// </summary>
		public FirebaseParam Shallow()
		{
			return Add("shallow=true");
		}

		/// <summary>
		/// For details see https://firebase.google.com/docs/reference/rest/database/
		/// </summary>
		public FirebaseParam Auth(string cred)
		{
			return Add("auth=" + cred);
		}

        /// <summary>
		/// For details see https://firebase.google.com/docs/reference/rest/database/
		/// </summary>
		public FirebaseParam AccesToken(string access_token)
        {
            return Add("access_token=" + access_token);
        }

        /// <summary>
        /// For details see https://firebase.google.com/docs/reference/rest/database/
        /// </summary>
        public FirebaseParam AddHttpHeader(string name, string value)
        {
            if (header.ContainsKey(name))
                header[name] = value;
            else
                header.Add(name, value);

            return this;
        }      

        /// <summary>
        /// WARNING: This plugin's Firebase request implementations are using X-HTTP-Method-Override by default.
        /// Only use this method to create a custom request or re-override Http Method at your own risk.
		/// For details see https://firebase.google.com/docs/reference/rest/database/
		/// </summary>
		public FirebaseParam HttpMethodOverride(string methodOverride)
        {
            return AddHttpHeader("X-HTTP-Method-Override" , methodOverride);
        }

        /// <summary>
        /// WARNING: This plugin's Firebase request implementations are using X-HTTP-Method-Override by default.
        /// Only use this method to create a custom request or re-override Http Method at your own risk.
		/// For details see https://firebase.google.com/docs/reference/rest/database/
		/// </summary>
        public FirebaseParam HttpMethodOverride(HttpMethodName methodName)
        {
            return AddHttpHeader("X-HTTP-Method-Override" , methodName.ToString());
        }

        /// <summary>
        /// For details see https://firebase.google.com/docs/reference/rest/database/
        /// </summary>
        public FirebaseParam XFirebaseEtagHeader()
        {
            return AddHttpHeader("X-Firebase-ETag", "true");
        }

        /// <summary>
        /// For details see https://firebase.google.com/docs/reference/rest/database/
        /// </summary>
        public FirebaseParam IfMatchHeader(string value)
        {
            return AddHttpHeader("if-match", value);
        }

        /// <summary>
        /// For details see https://firebase.google.com/docs/reference/rest/database/
        /// </summary>
        public FirebaseParam KeepAliveHeader()
        {
            return AddHttpHeader("Keep-Alive", "true");
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
