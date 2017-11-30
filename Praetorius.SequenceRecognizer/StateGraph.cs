using System;
using System.Text;

namespace Praetorius.SequenceRecognizer
{
    public class StateGraph
    {
        public static readonly int DefaultSymbol = -1;
        public static readonly int InitialState = 0;

        public readonly int AlphabetSize = 0;
        public int StateCount = 1;
        public readonly Node Root;

        public StateGraph(int alphabetSize)
        {
            if (alphabetSize < 1)
                throw new ArgumentException($"{nameof(alphabetSize)} should be greated than zero");

            Root = new Node(alphabetSize, 0) { SequenceIndex = 0 };
            AlphabetSize = alphabetSize;
        }

        public StateGraph(int alphabetSize, int[][] sequences)
            : this(alphabetSize)
        {
            this.BuildTrie(sequences);

            this.WriteInMissingSequenceIndices();

            this.WriteInMissingTransitions();
        }

        public class Node
        {
            public Node(int transitionCount, int nodeIndex)
            {
                Transitions = new Node[transitionCount];

                NodeIndex = nodeIndex;
            }

            public readonly int NodeIndex;

            public int SequenceIndex = -1;

            public readonly Node[] Transitions;

            public override string ToString()
            {
                var sb = new StringBuilder();

                sb.Append(PrintCore());

                if (!Transitions.Empty())
                    sb.Append(">");

                for (int i = 0; i < Transitions.Length; ++i)
                {
                    if (Transitions[i] == null)
                        continue;

                    sb.Append($"{i}'");
                }

                return sb.ToString();
            }

            public string PrintCore()
            {
                var sequenceIndexStr = SequenceIndex >= 0 ? SequenceIndex.ToString() : "_";

                return $"{sequenceIndexStr}@{NodeIndex}";
            }
        }
    }
}
