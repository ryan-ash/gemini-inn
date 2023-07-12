using UnityEngine;
using System;
using System.Collections;

public class Wait : MonoBehaviour {

    public struct WaitStruct 
    {
        public float seconds;
        public Action action;
    }

    public static Wait Run( float seconds, Action a )
    {
        GameObject go = new GameObject ( "Wait: " + seconds );
        Wait wait = go.AddComponent<Wait>();
        wait.Exec( 
            new WaitStruct { 
                seconds = seconds, 
                action = a 
            } 
        );
        return wait;
    }

    public void Stop() 
    {
        Destroy( gameObject );
    }

    public void Exec( WaitStruct obj ) 
    {
       StartCoroutine ( "RunAndWait", obj );
    }

    IEnumerator RunAndWait( WaitStruct obj ) 
    {
        yield return new WaitForSeconds( obj.seconds );
        obj.action();
        Destroy ( gameObject );
    }

}

