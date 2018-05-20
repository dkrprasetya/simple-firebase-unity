/*

Class: FirebaseObserver.cs
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

using UnityEngine;
using System.Collections;
using System;

namespace SimpleFirebaseUnity
{
	public class FirebaseObserver  {

		public Action<Firebase, DataSnapshot> OnChange; 

		protected Firebase firebase;
		protected Firebase target;
		protected float refreshRate;
		protected string getParam;
		protected bool active;

		protected bool firstTime;
		protected DataSnapshot lastSnapshot;

		protected IEnumerator routine;


		#region CONSTRUCTORS

		/// <summary>
		/// Creates an Observer that calls GetValue request at the given refresh rate (in seconds) and checks whether the value has changed.
		/// </summary>
		/// <param name="_firebase">Firebase.</param>
		/// <param name="_refreshRate">Refresh rate (in seconds).</param>
		/// <param name="_getParam">Parameter value for the Get request that will be called periodically.</param>
		public FirebaseObserver(Firebase _firebase, float _refreshRate, string _getParam = "")
		{
			active = false;
			lastSnapshot = null;
			firebase = _firebase;
			refreshRate = _refreshRate;
			getParam = _getParam;
			target = _firebase.Copy ();
			routine = null;
		}

		/// <summary>
		/// Creates an Observer that calls GetValue request at the given refresh rate (in seconds) and checks whether the value has changed.
		/// </summary>
		/// <param name="_firebase">Firebase.</param>
		/// <param name="_refreshRate">Refresh rate (in seconds).</param>
		/// <param name="_getParam">Parameter value for the Get request that will be called periodically.</param>
		public FirebaseObserver(Firebase _firebase, float _refreshRate, FirebaseParam _getParam)
		{
			active = false;
			lastSnapshot = null;
			firebase = _firebase;
			refreshRate = _refreshRate;
			getParam = _getParam.Parameter;
			target = _firebase.Copy ();
		}

		#endregion

		#region OBSERVER FUNCTIONS

		/// <summary>
		/// Start the observer.
		/// </summary>
		public void Start()
		{
			if (routine != null)
				Stop ();
			
			active = true;
			firstTime = true;

			target.OnGetSuccess += CompareSnapshot;

			routine = RefreshCoroutine ();

			target.root.StartCoroutine (routine);
		}

		/// <summary>
		/// Stop the observer.
		/// </summary>
		public void Stop()
		{
			active = false;
			target.OnGetSuccess -= CompareSnapshot;
			lastSnapshot = null;

			if (routine != null) {
				target.root.StopCoroutine (routine);
				routine = null;
			}
				
		}


		IEnumerator RefreshCoroutine()
		{
			while (active) {
				target.GetValue ();
				yield return new WaitForSeconds (refreshRate);
			}
		}
        
		void CompareSnapshot(Firebase dummyVar, DataSnapshot snapshot)
		{
			if (firstTime) {
				firstTime = false;
				lastSnapshot = snapshot;
				return;
			}


			if ((lastSnapshot == null && snapshot != null) || (lastSnapshot != null && !lastSnapshot.RawJson.Equals (snapshot.RawJson))){
				if (OnChange != null)
					OnChange (firebase, snapshot);
			}

			lastSnapshot = snapshot;
		}

		#endregion

	}
}
