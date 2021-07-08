using System;
using System.Linq;
using middlerApp.SharedModels.Interfaces;
using Newtonsoft.Json.Linq;

namespace middlerApp.DataAccess.Entities.Models
{
    public class TreeNode : ITreeNode
    {
        public Guid Id { get; set; }

        public string Parent { get; set; }
        public string Name { get; set; }

        public string Path => $"{Parent}/{Name}".Trim('/');

        public bool IsFolder { get; set; }

        public string Extension { get; set; }

        public JToken Content { get; set; }

        public byte[] Bytes { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public ITreeNode[] Children { get; set; }
    }

    public static class TreeNodeExtensions
    {
        public static ITreeNode GetNodeByPath(this ITreeNode treeNode, string path)
        {
            var splitted = path.Split('/');
            var current = treeNode;
            foreach (var s in splitted)
            {
                current = current.Children.FirstOrDefault(n => n.Name == s);
                if (current == null)
                {
                    return null;
                }
            }

            return current;
        }
        
    }
}
