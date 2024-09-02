using UnityEngine;
using System.Runtime.InteropServices;
using UnityEngine.Events;

public class ReactReceiver : MonoBehaviour
{
    [SerializeField]
    private UnityEvent<string> onReceivedFromReact;
    
    [DllImport("__Internal")]
    private static extern void DispatchReactUnityEvent();
    
    
    private void Start()
    {
    #if UNITY_WEBGL && !UNITY_EDITOR
        DispatchReactUnityEvent();
        WebGLInput.captureAllKeyboardInput = false;
    #endif
    }
    
    public void GetStringFromReact(string message)
    {
        onReceivedFromReact.Invoke(message);
    }
}
