/*

Class: DataSnapshot.cs
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
using System.Security;

namespace SimpleFirebaseUnity
{
    using MiniJSON;

    public class DataSnapshot
    {
        protected object val_obj;
        protected Dictionary<string, object> val_dict;
        protected List<string> keys;
        protected string json;

        protected DataSnapshot()
        {
            val_dict = null;
            val_obj = null;
            keys = null;
            json = null;
        }

        /// <summary>
        /// Creates snapshot from Json string 
        /// </summary>
        /// <param name="json">Json string</param>
        public DataSnapshot(string _json = "")
        {
            object obj = (_json != null && _json.Length > 0)?Json.Deserialize(_json):null;

            if (obj is Dictionary<string, object>)
                val_dict = obj as Dictionary<string, object>;
            else
                val_obj = obj;

            keys = null;
            json = (_json == null)?"":_json;
        }

        /// <summary>
        /// If snapshot is a Dictionary, returns keys of the snapshot , else null
        /// </summary>
        public List<string> Keys
        {
            get
            {
                if (keys == null && val_dict != null)
                    keys = new List<string>(val_dict.Keys);

                return keys;
            }
        }

        /// <summary>
        /// If snapshot is a Dictionary, gives the first key founded on snapshot, else null
        /// </summary>
        public string FirstKey
        {
            get
            {
                return (val_dict == null) ? null : Keys[0];
            }
        }

        /// <summary>
        /// Raw json of snapshot
        /// </summary>
        public string RawJson
        {
            get
            {
                return json;
            }

        }

        /// <summary>
        /// Raw value of snapshot
        /// </summary>
        public object RawValue
        {
            get
            {
                return (val_dict == null) ? val_obj : val_dict;
            }
        }

        /// <summary>
        /// Gets value from snapshot
        /// </summary>
        /// <typeparam name="T">Desired type</typeparam>
        /// <returns>Value of snapshot as the defined type, null if typecasting failed</returns>
        [SecuritySafeCritical]
        public T Value<T>()
        {
            try
            {
                if (val_obj != null)
                    return (T)val_obj;
                object obj = val_dict;
                return (T)obj;
            }
            catch {
                return default(T);
            }
        }
    }
}
