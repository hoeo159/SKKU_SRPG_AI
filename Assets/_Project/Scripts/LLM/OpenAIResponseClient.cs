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
        string apiKey = Environment.GetEnvironmentVariable(envKeyName);
        if (string.IsNullOrEmpty(apiKey))
            apiKey = fallbackApiKey;
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
