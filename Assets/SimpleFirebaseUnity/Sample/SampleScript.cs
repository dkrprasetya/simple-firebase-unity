using UnityEngine;

using SimpleFirebaseUnity;
using SimpleFirebaseUnity.MiniJSON;

using System.Collections.Generic;
using System.Collections;
using System;


public class SampleScript : MonoBehaviour
{

    static int debug_idx = 0;

    [SerializeField]
    TextMesh textMesh;


    // Use this for initialization
    void Start()
    {
        textMesh.text = "";
		StartCoroutine (Tests ());
    }
    
    void GetOKHandler(Firebase sender, DataSnapshot snapshot)
    {
        DoDebug("[OK] Get from key: <" + sender.FullKey + ">");
        DoDebug("[OK] Raw Json: " + snapshot.RawJson);

        Dictionary<string, object> dict = snapshot.Value<Dictionary<string, object>>();
        List<string> keys = snapshot.Keys;

        if (keys != null)
            foreach (string key in keys)
            {
                DoDebug(key + " = " + dict[key].ToString());
            }
    }

    void GetFailHandler(Firebase sender, FirebaseError err)
    {
        DoDebug("[ERR] Get from key: <" + sender.FullKey + ">,  " + err.Message + " (" + (int)err.Status + ")");
    }

    void SetOKHandler(Firebase sender, DataSnapshot snapshot)
    {
        DoDebug("[OK] Set from key: <" + sender.FullKey + ">");
    }

    void SetFailHandler(Firebase sender, FirebaseError err)
    {
        DoDebug("[ERR] Set from key: <" + sender.FullKey + ">, " + err.Message + " (" + (int)err.Status + ")");
    }

	void UpdateOKHandler(Firebase sender, DataSnapshot snapshot)
	{
		DoDebug("[OK] Update from key: <" + sender.FullKey + ">");
	}

	void UpdateFailHandler(Firebase sender, FirebaseError err)
	{
		DoDebug("[ERR] Update from key: <" + sender.FullKey + ">, " + err.Message + " (" + (int)err.Status + ")");
	}

    void DelOKHandler(Firebase sender, DataSnapshot snapshot)
    {
        DoDebug("[OK] Del from key: <" + sender.FullKey + ">");
    }

    void DelFailHandler(Firebase sender, FirebaseError err)
    {
        DoDebug("[ERR] Del from key: <" + sender.FullKey + ">, " + err.Message + " (" + (int)err.Status + ")");
    }

    void PushOKHandler(Firebase sender, DataSnapshot snapshot)
    {
        DoDebug("[OK] Push from key: <" + sender.FullKey + ">");
    }

    void PushFailHandler(Firebase sender, FirebaseError err)
    {
        DoDebug("[ERR] Push from key: <" + sender.FullKey + ">, " + err.Message + " (" + (int)err.Status + ")");
    }

	void GetRulesOKHandler(Firebase sender, DataSnapshot snapshot)
	{
		DoDebug("[OK] GetRules");
		DoDebug("[OK] Raw Json: " + snapshot.RawJson);
	}

	void GetRulesFailHandler(Firebase sender, FirebaseError err)
	{
		DoDebug("[ERR] GetRules,  " + err.Message + " (" + (int)err.Status + ")");
	}

	void GetTimeStamp(Firebase sender, DataSnapshot snapshot)
	{
		long timeStamp = snapshot.Value<long> ();
		DateTime dateTime = Firebase.TimeStampToDateTime (timeStamp);

		DoDebug ("[OK] Get on timestamp key: <" + sender.FullKey + ">");
		DoDebug("Date: " + timeStamp + " --> " + dateTime.ToString ());
	}

    void DoDebug(string str)
    {
        Debug.Log(str);
        if (textMesh != null)
        {
            textMesh.text += (++debug_idx + ". " + str) + "\n";
        }
    }

