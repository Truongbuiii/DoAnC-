using CommunityToolkit.Mvvm.Messaging.Messages;

namespace FoodTourApp.Models
{
    // Tin nhắn này sẽ mang theo mã ngôn ngữ mới (ví dụ: "en-US")
    public class LanguageChangedMessage : ValueChangedMessage<string>
    {
        public LanguageChangedMessage(string value) : base(value)
        {
        }
    }
}