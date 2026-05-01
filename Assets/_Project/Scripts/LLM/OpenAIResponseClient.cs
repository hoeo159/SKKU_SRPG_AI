using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Text;

public class OpenAIResponseClient : MonoBehaviour
{
    [Header("OpenAI API setting")]
    [SerializeField] private string     model = "gpt-4o-mini";
    // model = "gpt-5-nano"; // for test, but it may not be released yet.
    [SerializeField] private float      temperature = 0.7f;
    [SerializeField] private bool       store = false;
    [SerializeField] private int        maxOutputTokens = 400;

    [SerializeField] private string     envKeyName = "OPENAI_UNITY_KEY";
    //[SerializeField] private string     envKeyName = null;
    [SerializeField] private string     fallbackApiKey = ""; // 데모용 임시 키

    private const string URL = "https://api.openai.com/v1/responses";
    private const string PrefKey = "OPENAI_API_KEY";

#if UNITY_EDITOR
    [ContextMenu("Clear Saved API Key")]
    public void DebugClearKey()
    {
        ClearAndSaveKey();
    }
#endif

    public string GetAPIKey()
    {
        string key = PlayerPrefs.GetString(PrefKey, null);
        if(!string.IsNullOrEmpty(key)) return key;

        if(!string.IsNullOrEmpty(envKeyName))
        {
            Debug.Log($"Trying to get API key from env var : {envKeyName}");
            key = Environment.GetEnvironmentVariable(envKeyName);
            if(!string.IsNullOrEmpty(key)) return key;
        }

        Debug.LogWarning($"API key not found. Please set it in PlayerPrefs with key '{PrefKey}' or env var '{envKeyName}'.");
        return fallbackApiKey;
    }

    public void ClearAndSaveKey()
    {
        PlayerPrefs.DeleteKey(PrefKey);
        PlayerPrefs.Save();
    }

    public bool isAPIKey() => !string.IsNullOrEmpty(GetAPIKey());

    public void SaveAPIKey(string key)
    {
        if(string.IsNullOrEmpty(key)) return;
        PlayerPrefs.SetString(PrefKey, key);
        PlayerPrefs.Save();
    }

    [Serializable] class ResponseRequest
    {
        public string model;
        public InputMsg[] input;
        public float temperature;
        public bool store;
        public TextConfig text;
        public int max_output_tokens;
    }

    [Serializable] class InputMsg
    {
        public string role;
        public string content;
    }

    [Serializable] class TextConfig
    {
        public Format format;
    }

    [Serializable] class Format
    {
        public string type;
    }

    [Serializable] class  Response
    {
        public OutputItem[] output; 
    }

    [Serializable] class OutputItem
    {
        public string type;
        public string role;
        public OutputContent[] content;
    }

    [Serializable] class OutputContent
    {
        public string type;
        public string text;
    }

    public IEnumerator RequestJson(
        string systemPrompt,
        string developerPrompt,
        string userText,
        Action<string> onJsonText,
        Action<string> onError)
    {
        string apiKey = GetAPIKey();

        if (string.IsNullOrEmpty(apiKey))
        {
            onError?.Invoke($"API key env var : {envKeyName}");
            yield break;
        }

        var req = new ResponseRequest
        {
            model = model,
            input = new[]
            {
                new InputMsg{role = "system", content = systemPrompt},
                new InputMsg{role = "developer", content = developerPrompt},
                new InputMsg{role = "user", content = userText}
            },
            temperature = temperature,
            store = store,
            max_output_tokens = maxOutputTokens,
            text = new TextConfig
            {
                format = new Format { type = "json_object" }
            }
        };

        string body = JsonUtility.ToJson(req);
        using var www = new UnityWebRequest(URL, "POST");
        www.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body));
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SetRequestHeader("Content-type", "application/json");
        www.SetRequestHeader("Authorization", "Bearer " + apiKey);

        yield return www.SendWebRequest();

        if(www.result != UnityWebRequest.Result.Success)
        {
            if (www.responseCode == 401 || www.responseCode == 403)
            {
                Debug.LogWarning("[OpenAI] Auth failed. Clearing saved key.");
                ClearAndSaveKey();
                onError?.Invoke("INVALID_API_KEY");
                yield break;
            }


            onError?.Invoke($"OpenAI error : {www.error}");
            Debug.LogWarning($"[OpenAI] HTTP {www.responseCode} err={www.error}\n" +
                 $"Retry-After={www.GetResponseHeader("Retry-After")}\n" +
                 $"{www.downloadHandler.text}");
            yield break;
        }

        string raw = www.downloadHandler.text;
        var resp = JsonUtility.FromJson<Response>(raw);
        string outputText = ExtractOutputToText(resp);

        if(string.IsNullOrEmpty(outputText))
        {
            onError?.Invoke("OpenAI error : No output\n");
            yield break;
        }

        onJsonText?.Invoke(outputText);
    }

    public IEnumerator ValidateApiKey(string apiKey, Action<bool, string> onResult)
    {
        if (string.IsNullOrEmpty(apiKey))
        {
            onResult?.Invoke(false, "API 키가 비어있습니다.");
            yield break;
        }

        using var www = UnityWebRequest.Get("https://api.openai.com/v1/models");
        www.SetRequestHeader("Authorization", "Bearer " + apiKey);

        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            onResult?.Invoke(true, null);
        }
        else
        {
            string msg = www.responseCode switch
            {
                401 => "유효하지 않은 API 키입니다.",
                403 => "접근 권한이 없는 키입니다.",
                429 => "API 사용량 한도를 초과했거나 결제 문제가 있습니다.",
                0 => "네트워크 연결에 실패했습니다.",
                _ => $"검증 실패 (HTTP {www.responseCode})"
            };
            onResult?.Invoke(false, msg);
        }
    }

    private string ExtractOutputToText(Response resp)
    {
        if (resp == null || resp.output == null) return null;

        foreach(var output in resp.output)
        {
            if (output == null) continue;
            if (output.type != "message") continue;
            if (output.content == null) continue;

            foreach(var content in output.content)
            {
                if (content == null) continue;
                if (content.type == "output_text") return content.text;
            }
        }
        return null;
    }
}
