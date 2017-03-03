/*

Class: Firebase.cs
==============================================
Last update: 2016-07-27  (by Dikra)
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
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Net;

namespace SimpleFirebaseUnity
{
    using MiniJSON;
    [Serializable]
    public class SimpleFirebase
    {
        const string SERVER_VALUE_TIMESTAMP = "{\".sv\": \"timestamp\"}";

        public Action<SimpleFirebase, SimpleDataSnapshot> OnGetSuccess;
        public Action<SimpleFirebase, SimpleFirebaseError> OnGetFailed;

        public Action<SimpleFirebase, SimpleDataSnapshot> OnSetSuccess;
        public Action<SimpleFirebase, SimpleFirebaseError> OnSetFailed;

        public Action<SimpleFirebase, SimpleDataSnapshot> OnUpdateSuccess;
        public Action<SimpleFirebase, SimpleFirebaseError> OnUpdateFailed;

        public Action<SimpleFirebase, SimpleDataSnapshot> OnPushSuccess;
        public Action<SimpleFirebase, SimpleFirebaseError> OnPushFailed;

        public Action<SimpleFirebase, SimpleDataSnapshot> OnDeleteSuccess;
        public Action<SimpleFirebase, SimpleFirebaseError> OnDeleteFailed;

        protected SimpleFirebase parent;
        internal SimpleFirebaseRoot root;
        protected string key;
        protected string fullKey;

        #region GET-SET

        /// <summary>
        /// Parent of current firebase pointer
        /// </summary>                 
        public SimpleFirebase Parent
        {
            get
            {
                return parent;
            }
        }

        /// <summary>
        /// Root firebase pointer of the endpoint
        /// </summary>
        public SimpleFirebase Root
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
                return "https://" + Host + FullKey + "/.json";
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

        /// <summary>
        /// Gets the rules endpoint.
        /// </summary>
        /// <value>The rules endpoint.</value>
        public virtual string RulesEndpoint
        {
            get
            {
                return root.RulesEndpoint;
            }
        }


        /**** CONSTRUCTOR ****/

        /// <summary>
        /// Create new Firebase endpoint
        /// </summary>
        /// <param name="_parent">Parent Firebase pointer</param>
        /// <param name="_key">Key under parent Firebase</param>
        /// <param name="_root">Root Firebase pointer</param>
        /// <param name="inheritCallback">If set to <c>true</c> inherit callback.</param>
        internal SimpleFirebase(SimpleFirebase _parent, string _key, SimpleFirebaseRoot _root, bool inheritCallback = false)
        {
            parent = _parent;
            key = _key;
            root = _root;

            fullKey = parent.FullKey + "/" + key;

            if (inheritCallback)
            {
                OnGetSuccess = parent.OnGetSuccess;
                OnGetFailed = parent.OnGetFailed;

                OnSetSuccess = parent.OnSetSuccess;
                OnSetFailed = parent.OnSetFailed;

                OnUpdateSuccess = parent.OnUpdateSuccess;
                OnUpdateFailed = parent.OnUpdateFailed;

                OnPushSuccess = parent.OnPushSuccess;
                OnPushFailed = parent.OnPushFailed;

                OnDeleteSuccess = parent.OnDeleteSuccess;
                OnDeleteFailed = parent.OnDeleteFailed;
            }
        }

        internal SimpleFirebase()
        {
            parent = null;
            key = string.Empty;
            root = null;
        }

        #endregion

        #region BASIC FUNCTIONS

        /// <summary>
        /// Get Firebase child from given key
        /// </summary>
        /// <param name="_key">A string</param>
        /// <param name="inheritCallback">If set to <c>true</c> inherit callback.</param>
        public SimpleFirebase Child(string _key, bool inheritCallback = false)
        {
            return new SimpleFirebase(this, _key, root, inheritCallback);
        }

        /// <summary>
        /// Get Firebase childs from given keys
        /// </summary>
        /// <param name="_keys">List of string</param>
        public List<SimpleFirebase> Childs(List<string> _keys)
        {
            List<SimpleFirebase> childs = new List<SimpleFirebase>();
            foreach (string k in _keys)
                childs.Add(Child(k));
            return childs;
        }

        /// <summary>
        /// Get Firebase childs from given keys
        /// </summary>
        /// <param name="_keys">Array of string</param>
        public List<SimpleFirebase> Childs(string[] _keys)
        {
            List<SimpleFirebase> childs = new List<SimpleFirebase>();
            foreach (string k in _keys)
                childs.Add(Child(k));

            return childs;
        }

        /// <summary>
        /// Get a fresh copy of this Firebase object
        /// </summary>
        /// <param name="inheritCallback">If set to <c>true</c> inherit callback.</param>
        public SimpleFirebase Copy(bool inheritCallback = false)
        {
            SimpleFirebase temp;
            if (parent == null)
                temp = root.Copy();
            else
                temp = new SimpleFirebase(parent, key, root);

            if (inheritCallback)
            {
                temp.OnGetSuccess = OnGetSuccess;
                temp.OnGetFailed = OnGetFailed;

                temp.OnSetSuccess = OnSetSuccess;
                temp.OnSetFailed = OnSetFailed;

                temp.OnUpdateSuccess = OnUpdateSuccess;
                temp.OnUpdateFailed = OnUpdateFailed;

                temp.OnPushSuccess = OnPushSuccess;
                temp.OnPushFailed = OnPushFailed;

                temp.OnDeleteSuccess = OnDeleteSuccess;
                temp.OnDeleteFailed = OnDeleteFailed;
            }

            return temp;
        }

        #endregion

        #region REST FUNCTIONS

        /// <summary>
        /// Fetch data from Firebase. Calls OnGetSuccess on success, OnGetFailed on failed.
        /// OnGetSuccess action contains the corresponding Firebase and the fetched Snapshot
        /// OnGetFailed action contains the error exception
        /// </summary>
        /// <param name="query">REST call parameters wrapped in FirebaseQuery class</param>
        /// <returns></returns>
        public void GetValue(SimpleFirebaseParam query)
        {
            GetValue(query.Parameter);
        }

        /// <summary>
        /// Fetch data from Firebase. Calls OnGetSuccess on success, OnGetFailed on failed.
        /// OnGetSuccess action contains the corresponding Firebase and the fetched Snapshot
        /// OnGetFailed action contains the error exception
        /// </summary>
        /// <param name="param">REST call parameters on a string. Example: &quot;orderBy=&#92;"$key&#92;"&quot;print=pretty&quot;shallow=true"></param>
        /// <returns></returns>
        public void GetValue(string param = "")
        {
            try
            {
                if (Credential != "")
                {
                    param = (new SimpleFirebaseParam(param).Auth(Credential)).Parameter;
                }

                string url = Endpoint;

                param = WWW.EscapeURL(param);

                if (param != "")
                    url += "?" + param;

                root.StartCoroutine(RequestCoroutine(url, null, null, OnGetSuccess, OnGetFailed));
            }
            catch (WebException webEx)
            {
                if (OnGetFailed != null) OnGetFailed(this, SimpleFirebaseError.Create(webEx));
            }
            catch (Exception ex)
            {
                if (OnGetFailed != null) OnGetFailed(this, new SimpleFirebaseError(ex.Message));
            }

        }

        /// <summary>
        /// Set value of a key on Firebase. Calls OnUpdateSuccess on success, OnUpdateFailed on failed.
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
        /// Set value of a key on Firebase. Calls OnUpdateSuccess on success, OnUpdateFailed on failed.
        /// OnSetSuccess action contains the corresponding Firebase and the response Snapshot
        /// OnSetFailed action contains the error exception
        /// </summary>
        /// <param name="_val">Set value</param>
        /// <param name="param">REST call parameters on a string. Example: "auth=ASDF123"</param>
        /// <returns></returns>
        public void SetValue(object _val, string param = "")
        {
            try
            {
                if (Credential != "")
                {
                    param = (new SimpleFirebaseParam(param).Auth(Credential)).Parameter;
                }

                string url = Endpoint;

                param = WWW.EscapeURL(param);

                if (param != string.Empty)
                    url += "?" + param;

                Dictionary<string, string> headers = new Dictionary<string, string>();
                headers.Add("Content-Type", "application/json");
                headers.Add("X-HTTP-Method-Override", "PUT");

                //UTF8Encoding encoding = new UTF8Encoding();
                byte[] bytes = Encoding.GetEncoding("iso-8859-1").GetBytes(Json.Serialize(_val));

                root.StartCoroutine(RequestCoroutine(url, bytes, headers, OnSetSuccess, OnSetFailed));
            }
            catch (WebException webEx)
            {
                if (OnSetFailed != null) OnSetFailed(this, SimpleFirebaseError.Create(webEx));
            }
            catch (Exception ex)
            {
                if (OnSetFailed != null) OnSetFailed(this, new SimpleFirebaseError(ex.Message));
            }

        }

        /// <summary>
        /// Set value of a key on Firebase. Calls OnUpdateSuccess on success, OnUpdateFailed on failed.
        /// OnSetSuccess action contains the corresponding Firebase and the response Snapshot
        /// OnSetFailed action contains the error exception
        /// </summary>
        /// <param name="json">String</param>
        /// <param name="isJson">True if string is json (necessary to differentiate the other overloading)</param>
        /// <param name="query">REST call parameters wrapped in FirebaseQuery class</param>
        /// <returns></returns>
        public void SetValue(string json, bool isJson, SimpleFirebaseParam query)
        {
            if (!isJson)
                SetValue(json, query.Parameter);
            else
                SetValue(Json.Deserialize(json), query.Parameter);
        }

        /// <summary>
        /// Set value of a key on Firebase. Calls OnUpdateSuccess on success, OnUpdateFailed on failed.
        /// OnSetSuccess action contains the corresponding Firebase and the response Snapshot
        /// OnSetFailed action contains the error exception
        /// </summary>
        /// <param name="_val">Update value</param>
        /// <param name="query">REST call parameters wrapped in FirebaseQuery class</param>
        /// <returns></returns>
        public void SetValue(object _val, SimpleFirebaseParam query)
        {
            SetValue(_val, query.Parameter);
        }



        /// <summary>
        /// Update value of a key on Firebase. Calls OnUpdateSuccess on success, OnUpdateFailed on failed.
        /// OnUpdateSuccess action contains the corresponding Firebase and the response Snapshot
        /// OnUpdateFailed action contains the error exception
        /// </summary>
        /// <param name="_val">Set value</param>
        /// <param name="param">REST call parameters on a string. Example: "auth=ASDF123"</param>
        /// <returns></returns>
        public void UpdateValue(object _val, string param = "")
        {
            try
            {
                if (!(_val is Dictionary<string, object>))
                {
                    if (OnUpdateFailed != null)
                        OnUpdateFailed(this, new SimpleFirebaseError((HttpStatusCode)400, "Invalid data; couldn't parse JSON object. Are you sending a JSON object with valid key names?"));

                    return;
                }

                if (Credential != "")
                {
                    param = (new SimpleFirebaseParam(param).Auth(Credential)).Parameter;
                }

                string url = Endpoint;

                param = WWW.EscapeURL(param);

                if (param != string.Empty)
                    url += "?" + param;

                Dictionary<string, string> headers = new Dictionary<string, string>();
                headers.Add("Content-Type", "application/json");
                headers.Add("X-HTTP-Method-Override", "PATCH");

                //UTF8Encoding encoding = new UTF8Encoding();
                byte[] bytes = Encoding.GetEncoding("iso-8859-1").GetBytes(Json.Serialize(_val));

                root.StartCoroutine(RequestCoroutine(url, bytes, headers, OnUpdateSuccess, OnUpdateFailed));
            }
            catch (WebException webEx)
            {
                if (OnUpdateFailed != null) OnUpdateFailed(this, SimpleFirebaseError.Create(webEx));
            }
            catch (Exception ex)
            {
                if (OnUpdateFailed != null) OnUpdateFailed(this, new SimpleFirebaseError(ex.Message));
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
        public void UpdateValue(string json, bool isJson, SimpleFirebaseParam query)
        {
            if (!isJson)
                UpdateValue(json, query.Parameter);
            else
                UpdateValue(Json.Deserialize(json), query.Parameter);
        }

        /// <summary>
        /// Update value of a key on Firebase. Calls OnUpdateSuccess on success, OnUpdateFailed on failed.
        /// OnUpdateSuccess action contains the corresponding Firebase and the response Snapshot
        /// OnUpdateFailed action contains the error exception
        /// </summary>
        /// <param name="_val">Update value</param>
        /// <param name="query">REST call parameters wrapped in FirebaseQuery class</param>
        /// <returns></returns>
        public void UpdateValue(object _val, SimpleFirebaseParam query)
        {
            UpdateValue(_val, query.Parameter);
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
                    param = (new SimpleFirebaseParam(param).Auth(Credential)).Parameter;
                }

                string url = Endpoint;

                param = WWW.EscapeURL(param);

                if (param != string.Empty)
                    url += "?" + param;


                //UTF8Encoding encoding = new UTF8Encoding();
                byte[] bytes = Encoding.GetEncoding("iso-8859-1").GetBytes(Json.Serialize(_val));

                root.StartCoroutine(RequestCoroutine(url, bytes, null, OnPushSuccess, OnPushFailed));
            }
            catch (WebException webEx)
            {
                if (OnPushFailed != null) OnPushFailed(this, SimpleFirebaseError.Create(webEx));
            }
            catch (Exception ex)
            {
                if (OnPushFailed != null) OnPushFailed(this, new SimpleFirebaseError(ex.Message));
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
        public void Push(string json, bool isJson, SimpleFirebaseParam query)
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
        public void Push(object _val, SimpleFirebaseParam query)
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
                    param = (new SimpleFirebaseParam(param).Auth(Credential)).Parameter;
                }

                string url = Endpoint;

                param = WWW.EscapeURL(param);

                if (param != string.Empty)
                    url += "?" + param;

                Dictionary<string, string> headers = new Dictionary<string, string>();
                headers.Add("Content-Type", "application/json");
                headers.Add("X-HTTP-Method-Override", "DELETE");

                //UTF8Encoding encoding = new UTF8Encoding();
                byte[] bytes = Encoding.GetEncoding("iso-8859-1").GetBytes("{ \"dummy\" : \"dummies\"}");

                root.StartCoroutine(RequestCoroutine(url, bytes, headers, OnDeleteSuccess, OnDeleteFailed));

            }
            catch (WebException webEx)
            {
                if (OnDeleteFailed != null) OnDeleteFailed(this, SimpleFirebaseError.Create(webEx));
            }
            catch (Exception ex)
            {
                if (OnDeleteFailed != null) OnDeleteFailed(this, new SimpleFirebaseError(ex.Message));
            }
        }

        /// <summary>
        /// Delete a key in Firebase. Calls OnDeleteSuccess on success, OnDeleteFailed on failed.
        /// OnDeleteSuccess action contains the corresponding Firebase and the response Snapshot
        /// OnDeleteFailed action contains the error exception
        /// </summary>
        /// <param name="query">REST call parameters wrapped in FirebaseQuery class</param>
        /// <returns></returns>
        public void Delete(SimpleFirebaseParam query)
        {
            Delete(query.Parameter);
        }


        /// <summary>
        /// Sets the time stamp with the time since UNIX epoch by server value (in milliseconds).
        /// </summary>
        /// <param name="keyName">Key name.</param>
        public void SetTimeStamp(string keyName)
        {
            Child(keyName).SetValue(SERVER_VALUE_TIMESTAMP, true);
        }

        /// <summary>
        /// Sets the time stamp with the time since UNIX epoch by server value (in milliseconds).
        /// </summary>
        /// <param name="keyName">Key name.</param>
        /// <param name="OnSuccess">On success callback.</param>
        /// <param name="OnFailed">On fail callback.</param>
        public void SetTimeStamp(string keyName, Action<SimpleFirebase, SimpleDataSnapshot> OnSuccess, Action<SimpleFirebase, SimpleFirebaseError> OnFailed)
        {
            SimpleFirebase temp = Child(keyName);
            temp.OnSetSuccess += OnSuccess;
            temp.OnSetFailed += OnFailed;

            temp.SetValue(SERVER_VALUE_TIMESTAMP, true);
        }


        /// <summary>
        /// Gets Firebase Rules. Returned value is treated the same as returned value on Get request, packaged in DataSnapshot. Please note that FIREBASE_SECRET is required. If secret parameter is not set, it will use the Credential that has been set when CreateNew called.
        /// </summary>
        /// <param name="OnSuccess">On success callback.</param>
        /// <param name="OnFailed">On failed callback.</param>
        /// <param name="secret">Firebase Secret.</param>
        public void GetRules(Action<SimpleFirebase, SimpleDataSnapshot> OnSuccess, Action<SimpleFirebase, SimpleFirebaseError> OnFailed, string secret = "")
        {
            try
            {
                if (string.IsNullOrEmpty(secret))
                {
                    if (!string.IsNullOrEmpty(Credential))
                        secret = Credential;
                }

                string url = RulesEndpoint;

                url += "?auth=" + secret;

                root.StartCoroutine(RequestCoroutine(url, null, null, OnSuccess, OnFailed));
            }
            catch (WebException webEx)
            {
                if (OnFailed != null) OnFailed(this, SimpleFirebaseError.Create(webEx));
            }
            catch (Exception ex)
            {
                if (OnFailed != null) OnFailed(this, new SimpleFirebaseError(ex.Message));
            }
        }

        /// <summary>
        /// Sets Firebase Rules. Returned value is treated the same as returned value on Set request, packaged in DataSnapshot.Please note that FIREBASE_SECRET is required. If secret parameter is not set, it will use the Credential that has been set when CreateNew called.
        /// </summary>
        /// <param name="json">Valid rules Json.</param>
        /// <param name="OnSuccess">On success callback.</param>
        /// <param name="OnFailed">On failed callback.</param>
        /// <param name="secret">Firebase Secret.</param>
        public void SetRules(string json, Action<SimpleFirebase, SimpleDataSnapshot> OnSuccess, Action<SimpleFirebase, SimpleFirebaseError> OnFailed, string secret = "")
        {
            try
            {
                if (string.IsNullOrEmpty(secret))
                {
                    if (!string.IsNullOrEmpty(Credential))
                        secret = Credential;
                }

                string url = RulesEndpoint;

                url += "?auth=" + secret;

                Dictionary<string, string> headers = new Dictionary<string, string>();
                headers.Add("Content-Type", "application/json");
                headers.Add("X-HTTP-Method-Override", "PUT");

                //UTF8Encoding encoding = new UTF8Encoding();
                byte[] bytes = Encoding.GetEncoding("iso-8859-1").GetBytes(json);

                root.StartCoroutine(RequestCoroutine(url, bytes, headers, OnSuccess, OnFailed));
            }
            catch (WebException webEx)
            {
                if (OnFailed != null) OnFailed(this, SimpleFirebaseError.Create(webEx));
            }
            catch (Exception ex)
            {
                if (OnFailed != null) OnFailed(this, new SimpleFirebaseError(ex.Message));
            }
        }

        /// <summary>
        /// Sets Firebase Rules silently. Please note that FIREBASE_SECRET is required. If secret parameter is not set, it will use the Credential that has been set when CreateNew called.
        /// </summary>
        /// <param name="json">Valid rules Json.</param>
        /// <param name="secret">Firebase Secret.</param>
        public void SetRules(string json, string secret = "")
        {
            SetRules(json, null, null, secret);
        }

        /// <summary>
        /// Sets Firebase Rules silently. Please note that FIREBASE_SECRET is required. If secret parameter is not set, it will use the Credential that has been set when CreateNew called.Sets the rules.
        /// </summary>
        /// <param name="rules">Valid rules that could be serialized into json.</param>
        /// <param name="OnSuccess">On success.</param>
        /// <param name="OnFailed">On failed.</param>
        /// <param name="secret">Firebase Secret.</param>
        public void SetRules(Dictionary<string, object> rules, Action<SimpleFirebase, SimpleDataSnapshot> OnSuccess, Action<SimpleFirebase, SimpleFirebaseError> OnFailed, string secret = "")
        {
            SetRules(Json.Serialize(rules), OnSuccess, OnFailed, secret);
        }

        /// <summary>
        /// Sets Firebase Rules silently. Please note that FIREBASE_SECRET is required. If secret parameter is not set, it will use the Credential that has been set when CreateNew called.Sets the rules.
        /// </summary>
        /// <param name="rules">Valid rules that could be serialized into json.</param>
        /// <param name="secret">Firebase Secret.</param>
        public void SetRules(Dictionary<string, object> rules, string secret = "")
        {
            SetRules(Json.Serialize(rules), null, null, secret);
        }

        #endregion

        #region REQUEST COROUTINE

        protected IEnumerator RequestCoroutine(string url, byte[] postData, Dictionary<string, string> headers, Action<SimpleFirebase, SimpleDataSnapshot> OnSuccess, Action<SimpleFirebase, SimpleFirebaseError> OnFailed)
        {
            using (WWW www = (headers != null) ? new WWW(url, postData, headers) : (postData != null) ? new WWW(url, postData) : new WWW(url))
            {
                // Wait until load done
                yield return www;

                if (!string.IsNullOrEmpty(www.error))
                {

                    HttpStatusCode status = 0;
                    string errMessage = "";

                    // Parse status code
                    if (www.responseHeaders.ContainsKey("STATUS"))
                    {
                        string str = www.responseHeaders["STATUS"] as string;
                        string[] components = str.Split(' ');
                        int code = 0;
                        if (components.Length >= 3 && int.TryParse(components[1], out code))
                            status = (HttpStatusCode)code;
                    }

                    if (www.error.Contains("crossdomain.xml") || www.error.Contains("Couldn't resolve"))
                    {
                        errMessage = "No internet connection or crossdomain.xml policy problem";
                    }
                    else {

                        // Parse error message

                        try
                        {
                            if (!string.IsNullOrEmpty(www.text))
                            {
                                Dictionary<string, object> obj = Json.Deserialize(www.text) as Dictionary<string, object>;

                                if (obj != null && obj.ContainsKey("error"))
                                    errMessage = obj["error"] as string;
                            }
                        }
                        catch
                        {
                        }
                    }



                    if (OnFailed != null)
                    {
                        if (string.IsNullOrEmpty(errMessage))
                            errMessage = www.error;

                        if (errMessage.Contains("Failed downloading"))
                        {
                            errMessage = "Request failed with no info of error.";
                        }

                        OnFailed(this, new SimpleFirebaseError(status, errMessage));
                    }

#if UNITY_EDITOR
                    Debug.LogWarning(www.error + " (" + (int)status + ")\nResponse Message: " + errMessage);
#endif
                }
                else
                {
                    SimpleDataSnapshot snapshot = new SimpleDataSnapshot(www.text);
                    if (OnSuccess != null) OnSuccess(this, snapshot);
                }
            }
        }

#endregion

#region STATIC FUNCTIONS

        /// <summary>
        /// Creates new Firebase pointer at a valid Firebase url
        /// </summary>
        /// <param name="host">Example: "hostname.firebaseio.com" (with no https://)</param>
        /// <param name="credential">Credential value for auth parameter</param>
        /// <returns></returns>
        public static SimpleFirebase CreateNew(string host, string credential = "")
        {
            return new SimpleFirebaseRoot(host, credential);
        }

        /// <summary>
        /// Converts unix time stamp into DateTime
        /// </summary>
        /// <returns>The stamp to date time.</returns>
        /// <param name="unixTimeStamp">Unix time stamp.</param>
        public static DateTime TimeStampToDateTime(long unixTimeStamp)
        {
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dateTime = dateTime.AddMilliseconds(unixTimeStamp).ToLocalTime();
            return dateTime;
        }

#endregion

    }
}
