# simple-firebase-csharp
Unity-friendly Lightweighted Firebase REST API Wrapper in C#

Unity Asset Store: http://u3d.as/pGA

REST API Reference: https://firebase.google.com/docs/database/rest/retrieve-data

Copyright (c) 2016  M Dikra Prasetya

## Background

Our current official Firebase-Unity has not yet covered platforms other than Editor, Android, and iOS. 

If you only need to call simple methods for accessing Realtime Database, you could simply implement all of it with REST calls in Unity by yourself, or, you could implement it simply based on this!

To test these codes, you could download the Unity project from Asset Store, delete the .dll on the Plugin folder, then copy all of them to the Plugin folder.

I thought it maybe helpful to many of you guys out there. So, enjoy this plugin, and if possible, contribute to make this plugin complete!



## Sample Usage

I tried to make its behaviour as alike as possible with the official plugins. In Unity, wrap your firebase methods with IEnumerator coroutine manually or use Firebase.ToCoroutine() to make it behaves Asynchronously. If not, your script should be waiting until the method's completed.

```
firebase = Firebase.CreateNew("samplefirebasecsharp.firebaseio.com");

firebase.Child("Messages").Push("{ \"name\": \"simple-firebase-csharp\", \"message\": \"awesome!\"}", true);

firebase.OnDeleteSuccess += (Firebase sender, DataSnapshot snapshot)=>{
	Debug.Log("[OK] Delete from " + sender.Endpoint + ": " + snapshot.RawJson);
};

firebase.OnUpdateFailed += UpdateFailedHandler; 
// Method signature: void UpdateFailedHandler(Firebase sender, FirebaseError err)

StartCoroutine(Firebase.ToCoroutine(firebase.GetValue, "print=silent"));
```

On queries, you can either use the provided FirebaseParam struct or insert your own string parameter.

```
StartCoroutine(Firebase.ToCoroutine(firebase.GetValue, FirebaseParam.Empty.OrderByValue().StartAt(50).PrintPretty()));
StartCoroutine(Firebase.ToCoroutine(firebase.GetValue, "orderBy=\"$value\"&startAt=50&print=pretty"));
```


## Releases

###v0.2
Fixed a minor bug on the parameter builder, added handling for print=silent case (which returns status code 204, not the usual 200), and updated the FirebaseError class.

###v0.1
Simply wraps-up all of the basic methods of Firebase REST API which is documented in https://www.firebase.com/docs/rest

## Notes for Unity Plugin
1. This plugin has been tested and works like a charm on Desktop (Windows & Mac), iOS, and Android. (probably works on Linux)
2. The current version does not yet support Web platforms.
3. Auth token generation is not supported. Either use your Database secret key or somehow generate it by yourself (and kindly share it to us how you do it, please!).
4. Made pure with .NET, not UnityEngine library dependant.
5. FAQ: When publishing on Android, don't forget to set [Player Settings > Configuration >Internet Access] to Require.


## License
MIT
