using System;
using System.Collections.Generic;
using System.Linq;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            var calculator = new Calculator();
            var expression = Console.ReadLine();

            var result = calculator.Evaluate(expression);
            Console.WriteLine(result);
            Console.ReadKey();
        }
    }

    class Calculator
    {
        private readonly Dictionary<char, Func<Operation>> _operationPrototypes = new Dictionary<char, Func<Operation>>()
        {
            { '+', () => new Operation(1, (number1, number2) => number1 + number2) },
            { '-', () => new Operation(1, (number1, number2) => number1 - number2) },
            { '*', () => new Operation(2, (number1, number2) => number1 * number2) },
            { '/', () => new Operation(2, (number1, number2) => number1 / number2) }
        };

        public int Evaluate(string expression)
        {
            var nodes = Parse(expression);
            var nodesTree = CreateTree(nodes);

            return nodesTree.Evaluate();
        }

        private IEnumerable<Node> Parse(string expression)
        {
            var result = new List<Node>();
            var scobes = 0;

            for (int i = 0; i < expression.Length; i++)
            {
                var symbol = expression[i];
                if (symbol == '(' || symbol == ')')
                {
                    scobes += symbol == '(' ? 1 : -1;
                    continue;
                }

                var node = ParseIn(expression, i, scobes, out i);
                result.Add(node);
            }

            return result;
        }

        private Node ParseIn(string expression, int index, int scobes, out int lastIndex)
        {
            if (TryGetNumber(expression, index, out var number, out lastIndex))
            {
                number.AddScobes(scobes);

                return number;
            }

            return GetOperation(expression, index, scobes);
        }

        private Node CreateTree(IEnumerable<Node> nodes)
        {
            var lowest = GetLowestPriorityOf(nodes);
            var operation = lowest as Operation;

            if (operation != null)
            {
                var leftSide = nodes.TakeWhile(node => node != operation);
                var rightSide = nodes.SkipWhile(node => node != operation).Skip(1);

                var left = CreateTree(leftSide);
                var right = CreateTree(rightSide);

                return operation.Clone(left, right);
            }

            return lowest;
        }

        private Node GetLowestPriorityOf(IEnumerable<Node> nodes)
        {
            return nodes.OrderBy(node => node.Scobes).ThenBy(node => node.Priority).First();
        }

        private Operation GetOperation(string expression, int index, int scobes)
        {
            var symbol = expression[index];
            var operation = _operationPrototypes[symbol].Invoke();
            operation.AddScobes(scobes);

            return operation;
        }

        private bool TryGetNumber(string expression, int startIndex, out Number number, out int lastIndex)
        {
            var symbol = expression[startIndex];

            if (char.IsDigit(symbol) == false)
            {
                number = null;
                lastIndex = startIndex;
                return false;
            }

            number = GetNumber(expression, startIndex, out lastIndex);
            return true;
        }

        private Number GetNumber(string expression, int index, out int lastIndex)
        {
            var numberString = string.Empty;

            while (index < expression.Length && char.IsDigit(expression[index]))
            {
                numberString += expression[index];
                index++;
            }

            var numberInteger = int.Parse(numberString);
            lastIndex = index - 1;
            return new Number(numberInteger);
        }
    }

    abstract class Node
    {
        public int Scobes { get; private set; }

        public abstract int Priority { get; }

        public void AddScobes(int scobes)
        {
            Scobes += scobes;
        }

        public abstract int Evaluate();
    }

    class Operation : Node
    {
        private readonly int _priority;
        private readonly Func<int, int, int> _evaluation;

        private Node _left;
        private Node _right;

        public Operation(int priority, Func<int, int, int> evaluation)
        {
            _priority = priority;
            _evaluation = evaluation;
        }

        public Operation(int priority, Func<int, int, int> evaluation, Node left, Node right) : this(priority, evaluation)
        {
            _left = left;
            _right = right;
        }

        public override int Priority => _priority;

        public override int Evaluate()
        {
            var number1 = _left.Evaluate();
            var number2 = _right.Evaluate();

            return _evaluation.Invoke(number1, number2);
        }

        public Operation Clone(Node left, Node right)
        {
            return new Operation(_priority, _evaluation, left, right);
        }
    }

    class Number : Node
    {
        private int _number;

        public Number(int number)
        {
            _number = number;
        }

        public override int Priority => 100;

        public override int Evaluate()
        {
            return _number;
        }
    }
}
