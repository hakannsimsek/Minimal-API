using MinimalAPI.Models;

namespace MinimalAPI.Data
{
    public interface ICommandRepo
    {
        Task SaveChanges();
        Task<Command?> GetCommandById(int id);
        Task<IEnumerable<Command>> GetAllCommands();
        Task CreateCommands(Command command);
        void DeleteCommand(Command command);
    }
}
