using System.Collections;
using UnityEngine;

public class Transition : MonoBehaviour
{
    [SerializeField]
    private float frameDuration = 0.2f;
    [SerializeField, Tooltip("페이드 인이 끝난 상태에서 페이드 아웃을 시작하기까지의 대기 시간")]
    private float transitionDelay = 0.5f;


    private void Start()
    {
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }
    }

    /// <summary> 화면을 왼쪽에서 오른쪽으로 가린다. (코루틴) </summary>
    public IEnumerator WipeIn()
    {
        var wait = new WaitForSeconds(frameDuration);
        for (var i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(true);
            yield return wait;
        }
        yield return new WaitForSeconds(transitionDelay);
    }
    
    /// <summary> 화면을 오른쪽에서 왼쪽으로 보여준다. (코루틴) </summary>
    public IEnumerator WipeOut()
    {
        var wait = new WaitForSeconds(frameDuration);
        for (var i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(false);
            yield return wait;
        }
    }
}
