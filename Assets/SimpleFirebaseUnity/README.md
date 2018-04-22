# simple-firebase-unity
Firebase REST API Wrapper for Unity in C#

Unity Asset Store: http://u3d.as/pGA

REST API Reference: https://firebase.google.com/docs/database/rest/retrieve-data

GitHub: https://github.com/dkrprasetya/simple-firebase-unity

Copyright (c) 2016  M Dikra Prasetya

NOTE: This is not an official plugin from Firebase!

## Background

Our current official Firebase-Unity has not yet covered platforms other than Editor, Android, and iOS. 

If you only need to call simple methods, you could simply implement all of it with REST calls in Unity by yourself, or, you could use my implementation!

I thought it maybe helpful to many of you guys out there. So, enjoy this plugin, and if possible, contribute to make this plugin complete!

## Sample Usage

####Every available method is equipped with summary and parameter descriptions that are written as clear as possible. You could explore available methods through your favorite editor. Ask me if you have any questions!


As you could see on the example, the setup is very simple. For the optional credential parameter, since Token Auth generation is not provided, I recommend you to use your Firebase Secret.

Please note that FirebaseManager singleton object with "DontDestroyOnLoad" will be generated to your scene automatically by script.

Every request will be run as a coroutine, so it should behaves "asynchronously" (note that coroutines are not purely async).

Mapping of implemented basic methods with the Http Request Header it uses:

■ GetValue() --> GET header

■ SetValue() --> PUT header

■ UpdateValue() --> PATCH header

■ PushValue() --> POST header

■ DeleteValue() --> DELETE header



```
firebase = Firebase.CreateNew("samplefirebaseunity.firebaseio.com");

firebase.Child("Messages").Push("{ \"name\": \"simple-firebase-unity\", \"message\": \"awesome!\"}", true);

firebase.OnDeleteSuccess += (Firebase sender, DataSnapshot snapshot)=>{
    Debug.Log("[OK] Delete from " + sender.Endpoint + ": " + snapshot.RawJson);
};

firebase.OnUpdateFailed += UpdateFailedHandler; 
// Method signature: void UpdateFailedHandler(Firebase sender, FirebaseError err)

firebase.GetValue("print=pretty");
```

If you want to make the requests runs in order, I have implemented FirebaseQueue for you!

```
FirebaseQueue firebaseQueue = new FirebaseQueue();

// The requests should be running in order 
firebaseQueue.AddQueuePush (firebase.Child ("broadcasts", true), "{ \"name\": \"simple-firebase-unity\", \"message\": \"awesome!\"}", true);
firebaseQueue.AddQueueSet (firebase, GetSampleScoreBoard (), FirebaseParam.Empty.PrintSilent ());
firebaseQueue.AddQueueSetTimeStamp (firebase, "lastUpdate");
firebaseQueue.AddQueueGet (firebase, "print=pretty");
firebaseQueue.AddQueueUpdate (firebase.Child ("layout", true), "{\"x\": 5.8, \"y\":-94}", true);
```


On queries, you can either use the provided FirebaseParam struct or insert your own string parameter. 

When using string as the parameter I recommend you to use url-safe string (WWW.EscapeUrl() could help that). Not using url-safe could inflict bugs on requests, which one of the case is your request will be reported as "unsupported url" on iOS platform.

```
firebase.GetValue(FirebaseParam.Empty.OrderByKey().LimitToFirst(2));

// This equals to "orderBy=\"$key\"&limitToFirst=2"
firebase.GetValue("orderBy%3d%22%24key%22%26limitToFirst%3d2");

```

As we don't have push notification on data changes (especially through REST API is kind of impossible), I also have implemented FirebaseObserver class which many of you requested!

It is actually a simple Get request that is called periodically, which if used wisely will gives you a "not-efficient-but-good-enough" workaround to observe realtime changes on your Firebase. 

I recommend you to observe only on the necessary key with compact value (for example, time stamps) to avoid the observer from slowing down your application or eating out your bandwidth. Use the provided Start() and Stop() method to control your Observer.

```
FirebaseObserver observer = new FirebaseObserver (firebase.Child("timestamp"), 5f);

observer.OnChange += ChangeHandler; 
// Method signature: void ChangeHandler(Firebase sender, DataSnapshot snapshot) 

observer.Start (); // Starts the observer coroutine
observer.Stop (); // Stops the observer coroutine
observer.Start (); // Starts again the observer coroutine 
observer.Stop (); // Stops the observer coroutine
```



## Releases

###v1.0.0b
Major updates: 

1. Request calls now behaves asynchronously (with coroutines).

2. FirebaseManager, FirebaseQueue, and FirebaseObserver are implemented.

3. Set request is now using PUT as a header (replaces the whole key). 
The former is moved to Update request (PATCH as a header, will only update the mentioned keys).

4. Plugin is now UnityEngine dependant (the update includes replacement of HttpWebRequests to WWW).

5. Plugin is now working on Web platforms. Hopefully now working on all platforms.


###v0.2.0
Fixed a minor bug on the parameter builder, added handling for print=silent case (which returns status code 204, not the usual 200), and updated the FirebaseError class.

###v0.1.0
Simply wraps-up all of the basic methods of Firebase REST API which is documented in https://www.firebase.com/docs/rest/

## Notes
1. This plugin has been tested and works like a charm on Desktop (Windows & Mac), Web, iOS, and Android. Possibly it also does work well on other platforms.
2. Token auth nor JWT generation are not yet supported. 


## License
MIT

