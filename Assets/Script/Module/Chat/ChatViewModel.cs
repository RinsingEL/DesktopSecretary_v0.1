using Com.Module.Chat;
using Core.Framework.Config;
using Core.Framework.Event;
using Core.Framework.Network.ChatSystem;
using Core.Framework.Network;

public class ChatViewModel
{
    ChatData chatData = new ChatData();

    public ChatViewModel()
    {
        EventManager.Instance.AddEvent<string,string>(ClientEvent.ON_SEND_CHAT_REQUEST, OnSendChatMessage);
        EventManager.Instance.AddEvent<string , string, string>(ClientEvent.ON_SEND_FUNC_REQUEST, OnSendFunctionRequest);
    }

    public void OnSendChatMessage(string prompt, string msg)
    {
        var body = new ChatRequestClass.ChatReuestBody();
        body.model = ConfigManager.Instance.Network.Model;
        body.messages = new();
        body.messages.Add(new ChatRequestClass.ChatReuestBody.Message() { role = "system", content = prompt });
        body.messages.Add(new ChatRequestClass.ChatReuestBody.Message() { role = "user", content = msg });
        body.safe_mode = false;

        var sendMsgRequest = new ChatRequest();
        sendMsgRequest.Config.URL += "/v1/chat/completions";
        sendMsgRequest.Config.Headers["Authorization"] += $"Bearer {ConfigManager.Instance.Network.apiKey}";
        sendMsgRequest.RequestBody = body;

        NetworkManager.Instance.SendMessage(sendMsgRequest);
    }
    public void OnSendFunctionRequest(string prompt, string msg , string func)
    {
        var body = new ChatRequestClass.ChatReuestBody();
        body.model = ConfigManager.Instance.Network.Model;
        body.messages = new();
        body.messages.Add(new ChatRequestClass.ChatReuestBody.Message() { role = "system", content = prompt });
        body.messages.Add(new ChatRequestClass.ChatReuestBody.Message() { role = "user", content = msg });
        body.functions = new();
        body.functions.Add(new ChatRequestClass.SelectFunctionCalling());
        if (func != null)
        {
            body.function_call = new ChatRequestClass.ChatReuestBody.FunctionCall()
            {
                name = func
            };
        }
        else
            body.function_call = "auto";

        var sendMsgRequest = new ChatRequest();
        sendMsgRequest.Config.URL += "/v1/chat/completions";
        sendMsgRequest.Config.Headers["Authorization"] += $"Bearer {ConfigManager.Instance.Network.apiKey}";
        sendMsgRequest.RequestBody = body;

        NetworkManager.Instance.SendMessage(sendMsgRequest);
    }
    public void Cleanup()
    {
        EventManager.Instance.RemoveEvent<string ,string>(ClientEvent.ON_SEND_CHAT_REQUEST, OnSendChatMessage);
        EventManager.Instance.RemoveEvent<string ,string, string>(ClientEvent.ON_SEND_FUNC_REQUEST, OnSendFunctionRequest);
    }
}