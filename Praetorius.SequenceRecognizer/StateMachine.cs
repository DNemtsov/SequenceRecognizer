using System;

namespace Praetorius.SequenceRecognizer
{
public class StateMachine
{
    public StateMachine(StateGraph stateGraph)
    {
        Graph = stateGraph
            ?? throw new ArgumentNullException(nameof(stateGraph));

        _currentNode = stateGraph.Root;

        Symbol = StateGraph.DefaultSymbol;
    }

    public readonly StateGraph Graph;
    private StateGraph.Node _currentNode;

    public int Symbol { get; private set; }

    public int State => _currentNode.NodeIndex;

    public void AcceptSymbol(int symbol)
    {
        try
        {
            _currentNode = _currentNode.Transitions[symbol];
        }
        catch (IndexOutOfRangeException e)
        {
            throw new ArgumentException("symbol is out of state range", e);
        }

        Symbol = symbol;
    }

    public int AcceptSymbolAndGetSequenceIndex(int state)
    {
        AcceptSymbol(state);

        return _currentNode.SequenceIndex;
    }

    public void Reset()
    {
        _currentNode = Graph.Root;
        Symbol = StateGraph.DefaultSymbol;
    }

    public int Sequence => _currentNode.SequenceIndex;
}
}
