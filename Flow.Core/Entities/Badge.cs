public class Badge
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty; // Ex: "Colaborador", "Lider", "Inovador", "Engajado", "Veterano"
    public string Description { get; set; } = string.Empty;
    public int FlowbitsThreshold { get; set; }
    public string IconUrl { get; set; } = string.Empty;
}