/*

Class: FirebaseQueue.cs
==============================================
Last update: 2018-04-22  (by Dikra)
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

namespace SimpleFirebaseUnity
{
    using MiniJSON;
    using System.Collections;
    using UnityEngine;

    public class FirebaseQueue
    {

        #region FIREBASE COMMAND QUEUE

        const string SERVER_VALUE_TIMESTAMP = "{\".sv\": \"timestamp\"}";

        protected enum FirebaseCommand
        {
            Get = 0,
            Set = 1,
            Update = 2,
            Push = 3,
            Delete = 4
        }

        protected class CommandLinkedList
        {
            public Firebase firebase;
            FirebaseCommand command;
            string param;
            object valObj;
            string valStr;
            bool parseToJson;
            public CommandLinkedList next;

            public CommandLinkedList(Firebase _firebase, FirebaseCommand _command, string _param)
            {
                firebase = _firebase;
                command = _command;
                param = _param;
                valObj = null;
                valStr = null;
                next = null;
            }

            public CommandLinkedList(Firebase _firebase, FirebaseCommand _command, string _param, object _valObj)
            {
                firebase = _firebase;
                command = _command;
                param = _param;
                valObj = _valObj;
                valStr = null;
                next = null;
            }

            public CommandLinkedList(Firebase _firebase, FirebaseCommand _command, string _param, string _valStr, bool _parseToJson)
            {
                firebase = _firebase;
                command = _command;
                param = _param;
                valObj = null;
                valStr = _valStr;
                parseToJson = _parseToJson;
                next = null;
            }

            public void AddNext(CommandLinkedList _next)
            {
                next = _next;
            }

            public void DoCommand()
            {
                switch (command)
                {
                    case FirebaseCommand.Get:
                        firebase.GetValue(param);
                        break;
                    case FirebaseCommand.Set:
                        if (valObj != null)
                            firebase.SetValue(valObj, param);
                        else
                            firebase.SetValue(valStr, parseToJson, param);
                        break;
                    case FirebaseCommand.Update:
                        if (valObj != null)
                            firebase.UpdateValue(valObj, param);
                        else
                            firebase.UpdateValueJson(valStr, param);
                        break;
                    case FirebaseCommand.Push:
                        if (valObj != null)
                            firebase.Push(valObj, param);
                        else
                            firebase.Push(valStr, parseToJson, param);
                        break;
                    case FirebaseCommand.Delete:
                        firebase.Delete(param);
                        break;
                }
            }
        }

        public Action OnQueueCompleted;
        public Action OnQueueInterrupted;

        protected CommandLinkedList head;
        protected CommandLinkedList tail;
        protected bool autoStart;
        protected float retryWait;
        protected int retryCounterLimit;
        protected int count;
        protected int retryCounter;

        protected void AddQueue(Firebase firebase, FirebaseCommand command, string param)
        {
            CommandLinkedList commandNode = new CommandLinkedList(firebase, command, param);
            InsertNodeToQueue(commandNode);
        }

        protected void AddQueue(Firebase firebase, FirebaseCommand command, string param, string valStr, bool parseToJson)
        {
            CommandLinkedList commandNode = new CommandLinkedList(firebase, command, param, valStr, parseToJson);
            InsertNodeToQueue(commandNode);
        }

        protected void AddQueue(Firebase firebase, FirebaseCommand command, string param, object valObj)
        {
            CommandLinkedList commandNode = new CommandLinkedList(firebase, command, param, valObj);
            InsertNodeToQueue(commandNode);
        }

        void InsertNodeToQueue(CommandLinkedList commandNode)
        {
            if (head == null)
            {
                head = commandNode;
                tail = commandNode;

                if (autoStart)
                    head.DoCommand();
            }
            else
            {
                tail.next = commandNode;
                tail = commandNode;
            }

            ++count;
        }

        protected void ClearQueueTopDown(CommandLinkedList node)
        {
            CommandLinkedList temp = node.next;
            node.next = null;
            ClearQueueTopDown(temp);
        }

        protected void StartNextCommand()
        {
            head = head.next;
            Start();
        }


        protected void OnSuccess(Firebase sender, DataSnapshot snapshot)
        {
            --count;
            StartNextCommand();
            ClearCallbacks(sender);
        }

        protected void OnFailed(Firebase sender, FirebaseError err)
        {
            if (retryCounter < retryCounterLimit)
            {
                FirebaseManager.Instance.StartCoroutine(DelayedDoCommand());
            }
            else
            {
                if (OnQueueInterrupted != null)
                    OnQueueInterrupted();
            }
        }

        IEnumerator DelayedDoCommand()
        {
            yield return new WaitForSeconds(retryWait);
            retryCounter++;
            head.DoCommand(); // Redo last command.
        }

        protected void ClearCallbacks(Firebase sender)
        {
            sender.OnGetSuccess -= OnSuccess;
            sender.OnGetFailed -= OnFailed;
            sender.OnUpdateSuccess -= OnSuccess;
            sender.OnUpdateFailed -= OnFailed;
            sender.OnPushSuccess -= OnSuccess;
            sender.OnPushFailed -= OnFailed;
            sender.OnDeleteSuccess -= OnSuccess;
            sender.OnDeleteFailed -= OnFailed;
        }

        #endregion

        #region PUBLIC FUNCTIONS

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleFirebaseUnity.FirebaseQueue/> class.
        /// </summary>
        /// <param name="_autoStart">If set to <c>true</c> auto start when a queue added.</param>
        /// <param name="_retryCounterLimit">Number of retries allowed when a request got an error. After limit reached, next command in queue will be stopped (can be restarted manually, starting from the last uncompleted command in queue).</param>
        /// <param name="_retryWait">Wait duration of each retries in seconds.</param>
        /// <param name="_OnQueueCompleted">Callback which is called when process on last command in queue completed.</param>
        /// <param name="_OnQueueInterrupted">Callback which is called when queue process stopped before completing last command.</param>
        public FirebaseQueue(bool _autoStart, int _retryCounterLimit, float _retryWait, Action _OnQueueCompleted, Action _OnQueueInterrupted)
        {
            Init(_autoStart, _retryCounterLimit, _retryWait, _OnQueueCompleted, _OnQueueInterrupted);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleFirebaseUnity.FirebaseQueue/> class.
        /// </summary>
        /// <param name="_autoStart">If set to <c>true</c> auto start when a queue added.</param>
        /// <param name="_retryCounterLimit">Number of retries allowed when a request got an error. After limit reached, next command in queue will be stopped (can be restarted manually, starting from the last uncompleted command in queue).</param>
        /// <param name="_retryWait">Wait duration of each retries in seconds.</param>
        public FirebaseQueue(bool _autoStart, int _retryCounterLimit, float _retryWait)
        {
            Init(_autoStart, _retryCounterLimit, _retryWait, null, null);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleFirebaseUnity.FirebaseQueue/> class.
        /// </summary>
        /// <param name="_autoStart">If set to <c>true</c> auto start when a queue added.</param>
        /// <param name="_OnQueueCompleted">Callback which is called when process on last command in queue completed.</param>
        /// <param name="_OnQueueInterrupted">Callback which is called when queue process stopped before completing last command.</param>
        public FirebaseQueue(bool _autoStart, Action _OnQueueCompleted, Action _OnQueueInterrupted)
        {
            Init(_autoStart, 0, float.MaxValue, _OnQueueCompleted, _OnQueueInterrupted);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleFirebaseUnity.FirebaseQueue/> class.
        /// </summary>
        /// <param name="_autoStart">If set to <c>true</c> auto start when a queue added.</param>
        public FirebaseQueue(bool _autoStart)
        {
            Init(_autoStart, 0, float.MaxValue, null, null);
        }

        void Init(bool _autoStart, int _retryCounterLimit, float _retryWait, Action _OnQueueCompleted, Action _OnQueueInterrupted)
        {
            autoStart = _autoStart;
            retryCounterLimit = _retryCounterLimit;
            retryWait = _retryWait;
            retryCounter = 0;
            count = 0;

            OnQueueCompleted = _OnQueueCompleted;
            OnQueueInterrupted = _OnQueueInterrupted;
        }


        /// <summary>
        /// Start processing the queue until all commands completed, or, an error occured and number of allowed retries is over the limit.
        /// </summary>
        public void Start()
        {
            retryCounter = 0;

            if (head != null)
                head.DoCommand();
            else
            {
                tail = null;

                if (OnQueueCompleted != null)
                    OnQueueCompleted();
            }
        }

        /// <summary>
        /// Gets the number of request in queue.
        /// </summary>
        /// <value>The count.</value>
        public int Count
        {
            get
            {
                return count;
            }
        }

        /// <summary>
        /// Determines whether the queue is empty.
        /// </summary>
        /// <returns><c>true</c> if this instance is empty; otherwise, <c>false</c>.</returns>
        public bool IsEmpty()
        {
            return (count == 0);
        }

        /// <summary>
        /// Adds Firebase Get request to queue.
        /// </summary>
        /// <param name="firebase">Firebase.</param>
        /// <param name="param">Parameter.</param>
        public void AddQueueGet(Firebase firebase, string param = "")
        {
            Firebase temp = firebase.Copy(true);
            temp.OnGetSuccess += OnSuccess;
            temp.OnGetFailed += OnFailed;
            AddQueue(temp, FirebaseCommand.Get, param);
        }

        /// <summary>
        /// Adds Firebase Get request to queue.
        /// </summary>
        /// <param name="firebase">Firebase.</param>
        /// <param name="param">Parameter.</param>
        public void AddQueueGet(Firebase firebase, FirebaseParam param)
        {
            Firebase temp = firebase.Copy(true);
            temp.OnGetSuccess += OnSuccess;
            temp.OnGetFailed += OnFailed;
            AddQueue(temp, FirebaseCommand.Get, param.Parameter);
        }
        
        /// <summary>
        /// Adds Firebase Set request to queue.
        /// </summary>
        /// <param name="firebase">Firebase.</param>
        /// <param name="val">Value.</param>
        /// <param name="param">Parameter.</param>
        public void AddQueueSet(Firebase firebase, object val, string param = "")
        {
            Firebase temp = firebase.Copy(true);
            temp.OnSetSuccess += OnSuccess;
            temp.OnSetFailed += OnFailed;
            AddQueue(temp, FirebaseCommand.Set, param, val);
        }

        /// <summary>
        /// Adds Firebase Set request to queue.
        /// </summary>
        /// <param name="firebase">Firebase.</param>
        /// <param name="val">Value.</param>
        /// <param name="param">Parameter.</param>
        public void AddQueueSet(Firebase firebase, object val, FirebaseParam param)
        {
            AddQueueSet(firebase, val, param.Parameter);
        }

        /// <summary>
        /// Adds Firebase Set request to queue.
        /// </summary>
        /// <param name="firebase">Firebase.</param>
        /// <param name="val">Set value.</param>
        /// <param name="isJson">If set to <c>true</c> is json.</param>
        /// <param name="param">Parameter.</param>
        public void AddQueueSet(Firebase firebase, string val, bool parseToJson, string param = "")
        {
            Firebase temp = firebase.Copy(true);
            temp.OnSetSuccess += OnSuccess;
            temp.OnSetFailed += OnFailed;

            AddQueue(temp, FirebaseCommand.Set, param, val, parseToJson);
        }

        /// <summary>
        /// Adds Firebase Set request to queue.
        /// </summary>
        /// <param name="firebase">Firebase.</param>
        /// <param name="json">Json.</param>
        /// <param name="isJson">If set to <c>true</c> is json.</param>
        /// <param name="param">Parameter.</param>
        public void AddQueueSet(Firebase firebase, string val, bool parseToJson, FirebaseParam param)
        {
            AddQueueSet(firebase, val, parseToJson, param.Parameter);
        }

        /// <summary>
        /// Adds Firebase Update request to queue.
        /// </summary>
        /// <param name="firebase">Firebase.</param>
        /// <param name="val">Value.</param>
        /// <param name="param">Parameter.</param>
        public void AddQueueUpdate(Firebase firebase, object val, string param = "")
        {
            Firebase temp = firebase.Copy(true);
            temp.OnSetSuccess += OnSuccess;
            temp.OnSetFailed += OnFailed;

            AddQueue(firebase, FirebaseCommand.Update, param, val);
        }

        /// <summary>
        /// Adds Firebase Update request to queue.
        /// </summary>
        /// <param name="firebase">Firebase.</param>
        /// <param name="val">Value.</param>
        /// <param name="param">Parameter.</param>
        public void AddQueueUpdate(Firebase firebase, object val, FirebaseParam param)
        {
            AddQueueUpdate(firebase, val, param.Parameter);
        }

        /// <summary>
        /// Adds Firebase Update request to queue.
        /// </summary>
        /// <param name="firebase">Firebase.</param>
        /// <param name="valJson">Value in json format.</param>
        /// <param name="param">Parameter.</param>
        public void AddQueueUpdate(Firebase firebase, string valJson, string param = "")
        {
            Firebase temp = firebase.Copy(true);
            temp.OnUpdateSuccess += OnSuccess;
            temp.OnUpdateFailed += OnFailed;

            AddQueue(temp, FirebaseCommand.Update, param, valJson, false); // Update value is strictly json.
        }

        /// <summary>
        /// Adds Firebase Update request to queue.
        /// </summary>
        /// <param name="firebase">Firebase.</param>
        /// <param name="valJson">Value in json format.</param>
        /// <param name="param">Parameter.</param>
        public void AddQueueUpdate(Firebase firebase, string valJson, FirebaseParam param)
        {
            AddQueueUpdate(firebase, valJson, param.Parameter);
        }

        /// <summary>
        /// Adds Firebase Push request to queue.
        /// </summary>
        /// <param name="firebase">Firebase.</param>
        /// <param name="val">Value.</param>
        /// <param name="param">Parameter.</param>
        public void AddQueuePush(Firebase firebase, object val, string param = "")
        {
            Firebase temp = firebase.Copy(true);
            temp.OnPushSuccess += OnSuccess;
            temp.OnPushFailed += OnFailed;
            AddQueue(temp, FirebaseCommand.Push, param, Json.Serialize(val));
        }

        /// <summary>
        /// Adds Firebase Push request to queue.
        /// </summary>
        /// <param name="firebase">Firebase.</param>
        /// <param name="val">Value.</param>
        /// <param name="param">Parameter.</param>
        public void AddQueuePush(Firebase firebase, object val, FirebaseParam param)
        {
            AddQueuePush(firebase, val, param.Parameter);
        }

        /// <summary>
        /// Adds Firebase Push request to queue.
        /// </summary>
        /// <param name="firebase">Firebase.</param>
        /// <param name="val">Value.</param>
        /// <param name="isJson">If set to <c>true</c> is json.</param>
        /// <param name="param">Parameter.</param>
        public void AddQueuePush(Firebase firebase, string val, bool parseToJson, string param = "")
        {
            Firebase temp = firebase.Copy(true);
            temp.OnPushSuccess += OnSuccess;
            temp.OnPushFailed += OnFailed;

            AddQueue(temp, FirebaseCommand.Push, param, val, parseToJson);
        }

        /// <summary>
        /// Adds Firebase Push request to queue.
        /// </summary>
        /// <param name="firebase">Firebase.</param>
        /// <param name="val">Value.</param>
        /// <param name="isJson">If set to <c>true</c> is json.</param>
        /// <param name="param">Parameter.</param>
        public void AddQueuePush(Firebase firebase, string val, bool parseToJson, FirebaseParam param)
        {
            AddQueuePush(firebase, val, parseToJson, param.Parameter);
        }

        /// <summary>
        /// Adds Firebase Delete request to queue.
        /// </summary>
        /// <param name="firebase">Firebase.</param>
        /// <param name="param">Parameter.</param>
        public void AddQueueDelete(Firebase firebase, string param = "")
        {
            Firebase temp = firebase.Copy(true);
            temp.OnDeleteSuccess += OnSuccess;
            temp.OnDeleteFailed += OnFailed;
            AddQueue(temp, FirebaseCommand.Delete, param);
        }

        /// <summary>
        /// Adds Firebase Delete request to queue.
        /// </summary>
        /// <param name="firebase">Firebase.</param>
        /// <param name="param">Parameter.</param>
        public void AddQueueDelete(Firebase firebase, FirebaseParam param)
        {
            AddQueueDelete(firebase, param.Parameter);
        }

        /// <summary>
        /// Adds Firebase Set Time Stamp request to queue.
        /// </summary>
        /// <param name="firebase">Firebase.</param>
        /// <param name="keyName">Time stamp key name.</param>
        public void AddQueueSetTimeStamp(Firebase firebase, string keyName)
        {
            Firebase temp = firebase.Child(keyName, false);

            UnityEngine.Debug.LogWarning("SERVER VALUE TIMESTAMP = " + SERVER_VALUE_TIMESTAMP);
            AddQueueSet(temp, SERVER_VALUE_TIMESTAMP, false, "print=silent");
        }

        /// <summary>
        /// Adds Firebase Set Time Stamp request with callback to queue.
        /// </summary>
        /// <param name="firebase">Firebase.</param>
        /// <param name="keyName">Key name.</param>
        /// <param name="_OnSuccess">On success callback.</param>
        /// <param name="_OnFailed">On fail callback.</param>
        public void AddQueueSetTimeStamp(Firebase firebase, string keyName, Action<Firebase, DataSnapshot> _OnSuccess, Action<Firebase, FirebaseError> _OnFailed)
        {
            Firebase temp = firebase.Child(keyName);
            temp.OnSetSuccess += _OnSuccess;
            temp.OnSetFailed += _OnFailed;

            AddQueueSet(temp, SERVER_VALUE_TIMESTAMP, false);
        }


        /// <summary>
        /// Force stop the command queue chain and clears the queue. Warning: processed command cannot be undone.
        /// </summary>
        public void ForceClearQueue()
        {
            FirebaseManager.Instance.StopCoroutine("DelayedDoCommand");
            ClearQueueTopDown(head);
        }


        #endregion
    }
}