	IEnumerator Tests()
	{
		// Inits Firebase using Firebase Secret Key as Auth
		// The current provided implementation not yet including Auth Token Generation
		// If you're using this sample Firebase End, 
		// there's a possibility that your request conflicts with other simple-firebase-c# user's request
		Firebase firebase = Firebase.CreateNew ("simple-firebase-csharp.firebaseio.com", "UPaocwJVKvhnKL1orrf5XwWHMz3HUzckgCnGp6id");

		// Init callbacks
		firebase.OnGetSuccess += GetOKHandler;
		firebase.OnGetFailed += GetFailHandler;
		firebase.OnSetSuccess += SetOKHandler;
		firebase.OnSetFailed += SetFailHandler;
		firebase.OnUpdateSuccess += UpdateOKHandler;
		firebase.OnUpdateFailed += UpdateFailHandler;
		firebase.OnPushSuccess += PushOKHandler;
		firebase.OnPushFailed += PushFailHandler;
		firebase.OnDeleteSuccess += DelOKHandler;
		firebase.OnDeleteFailed += DelFailHandler;

		// Get child node from firebase, if false then all the callbacks are not inherited.
		Firebase temporary = firebase.Child ("temporary", true);
		Firebase lastUpdate = firebase.Child ("lastUpdate");

		lastUpdate.OnGetSuccess += GetTimeStamp;

		// Make observer on "last update" time stamp
		FirebaseObserver observer = new FirebaseObserver(lastUpdate, 1f);
		observer.OnChange += (Firebase sender, DataSnapshot snapshot)=>{
			DoDebug("[OBSERVER] Last updated changed to: " + snapshot.Value<long>());
		};
		observer.Start ();
		DoDebug ("[OBSERVER] FirebaseObserver on " + lastUpdate.FullKey +" started!");

		// Print details
		DoDebug("Firebase endpoint: " + firebase.Endpoint);
		DoDebug("Firebase key: " + firebase.Key);
		DoDebug("Firebase fullKey: " + firebase.FullKey);
		DoDebug("Firebase child key: " + temporary.Key);
		DoDebug("Firebase child fullKey: " + temporary.FullKey);

		// Unnecessarily skips a frame, really, unnecessary.
		yield return null;

		// Create a FirebaseQueue
		FirebaseQueue firebaseQueue = new FirebaseQueue();

		// Test #1: Test all firebase commands, using FirebaseQueueManager
		// The requests should be running in order 
		firebaseQueue.AddQueueSet (firebase, GetSampleScoreBoard (), FirebaseParam.Empty.PrintSilent ());
		firebaseQueue.AddQueuePush (firebase.Child ("broadcasts", true), "{ \"name\": \"simple-firebase-csharp\", \"message\": \"awesome!\"}", true);
		firebaseQueue.AddQueueSetTimeStamp (firebase, "lastUpdate");
		firebaseQueue.AddQueueGet (firebase, "print=pretty");
		firebaseQueue.AddQueueUpdate (firebase.Child ("layout", true), "{\"x\": 5.8, \"y\":-94}", true);
		firebaseQueue.AddQueueGet (firebase.Child ("layout", true));
		firebaseQueue.AddQueueGet (lastUpdate);

		//Deliberately make an error for an example
		DoDebug("[WARNING] There is one invalid request below which will gives error, only for example on error handling.");
		firebaseQueue.AddQueueGet (firebase, FirebaseParam.Empty.LimitToLast(-1));


		// (~~ -.-)~~
		DoDebug ("==== Wait for seconds 15f ======");
		yield return new WaitForSeconds (15f);
		DoDebug ("==== Wait over... ====");


		// Test #2: Calls without using FirebaseQueueManager
		// The requests could overtake each other (ran asynchronously)
		firebase.Child("broadcasts", true).Push("{ \"name\": \"dikra\", \"message\": \"hope it runs well...\"}", true);
		firebase.GetValue(FirebaseParam.Empty.OrderByKey().LimitToFirst(2));
		temporary.GetValue ();
		firebase.GetValue (FirebaseParam.Empty.OrderByKey().LimitToLast(2));
		temporary.GetValue ();
		firebase.Child ("scores", true).GetValue(FirebaseParam.Empty.OrderByChild ("rating").LimitToFirst(2));
		firebase.GetRules (GetRulesOKHandler, GetRulesFailHandler);

		// ~~(-.- ~~)
		yield return null;
		DoDebug ("==== Wait for seconds 15f ======");
		yield return new WaitForSeconds (15f);
		DoDebug ("==== Wait over... ====");


		// Test #3: Delete the frb_child and broadcasts
		firebaseQueue.AddQueueGet (firebase);
		firebaseQueue.AddQueueDelete(temporary);
		// please notice that the OnSuccess/OnFailed handler is not inherited since Child second parameter not set to true.
		DoDebug("'broadcasts' node is deleted silently.");
		firebaseQueue.AddQueueDelete (firebase.Child ("broadcasts")); 
		firebaseQueue.AddQueueGet (firebase);

		// ~~(-.-)~~
		yield return null;
		DoDebug ("==== Wait for seconds 15f ======");
		yield return new WaitForSeconds (15f);
		DoDebug ("==== Wait over... ===="); 
		observer.Stop ();
		DoDebug ("[OBSERVER] FirebaseObserver on " + lastUpdate.FullKey +" stopped!");
	}


	Dictionary<string, object> GetSampleScoreBoard()
	{
		Dictionary<string, object> scoreBoard = new Dictionary<string, object> ();
		Dictionary<string, object> scores = new Dictionary<string, object> ();
		Dictionary<string, object> p1 = new Dictionary<string, object> ();
		Dictionary<string, object> p2 = new Dictionary<string, object> ();
		Dictionary<string, object> p3 = new Dictionary<string, object> ();

		p1.Add ("name", "simple");
		p1.Add("score", 80);

		p2.Add ("name", "firebase");
		p2.Add ("score", 100);

		p3.Add ("name", "csharp");
		p3.Add ("score", 60);

		scores.Add ("p1", p1);
		scores.Add ("p2", p2);
		scores.Add ("p3", p3);

		scoreBoard.Add ("scores", scores);

		scoreBoard.Add("layout", Json.Deserialize("{\"x\": 0, \"y\":10}") as Dictionary<string, object>);
		scoreBoard.Add ("resizable", true);

		scoreBoard.Add("temporary" , "will be deleted later");

		return scoreBoard;
	}
}