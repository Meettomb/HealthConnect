using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

[Route("api/chat")]
[ApiController]
public class ChatController : ControllerBase
{
    private static readonly string filePath = "wwwroot/chatMessages.json";

    [HttpPost("save")]
    public IActionResult SaveMessage([FromBody] ChatMessage message)
    {
        var messages = new List<ChatMessage>();

        if (System.IO.File.Exists(filePath))
        {
            string existingData = System.IO.File.ReadAllText(filePath);
            messages = JsonSerializer.Deserialize<List<ChatMessage>>(existingData) ?? new List<ChatMessage>();
        }

        messages.Add(message);
        System.IO.File.WriteAllText(filePath, JsonSerializer.Serialize(messages));

        return Ok();
    }

    [HttpGet("load")]
    public IActionResult LoadMessages([FromQuery] string senderId, [FromQuery] string receiverId)
    {
        if (!System.IO.File.Exists(filePath))
            return Ok(new List<ChatMessage>());

        string existingData = System.IO.File.ReadAllText(filePath);
        var messages = JsonSerializer.Deserialize<List<ChatMessage>>(existingData) ?? new List<ChatMessage>();

        var filteredMessages = messages
            .Where(msg => (msg.SenderId == senderId && msg.ReceiverId == receiverId) ||
                          (msg.SenderId == receiverId && msg.ReceiverId == senderId))
            .ToList();

        return Ok(filteredMessages);
    }

    [HttpPost("clear")]
    public IActionResult ClearChatHistory([FromBody] ClearChatRequest request)
    {
        if (!System.IO.File.Exists(filePath))
            return Ok(new { message = "No chat history found." });

        string existingData = System.IO.File.ReadAllText(filePath);
        var messages = JsonSerializer.Deserialize<List<ChatMessage>>(existingData) ?? new List<ChatMessage>();

        var filteredMessages = messages.Where(msg => !(msg.SenderId == request.SenderId && msg.ReceiverId == request.ReceiverId)).ToList();

        System.IO.File.WriteAllText(filePath, JsonSerializer.Serialize(filteredMessages));

        return Ok(new { message = "Chat history cleared successfully." });
    }

   
}
public class ClearChatRequest
{
    public string SenderId { get; set; }
    public string ReceiverId { get; set; }
}


public class ChatMessage
{
    public string SenderId { get; set; } 
    public string ReceiverId { get; set; } 
    public string Message { get; set; } 
}

