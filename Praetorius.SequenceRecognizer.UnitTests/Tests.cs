using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Praetorius.SequenceRecognizer.UnitTests
{
    [TestClass]
    public class Tests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void EmptyAlphabet()
        {
            var sequences = new[]
            {
                new[] { 0, 0 },
                new[] { 1, 3 }
            };

            var g = new StateGraph(0, sequences);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void NonUniqueSequence()
        {
            var sequences = new[]
            {
                new[] { 0, 1 },
                new[] { 2, 1 },
                new[] { 2, 1 }
            };

            var g = new StateGraph(3, sequences);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void OneSymbolSequencesAreNotAllowed()
        {
            var sequences = new[]
            {
                new[] { 1 },
                new[] { 2 },
                new[] { 3 }
            };

            var g = new StateGraph(3, sequences);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullSequencesTest()
        {
            var g = new StateGraph(3, null);
        }

        [TestMethod]
        public void SameSymbolSequences()
        {
            for (int alphabetSize = 2; alphabetSize < 10; ++alphabetSize)
                for (int sequenceLenght = 2; sequenceLenght < 20; ++sequenceLenght)
                {
                    var sequences = new int[alphabetSize][];

                    for (int i = 0; i < alphabetSize; ++i)
                    {
                        var sequence = new int[sequenceLenght];

                        for (int j = 0; j < sequenceLenght; ++j)
                            sequence[j] = i;

                        sequences[i] = sequence;
                    }

                    var g = new StateMachine(new StateGraph(alphabetSize, sequences));

                    Assert.AreEqual(StateGraph.DefaultSymbol, g.Symbol);
                    Assert.AreEqual(StateGraph.InitialState, g.State);
                    Assert.AreEqual(StateGraph.InitialState, g.Sequence);

                    for (int i = 0; i < alphabetSize; ++i)
                    {
                        g.AcceptSymbol(i);

                        Assert.AreEqual(i + 1, g.Sequence);

                        for (int j = 0; j < sequenceLenght + 10; ++j)
                        {
                            g.AcceptSymbol(i);
                        }

                        Assert.AreEqual(i + 1 + alphabetSize, g.Sequence);
                    }
                }
                    
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullSequenceTest()
        {
            var sequences = new[]
            {
                new[] { 0, 1 },
                null,
                new[] { 2, 3 }
            };

            var g = new StateGraph(3, sequences);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void EmptySequenceTest()
        {
            var sequences = new[]
            {
                new[] { 0, 0, 0 },
                new int[0],
                new[] { 2, 2, 1 }
            };

            var g = new StateGraph(3, sequences);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void NegativeSymbolIndexTest()
        {
            var sequences = new[]
            {
                new[] { 0, 0 },
                new[] { 2, -2 }
            };

            var g = new StateGraph(3, sequences);
        }

        [TestMethod]
        public void AtomicStatesCountEqualsToAlphabetSizeWhenSequencesAreEmpty()
        {
            var sequences = new int[0][];

            for (int alphabetSize = 1; alphabetSize < 123; ++alphabetSize)
            {
                var g = new StateGraph(alphabetSize, sequences);

                Assert.AreEqual(alphabetSize + 1, g.StateCount);
            }
        }

        private string tgfSeparator = ",";

        [TestMethod]
        public void IntegrationTest()
        {
            var symbolSequences = new[]
            {
                new[] { 1, 3 },
                new[] { 2, 2, 2 },
                new[] { 2, 2, 1, 1 },
                new[] { 2, 2, 2, 2 },
                new[] { 2, 2, 2, 3 }
            };

            var alphabetSize = 4;

            var g = new StateGraph(alphabetSize);

            g.BuildTrie(symbolSequences);

            var initialTrieExp = g.PrintGraphExpression();
            var initialTrieTgf = g.PrintTrivialGraphFormat(tgfSeparator);

            Assert.AreEqual("(0@0>0'(1@1)1'(2@2>3'(5@5))2'(3@3>2'(_@6>1'(_@8>1'(7@9))2'(6@7>2'(8@10)3'(9@11))))3'(4@4))", initialTrieExp);
            Assert.AreEqual("0 0@0,1 1@1,2 2@2,3 3@3,4 4@4,5 5@5,6 _@6,8 _@8,7 6@7,9 7@9,10 8@10,11 9@11,#,0 1 0,0 2 1,0 3 2,0 4 3,2 5 3,3 6 2,6 8 1,6 7 2,8 9 1,7 10 2,7 11 3", initialTrieTgf);

            g.WriteInMissingSequenceIndices();

            var trieWithFilledSeqIndexes = g.PrintGraphExpression();
            var trieWithFilledSeqIndexesTgf = g.PrintTrivialGraphFormat(tgfSeparator);
            
            Assert.AreEqual("(0@0>0'(1@1)1'(2@2>3'(5@5))2'(3@3>2'(3@6>1'(2@8>1'(7@9))2'(6@7>2'(8@10)3'(9@11))))3'(4@4))", trieWithFilledSeqIndexes);
            Assert.AreEqual("0 0@0,1 1@1,2 2@2,3 3@3,4 4@4,5 5@5,6 3@6,8 2@8,7 6@7,9 7@9,10 8@10,11 9@11,#,0 1 0,0 2 1,0 3 2,0 4 3,2 5 3,3 6 2,6 8 1,6 7 2,8 9 1,7 10 2,7 11 3", trieWithFilledSeqIndexesTgf);

            g.WriteInMissingTransitions();

            var trieExpandedToGraph = g.PrintGraphExpression();
            var trieExpandedToGraphTgf = g.PrintTrivialGraphFormat(tgfSeparator);

            Assert.AreEqual("(0@0>0'(1@1>0'(~1@1)1'(2@2>0'(~1@1)1'(~2@2)2'(3@3>0'(~1@1)1'(~2@2)2'(3@6>0'(~1@1)1'(2@8>0'(~1@1)1'(7@9>0'(~1@1)1'(~2@2)2'(~3@3)3'(5@5>0'(~1@1)1'(~2@2)2'(~3@3)3'(4@4>0'(~1@1)1'(~2@2)2'(~3@3)3'(~4@4))))2'(~3@3)3'(~5@5))2'(6@7>0'(~1@1)1'(~2@8)2'(8@10>0'(~1@1)1'(~2@8)2'(~8@10)3'(9@11>0'(~1@1)1'(~2@2)2'(~3@3)3'(~4@4)))3'(~9@11))3'(~4@4))3'(~4@4))3'(~5@5))2'(~3@3)3'(~4@4))1'(~2@2)2'(~3@3)3'(~4@4))", trieExpandedToGraph);
            Assert.AreEqual("0 0@0,1 1@1,2 2@2,3 3@3,4 4@4,5 5@5,6 3@6,8 2@8,7 6@7,9 7@9,10 8@10,11 9@11,#,0 1 0,0 2 1,0 3 2,0 4 3,1 1 0,1 2 1,1 3 2,1 4 3,2 1 0,2 2 1,2 3 2,2 5 3,3 1 0,3 2 1,3 6 2,3 4 3,4 1 0,4 2 1,4 3 2,4 4 3,5 1 0,5 2 1,5 3 2,5 4 3,6 1 0,6 8 1,6 7 2,6 4 3,8 1 0,8 9 1,8 3 2,8 5 3,7 1 0,7 8 1,7 10 2,7 11 3,9 1 0,9 2 1,9 3 2,9 5 3,10 1 0,10 8 1,10 10 2,10 11 3,11 1 0,11 2 1,11 3 2,11 4 3", trieExpandedToGraphTgf);

            Assert.AreEqual(12, g.StateCount);

            var tm = g.ToTransitionMatrix();

            var m = new StateMachine(g);

            for (int symbol = 0; symbol < alphabetSize; ++symbol)
            {
                Assert.AreEqual(StateGraph.DefaultSymbol, m.Symbol);
                Assert.AreEqual(StateGraph.InitialState, m.State);
                Assert.AreEqual(StateGraph.InitialState, m.Sequence);

                m.AcceptSymbol(symbol);

                var expectedSequenceIdx = symbol + 1;

                Assert.AreEqual(symbol, m.Symbol);
                Assert.AreEqual(expectedSequenceIdx, m.State);
                Assert.AreEqual(expectedSequenceIdx, m.Sequence);

                m.Reset();

                Assert.AreEqual(StateGraph.DefaultSymbol, m.Symbol);
                Assert.AreEqual(StateGraph.InitialState, m.State);
                Assert.AreEqual(StateGraph.InitialState, m.Sequence);
            }

            for (int sequenceIndex = 0; sequenceIndex < symbolSequences.Length; ++sequenceIndex)
            {
                Assert.AreEqual(StateGraph.DefaultSymbol, m.Symbol);
                Assert.AreEqual(StateGraph.InitialState, m.State);
                Assert.AreEqual(StateGraph.InitialState, m.Sequence);

                var sequence = symbolSequences[sequenceIndex];

                foreach (var symbol in sequence)
                {
                    m.AcceptSymbol(symbol);

                    Assert.AreEqual(symbol, m.Symbol);
                }

                Assert.AreEqual(sequenceIndex + alphabetSize + 1, m.Sequence);

                m.Reset();

                Assert.AreEqual(StateGraph.DefaultSymbol, m.Symbol);
                Assert.AreEqual(StateGraph.InitialState, m.State);
                Assert.AreEqual(StateGraph.InitialState, m.Sequence);
            }

            Assert.AreEqual(StateGraph.DefaultSymbol, m.Symbol);
            Assert.AreEqual(StateGraph.InitialState, m.State);
            Assert.AreEqual(StateGraph.InitialState, m.Sequence);

            for (int sequenceIndex = 0; sequenceIndex < symbolSequences.Length; ++sequenceIndex)
            {
                var sequence = symbolSequences[sequenceIndex];

                foreach (var symbol in sequence)
                {
                    m.AcceptSymbol(symbol);

                    Assert.AreEqual(symbol, m.Symbol);
                }

                Assert.AreEqual(sequenceIndex + alphabetSize + 1, m.Sequence);
            }

            m.Reset();

            Assert.AreEqual(StateGraph.DefaultSymbol, m.Symbol);
            Assert.AreEqual(StateGraph.InitialState, m.State);
            Assert.AreEqual(StateGraph.InitialState, m.Sequence);

            m.AcceptSymbol(2);

            Assert.AreEqual(2, m.Symbol);
            Assert.AreEqual(3, m.State);
            Assert.AreEqual(3, m.Sequence);

            m.AcceptSymbol(2);

            Assert.AreEqual(2, m.Symbol);
            Assert.AreEqual(6, m.State);
            Assert.AreEqual(3, m.Sequence);

            m.AcceptSymbol(2);
            Assert.AreEqual(2, m.Symbol);
            Assert.AreEqual(7, m.State);
            Assert.AreEqual(6, m.Sequence);

            for (int i = 0; i < 123; ++i)
            {
                m.AcceptSymbol(2);

                Assert.AreEqual(2, m.Symbol);
                Assert.AreEqual(10, m.State);
                Assert.AreEqual(8, m.Sequence);
            }

            m.AcceptSymbol(3);

            Assert.AreEqual(3, m.Symbol);
            Assert.AreEqual(11, m.State);
            Assert.AreEqual(9, m.Sequence);

            for (int i = 0; i < 123; ++i)
            {
                m.AcceptSymbol(3);

                Assert.AreEqual(3, m.Symbol);
                Assert.AreEqual(4, m.State);
                Assert.AreEqual(4, m.Sequence);
            }

            for (int i = 0; i < 123; ++i)
            {
                m.AcceptSymbol(0);

                Assert.AreEqual(0, m.Symbol);
                Assert.AreEqual(1, m.State);
                Assert.AreEqual(1, m.Sequence);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void UnderpopulatedGraphToStateMatrix()
        {
            var g = new StateGraph(3);

            g.ToTransitionMatrix();
        }

        [TestMethod]
        public void TrivialGraphToStateMatrix()
        {
            for (int alphabetSize = 1; alphabetSize < 100; ++alphabetSize)
            {
                var g = new StateGraph(alphabetSize, new int[0][]);

                var tm = g.ToTransitionMatrix();

                var numberOfElements = alphabetSize * (alphabetSize + 1);

                Assert.AreEqual(numberOfElements, tm.Length);

                for (int i = 0; i < numberOfElements; ++i)
                {
                    Assert.AreEqual(i % alphabetSize + 1, tm[i]);
                }
            }
        }
    }
}