namespace MovieManager.Application.DTOs.Movies;

public class MovieDto
{
    public string Id { get; set; } = string.Empty;
    public string ExternalId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Year { get; set; }
    public List<string> Genres { get; set; } = new();
    public string Director { get; set; } = string.Empty;
    public List<string> Actors { get; set; } = new();
    public decimal Rating { get; set; }
    public int Duration { get; set; }
    public string Poster { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
