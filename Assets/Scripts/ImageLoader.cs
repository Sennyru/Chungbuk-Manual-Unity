using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class ImageLoader : MonoBehaviour
{
    private readonly Dictionary<string, Texture2D> textureCache = new();
    
    
    /// <summary> 웹 링크를 통해 이미지를 다운로드한다. (비동기) </summary>
    /// <returns> <see cref="Texture2D"/> 객체, 만약 다운로드에 실패하면 null </returns>
    private async Task<Texture2D> GetTextureFromWeb(string url) 
    {
        if (string.IsNullOrEmpty(url))
        {
            Debug.LogWarning("URL이 null이거나 비어 있습니다!");
            return null;
        }
        
        // 이미 다운로드했다면 가져오기
        if (textureCache.ContainsKey(url) && textureCache[url] != null)
        {
            Debug.Log($"이미지가 캐시되어 있습니다. ({url})");
            return textureCache[url];
        }
        
        // 웹에서 이미지 다운로드
        Debug.Log($"이미지 다운로드 중... ({url})");
        using var www = UnityWebRequestTexture.GetTexture(url);
        UnityWebRequestAsyncOperation req;
        try
        {
            req = www.SendWebRequest();
            while (!req.isDone)
            {
                await Task.Yield();
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"이미지 다운로드에 실패했습니다! (try-catch에 걸림, CORS 오류일 수 있음) ({url})");
            Debug.LogWarning(e);
            return null;
        }
        
        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogWarning($"이미지 다운로드에 실패했습니다! 9결과가 Success가 아님) ({url})");
            Debug.LogWarning(www.error);
            return null;
        }
        
        var texture = ((DownloadHandlerTexture)www.downloadHandler).texture;
        if (texture == null)
        {
            Debug.LogWarning($"다운로드한 이미지가 null입니다! ({url})");
            return null;
        }
        
        Debug.Log($"이미지를 다운로드했습니다. ({url})");
        return texture;
    }
    
    /// <summary> 다음 맵에 보일 이미지들을 미리 다운로드해 둔다. (비동기) </summary>
    /// <seealso cref="GetTextureFromWeb"/>
    public async Task CacheImages(params string[] imgLinks)
    {
        Debug.Log($"다운로드할 이미지 목록: {string.Join(", ", imgLinks)}");
        
        for (var i = 0; i < imgLinks.Length; i++)
        {
            Debug.Log($"{i}번 이미지 다운로드 시작");
            
            var imgLink = imgLinks[i];
            if (textureCache.ContainsKey(imgLink))
            {
                Debug.Log("이미 다운로드했던 이미지입니다.");
                continue;
            }
            
            Texture2D texture;
            try
            {
                texture = await GetTextureFromWeb(imgLink);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"이미지 다운로드에 실패했습니다! (예상치 못한 예외 발생) {imgLink}");
                Debug.LogWarning(e);
                texture = null;
            }
            
            if (texture == null)
            {
                Debug.LogWarning("이미지 다운로드에 실패하여 캐싱에서 제외합니다.");
                continue;
            }
            
            textureCache[imgLink] = texture;
            Debug.Log($"{i}번 이미지 다운로드 완료");
        }
        
        Debug.Log("모든 이미지 다운로드 완료");
    }
    
    /// <summary> <see cref="textureCache"/>에 저장되어 있는 텍스처를 가져온다. </summary>
    /// <seealso cref="Frontend.UpdateMap"/>
    /// <seealso cref="Frame.LoadFrame"/>
    public List<Texture2D> GetTexture2DArrayFromCache(params string[] imgLinks)
    {
        var textures = new List<Texture2D>();
        foreach (var imgLink in imgLinks)
        {
            if (textureCache.TryGetValue(imgLink, out var texture))
            {
                textures.Add(texture);
            }
        }
        return textures;
    }
}
