using System.Threading.Tasks;

namespace middlerApp.SharedModels.Interfaces
{
    public interface IVariablesRepository
    {
        ITreeNode GetVariable(string parent, string name);
        Task<ITreeNode> GetFolderTree();
    }
}