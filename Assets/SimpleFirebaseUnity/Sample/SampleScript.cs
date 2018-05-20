// Last update: 2018-05-20  (by Dikra)

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
        StartCoroutine(Tests());
    }

    void GetOKHandler(Firebase sender, DataSnapshot snapshot)
    {
        DebugLog("[OK] Get from key: <" + sender.FullKey + ">");
        DebugLog("[OK] Raw Json: " + snapshot.RawJson);

        Dictionary<string, object> dict = snapshot.Value<Dictionary<string, object>>();
        List<string> keys = snapshot.Keys;

        if (keys != null)
            foreach (string key in keys)
            {
                DebugLog(key + " = " + dict[key].ToString());
            }
    }

    void GetFailHandler(Firebase sender, FirebaseError err)
    {
        DebugError("[ERR] Get from key: <" + sender.FullKey + ">,  " + err.Message + " (" + (int)err.Status + ")");
    }

    void SetOKHandler(Firebase sender, DataSnapshot snapshot)
    {
        DebugLog("[OK] Set from key: <" + sender.FullKey + ">");
    }

    void SetFailHandler(Firebase sender, FirebaseError err)
    {
        DebugError("[ERR] Set from key: <" + sender.FullKey + ">, " + err.Message + " (" + (int)err.Status + ")");
    }

    void UpdateOKHandler(Firebase sender, DataSnapshot snapshot)
    {
        DebugLog("[OK] Update from key: <" + sender.FullKey + ">");
    }

    void UpdateFailHandler(Firebase sender, FirebaseError err)
    {
        DebugError("[ERR] Update from key: <" + sender.FullKey + ">, " + err.Message + " (" + (int)err.Status + ")");
    }

    void DelOKHandler(Firebase sender, DataSnapshot snapshot)
    {
        DebugLog("[OK] Del from key: <" + sender.FullKey + ">");
    }

    void DelFailHandler(Firebase sender, FirebaseError err)
    {
        DebugError("[ERR] Del from key: <" + sender.FullKey + ">, " + err.Message + " (" + (int)err.Status + ")");
    }

    void PushOKHandler(Firebase sender, DataSnapshot snapshot)
    {
        DebugLog("[OK] Push from key: <" + sender.FullKey + ">");
    }

    void PushFailHandler(Firebase sender, FirebaseError err)
    {
        DebugError("[ERR] Push from key: <" + sender.FullKey + ">, " + err.Message + " (" + (int)err.Status + ")");
    }

    void GetRulesOKHandler(Firebase sender, DataSnapshot snapshot)
    {
        DebugLog("[OK] GetRules");
        DebugLog("[OK] Raw Json: " + snapshot.RawJson);
    }

    void GetRulesFailHandler(Firebase sender, FirebaseError err)
    {
        DebugError("[ERR] GetRules,  " + err.Message + " (" + (int)err.Status + ")");
    }

    void GetTimeStamp(Firebase sender, DataSnapshot snapshot)
    {
        long timeStamp = snapshot.Value<long>();
        DateTime dateTime = Firebase.TimeStampToDateTime(timeStamp);

        DebugLog("[OK] Get on timestamp key: <" + sender.FullKey + ">");
        DebugLog("Date: " + timeStamp + " --> " + dateTime.ToString());
    }

    void DebugLog(string str)
    {
        Debug.Log(str);
        if (textMesh != null)
        {
            textMesh.text += (++debug_idx + ". " + str) + "\n";
        }
    }

    void DebugWarning(string str)
    {
        Debug.LogWarning(str);
        if (textMesh != null)
        {
            textMesh.text += (++debug_idx + ". " + str) + "\n";
        }
    }

    void DebugError(string str)
    {
        Debug.LogError(str);
        if (textMesh != null)
        {
            textMesh.text += (++debug_idx + ". " + str) + "\n";
        }
    }

    IEnumerator Tests()
    {
        // README
        DebugLog("This plugin simply wraps Firebase's RealTime Database REST API.\nPlease read here for better understanding of the API: https://firebase.google.com/docs/reference/rest/database/\n");
              
        // Inits Firebase using Firebase Secret Key as Auth
        // The current provided implementation not yet including Auth Token Generation
        // If you're using this sample Firebase End, 
        // there's a possibility that your request conflicts with other simple-firebase-c# user's request
        Firebase firebase = Firebase.CreateNew("https://simple-firebase-unity.firebaseio.com", "WQV9t78OywD8Pp7jvGuAi8K6g0MV8p9FAzkJ7rWK");

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
        Firebase temporary = firebase.Child("temporary", true);
        Firebase lastUpdate = firebase.Child("lastUpdate");

        lastUpdate.OnGetSuccess += GetTimeStamp;

        // Make observer on "last update" time stamp
        FirebaseObserver observer = new FirebaseObserver(lastUpdate, 1f);
        observer.OnChange += (Firebase sender, DataSnapshot snapshot) =>
        {
            DebugLog("[OBSERVER] Last updated changed to: " + snapshot.Value<long>());
        };
        observer.Start();
        DebugLog("[OBSERVER] FirebaseObserver on " + lastUpdate.FullKey + " started!");

        // Print details
        DebugLog("Firebase endpoint: " + firebase.Endpoint);
        DebugLog("Firebase key: " + firebase.Key);
        DebugLog("Firebase fullKey: " + firebase.FullKey);
        DebugLog("Firebase child key: " + temporary.Key);
        DebugLog("Firebase child fullKey: " + temporary.FullKey);

        // Unnecessarily skips a frame, really, unnecessary.
        yield return null;

        // Create a FirebaseQueue
        FirebaseQueue firebaseQueue = new FirebaseQueue(true, 3, 1f); // if _skipOnRequestError is set to false, queue will stuck on request Get.LimitToLast(-1).
                                                                    

        // Test #1: Test all firebase commands, using FirebaseQueueManager
        // The requests should be running in order 
        firebaseQueue.AddQueueSet(firebase, GetSampleScoreBoard(), FirebaseParam.Empty.PrintSilent());
        firebaseQueue.AddQueuePush(firebase.Child("broadcasts", true), "{ \"name\": \"simple-firebase-csharp\", \"message\": \"awesome!\"}", true);
        firebaseQueue.AddQueueSetTimeStamp(firebase, "lastUpdate");
        firebaseQueue.AddQueueGet(firebase, "print=pretty");
        firebaseQueue.AddQueueUpdate(firebase.Child("layout", true), "{\"x\": 5.8, \"y\":-94}");
        firebaseQueue.AddQueueGet(firebase.Child("layout", true));
        firebaseQueue.AddQueueGet(lastUpdate);

        //Deliberately make an error for an example
        DebugWarning("[WARNING] There is one invalid request below (Get with invalid OrderBy) which will gives error, only for the sake of example on error handling.");
        firebaseQueue.AddQueueGet(firebase, FirebaseParam.Empty.LimitToLast(-1));

        // (~~ -.-)~~
        DebugLog("==== Wait for seconds 15f ======");
        yield return new WaitForSeconds(15f);
        DebugLog("==== Wait over... ====");


        // Test #2: Calls without using FirebaseQueueManager
        // The requests could overtake each other (ran asynchronously)
        firebase.Child("broadcasts", true).Push("{ \"name\": \"dikra\", \"message\": \"hope it runs well...\"}", false);
        firebase.GetValue(FirebaseParam.Empty.OrderByKey().LimitToFirst(2));
        temporary.GetValue();
        firebase.GetValue(FirebaseParam.Empty.OrderByKey().LimitToLast(2));
        temporary.GetValue();

        // Please note that orderBy "rating" is possible because I already defined the index on the Rule.
        // If you use your own endpoint, you might get an error if you haven't set it on your Rule.
        firebase.Child("scores", true).GetValue(FirebaseParam.Empty.OrderByChild("rating").LimitToFirst(2));
        firebase.GetRules(GetRulesOKHandler, GetRulesFailHandler);

        // ~~(-.- ~~)
        yield return null;
        DebugLog("==== Wait for seconds 15f ======");
        yield return new WaitForSeconds(15f);
        DebugLog("==== Wait over... ====");

        // We need to clear the queue as the queue is left with one error command (the one we deliberately inserted last time).
        // When the queue stuck with an error command at its head, the next (or the newly added command) will never be processed.
        firebaseQueue.ForceClearQueue();
        yield return null;      

        // Test #3: Delete the frb_child and broadcasts
        firebaseQueue.AddQueueGet(firebase);
        firebaseQueue.AddQueueDelete(temporary);

        // Please notice that the OnSuccess/OnFailed handler is not inherited since Child second parameter not set to true.
        DebugLog("'broadcasts' node is deleted silently.");
        firebaseQueue.AddQueueDelete(firebase.Child("broadcasts"));
        firebaseQueue.AddQueueGet(firebase);

        // ~~(-.-)~~
        yield return null;
        DebugLog("==== Wait for seconds 15f ======");
        yield return new WaitForSeconds(15f);
        DebugLog("==== Wait over... ====");
        observer.Stop();
        DebugLog("[OBSERVER] FirebaseObserver on " + lastUpdate.FullKey + " stopped!");
    }


    Dictionary<string, object> GetSampleScoreBoard()
    {
        Dictionary<string, object> scoreBoard = new Dictionary<string, object>();
        Dictionary<string, object> scores = new Dictionary<string, object>();
        Dictionary<string, object> p1 = new Dictionary<string, object>();
        Dictionary<string, object> p2 = new Dictionary<string, object>();
        Dictionary<string, object> p3 = new Dictionary<string, object>();

        p1.Add("name", "simple");
        p1.Add("score", 80);

        p2.Add("name", "firebase");
        p2.Add("score", 100);

        p3.Add("name", "csharp");
        p3.Add("score", 60);

        scores.Add("p1", p1);
        scores.Add("p2", p2);
        scores.Add("p3", p3);

        scoreBoard.Add("scores", scores);

        scoreBoard.Add("layout", Json.Deserialize("{\"x\": 0, \"y\":10}") as Dictionary<string, object>);
        scoreBoard.Add("resizable", true);

        scoreBoard.Add("temporary", "will be deleted later");

        return scoreBoard;
    }
}