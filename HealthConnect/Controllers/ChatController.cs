using HealthConnect.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

[Route("api/chat")]
[ApiController]
public class ChatController : ControllerBase
{
   

    private readonly string _connectionString;
    public ChatController(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("HealthConnect");
    }

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

    // Load messages between sender and receiver
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

    [HttpGet("getReceivers")]
    public IActionResult GetReceivers([FromQuery] string senderId)
    {
        if (!System.IO.File.Exists(filePath))
            return Ok(new List<object>());

        string existingData = System.IO.File.ReadAllText(filePath);
        var messages = JsonSerializer.Deserialize<List<ChatMessage>>(existingData) ?? new List<ChatMessage>();

        var receiverIds = messages
            .Where(msg => msg.SenderId == senderId || msg.ReceiverId == senderId)  // Include both SenderId and ReceiverId
            .Select(msg => msg.SenderId == senderId ? msg.ReceiverId : msg.SenderId) // Get the opposite party (Receiver or Sender)
            .Distinct()
            .ToList();

        var receiverDetails = new List<object>();

        using (SqlConnection con = new SqlConnection(_connectionString))
        {
            con.Open();
            foreach (var receiverId in receiverIds)
            {
                string query = "SELECT id, first_name, last_name, profile_pic, role, country, state, city FROM User_Table WHERE id = @ReceiverId";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@ReceiverId", receiverId);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            receiverDetails.Add(new
                            {
                                Id = reader["id"].ToString(),
                                FirstName = reader["first_name"].ToString(),
                                LastName = reader["last_name"].ToString(),
                                ProfilePic = reader["profile_pic"].ToString(),
                                Role = reader["role"].ToString(),
                                Country = reader["country"].ToString(),
                                State = reader["state"].ToString(),
                                City = reader["city"].ToString()
                            });
                        }
                    }
                }
            }
        }

        return Ok(receiverDetails);
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

public class ChatMessage2
{
    public int SenderId { get; set; } // Change from string to int
    public int ReceiverId { get; set; } // Change from string to int
    public string Message { get; set; }
    public DateTime Timestamp { get; set; }
}
