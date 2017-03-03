/*

Class: FirebaseQueue.cs
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

using System;

namespace SimpleFirebaseUnity
{
	using MiniJSON;

	public class SimpleFirebaseQueue {

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

		protected class CommandLinkedList {
			public SimpleFirebase firebase;
			FirebaseCommand command;
			string param;
			object obj;
			public CommandLinkedList next;

			public CommandLinkedList(SimpleFirebase _firebase, FirebaseCommand _command, string _param, object _obj = null){
				firebase = _firebase;
				command = _command;
				param = _param;
				obj = _obj;
				next = null;
			}

			public CommandLinkedList(SimpleFirebase _firebase, FirebaseCommand _command, SimpleFirebaseParam firebaseParam, object _obj = null){
				firebase = _firebase;
				command = _command;
				param = firebaseParam.Parameter;
				obj = _obj;
				next = null;
			}

			public void AddNext(CommandLinkedList _next){
				next = _next;
			}

			public void DoCommand()
			{
				switch (command) {
				case FirebaseCommand.Get:
					firebase.GetValue (param);
					break;
				case FirebaseCommand.Set:
					firebase.SetValue (obj, param);
					break;
				case FirebaseCommand.Update:
					firebase.UpdateValue (obj, param);
					break;
				case FirebaseCommand.Push:
					firebase.Push (obj, param);
					break;
				case FirebaseCommand.Delete:
					firebase.Delete (param);
					break;
				}
			}
		}

		public Action OnQueueFinished;

		protected CommandLinkedList head;
		protected CommandLinkedList tail;
		protected bool autoStart;
		protected int count;

		protected void AddQueue(SimpleFirebase firebase, FirebaseCommand command, string param, object obj = null)
		{
			CommandLinkedList commandNode =  new CommandLinkedList (firebase, command, param, obj);

			if (head == null) {
				head = commandNode;
				tail = commandNode;

				if (autoStart)
					head.DoCommand();
			} else {
				tail.next = commandNode;
				tail = commandNode;
			}

			++count;
		}

		protected void ClearQueueTopDown(CommandLinkedList node)
		{
			CommandLinkedList temp = node.next;
			node.next = null;
			ClearQueueTopDown (temp);
		}

		protected void StartNextCommand(){
			head = head.next;
			if (head != null)
				head.DoCommand ();
			else {
				tail = null;

				if (OnQueueFinished != null)
					OnQueueFinished ();
			}
		}

		protected void OnSuccess(SimpleFirebase sender, SimpleDataSnapshot snapshot)
		{
			--count;
			StartNextCommand ();
			ClearCallbacks (sender);
		}

		protected void OnFailed(SimpleFirebase sender, SimpleFirebaseError err)
		{
			--count;
			StartNextCommand ();
			ClearCallbacks (sender);
		}

		protected void ClearCallbacks(SimpleFirebase sender)
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
		/// Initializes a new instance of the <see cref="SimpleFirebaseUnity.FirebaseQueueManager"/> class.
		/// </summary>
		/// <param name="_autoStart">If set to <c>true</c> auto start when a queue added.</param>
		public SimpleFirebaseQueue(bool _autoStart = true)
		{
			autoStart = _autoStart;
			count = 0;
		}

		/// <summary>
		/// Gets the number of request in queue.
		/// </summary>
		/// <value>The count.</value>
		public int Count
		{
			get {
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
		public void AddQueueGet(SimpleFirebase firebase, string param = "")
		{
			SimpleFirebase temp = firebase.Copy (true);
			temp.OnGetSuccess += OnSuccess;
			temp.OnGetFailed += OnFailed;
			AddQueue (temp, FirebaseCommand.Get, param);
		}

		/// <summary>
		/// Adds Firebase Get request to queue.
		/// </summary>
		/// <param name="firebase">Firebase.</param>
		/// <param name="param">Parameter.</param>
		public void AddQueueGet(SimpleFirebase firebase, SimpleFirebaseParam param)
		{
			SimpleFirebase temp = firebase.Copy (true);
			temp.OnGetSuccess += OnSuccess;
			temp.OnGetFailed += OnFailed;
			AddQueue (temp, FirebaseCommand.Get, param.Parameter);
		}


		/// <summary>
		/// Adds Firebase Set request to queue.
		/// </summary>
		/// <param name="firebase">Firebase.</param>
		/// <param name="val">Value.</param>
		/// <param name="param">Parameter.</param>
		public void AddQueueSet(SimpleFirebase firebase, object val, string param = "")
		{
			SimpleFirebase temp = firebase.Copy (true);
			temp.OnSetSuccess += OnSuccess;
			temp.OnSetFailed += OnFailed;
			AddQueue (temp, FirebaseCommand.Set, param, val);
		}

		/// <summary>
		/// Adds Firebase Set request to queue.
		/// </summary>
		/// <param name="firebase">Firebase.</param>
		/// <param name="val">Value.</param>
		/// <param name="param">Parameter.</param>
		public void AddQueueSet(SimpleFirebase firebase, object val, SimpleFirebaseParam param)
		{
			SimpleFirebase temp = firebase.Copy (true);
			temp.OnSetSuccess += OnSuccess;
			temp.OnSetFailed += OnFailed;
			AddQueue (temp, FirebaseCommand.Set, param.Parameter, val);
		}

		/// <summary>
		/// Adds Firebase Set request to queue.
		/// </summary>
		/// <param name="firebase">Firebase.</param>
		/// <param name="json">Json.</param>
		/// <param name="isJson">If set to <c>true</c> is json.</param>
		/// <param name="param">Parameter.</param>
		public void AddQueueSet(SimpleFirebase firebase, string json, bool isJson, string param = "")
		{
			SimpleFirebase temp = firebase.Copy (true);
			temp.OnSetSuccess += OnSuccess;
			temp.OnSetFailed += OnFailed;
			if (!isJson)
				AddQueue (temp, FirebaseCommand.Set, param, json);
			else
				AddQueue (temp, FirebaseCommand.Set, param, Json.Deserialize(json));
		}

		/// <summary>
		/// Adds Firebase Set request to queue.
		/// </summary>
		/// <param name="firebase">Firebase.</param>
		/// <param name="json">Json.</param>
		/// <param name="isJson">If set to <c>true</c> is json.</param>
		/// <param name="param">Parameter.</param>
		public void AddQueueSet(SimpleFirebase firebase, string json, bool isJson, SimpleFirebaseParam param)
		{
			SimpleFirebase temp = firebase.Copy (true);
			temp.OnSetSuccess += OnSuccess;
			temp.OnSetFailed += OnFailed;
			if (!isJson)
				AddQueue (temp, FirebaseCommand.Set, param.Parameter, json);
			else
				AddQueue (temp, FirebaseCommand.Set, param.Parameter, Json.Deserialize(json));
		}

		/// <summary>
		/// Adds Firebase Update request to queue.
		/// </summary>
		/// <param name="firebase">Firebase.</param>
		/// <param name="val">Value.</param>
		/// <param name="param">Parameter.</param>
		public void AddQueueUpdate(SimpleFirebase firebase, object val, string param = "")
		{
			firebase.OnUpdateSuccess += OnSuccess;
			firebase.OnUpdateFailed += OnFailed;
			AddQueue (firebase, FirebaseCommand.Update, param, val);
		}

		/// <summary>
		/// Adds Firebase Update request to queue.
		/// </summary>
		/// <param name="firebase">Firebase.</param>
		/// <param name="val">Value.</param>
		/// <param name="param">Parameter.</param>
		public void AddQueueUpdate(SimpleFirebase firebase, object val, SimpleFirebaseParam param)
		{
			SimpleFirebase temp = firebase.Copy (true);
			temp.OnUpdateSuccess += OnSuccess;
			temp.OnUpdateFailed += OnFailed;
			AddQueue (temp, FirebaseCommand.Update, param.Parameter, val);
		}

		/// <summary>
		/// Adds Firebase Update request to queue.
		/// </summary>
		/// <param name="firebase">Firebase.</param>
		/// <param name="json">Json.</param>
		/// <param name="isJson">If set to <c>true</c> is json.</param>
		/// <param name="param">Parameter.</param>
		public void AddQueueUpdate(SimpleFirebase firebase, string json, bool isJson, string param = "")
		{
			SimpleFirebase temp = firebase.Copy (true);
			temp.OnUpdateSuccess += OnSuccess;
			temp.OnUpdateFailed += OnFailed;
			if (!isJson)
				AddQueue (temp, FirebaseCommand.Update, param, json);
			else
				AddQueue (temp, FirebaseCommand.Update, param, Json.Deserialize(json));
		}

		/// <summary>
		/// Adds Firebase Update request to queue.
		/// </summary>
		/// <param name="firebase">Firebase.</param>
		/// <param name="json">Json.</param>
		/// <param name="isJson">If set to <c>true</c> is json.</param>
		/// <param name="param">Parameter.</param>
		public void AddQueueUpdate(SimpleFirebase firebase, string json, bool isJson, SimpleFirebaseParam param)
		{
			SimpleFirebase temp = firebase.Copy (true);
			temp.OnUpdateSuccess += OnSuccess;
			temp.OnUpdateFailed += OnFailed;
			if (!isJson)
				AddQueue (temp, FirebaseCommand.Update, param.Parameter, json);
			else
				AddQueue (temp, FirebaseCommand.Update, param.Parameter, Json.Deserialize(json));
		}

		/// <summary>
		/// Adds Firebase Push request to queue.
		/// </summary>
		/// <param name="firebase">Firebase.</param>
		/// <param name="val">Value.</param>
		/// <param name="param">Parameter.</param>
		public void AddQueuePush(SimpleFirebase firebase, object val, string param = "")
		{
			SimpleFirebase temp = firebase.Copy (true);
			temp.OnPushSuccess += OnSuccess;
			temp.OnPushFailed += OnFailed;
			AddQueue (temp, FirebaseCommand.Push, param, val);
		}

		/// <summary>
		/// Adds Firebase Push request to queue.
		/// </summary>
		/// <param name="firebase">Firebase.</param>
		/// <param name="val">Value.</param>
		/// <param name="param">Parameter.</param>
		public void AddQueuePush(SimpleFirebase firebase, object val, SimpleFirebaseParam param)
		{
			SimpleFirebase temp = firebase.Copy (true);
			temp.OnPushSuccess += OnSuccess;
			temp.OnPushFailed += OnFailed;
			AddQueue (temp, FirebaseCommand.Push, param.Parameter, val);
		}

		/// <summary>
		/// Adds Firebase Push request to queue.
		/// </summary>
		/// <param name="firebase">Firebase.</param>
		/// <param name="json">Json.</param>
		/// <param name="isJson">If set to <c>true</c> is json.</param>
		/// <param name="param">Parameter.</param>
		public void AddQueuePush(SimpleFirebase firebase, string json, bool isJson, string param = "")
		{
			SimpleFirebase temp = firebase.Copy (true);
			temp.OnPushSuccess += OnSuccess;
			temp.OnPushFailed += OnFailed;
			if (!isJson)
				AddQueue (temp, FirebaseCommand.Push, param, json);
			else
				AddQueue (temp, FirebaseCommand.Push, param, Json.Deserialize(json));
		}

		/// <summary>
		/// Adds Firebase Push request to queue.
		/// </summary>
		/// <param name="firebase">Firebase.</param>
		/// <param name="json">Json.</param>
		/// <param name="isJson">If set to <c>true</c> is json.</param>
		/// <param name="param">Parameter.</param>
		public void AddQueuePush(SimpleFirebase firebase, string json, bool isJson, SimpleFirebaseParam param)
		{
			SimpleFirebase temp = firebase.Copy (true);
			temp.OnPushSuccess += OnSuccess;
			temp.OnPushFailed += OnFailed;
			if (!isJson)
				AddQueue (temp, FirebaseCommand.Push, param.Parameter, json);
			else
				AddQueue (temp, FirebaseCommand.Push, param.Parameter, Json.Deserialize(json));
		}

		/// <summary>
		/// Adds Firebase Delete request to queue.
		/// </summary>
		/// <param name="firebase">Firebase.</param>
		/// <param name="param">Parameter.</param>
		public void AddQueueDelete(SimpleFirebase firebase, string param = "")
		{
			SimpleFirebase temp = firebase.Copy (true);
			temp.OnDeleteSuccess += OnSuccess;
			temp.OnDeleteFailed += OnFailed;
			AddQueue (temp, FirebaseCommand.Delete, param);
		}

		/// <summary>
		/// Adds Firebase Delete request to queue.
		/// </summary>
		/// <param name="firebase">Firebase.</param>
		/// <param name="param">Parameter.</param>
		public void AddQueueDelete(SimpleFirebase firebase, SimpleFirebaseParam param)
		{
			SimpleFirebase temp = firebase.Copy (true);
			temp.OnDeleteSuccess += OnSuccess;
			temp.OnDeleteFailed += OnFailed;
			AddQueue (temp, FirebaseCommand.Delete, param.Parameter);
		}

		/// <summary>
		/// Adds Firebase Set Time Stamp request to queue.
		/// </summary>
		/// <param name="firebase">Firebase.</param>
		/// <param name="keyName">Time stamp key name.</param>
		public void AddQueueSetTimeStamp(SimpleFirebase firebase, string keyName)
		{
			SimpleFirebase temp = firebase.Child (keyName, false);
			AddQueueSet (temp, SERVER_VALUE_TIMESTAMP, true, "print=silent");
		}

		/// <summary>
		/// Adds Firebase Set Time Stamp request with callback to queue.
		/// </summary>
		/// <param name="firebase">Firebase.</param>
		/// <param name="keyName">Key name.</param>
		/// <param name="_OnSuccess">On success callback.</param>
		/// <param name="_OnFailed">On fail callback.</param>
		public void AddQueueSetTimeStamp(SimpleFirebase firebase, string keyName, Action<SimpleFirebase, SimpleDataSnapshot> _OnSuccess, Action<SimpleFirebase, SimpleFirebaseError> _OnFailed)
		{
			SimpleFirebase temp = firebase.Child (keyName);
			temp.OnSetSuccess += _OnSuccess;
			temp.OnSetFailed += _OnFailed;

			AddQueueSet (temp, SERVER_VALUE_TIMESTAMP, true);
		}


		/// <summary>
		/// Force stop the command queue chain and clears the queue.
		/// </summary>
		public void ForceClearQueue()
		{
			ClearQueueTopDown (head);
		}


		#endregion
	}
}

