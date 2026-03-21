// Models/Itinerary.cs
public class Itinerary
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string ImageSource { get; set; }
    public string Duration { get; set; } // Ví dụ: "60 phút"
    public int StopCount { get; set; }   // Ví dụ: 4
    public string Description { get; set; }
    public List<int> PoiIds { get; set; } // Danh sách ID các quán trong tour này
}