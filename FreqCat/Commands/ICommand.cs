
namespace FreqCat.Commands
{
    public interface ICommand
    {
        public void Execute(bool isRedoing);

        public void UnExecute();
    }
}
