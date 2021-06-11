namespace Application.Interfaces
{
    public interface IKeyProvider
    {
        string GenerateSlug(string title);
        string GetUniqueKey(int size);
    }
}