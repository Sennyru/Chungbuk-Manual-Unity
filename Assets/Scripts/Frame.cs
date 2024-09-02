using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Frame : MonoBehaviour
{
    [SerializeField]
    private float fadeDuration = 1f;
    
    [Header("Components")]
    [SerializeField]
    private RawImage frameImage1;
    [SerializeField]
    private RawImage frameImage2;
    [SerializeField]
    private Texture2D defaultTexture;
    
    private List<Texture2D> textures;
    private int currentTextureIdx;
    private bool isImage1Active = true;
    private Coroutine seqCoro;
    
    
    /// <summary> 텍스처를 불러와서 프레임에 넣는다. </summary>
    /// <seealso cref="Frontend.UpdateMap"/>
    /// <seealso cref="ImageLoader.GetTexture2DArrayFromCache"/>
    public void LoadFrame(List<Texture2D> textures)
    {
        textures ??= new();
        
        // 로드된 이미지가 없으면 기본 이미지 넣기
        if (textures.Count == 0)
        {
            textures.Add(defaultTexture);
            Debug.Log("로드된 이미지가 없어 기본 이미지를 추가했습니다.");
        }
        
        this.textures = textures;
        
        StopSequence();
        currentTextureIdx = 0;
        if (textures.Count > 0) frameImage1.texture = textures[0];
        frameImage1.color = Color.white;
        frameImage2.color = new(1f, 1f, 1f, 0f);
        isImage1Active = true;
        
        // 이미지가 2개 이상일 경우 전환 애니메이션 시작
        // if (textures.Count <= 1) return;
        // seqCoro = StartCoroutine(PlaySequence());
    }
    
    /// <summary> 이미지 교차 표시를 멈춘다. </summary>
    /// <seealso cref="PlaySequence"/>
    public void StopSequence()
    {
        if (seqCoro != null)
        {
            StopCoroutine(seqCoro);
        }
    }
    
    /// <summary> 액자에 이미지를 교차해서 보여주기 (코루틴) <br/>
    /// 이미지가 2개 이상일 경우에만 동작해야 함 </summary>
    /// <seealso cref="LoadFrame"/>
    // private IEnumerator PlaySequence() // 임시 비활성화
    // {
    //     while (true)
    //     {
    //         yield return new WaitForSeconds(displayDuration);
    //         yield return StartCoroutine(FadeInOut());
    //     }
    // }
    
    /// <summary> 다음 이미지로 전환 (코루틴)
    /// 이미지가 2개 이상일 경우에만 동작해야 함 </summary>
    public IEnumerator FadeInOut()
    {
        if (textures.Count <= 1)
        {
            Debug.Log("이미지가 2개 미만이므로 액자 이미지를 전환하지 않습니다.");
            yield break;
        }
        
        // 다음 텍스처를 비활성화된 이미지에 설정
        var activeImage = isImage1Active ? frameImage1 : frameImage2;
        var inactiveImage = isImage1Active ? frameImage2 : frameImage1;
        
        var nextTextureIndex = (currentTextureIdx + 1) % textures.Count;
        inactiveImage.texture = textures[nextTextureIndex];
        
        // 페이드 인/아웃을 동시에 진행
        var t = 0f;
        while (t <= fadeDuration)
        {
            var alpha = t / fadeDuration;
            inactiveImage.color = new(1f, 1f, 1f, alpha);
            activeImage.color = new(1f, 1f, 1f, 1f - alpha);
            
            t += Time.deltaTime;
            yield return null;
        }
        
        inactiveImage.color = Color.white;
        activeImage.color = new(1f, 1f, 1f, 0f);
        
        currentTextureIdx = nextTextureIndex;
        isImage1Active = !isImage1Active;
    }
}
