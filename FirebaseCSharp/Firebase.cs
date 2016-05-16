/*

Class: Firebase.cs
==============================================
Last update: 2016-03-14  (by Dikra)
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
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;

namespace FirebaseCSharp
{
    using MiniJSON;
    using System.Collections;
    [Serializable]
    public class Firebase
    {
        public Action<Firebase, DataSnapshot> OnFetchSuccess;
        public Action<Firebase, FirebaseError> OnFetchFailed;
        public Action<Firebase, DataSnapshot> OnUpdateSuccess;
        public Action<Firebase, FirebaseError> OnUpdateFailed;
        public Action<Firebase, DataSnapshot> OnPushSuccess;
        public Action<Firebase, FirebaseError> OnPushFailed;
        public Action<Firebase, DataSnapshot> OnDeleteSuccess;
        public Action<Firebase, FirebaseError> OnDeleteFailed;

        protected Firebase parent;
        internal FirebaseRoot root;
        protected string key;
        protected string fullKey;

        /**** GET-SET ****/

        /// <summary>
        /// Parent of current firebase pointer
        /// </summary>                 
        public Firebase Parent
        {
            get
            {
                return parent;
            }
        }

        /// <summary>
        /// Root firebase pointer of the endpoint
        /// </summary>
        public Firebase Root
        {
            get
            {
                return root;
            }
        }

        /// <summary>
        /// Returns .json endpoint to this Firebase point
        /// </summary>
        public virtual string Endpoint
        {
            get
            {
                return "https://" + Host + "/" + FullKey + "/.json";
            }
        }

        /// <summary>
        /// Returns main host of Firebase
        /// </summary>
        public virtual string Host
        {
            get
            {
                return root.Host;
            }
        }

        /// <summary>
        /// Returns full key path to current pointer from root endpoint
        /// </summary>
        public string FullKey
        {
            get
            {
                return fullKey;
            }
        }

        /// <summary>
        /// Returns key of current pointer
        /// </summary>
        public string Key
        {
            get
            {
                return key;
            }
        }

        /// <summary>
        /// Credential for auth parameter. If no credential set to empty string
        /// </summary>
        public virtual string Credential
        {
            get
            {
                return root.Credential;
            }

            set
            {
                root.Credential = value;
            }
        }

        /**** CONSTRUCTOR ****/

        /// <summary>
        /// Create new Firebase endpoint
        /// </summary>
        /// <param name="_parent">Parent Firebase pointer</param>
        /// <param name="_key">Key under parent Firebase</param>
        /// <param name="_root">Root Firebase pointer</param>
        internal Firebase(Firebase _parent, string _key, FirebaseRoot _root)
        {
            parent = _parent;
            key = _key;
            root = _root;
            fullKey = parent.FullKey + "/" + key;
        }

        internal Firebase()
        {
            parent = null;
            key = string.Empty;
            root = null;
        }

        /**** BASIC FUNCTIONS ****/

        /// <summary>
        /// Get Firebase child from given key
        /// </summary>
        /// <param name="_key">A string</param>
        /// <returns></returns>
        public Firebase Child(string _key)
        {
            return new Firebase(this, _key, root);
        }

        /// <summary>
        /// Get Firebase childs from given keys
        /// </summary>
        /// <param name="_keys">List of string</param>
        /// <returns></returns>
        public List<Firebase> Childs(List<string> _keys)
        {
            List<Firebase> childs = new List<Firebase>();
            foreach (string k in _keys)
                childs.Add(Child(k));
            return childs;
        }

        /// <summary>
        /// Get Firebase childs from given keys
        /// </summary>
        /// <param name="_keys">Array of string</param>
        /// <returns></returns>
        public List<Firebase> Childs(string[] _keys)
        {
            List<Firebase> childs = new List<Firebase>();
            foreach (string k in _keys)
                childs.Add(Child(k));

            return childs;
        }

        protected virtual string SeekEndpoint()
        {
            return parent.SeekEndpoint() + "/" + key;
        }

        /**** REST FUNCTIONS ****/

        /// <summary>
        /// Fetch data from Firebase. Calls OnFetchSuccess on success, OnFetchFailed on failed.
        /// OnFetchSuccess action contains the corresponding Firebase and the fetched Snapshot
        /// OnFetchFailed action contains the error exception
        /// </summary>
        /// <param name="query">REST call parameters wrapped in FirebaseQuery class</param>
        /// <returns></returns>
        public void GetValue(FirebaseParam query)
        {
            GetValue(query.Parameter);
        }

        /// <summary>
        /// Fetch data from Firebase. Calls OnFetchSuccess on success, OnFetchFailed on failed.
        /// OnFetchSuccess action contains the corresponding Firebase and the fetched Snapshot
        /// OnFetchFailed action contains the error exception
        /// </summary>
        /// <param name="param">REST call parameters on a string. Example: &quot;orderBy=&#92;"$key&#92;"&quot;print=pretty&quot;shallow=true"></param>
        /// <returns></returns>
        public void GetValue(string param = "")
        {
            try
            {
                if (Credential != "")
                {
                    param = (new FirebaseParam(param).Auth(Credential)).Parameter;
                }

                string url = Endpoint;
                if (param != "")
                    url += "?" + param;

                WebRequest rq = WebRequest.Create(url);

                rq.Headers = new CusteredHeaderCollection(Host); ;
                rq.Proxy = null;

                rq.Method = "GET";
                rq.ContentLength = 0;

                rq.BeginGetResponse(new AsyncCallback((result) =>
                {
                    HttpWebResponse response = (result.AsyncState as HttpWebRequest).EndGetResponse(result) as HttpWebResponse;
                    using (HttpWebResponse resp = response)
                    {
                        string responseValue = string.Empty;

                        if (resp.StatusCode != HttpStatusCode.OK)
                        {
                            if (OnFetchFailed != null) OnFetchFailed(this, new FirebaseError(string.Format("Request failed. Received HTTP {0}", resp.StatusCode)));
                            return;
                        }

                        using (Stream responseStream = resp.GetResponseStream())
                        {
                            if (responseStream != null)
                                using (StreamReader rdr = new StreamReader(responseStream))
                                {
                                    responseValue = rdr.ReadToEnd();
                                }
                        }

                        if (responseValue != "")
                        {
                            DataSnapshot snapshot = new DataSnapshot(responseValue);
                            if (OnFetchSuccess != null) OnFetchSuccess(this, snapshot);
                        }
                        else
                        {
                            if (OnFetchFailed != null) OnFetchFailed(this, new FirebaseError("No response received."));
                        }
                    }
                }), rq);
            }
            catch (Exception ex)
            {
                if (OnFetchFailed != null) OnFetchFailed(this, new FirebaseError(ex.Message));
            }

        }

        /// <summary>
        /// Update value of a key on Firebase. Calls OnUpdateSuccess on success, OnUpdateFailed on failed.
        /// OnUpdateSuccess action contains the corresponding Firebase and the response Snapshot
        /// OnUpdateFailed action contains the error exception
        /// </summary>
        /// <param name="json">String</param>
        /// <param name="isJson">True if string is json (necessary to differentiate with the other overloading)</param>
        /// <param name="param">REST call parameters on a string. Example: "auth=ASDF123"</param>
        /// <returns></returns>
        public void SetValue(string json, bool isJson, string param = "")
        {
            if (!isJson)
                SetValue(json, param);
            else
                SetValue(Json.Deserialize(json), param);
        }

        /// <summary>
        /// Update value of a key on Firebase. Calls OnUpdateSuccess on success, OnUpdateFailed on failed.
        /// OnUpdateSuccess action contains the corresponding Firebase and the response Snapshot
        /// OnUpdateFailed action contains the error exception
        /// </summary>
        /// <param name="_val">Update value</param>
        /// <param name="param">REST call parameters on a string. Example: "auth=ASDF123"</param>
        /// <returns></returns>
        public void SetValue(object _val, string param = "")
        {
            try
            {
                if (Credential != "")
                {
                    param = (new FirebaseParam(param).Auth(Credential)).Parameter;
                }

                string url;

                if (_val is Dictionary<string, object>)
                {
                    url = Endpoint;
                }
                else
                {
                    if (parent == null)
                    {
                        if (OnUpdateFailed != null) OnUpdateFailed(this, new FirebaseError("Cannot set non-{key:value} object to root Firebase."));
                        return;
                    }

                    url = parent.Endpoint;

                    Dictionary<string, object> tempDict = new Dictionary<string, object>();
                    tempDict[key] = _val;
                    _val = tempDict;
                }

                if (param != string.Empty)
                    url += "?" + param;

                WebRequest rq = WebRequest.Create(url);

                rq.Headers = new CusteredHeaderCollection(Host); ;
                rq.Proxy = null;

                rq.Method = "PATCH";
                rq.ContentLength = 0;
                rq.ContentType = "application/json";



                //UTF8Encoding encoding = new UTF8Encoding();
                byte[] bytes = Encoding.GetEncoding("iso-8859-1").GetBytes(Json.Serialize(_val));
                rq.ContentLength = bytes.Length;


                rq.BeginGetRequestStream(new AsyncCallback((result) => 
                {
                    Stream responseStream = (result.AsyncState as HttpWebRequest).EndGetRequestStream(result) as Stream;
                    using (Stream writeStream = responseStream)
                    {
                        writeStream.Write(bytes, 0, bytes.Length);
                    }
                }), rq);

                rq.BeginGetResponse(new AsyncCallback((result) =>
                {
                    HttpWebResponse response = (result.AsyncState as HttpWebRequest).EndGetResponse(result) as HttpWebResponse;
                    using (HttpWebResponse resp = response)
                    {
                        string responseValue = string.Empty;

                        if (resp.StatusCode != HttpStatusCode.OK)
                        {
                            if (OnUpdateFailed != null) OnUpdateFailed(this, new FirebaseError(string.Format("Request failed. Received HTTP {0}", resp.StatusCode)));
                            return;
                        }

                        using (Stream responseStream = resp.GetResponseStream())
                        {
                            if (responseStream != null)
                                using (StreamReader rdr = new StreamReader(responseStream))
                                {
                                    responseValue = rdr.ReadToEnd();
                                }
                        }

                        if (responseValue != "")
                        {
                            DataSnapshot snapshot = new DataSnapshot(responseValue);
                            if (OnUpdateSuccess != null) OnUpdateSuccess(this, snapshot);
                        }
                        else
                        {
                            if (OnUpdateFailed != null) OnUpdateFailed(this, new FirebaseError("No response received."));
                        }
                    }
                }), rq);

            }
            catch (Exception ex)
            {
                if (OnUpdateFailed != null) OnUpdateFailed(this, new FirebaseError(ex.Message));
            }

        }

        /// <summary>
        /// Update value of a key on Firebase. Calls OnUpdateSuccess on success, OnUpdateFailed on failed.
        /// OnUpdateSuccess action contains the corresponding Firebase and the response Snapshot
        /// OnUpdateFailed action contains the error exception
        /// </summary>
        /// <param name="json">String</param>
        /// <param name="isJson">True if string is json (necessary to differentiate the other overloading)</param>
        /// <param name="query">REST call parameters wrapped in FirebaseQuery class</param>
        /// <returns></returns>
        public void SetValue(string json, bool isJson, FirebaseParam query)
        {
            if (!isJson)
                SetValue(json, query.Parameter);
            else
                SetValue(Json.Deserialize(json), query.Parameter);
        }

        /// <summary>
        /// Update value of a key on Firebase. Calls OnUpdateSuccess on success, OnUpdateFailed on failed.
        /// OnUpdateSuccess action contains the corresponding Firebase and the response Snapshot
        /// OnUpdateFailed action contains the error exception
        /// </summary>
        /// <param name="_val">Update value</param>
        /// <param name="query">REST call parameters wrapped in FirebaseQuery class</param>
        /// <returns></returns>
        public void SetValue(object _val, FirebaseParam query)
        {
            SetValue(_val, query.Parameter);
        }

        /// <summary>
        /// Push a value (with random new key) on a key in Firebase. Calls OnPushSuccess on success, OnPushFailed on failed.
        /// OnPushSuccess action contains the corresponding Firebase and the response Snapshot
        /// OnPushFailed action contains the error exception
        /// </summary>
        /// <param name="json">String</param>
        /// <param name="isJson">True if string is json (necessary to differentiate with the other overloading)</param>
        /// <param name="param">REST call parameters on a string. Example: "auth=ASDF123"</param>
        /// <returns></returns>
        public void Push(string json, bool isJson, string param = "")
        {
            if (!isJson)
                Push(json, param);
            else
                Push(Json.Deserialize(json), param);
        }

        /// <summary>
        /// Update value of a key on Firebase. Calls OnUpdateSuccess on success, OnUpdateFailed on failed.
        /// OnUpdateSuccess action contains the corresponding Firebase and the response Snapshot
        /// OnUpdateFailed action contains the error exception
        /// </summary>
        /// <param name="_val">New value</param>
        /// <param name="param">REST call parameters on a string. Example: "auth=ASDF123"</param>
        /// <returns></returns>
        public void Push(object _val, string param = "")
        {
            try
            {
                if (Credential != "")
                {
                    param = (new FirebaseParam(param).Auth(Credential)).Parameter;
                }

                string url = Endpoint;

                if (param != string.Empty)
                    url += "?" + param;

                WebRequest rq = WebRequest.Create(url);

                rq.Headers = new CusteredHeaderCollection(Host); ;
                rq.Proxy = null;

                rq.Method = "POST";
                rq.ContentLength = 0;
                rq.ContentType = "application/json";

                //UTF8Encoding encoding = new UTF8Encoding();
                byte[] bytes = Encoding.GetEncoding("iso-8859-1").GetBytes(Json.Serialize(_val));
                rq.ContentLength = bytes.Length;

                rq.BeginGetRequestStream(new AsyncCallback((result) =>
                {
                    Stream responseStream = (result.AsyncState as HttpWebRequest).EndGetRequestStream(result) as Stream;
                    using (Stream writeStream = responseStream)
                    {
                        writeStream.Write(bytes, 0, bytes.Length);
                    }
                }), rq);

                rq.BeginGetResponse(new AsyncCallback((result) =>
                {
                    HttpWebResponse response = (result.AsyncState as HttpWebRequest).EndGetResponse(result) as HttpWebResponse;
                    using (HttpWebResponse resp = response)
                    {
                        string responseValue = string.Empty;

                        if (resp.StatusCode != HttpStatusCode.OK)
                        {
                            if (OnPushFailed != null) OnPushFailed(this, new FirebaseError(string.Format("Request failed. Received HTTP {0}", resp.StatusCode)));
                            return;
                        }

                        using (Stream responseStream = resp.GetResponseStream())
                        {
                            if (responseStream != null)
                                using (StreamReader rdr = new StreamReader(responseStream))
                                {
                                    responseValue = rdr.ReadToEnd();
                                }
                        }

                        if (responseValue != "")
                        {
                            DataSnapshot snapshot = new DataSnapshot(responseValue);
                            if (OnPushSuccess != null) OnPushSuccess(this, snapshot);
                        }
                        else
                        {
                            if (OnPushFailed != null) OnPushFailed(this, new FirebaseError("No response received."));
                        }
                    }
                }), rq);
            }
            catch (Exception ex)
            {
                if (OnPushFailed != null) OnPushFailed(this, new FirebaseError(ex.Message));
            }
        }

        /// <summary>
        /// Push a value (with random new key) on a key in Firebase. Calls OnPushSuccess on success, OnPushFailed on failed.
        /// OnPushSuccess action contains the corresponding Firebase and the response Snapshot
        /// OnPushFailed action contains the error exception
        /// </summary>
        /// <param name="json">String</param>
        /// <param name="isJson">True if string is json (necessary to differentiate with the other overloading)</param>
        /// <param name="param">REST call parameters on a string. Example: "auth=ASDF123"</param>
        /// <returns></returns>
        public void Push(string json, bool isJson, FirebaseParam query)
        {
            if (!isJson)
                Push(json, query.Parameter);
            else
                Push(Json.Deserialize(json), query.Parameter);
        }

        /// <summary>
        /// Push a value (with random new key) on a key in Firebase. Calls OnPushSuccess on success, OnPushFailed on failed.
        /// OnPushSuccess action contains the corresponding Firebase and the response Snapshot
        /// OnPushFailed action contains the error exception
        /// </summary>
        /// <param name="_val">New value</param>
        /// <param name="query">REST call parameters wrapped in FirebaseQuery class</param>
        /// <returns></returns>
        public void Push(object _val, FirebaseParam query)
        {
            Push(_val, query.Parameter);
        }

        /// <summary>
        /// Delete a key in Firebase. Calls OnDeleteSuccess on success, OnDeleteFailed on failed.
        /// OnDeleteSuccess action contains the corresponding Firebase and the response Snapshot
        /// OnDeleteFailed action contains the error exception
        /// </summary>
        /// <param name="param">REST call parameters on a string. Example: "auth=ASDF123"</param>
        /// <returns></returns>
        public void Delete(string param = "")
        {
            try
            {
                if (Credential != "")
                {
                    param = (new FirebaseParam(param).Auth(Credential)).Parameter;
                }

                string url = Endpoint;

                if (param != string.Empty)
                    url += "?" + param;

                WebRequest rq = WebRequest.Create(url);

                rq.Headers = new CusteredHeaderCollection(Host); ;
                rq.Proxy = null;

                rq.Method = "DELETE";
                rq.ContentLength = 0;
                rq.ContentType = "application/json";

                rq.BeginGetResponse(new AsyncCallback((result) =>
                {
                    HttpWebResponse response = (result.AsyncState as HttpWebRequest).EndGetResponse(result) as HttpWebResponse;
                    using (HttpWebResponse resp = response)
                    {
                        string responseValue = string.Empty;

                        if (resp.StatusCode != HttpStatusCode.OK)
                        {
                            if (OnDeleteFailed != null) OnDeleteFailed(this, new FirebaseError(string.Format("Request failed. Received HTTP {0}", resp.StatusCode)));
                            return;
                        }

                        using (Stream responseStream = resp.GetResponseStream())
                        {
                            if (responseStream != null)
                                using (StreamReader rdr = new StreamReader(responseStream))
                                {
                                    responseValue = rdr.ReadToEnd();
                                }
                        }

                        if (responseValue != "")
                        {
                            DataSnapshot snapshot = new DataSnapshot(responseValue);
                            if (OnDeleteSuccess != null) OnDeleteSuccess(this, snapshot);
                        }
                        else
                        {
                            if (OnDeleteFailed != null) OnDeleteFailed(this, new FirebaseError("No response received."));
                        }
                    }
                }), rq);
            }
            catch (Exception ex)
            {
                if (OnDeleteFailed != null) OnDeleteFailed(this, new FirebaseError(ex.Message));
            }
        }

        /// <summary>
        /// Delete a key in Firebase. Calls OnDeleteSuccess on success, OnDeleteFailed on failed.
        /// OnDeleteSuccess action contains the corresponding Firebase and the response Snapshot
        /// OnDeleteFailed action contains the error exception
        /// </summary>
        /// <param name="query">REST call parameters wrapped in FirebaseQuery class</param>
        /// <returns></returns>
        public void Delete(FirebaseParam query)
        {
            Delete(query.Parameter);
        }

        /**** TO COROUTINE WRAPPERS ****/

        /// <summary>
        /// Wraps function to IEnumerator
        /// </summary>
        /// <param name="action">Firebase method</param>
        /// <returns></returns>
        public static IEnumerator ToCoroutine(Action<FirebaseParam> action, FirebaseParam query)
        {
            action(query);
            yield return null;
        }

        /// <summary>
        /// Wraps function to IEnumerator
        /// </summary>
        /// <param name="action">Firebase method</param>
        /// <returns></returns>
        public static IEnumerator ToCoroutine(Action<string> action, string param = "")
        {
            action(param);
            yield return null;
        }

        /// <summary>
        /// Wraps function to IEnumerator
        /// </summary>
        /// <param name="action">Firebase method</param>
        /// <returns></returns>
        public static IEnumerator ToCoroutine(Action<object, FirebaseParam> action, object obj, FirebaseParam query)
        {
            action(obj, query);
            yield return null;
        }

        /// <summary>
        /// Wraps function to IEnumerator
        /// </summary>
        /// <param name="action">Firebase method</param>
        /// <returns></returns>
        public static IEnumerator ToCoroutine(Action<object, string> action, object obj, string param = "")
        {
            action(obj, param);
            yield return null;
        }

        /// <summary>
        /// Wraps function to IEnumerator
        /// </summary>
        /// <param name="action">Firebase method</param>
        /// <returns></returns>
        public static IEnumerator ToCoroutine(Action<string, bool, FirebaseParam> action, string json, bool isJson, FirebaseParam query)
        {
            action(json, isJson, query);
            yield return null;
        }

        /// <summary>
        /// Wraps function to IEnumerator
        /// </summary>
        /// <param name="action">Firebase method</param>
        /// <returns></returns>
        public static IEnumerator ToCoroutine(Action<string, bool, string> action, string json, bool isJson, string param = "")
        {
            action(json, isJson, param);
            yield return null; ;
        }


        /**** STATIC FUNCTIONS ****/

        /// <summary>
        /// Creates new Firebase pointer at a valid Firebase url
        /// </summary>
        /// <param name="host">Example: "hostname.firebaseio.com" (with no https://)</param>
        /// <param name="credential">Credential value for auth parameter</param>
        /// <returns></returns>
        public static Firebase CreateNew(string host, string credential = "")
        {
            return new FirebaseRoot(host, credential);
        }

        /**** SPECIAL CLASS ****/

        private class CusteredHeaderCollection : WebHeaderCollection
        {
            public bool HostHeaderValueReplaced { get; private set; }

            public string ClusterUrl { get; private set; }

            public CusteredHeaderCollection(string commonClusterUrl) : base()
            {
                if (string.IsNullOrEmpty("commonClusterUrl"))
                    throw new ArgumentNullException("commonClusterUrl");

                this.ClusterUrl = commonClusterUrl;
            }

            public override string ToString()
            {
                this["Host"] = this.ClusterUrl;
                string tmp = base.ToString();
                this.HostHeaderValueReplaced = true;

                return tmp;
            }

        }


    }
}
