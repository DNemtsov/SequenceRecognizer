using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Praetorius.SequenceRecognizer
{
    public static class StateGraphExtensions
    {
        public static string PrintGraphExpression(this StateGraph graph)
        {
            var sb = new StringBuilder(1000);

            var visitedNodes = new HashSet<StateGraph.Node>();

            PrintGraphExpressionRecursive(graph.Root, sb, visitedNodes);

            return sb.ToString();
        }

        private static void PrintGraphExpressionRecursive(StateGraph.Node root, StringBuilder sb, HashSet<StateGraph.Node> visitedNodes)
        {
            if (visitedNodes.Contains(root))
                sb.Append($"(~{root.PrintCore()}");
            else
            {
                sb.Append($"({root.PrintCore()}");

                visitedNodes.Add(root);

                if (root.Transitions.Any(n => n != null))
                    sb.Append(">");

                var t = root.Transitions;

                for (int i = 0; i < t.Length; ++i)
                {
                    var transitionNode = t[i];

                    if (transitionNode == null)
                        continue;

                    sb.Append($"{i}'");

                    PrintGraphExpressionRecursive(transitionNode, sb, visitedNodes);
                }
            }

            sb.Append(")");
        }

        public static string PrintTrivialGraphFormat(this StateGraph graph, string separator = null)
        {
            separator = separator ?? Environment.NewLine;

            var traversalNodes = new LinkedList<StateGraph.Node>();

            var visitedNodes = new HashSet<StateGraph.Node>();

            var root = graph.Root;

            traversalNodes.AddLast(root);

            var nodesPrint = new LinkedList<string>();
            var transitionsPrint = new LinkedList<string>();

            while (traversalNodes.NotEmpty())
            {
                var currentNode = traversalNodes.GetRemoveFirst();

                if (visitedNodes.Contains(currentNode))
                    continue;

                var t = currentNode.Transitions;

                nodesPrint.AddLast($"{currentNode.NodeIndex} {currentNode.PrintCore()}");

                for (int i = 0; i < t.Length; ++i)
                {
                    var transitionNode = t[i];

                    if (transitionNode == null)
                        continue;

                    transitionsPrint.AddLast($"{currentNode.NodeIndex} {transitionNode.NodeIndex} {i}");

                    if (visitedNodes.Contains(transitionNode))
                        continue;

                    traversalNodes.AddLast(transitionNode);
                }

                visitedNodes.Add(currentNode);
            }

            return string.Join(separator, nodesPrint.Concat(Enumerable.Repeat("#", 1)).Concat(transitionsPrint));
        }

        public static void BuildTrie(this StateGraph graph, int[][] sequences)
        {
            if (sequences == null)
                throw new ArgumentNullException(nameof(sequences));

            var root = graph.Root;

            var nodeIndex = 1;
            var sequenceIndex = 1;
            var alphabetSize = graph.AlphabetSize;

            for (int i = 0; i < alphabetSize; ++i)
                root.Transitions[i] = new StateGraph.Node(alphabetSize, nodeIndex++) { SequenceIndex = sequenceIndex++ };

            for (int k = 0; k < sequences.Length; ++k)
            {
                var sequence = sequences[k]
                    ?? throw new ArgumentNullException($"sequence {sequenceIndex}");

                var lastSymbolIndex = sequence.Length - 1;

                if (lastSymbolIndex < 0)
                    throw new ArgumentException($"sequence {sequenceIndex} is empty");

                var currentNode = root;

                for (int j = 0; j <= lastSymbolIndex; ++j)
                {
                    var symbol = sequence[j];

                    if (symbol >= alphabetSize || symbol < 0)
                        throw new ArgumentException($"symbol {symbol} at index {j} of sequence {sequenceIndex} is out of the boundaries of the alphabet");

                    var t = currentNode.Transitions;

                    StateGraph.Node childNode = t[symbol];

                    if (childNode == null)
                    {
                        childNode = new StateGraph.Node(alphabetSize, nodeIndex++);
                        t[symbol] = childNode;
                    }

                    if (j == lastSymbolIndex)
                    {
                        if (childNode.SequenceIndex == -1)
                            childNode.SequenceIndex = sequenceIndex++;
                        else
                            throw new ArgumentException($"sequence {sequenceIndex} is equal to sequence {childNode.SequenceIndex}");
                    }

                    currentNode = childNode;
                }
            }

            graph.StateCount = nodeIndex;
        }

        public static void WriteInMissingSequenceIndices(this StateGraph graph)
            => WriteInMissingSequenceIndices(graph.Root, graph, new List<int>());

        private static void WriteInMissingSequenceIndices(StateGraph.Node node, StateGraph graph, List<int> sequenceReversed)
        {
            if (node.SequenceIndex < 0)
            {
                node.SequenceIndex = graph.FindNode(sequenceReversed).SequenceIndex;
            }

            var t = node.Transitions;
            var tLenght = t.Length;

            if (tLenght != graph.AlphabetSize)
                throw new ArgumentException("Node transition count is not consistent with the alphabet size");

            for (int i = 0; i < tLenght; ++i)
            {
                sequenceReversed.Add(i);

                var transitionNode = t[i];

                if (transitionNode != null)
                    WriteInMissingSequenceIndices(transitionNode, graph, sequenceReversed);

                sequenceReversed.RemoveLast();
            }
        }

        public static void WriteInMissingTransitions(this StateGraph graph)
            => WriteInMissingTransitions(graph.Root, graph, new List<int>());
        
        private static void WriteInMissingTransitions(StateGraph.Node node, StateGraph graph, List<int> sequenceReversed)
        {
            // the sequence is in reversed order which is kinda unnatural, but the Stack<> container does not have index-based
            // access, thus I'm using List<> adding to and removing from its back

            var t = node.Transitions;

            for (int i = 0; i < t.Length; ++i)
            {
                sequenceReversed.Add(i);

                StateGraph.Node transitionNode = t[i];

                if (transitionNode != null)
                    WriteInMissingTransitions(transitionNode, graph, sequenceReversed);
                else
                    t[i] = graph.FindNode(sequenceReversed);

                sequenceReversed.RemoveLast();
            }
        }

        public static StateGraph.Node FindNode(this StateGraph graph, IList<int> sequence)
        {
            var stateCount = sequence.Count;

            for (int beginIndex = 0; beginIndex < stateCount; ++beginIndex)
            {
                var currentNode = graph.Root;

                var lastStateIndex = stateCount - 1;

                for (int i = beginIndex; i <= lastStateIndex; ++i)
                {
                    var transitionNode = currentNode.Transitions[sequence[i]];

                    if (transitionNode == null)
                        break;

                    if (transitionNode.SequenceIndex < 0)
                        break;

                    if (i == lastStateIndex)
                        return transitionNode;

                    currentNode = transitionNode;
                }
            }

            throw new ArgumentException("the tree doesn't contain any nodes corresponding to the sequence provided");
        }

        public static int[] ToTransitionMatrix(this StateGraph graph)
        {
            var alphabetSize = graph.AlphabetSize;
            var nodeCount = graph.StateCount;

            if (nodeCount < alphabetSize + 1)
                throw new InvalidOperationException("The graph is underpopulated with nodes");

            var result = new int[alphabetSize * nodeCount];

            var traversalNodes = new LinkedList<StateGraph.Node>();
            var visitedNodes = new HashSet<StateGraph.Node>();

            var root = graph.Root;

            traversalNodes.AddLast(root);

            while (traversalNodes.NotEmpty())
            {
                var currentNode = traversalNodes.GetRemoveFirst();

                if (visitedNodes.Contains(currentNode))
                    continue;

                var t = currentNode.Transitions;

                var currentNodeIndex = currentNode.NodeIndex;

                var rowFirstIndex = currentNode.NodeIndex * alphabetSize;

                for (int i = 0; i < t.Length; ++i)
                {
                    var transitionNode = t[i];

                    if (transitionNode == null)
                        throw new InvalidOperationException($"Transition {i} is absent in node {currentNodeIndex}");

                    result[rowFirstIndex + i] = transitionNode.NodeIndex;

                    if (visitedNodes.Contains(transitionNode))
                        continue;

                    traversalNodes.AddLast(transitionNode);
                }

                visitedNodes.Add(currentNode);

                if (visitedNodes.Count == nodeCount)
                    return result;
            }

            return result;
        }
    }
}
