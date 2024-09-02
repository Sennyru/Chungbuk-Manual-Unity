using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Frontend : MonoBehaviour
{
    [SerializeField]
    private GameObject jsonConsole;
    [SerializeField]
    private ImageLoader imageLoader;
    [SerializeField]
    private Frame frame;
    [SerializeField]
    private Text titleText;
    [SerializeField]
    private GameObject speechBubble;
    [SerializeField]
    private Text sayText;
    [SerializeField]
    private Review reviewPrefab;
    [SerializeField]
    private Transform reviewParent;
    [SerializeField]
    private Transition transition;
    [SerializeField]
    private Animator guide;
    
    [Header("Read Only")]
    [SerializeField]
    private FrontendData data;
    
    private RectTransform holdingReview;
    private Vector2 holdingReviewOffset;
    
    private bool isProcessingQuery; // 쿼리 다중 실행 방지
    private bool isDownloadEnded;
    
    
    private void Start()
    {
        // 빌드 시 숨기기
    #if UNITY_WEBGL && !UNITY_EDITOR
        jsonConsole.SetActive(false);
    #endif
    }
    
    /// <seealso cref="ReactReceiver.GetStringFromReact"/>
    public void GetJSONFromReactReceiver(string json)
    {
        try
        {
            data = JsonUtility.FromJson<FrontendData>(json);
        }
        catch (ArgumentException e)
        {
            Debug.LogWarning("JSON 처리에 실패했습니다!");
            Debug.LogWarning(e);
            return;
        }
        
        Debug.Log("JSON 데이터를 받았습니다.");
        Debug.Log(json);
        
        if (isProcessingQuery)
        {
            Debug.Log("쿼리가 아직 처리 중입니다.");
            return;
        }
        
        StartCoroutine(ProcessQuery());
    }
    
    private IEnumerator ProcessQuery()
    {
        isProcessingQuery = true;
        
        // 맵이 바뀌지 않았다면 말풍선만 바꾸기
        if (data.map_data == null || string.IsNullOrEmpty(data.map_data.name)) // map_data가 항상 not null인 것 같음
        {
            UpdateSpeechBubble(data.say);
            StartCoroutine(frame.FadeInOut()); // TODO 코루틴 중첩 수정하기
            Debug.Log("map_data가 없으므로 말풍선 내용을 변경했습니다.");
            
            isProcessingQuery = false;
            yield break;
        }
        
        // 이미지 미리 다운로드 돌려놓기
        StartCoroutine(DownloadImages());
        
        // 전환 효과가 끝나고 이미지가 전부 다운로드될 때까지 대기
        frame.StopSequence();
        yield return StartCoroutine(transition.WipeIn());
        yield return new WaitUntil(() => isDownloadEnded);
        isDownloadEnded = false;
        
        // 맵 업데이트 후 전환
        UpdateMap();
        yield return StartCoroutine(transition.WipeOut());
        
        isProcessingQuery = false;
    }
    
    /// <summary> 말풍선의 내용을 바꾼다. 만약 빈 문자열을 받았으면 말풍선을 숨긴다. </summary>
    private void UpdateSpeechBubble(string say)
    {
        sayText.text = say;
        var hasText = !string.IsNullOrEmpty(say);
        speechBubble.SetActive(hasText);
        if (hasText)
        {
            StartCoroutine(GuideReact());
        }
    }
    
    private IEnumerator GuideReact()
    {
        yield break; // 0
    }
    
    /// <summary> 이미지 링크가 하나 이상 있는 경우 이미지를 미리 다운로드해 놓는다. <br/>
    /// 그리고 <see cref="isDownloadEnded"/>를 <c>true</c>로 바꾼다. </summary>
    private IEnumerator DownloadImages()
    {
        if (data.map_data.image_links != null && data.map_data.image_links.Length > 0)
        {
            var task = imageLoader.CacheImages(data.map_data.image_links);
            yield return new WaitUntil(()=> task.IsCompleted);
        }
        isDownloadEnded = true;
    }
    
    /// <summary> 타이틀을 바꾸고, 이미지를 적용시키고, 말풍선 텍스트를 바꾸고, 리뷰를 만든다. </summary>
    private void UpdateMap()
    {
        Debug.Log("맵 업데이트 중...");
        var mapData = data.map_data;
        
        // 타이틀
        titleText.text = mapData.name;
        
        // 이미지
        if (mapData.image_links != null && mapData.image_links.Length > 0)
        {
            var textures = imageLoader.GetTexture2DArrayFromCache(mapData.image_links);
            frame.LoadFrame(textures);
        }
        
        // 말풍선
        UpdateSpeechBubble(data.say);
        
        // 리뷰
        foreach (Transform child in reviewParent)
        {
            Destroy(child.gameObject);
        }
        if (mapData.reviews != null && mapData.reviews.Length != 0)
        {
            for (var i = 0; i < mapData.reviews.Length; i++)
            {
                var reviewText = mapData.reviews[i];
                var review = Instantiate(reviewPrefab, reviewParent);
                review.name = $"Review[{i}]";
                review.Initialize(reviewText, OnMouseDownReview, OnMouseUpReview);
                
                // 랜덤한 좌표에 배치
                var rt = review.GetComponent<RectTransform>();
                rt.anchoredPosition += 30f * new Vector2(Random.Range(-15f, 15f), Random.Range(-12f, 12f));
            }
        }
        
        Debug.Log("맵 업데이트 완료");
    }
    
    /// <summary> <seealso cref="Review.OnPointerDown"/> </summary>
    private void OnMouseDownReview(Review review)
    {
        holdingReview = review.GetComponent<RectTransform>();
        holdingReviewOffset = (Vector2)holdingReview.position - Mouse.current.position.ReadValue();
        holdingReview.SetAsLastSibling(); // 맨 위로 올리기
    }
    
    /// <summary> <seealso cref="Review.OnPointerUp"/> </summary>
    private void OnMouseUpReview(Review review)
    {
        holdingReview = null;
        holdingReviewOffset = default;
    }
    
    
    private void Update()
    {
        // 리뷰 드래그
        if (holdingReview != null)
        {
            var mousePos = Mouse.current.position.ReadValue();
            
            // 화면 밖으로 나가지 않게
            const float margin = 10f;
            mousePos.x = Mathf.Clamp(mousePos.x, margin, Screen.width - margin);
            mousePos.y = Mathf.Clamp(mousePos.y, margin, Screen.height - margin);
            
            holdingReview.position = mousePos + holdingReviewOffset;
        }
        
        // (에디터 전용) 탭 키로 콘솔 열기
    #if UNITY_EDITOR
        if (Keyboard.current.tabKey.wasPressedThisFrame)
        {
            jsonConsole.SetActive(!jsonConsole.activeSelf);
        }
    #endif
    }
}
