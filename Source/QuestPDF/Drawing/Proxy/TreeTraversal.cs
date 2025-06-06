﻿using System.Collections.Generic;
using System.Linq;
using QuestPDF.Infrastructure;

namespace QuestPDF.Drawing.Proxy;

internal sealed class TreeNode<T>
{
    public T Value { get; }
    public TreeNode<T>? Parent { get; set; }
    public ICollection<TreeNode<T>> Children { get; } = new List<TreeNode<T>>();
    
    public TreeNode(T Value)
    {
        this.Value = Value;
    }
}

internal static class TreeTraversal
{
    public static IEnumerable<TreeNode<T>> ExtractElementsOfType<T>(this Element element) where T : Element
    {
        if (element is T proxy)
        {
            var result = new TreeNode<T>(proxy);

            foreach (var treeNode in proxy.GetChildren().SelectMany(ExtractElementsOfType<T>))
            {
                result.Children.Add(treeNode);
                treeNode.Parent = result;
            }
                
            yield return result;
        }
        else
        {
            foreach (var treeNode in element.GetChildren().SelectMany(ExtractElementsOfType<T>))
                yield return treeNode;
        }
    }
    
    public static IEnumerable<TreeNode<T>> Flatten<T>(this TreeNode<T> element) where T : Element
    {
        yield return element;

        foreach (var child in element.Children)
            foreach (var innerChild in Flatten(child))
                yield return innerChild;
    }
    
    public static IEnumerable<TreeNode<T>> ExtractAncestors<T>(this TreeNode<T> node)
    {
        while (true)
        {
            node = node.Parent;
            
            if (node is null)
                yield break;

            yield return node;
        }
    }
}