# simple-firebase-unity
Firebase Realtime-Database's REST API Wrapper for Unity in C#

Unity Asset Store: http://u3d.as/pGA

REST API Reference: https://firebase.google.com/docs/database/rest/retrieve-data

GitHub: https://github.com/dkrprasetya/simple-firebase-unity

by M Dikra Prasetya

NOTE: This is not an official plugin from Firebase!

## Background

Our current official Firebase-Unity seems to be tailored for uses on Mobile Platform, but not for Desktop or any other platform.

In my opinion, using its REST API would be the simplest way to perform requests from Unity to Firebase's Realtime-Database. You could implement it with http request as usual, or use this implementation.

In 2016, I wasted my time to wrap almost everything you need for communicating with the REST API. Somehow this asset proves to be helpful as a lot of people uses this plugin. Two years later, I finally had the mood to got back in touch with this repository and updated it just a bit. Per May 2018, this asset still works just fine (thank god). 

Please note that I only check this repository once in a while. Contribution is always welcomed.

Hope this asset could help you save a lot of work.

## Sample Usage

#### Upgrading from v.1.0 to v.1.1
```
1. In the previous version, the plugin was provided in .dll format. Please delete the .dll file before importing.
2. Now the namespace is changed to "SimpleFirebaseUnity" to avoid clashing with the official Firebase SDK's naming.
```

#### Basic requests
As you could see on the example, the setup is quite simple. For the optional credential parameter, since Token Auth generation is not provided, I recommend you to use your Firebase Secret.

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
#### FirebaseQueue
If you want to make the requests runs in order, I have implemented FirebaseQueue helper class just for you :).

```
FirebaseQueue firebaseQueue = new FirebaseQueue();

// The requests should be running in order
firebaseQueue.AddQueuePush (firebase.Child ("broadcasts", true), "{ \"name\": \"simple-firebase-unity\", \"message\": \"awesome!\"}", true);
firebaseQueue.AddQueueSet (firebase, GetSampleScoreBoard (), FirebaseParam.Empty.PrintSilent ());
firebaseQueue.AddQueueSetTimeStamp (firebase, "lastUpdate");
firebaseQueue.AddQueueGet (firebase, "print=pretty");
firebaseQueue.AddQueueUpdate (firebase.Child ("layout", true), "{\"x\": 5.8, \"y\":-94}", true);
```

#### FirebaseParam
On queries, you can either use the provided FirebaseParam struct or insert your own string parameter.

When using string as the parameter I recommend you to use url-safe string (WWW.EscapeUrl() could help that). Not using url-safe could inflict bugs on requests, which one of the case is your request will be reported as "unsupported url" on iOS platform.

```
firebase.GetValue(FirebaseParam.Empty.OrderByKey().LimitToFirst(2));

// This equals to "orderBy=\"$key\"&limitToFirst=2"
firebase.GetValue("orderBy%3d%22%24key%22%26limitToFirst%3d2");

```
#### FirebaseObserver
As we don't have push notification on data changes (especially through REST API is kind of impossible), I also have implemented FirebaseObserver class which some of you have requested.

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
#### Notes on Authorization
For authorization, the easiest way that I could recommend would be to use Firebase's secret key (now deprecated). You could create your own token with Google's API or library then pass it through "auth" or "access_token" parameter. See https://firebase.google.com/docs/database/rest/auth for more detail.

#### Android Manifest
The included Android manifest is just for adding internet permission to your app just in case. You could delete that and add the permission to your own Android manifest.

## Releases

### v1.1.1
Disabled multi-threading on WebGL platform.

### v1.1
1. Repository is now on Unity project format, and the scripts are provided as it is inside plugin's folder.

2. Namespace is changed to "SimpleFirebaseUnity".

3. Json serialize/deserialize inside Firebase request's process will be handled in different thread, which will prevent blocking on Unity's main thread.

4. FirebaseQueue is now behaves properly when encountering an error. Pop will not occur until the current head is successfully processed (which previously on that case will just skip the error request instead).

5. Added some new parameters that are now included on Firebase's documentation.

6. Some minor refactorings are included.

### v1.0.0b
Major updates:

1. Request calls now behaves asynchronously (with coroutines).

2. FirebaseManager, FirebaseQueue, and FirebaseObserver are implemented.

3. Set request is now using PUT as a header (replaces the whole key).
The former is moved to Update request (PATCH as a header, will only update the mentioned keys).

4. Plugin is now UnityEngine dependant (the update includes replacement of HttpWebRequests to WWW).

5. Plugin is now working on Web platforms. Hopefully now working on all platforms.


### v0.2.0
Fixed a minor bug on the parameter builder, added handling for print=silent case (which returns status code 204, not the usual 200), and updated the FirebaseError class.

### v0.1.0
Simply wraps-up all of the basic methods of Firebase REST API which is documented in https://www.firebase.com/docs/rest/

## Notes
This plugin has been tested and works like a charm on Desktop (Windows & Mac), Web, iOS, and Android. Possibly it also does work well on other platforms since most of the implementation is only a simple http REST request.


## License
MIT

